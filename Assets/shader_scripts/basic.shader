Shader "Unlit/basic"
{
    Properties
    {
        [Space(5)]
        _MainTex ("Texture", 2D) = "white" {}

        [Space(5)]
        [NoScaleOffset]
        _GlossMap("Gloss Map", 2D) = "white" {}

        [Space(5)]
        [NoScaleOffset][Normal]
        _NormalMap("Normal Map", 2D) = "bump"{}
        _NIntensity("Normal Intensity", Range(0.,1.0)) = 1.0


        [Space(15)]
        _AmbientL ("Ambient Light", Color) = (1,.75,.875,.25)

        [Space(5)]
        _FresnelCol("Fresnel Color", Color) = (.85,.55,1.,.25)

        [Space(5)]
        _Spec("Specular Color", Color) = (1,.75,.875,1.)

    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass // base
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd_fullshadows
            #define IS_IN_BASE_PASS
            #include "Assets\shader_scripts\BaseShader.cginc"
            
            ENDCG
        }
        Pass // forward add
        {
            Tags { "LightMode" = "ForwardAdd" }
            Blend OneMinusDstColor One
            //Blend One Zero //for debugging the spotlight and stuff
            // the first value is what this pass is multiplied by
            // the second value is what the color in the screen is multiplied by
            // then they are added together

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd
            #pragma multi_compile_fwdadd_fullshadows
            #include "Assets\shader_scripts\BaseShader.cginc"

            ENDCG

        }

        Pass // shadows
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
    Fallback "Specular"
}
