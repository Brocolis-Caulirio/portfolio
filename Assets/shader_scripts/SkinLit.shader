Shader "Unlit/SkinShader"
{
    Properties
    {
        [NoScaleOffset]
        _MainTex("Texture", 2D) = "white" {}

        [NoScaleOffset]
        [Normal]
        _NormalMap("Normal Map", 2D) = "white" {}

        [NoScaleOffset]
        _ReflectionMap("Reflection Map", 2D) = "white" {}

        [NoScaleOffset]
        _Glossiness("Glossiness", 2D) = "white" {}

        [NoScaleOffset]
        _HeightMap("Height Map", 2D) = "white" {}

        _Shinniness("Shinniness", Range(0.,1.)) = 1.
        _ShadowDepth("Shadow Depth", Range(0.,1.)) = .5
        _Teste("iN cT cB cN wN", Range(0.,3.)) = .25
        [ToggleOff] _InBool("ativar normal map", float) = 0.

    }
    SubShader
    {
        
        LOD 100

        Pass
        {

            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM

            #pragma multi_compile_fwdbase 
            #pragma multi_compile_fwdadd_fullshadows
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            float4 _LightColor0;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            //float4 _WorldSpaceLightPos0;

            sampler2D _NormalMap;
            sampler2D _ReflectionMap;
            sampler2D _Glossiness;
            sampler2D _HeightMap;
            float _Shinniness;
            float _ShadowDepth;
            float _Teste;
            float _InBool;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;                            
                float4 worldPos : TEXCOORD1;
                SHADOW_COORDS(2)
                float3 worldNormal : NORMAL;
                float3 tangent : TANGENT;                
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.tangent = UnityObjectToWorldDir(v.tangent);
                TRANSFER_SHADOW(o)                         

                return o;

                //UnityObjectToWorldNormal is just a mul(input, (float3x3)unity_WorldToObject) to transform it to world space
                //https://github.com/TwoTailsGames/Unity-Built-in-Shaders/blob/master/CGIncludes/UnityCG.cginc    

            }

            fixed4 frag(v2f i) : SV_Target
            {
                //colors and textures
                float4 cLightColor = _LightColor0;
                float4 cGloss = tex2D(_Glossiness, i.uv);
                float4 cReflection = tex2D(_ReflectionMap, i.uv);
                float4 cHeight = tex2D(_HeightMap, i.uv);
                float4 cTex = tex2D(_MainTex, i.uv);

                //vectors
                float3 wVertToLight = _WorldSpaceLightPos0.xyz;
                float3 wCamToVert = i.worldPos.xyz - _WorldSpaceCameraPos.xyz;

                float3 cNormal = tex2D(_NormalMap, i.uv);
                cNormal = normalize(float3(cNormal.r, cNormal.g, cNormal.b) * 2. - 1.); //inverted red bc unity �\_(o-o)_/�

                float3 tan = normalize(i.tangent);
                float3 nor = normalize(i.worldNormal);
                //input Tangent Bitangent Normal
                float3 iTBN[3] = {  tan,
                                    normalize(cross(tan, nor)),
                                    nor };

                float3 wNormal = float3(iTBN[0] * -cNormal.x +
                                        iTBN[1] * cNormal.y +
                                        iTBN[2] * cNormal.z);

                //wNormal = normalize(float3(wNormal.x * -1., wNormal.yz));

                // they all come from the same value so they complete each other
                // the normal will always be > 0 and so its tangents will too
                // there is the possibility of my specific vector aligning with the normal, but its REALLY rare
                // https://www.youtube.com/watch?v=6_-NNKc4lrk&ab_channel=Makin%27StuffLookGood


                //values
                float atten = 1.;
                float sAtten = SHADOW_ATTENUATION(i);

                //preparations for directional light or not
                if (_WorldSpaceLightPos0.w == 0)//directional light
                {
                    atten = 1.; //no attenuation
                    wVertToLight = normalize(_WorldSpaceLightPos0.xyz);
                }
                else
                {
                    float3 calc = i.worldPos.xyz - _WorldSpaceLightPos0.xyz;
                    wVertToLight = calc;
                    atten = smoothstep(0., 1., 1. / length(calc));
                }

                float NDotL = dot(wNormal, wVertToLight);
                float sNDotL = saturate(NDotL);
                float3 LightReflect = reflect(wVertToLight, wNormal);

                //diffisue light
                float3 dif = sNDotL * .6 + .3;


                //specular
                float3 spec = (dot(LightReflect, normalize(-wCamToVert)));                
                float shinniness = _Shinniness * 100.;
                spec = max(0., pow(spec, shinniness) * _Shinniness);
                spec *= cGloss;

                //shadow
                _ShadowDepth = 1. - _ShadowDepth;
                float shadow = smoothstep(.5, 1., sAtten) / 2. + (.5 * _ShadowDepth);
                shadow = min(shadow, dif.x);
                shadow = lerp( shadow, 1., _ShadowDepth);
                

                float4 col = cTex * float4(dif.xyz, 1.) * shadow + float4(spec.xyz, 0.);


                return sAtten;


                //half3 shadow = ShadeSH9(half4(wNormal, 1.));
                //                if (_Teste == 0.)
                //                    return float4(iTBN[2], 1.);
                //                else if (_Teste < 1.)
                //                    return float4(iTBN[0] * -cNormal.r, 1.);
                //                else if (_Teste < 2.)
                //                    return float4(iTBN[1] * cNormal.g, 1.);
                //                else if (_Teste < 3.)
                //                    return float4(iTBN[2] * cNormal.b, 1.);
                //                else
                //                    return float4(wNormal, 1.);

                //WRONG TO REVIEW WHY LATER
                //specular light RefView
                //float3 ViewReflect = normalize(reflect(wCamToVert, wNormal.xyz));
                //float VdotL = max(0., dot(ViewReflect, normalize(-wVertToLight) ));//(smoothstep(100.,0.,_Shinniness)+1.);
                //float4 spec = pow((VdotL), _Shinniness) ;
                

            }

            ENDCG
        }

        Pass
        {
            Tags {"LightMode" = "ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f
            {   //this just gets the ver position in shadow space
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                //can use normal to offset shadow, require normal as input in vertex shader
                //appdata_base has normals
                //v.vertex = UnityObjectToClipPos(v.vertex);
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
        
    }
}
