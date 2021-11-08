Shader "MyShaders/freya"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [NoScaleOffset][Normal] 
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NIntensity("Normal Intensity", Range(0, 1)) = 1

        [NoScaleOffset]
        _HeightMap ("Height map", 2D) = "gray" {}
        _HeightStrength ("Height Strength", Range(0,0.2)) = 0
        
        [NoScaleOffset]
        _GlossMap("Gloss map", 2D) = "white" {}
        _Gloss ("Glossiness", Range(0,1)) = 1

        _Color ("Tint", Color) = (1.,1.,1.,1.)
        _ShadowDepth ("Shadow depth", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }


        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
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
            sampler2D _HeightMap;
            
            float _NIntensity;
            float _Gloss;
            float4 _Color;
            float _ShadowDepth;
            float _HeightStrength;

            v2f vert (appdata v)
            {
                v2f o;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float height = tex2Dlod(_HeightMap, float4(o.uv, 0, 0)) * 2 - 1;
                //the float 4 takes the first two parameters as the coordinates and two last as the mip levels
                //0 being the highest, 1 being the next one downscaled and so on

                v.vertex.xyz += v.normal * (height * _HeightStrength);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.wPos = mul(unity_ObjectToWorld, v.vertex);

                o.normal = UnityObjectToWorldNormal(v.normal);
                o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.bitangent =   cross(o.normal, o.tangent);
                o.bitangent *=  v.tangent.w * unity_WorldTransformParams.w;
                //tangent.w is the sign of flipped uvs
                //unity_WorldTransformParams.w is the sign of the negative scale transformation
                

                TRANSFER_VERTEX_TO_FRAGMENT(o); // lighting, actually
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //textures
                float GlossMap = tex2D(_GlossMap, i.uv).x;
                float4 tex = tex2D(_MainTex, i.uv);
                float3 tangentNormal = UnpackNormal(tex2D(_NormalMap, i.uv));
                //tex2D gets the unpacked val, which has only x and y values
                //UnpackNormal does the math for z and get you the full dir
                tangentNormal = lerp(normalize(float3(0,0,1)), tangentNormal, _NIntensity);
                //applied intensity
                float3x3 m_TangentToWorld = {
                    i.tangent.x, i.bitangent.x, i.normal.x,
                    i.tangent.y, i.bitangent.y, i.normal.y,
                    i.tangent.z, i.bitangent.z, i.normal.z,
                };
            
                //diffuse
                float3 N = mul(m_TangentToWorld, tangentNormal); //normalize(i.normal);
                float3 L = _WorldSpaceLightPos0.xyz;
                float3 lambert = saturate(dot(N, L));
                float3 diff = min(1., lambert * _LightColor0.xyz + _ShadowDepth);
                float atten = LIGHT_ATTENUATION(i);

                //specular
                float3 V = normalize(_WorldSpaceCameraPos - i.wPos);
                float3 H = normalize(L + V); // halfway between them
                //float3 R = reflect(-L, N);
                float3 spec = saturate(dot(H, N)) * (lambert > 0); 
                //this last part multiplies by 1 when true
                //to add a glossmap you multiply the _Gloss value here
                _Gloss *= GlossMap;
                float specExp = exp2(_Gloss * 11) + 2.;
                //2^(gloss*11)
                spec = pow(spec, specExp) * _Gloss;
                //multiplied by glossiness to lower the output 
                //so it does not return a value greater than the input
                spec *= _LightColor0.xyz;
                
                
                float4 col = float4(atten * (diff * _Color + spec), 1.);
                col *= tex;

                return col;

            }
            ENDCG
        }
    }
}
