Shader "MyShaders/GameStudy/sunset"
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
        
        [Space(25)]
        [NoScaleOffset]
        _GlossMap("Gloss map", 2D) = "white" {}
        _Gloss ("Glossiness", Range(0,1)) = 1

        [Space(25)]
        _AmbientLight ("Ambient Light", Color) = (0.,0.,0.,0.)
        _Color ("Tint", Color) = (1.,1.,1.,1.)

        _ToonShades ("Shade dubdivision count", Range(3,10)) = 5
        _Fresnel("Backlight Strength", Range(0,1)) = 1
        _FresnelTint("Baclight Tint", Color) = (1,1,1,1)

        _Lower("lower", Range(0.,1.)) = 1.
        _Upper("upper", Range(0.,1.)) = 0.
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
            #pragma multi_compile_fwdadd_fullshadows
            #define IS_IN_BASE_PASS
            #include "sunsetStudy.cginc"
            
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Blend OneMinusDstColor One

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd
            #pragma multi_compile_fwdadd_fullshadows
            #include "sunsetStudy.cginc"
            
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
