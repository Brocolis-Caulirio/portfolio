Shader "MyShaders/SkinGlowing"
{

    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [NoScaleOffset][Normal] 
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NIntensity("Normal Intensity", Range(0, 1)) = 1

        [Space(25)]
        [Toggle] 
        _BacklightToggle("Using Backlight", Float) = 0
        [NoScaleOffset]
        _BackResist("Backlight glow resistance", 2D) = "black" {}
        _BackStr("Backlight effect strength", Range(0,1)) = 1
        _BackDist("Backlight min distance", Range(0.1,1)) = .5
        _BackLumen("Backlight min intensity", Range(0.1, 8)) = 1
        
        [Space(25)]
        [NoScaleOffset]
        _GlossMap("Gloss map", 2D) = "white" {}
        _Gloss ("Glossiness", Range(0,1)) = 1
        
        [Space(25)]
        [NoScaleOffset]
        _HeightMap ("Height map", 2D) = "gray" {}

        [Space(25)]
        _AmbientLight ("Ambient Light", Color) = (0.,0.,0.,0.)
        _Color ("Tint", Color) = (1.,1.,1.,1.)

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
            #include "skinGlow.cginc"
            
            ENDCG
        }

        Pass
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
            #include "skinGlow.cginc"
            
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
