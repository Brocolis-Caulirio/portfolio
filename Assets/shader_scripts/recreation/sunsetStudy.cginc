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
    //SHADOW_COORDS(8)
};

sampler2D _MainTex;
float4 _MainTex_ST;
sampler2D _GlossMap;
sampler2D _NormalMap;
sampler2D _HeightMap;
float4 _AmbientLight;

float _NIntensity;
float _Gloss;
float4 _Color;
float _HeightStrength;
float _ToonShades;
float _ShadowPrecision;

float _Fresnel;
float4 _FresnelTint;

float _Lower;
float _Upper;


v2f vert (appdata v)
{
    v2f o;

    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    float height = tex2Dlod(_HeightMap, float4(o.uv, 0, 0)) * 2 - 1;
    //the float 4 takes the first two parameters as the coordinates and two last as the mip levels
    //0 being the highest, 1 being the next one downscaled and so on

    v.vertex.xyz += v.normal * (height * _HeightStrength);
    
    o.pos = UnityObjectToClipPos(v.vertex);
    o.wPos = mul(unity_ObjectToWorld, v.vertex);

    o.normal = UnityObjectToWorldNormal(v.normal);
    o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
    o.bitangent =   cross(o.normal, o.tangent);
    o.bitangent *=  v.tangent.w * unity_WorldTransformParams.w;
    //tangent.w is the sign of flipped uvs
    //unity_WorldTransformParams.w is the sign of the negative scale transformation
    
    TRANSFER_VERTEX_TO_FRAGMENT(o); // lighting, actually
    TRANSFER_SHADOW(o)    
    return o;
}

fixed4 frag(v2f i) : SV_Target
{

    _ToonShades = floor(_ToonShades);

    //textures
    float GlossMap = tex2D(_GlossMap, i.uv).x;
    float4 tex = tex2D(_MainTex, i.uv);
    float3 tangentNormal = UnpackNormal(tex2D(_NormalMap, i.uv));
    //tex2D gets the unpacked val, which has only x and y values
    //UnpackNormal does the math for z and get you the full dir

    tangentNormal = lerp(normalize(float3(0,0,1)), tangentNormal, _NIntensity);
    //applied intensity, it lerps between a bump map and the normal map

    float3x3 m_TangentToWorld = {
        i.tangent.x, i.bitangent.x, i.normal.x,
        i.tangent.y, i.bitangent.y, i.normal.y,
        i.tangent.z, i.bitangent.z, i.normal.z,
    };

    //diffuse
    float3 N = mul(m_TangentToWorld, tangentNormal); //normalize(i.normal);
    float3 L = normalize(UnityWorldSpaceLightDir(i.wPos));
    float atten = LIGHT_ATTENUATION(i);
    float ndotl = dot(N, L);

    float3 lambert = saturate(ndotl);
    float3 diff = (lambert * atten); 
    #ifdef IS_IN_BASE_PASS
        diff += _AmbientLight;
    #endif
    diff = toon(diff, _ToonShades);   

    //specular
    float3 V = normalize(_WorldSpaceCameraPos - i.wPos);
    float3 H = normalize(L + V); // halfway between them
    //float3 R = reflect(-L, N);
    float3 spec = saturate(dot(H, N)) * (lambert > 0); //this last part multiplies by 1 when true 
    float Gloss = _Gloss * GlossMap;
    float specExp = exp2(Gloss * 11) + 2.;
    //2^(gloss*11)
    spec = pow(spec, specExp) * Gloss * atten;
    //multiplied by glossiness to lower the output 
    //so it does not return a value greater than the input     

    spec *= _LightColor0.xyz;   
    diff *= _LightColor0.xyz;
    

    float4 col = float4( (diff * tex * _Color + spec), 1.);   
    float4 test = toon(LIGHT_ATTENUATION(i)+.5, _ToonShades) * tex; 
    float4 Fresnel = pow(1-dot(V, N), 10) ;
    //Fresnel = saturate()    

    return col;
}
/*

fresnel on shadows for a better lit model too

*/