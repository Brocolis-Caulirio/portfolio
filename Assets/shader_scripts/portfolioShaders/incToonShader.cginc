#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "Assets\shader_scripts\customFunctions.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
};

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : TEXCOORD1;
    float3 tangent : TEXCOORD2;
    float3 bitangent : TEXCOORD3;
    float3 wPos : TEXCOORD4;
    LIGHTING_COORDS(5, 6)
        float3 oPos : TEXCOORD7;
};

sampler2D _MainTex;
sampler2D _NormalMap;
sampler2D _GlossMap;

float4 _MainTex_ST;
float _NIntensity;
float _ToonCount;
float _ToonMD;

float4 _Spec;
float4 _AmbientL;
float4 _FresnelCol;


v2f vert(appdata v)
{
    v2f o;
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.pos = UnityObjectToClipPos(v.vertex);
    o.wPos = mul(unity_ObjectToWorld, v.vertex);

    o.normal = UnityObjectToWorldNormal(v.normal);
    o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
    o.bitangent = cross(o.normal, o.tangent);
    o.bitangent *= v.tangent.w * unity_WorldTransformParams.w;

    TRANSFER_VERTEX_TO_FRAGMENT(o); // lighting 
    TRANSFER_SHADOW(o)
    return o;
}

fixed4 frag(v2f i) : SV_Target
{

    //SETUPS
    //------------------------// //------------------------//

    //normal values
    //------------------------//

    float3 tangentNormal = UnpackNormal(tex2D(_NormalMap, i.uv));
    //tex2D gets the unpacked val, which has only x and y values
    //UnpackNormal does the math for z and get you the full dir
    tangentNormal = lerp(normalize(float3(0,0,1)), tangentNormal, _NIntensity);
    //applied intensity, it lerps between a bump map and the normal map
    float3x3 m_TBN = TBN_maker(i.tangent, i.bitangent, i.normal);

    //------------------------//

    //textures
    //------------------------//

    fixed GlossMap = tex2D(_GlossMap, i.uv).x;
    fixed4 tex = tex2D(_MainTex, i.uv);

    //------------------------//

    //directions
    //------------------------//

    float3 N = mul(m_TBN, tangentNormal); //normalize(i.normal);
    float3 L = normalize(UnityWorldSpaceLightDir(i.wPos));
    float3 V = normalize(_WorldSpaceCameraPos - i.wPos);
    float3 HV = normalize(L + V); // halfway between them
    //makes for a better specular, got it from Acegikmo
    float3 R = reflect(-L, N);

    //------------------------//

    //values
    //------------------------//

    //basic dots and shadows
    float atten = LIGHT_ATTENUATION(i);
    float ndotl = dot(N, L);
    float nDotV = dot(N, V);

    //lambertion pre calc
    float lambert = (ndotl/2 + .5) * (atten/2. + .5);

    //fresnel aka backlight calculation
    float4 fresnel = (pow(1 - nDotV, 3) * _FresnelCol);
    fresnel = smoothstep(0, 1, fresnel) * _FresnelCol.a;
    fresnel = float4(_FresnelCol.rgb * fresnel, fresnel.a);

    //ambient light and light color
    float4 ambientLight = float4(_AmbientL.rgb * _AmbientL.a, 1) * (1 - lambert);
    float4 lightColor = normalize(_LightColor0);

    //------------------------//

    //------------------------// //------------------------//   



    //lights and finalization
    //------------------------// //------------------------//   

    //specular light
    float4 spec = saturate(dot(HV, N)) * (lambert > 0); //this last part multiplies by 1 when true 
    float Gloss = _Spec.a * GlossMap;
    float specExp = exp2(Gloss * 11) + 2.;
    spec = pow(spec, specExp) * Gloss * atten;
    spec.rgb *= _Spec.rgb;
    spec = smoothToon(spec, _ToonCount, _ToonMD);

    //diffuse light
    float diff = lambert;

    //tooning

    #ifdef IS_IN_BASE_PASS       
        diff += ambientLight;
    #endif
    diff = smoothToon(lambert/2 + .5, _ToonCount, _ToonMD);// if I put this befor ambient light, it turns into the shader I wanted lol

    //values application
    fixed4 col = tex * diff * lightColor;

    col += fresnel + spec;

    return col;

    //------------------------// //------------------------//   

}
