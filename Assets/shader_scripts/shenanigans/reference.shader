Shader "Unlit/Reference"
{
	Properties
	{
		_NormalTex("Normal Map", 2D) = "white" {}
		_Teste("iN cT cB cN wN", Range(0.,3.)) = .25
	}

		SubShader
	{
		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 tbn[3] : TEXCOORD1; // TEXCOORD2; TEXCOORD3;
			};

			sampler2D _NormalTex;
			float4 _NormalTex_ST;
			float _Teste;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _NormalTex);

				float3 normal = UnityObjectToWorldNormal(v.normal);
				float3 tangent = cross( v.normal,  float3(1.123, 1.456, 1.789) );
				tangent = UnityObjectToWorldNormal(v.tangent);
				float3 bitangent = cross(tangent, normal);

				o.tbn[0] = tangent;
				o.tbn[1] = bitangent;
				o.tbn[2] = normal;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float3 colorNormal = normalize(tex2D(_NormalTex, i.uv));
				colorNormal = float3(1. - colorNormal.r, colorNormal.gb);

				float3 surfaceNormal = normalize(i.tbn[2]);
				float3 worldNormal = float3(i.tbn[0] * -colorNormal.r + 
											i.tbn[1] *  colorNormal.g + 
											i.tbn[2] *  colorNormal.b);
				//worldNormal = normalize(worldNormal);

				if (_Teste == 0.)
					return float4(i.tbn[2], 1.);
				else if (_Teste < 1.)
					return float4(i.tbn[0] * -colorNormal.r, 1.);
				else if (_Teste < 2.)
					return float4(i.tbn[1] * colorNormal.g, 1.);
				else if (_Teste < 3.)
					return float4(i.tbn[2] * colorNormal.b, 1.);
				else
					return float4(worldNormal, 1.);

			}
			ENDCG
		}
	}
}
