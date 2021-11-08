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
float4 _MainTex_ST;
sampler2D _GlossMap;
sampler2D _NormalMap;
sampler2D _BackResist;
sampler2D _BackBoost;
sampler2D _HeightMap;
float4 _AmbientLight;
float _NIntensity;
float _Gloss;
float _BackStr;
float _BacklightToggle;
float _BackDist;
float _BackLumen;

v2f vert (appdata v)
{

    v2f o;
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.pos = UnityObjectToClipPos(v.vertex);
    o.wPos = mul(unity_ObjectToWorld, v.vertex);

    o.normal = UnityObjectToWorldNormal(v.normal);
    o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
    o.bitangent =   cross(o.normal, o.tangent);
    o.bitangent *=  v.tangent.w * unity_WorldTransformParams.w;
    
    TRANSFER_VERTEX_TO_FRAGMENT(o); // lighting 
    TRANSFER_SHADOW(o)    
    return o;

}

fixed4 frag(v2f i) : SV_Target
{

    //SETUPS
    //------------------------// //------------------------//
    float3 tangentNormal = UnpackNormal(tex2D(_NormalMap, i.uv));
    //tex2D gets the unpacked val, which has only x and y values
    //UnpackNormal does the math for z and get you the full dir
    tangentNormal = lerp(normalize(float3(0,0,1)), tangentNormal, _NIntensity);
    //applied intensity, it lerps between a bump map and the normal map
    float3x3 m_TBN = TBN_maker(i.tangent, i.bitangent, i.normal);
    //------------------------// //------------------------//

    //FREQUENT VARIABLES
    //------------------------// //------------------------//

    //textures
    float GlossMap = tex2D(_GlossMap, i.uv).x;
    float4 tex = tex2D(_MainTex, i.uv);  
    float4 bResist = tex2D(_BackResist, i.uv);
    float4 bBoost = tex;
    float height = tex2D(_HeightMap, i.uv).x * 2 - 1;
    //this is different bc certain part can have negative resistance and "glow" easily ie ears for being thin

    //directions
    float3 N = mul(m_TBN, tangentNormal); //normalize(i.normal);
    float3 L = normalize(UnityWorldSpaceLightDir(i.wPos));
    float3 V = normalize(_WorldSpaceCameraPos - i.wPos);
    float3 HV = normalize(L + V); // halfway between them
    float3 R = reflect(-L, N);

    //values
    float atten = LIGHT_ATTENUATION(i);
    float ndotl = dot(N, L);
    float pNdotL = (ndotl>0) * ndotl;
    float nNdotL = abs((ndotl<0) * ndotl);
    float nDotV = dot(N, V);
    float fresnel = pow(1-nDotV, 3);
    float lightStr = LightIntensity(_LightColor0);
    float lD = (_WorldSpaceLightPos0.w==1) * length(_WorldSpaceLightPos0 - i.wPos);
    float4 lightColor = normalize(_LightColor0);
    float4 backFilter = max(0, min(1, bBoost - bResist));

    //------------------------// //------------------------//

    //SHADING ALGORITHMS
    //------------------------// //------------------------//
    
    //diffuse light
    float4 diff = (max(0, ndotl));

    //specular light
    float4 spec = saturate(dot(HV, N)) * (diff > 0); //this last part multiplies by 1 when true 
    float Gloss = _Gloss * GlossMap;
    float specExp = exp2(Gloss * 11) + 2.;
    //2^(gloss*11)
    spec = pow(spec, specExp) * Gloss * atten;

    //------------------------// //------------------------//
    

    //BACK GLOW AND AMBIENT LIGHT
    //------------------------// //------------------------//

    float4 backGlow = 0.;
    float4 bValue = 1-nNdotL;
    
    #ifndef IS_IN_BASE_PASS       
    //only when not on base pass for the love of the gods   
        if(_BacklightToggle > 0) 
        {
            //area of effect
            backGlow = bValue;
            backGlow *= _BackStr;
            backGlow *= pow(1-pNdotL, 6) ;

            //if the effect is applicable
            float distRange = clamp(_BackDist - lD, 0.01, _BackDist)/_BackDist;
            float lumen = ((lightStr/2) * distRange) / _BackLumen;

            backGlow = backGlow * lumen * lightColor * backFilter;
            // * (1-abs(nDotV)); // probably change this for an inverse lerp to make it more visible but still fresnel-ish
            diff += _AmbientLight * _AmbientLight.a;

        }
    #else
        diff += float4(_AmbientLight.rgb, 1.) * _AmbientLight.a;
        diff += fresnel * _AmbientLight * _AmbientLight.a;
    #endif    

    //------------------------// //------------------------//

    //color aplication 
    //------------------------// //------------------------//
    
    //shading algorithms 
    spec *= _LightColor0 * atten;
    diff *= _LightColor0 * atten;    

    //texture aplication
    float4 col = diff * tex;
    col +=  spec + backGlow;

    //------------------------// //------------------------//     

    return max(0, col);//debugKRGBW(lumen, 0, 1 ) ;// debugKRGBW(lightStr * lD, 0, 1);
    //return backGlow;

    /*
    this is only in case if its a thin part:
    (pow(nNdotL, 12))
    this is multiplied by the fresnel:
    pow(1-nNdotL, 6)
    so it is:
    pow(1-nNdotL, 6) + ( (pow(nNdotL, 12)) * (1-thickness))
    */
    //float teste = 1- max(0, min(_BackDist, uInvLerp(0, _BackDist, lD)))  / _BackDist;

}
