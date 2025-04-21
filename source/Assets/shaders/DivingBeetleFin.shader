Shader "DivingBeetleFin"
{
	Properties
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	Category
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Fog { Color(0.0, 0.0, 0.0, 0.0) }
		Lighting Off
		Cull Off

		BindChannels
		{
			Bind "Vertex", vertex
			Bind "texcoord", texcoord
			Bind "Color", color
		}

		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				//#include "_ShaderFix.cginc" -> unused code
				sampler2D _MainTex;
				float4 _MainTex_ST;
				
				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float2 scrPos : TEXCOORD1;
					float4 clr : COLOR;
				};

				v2f vert(appdata_full v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.scrPos = ComputeScreenPos(o.pos);
					o.clr = v.color;
					float s = sin(o.clr.w);
					float c = cos(o.clr.w);
					o.uv.xy = mul(float2x2(c, -s, s, c), o.uv.xy * 2.0 - float2(1.0, 1.0)) * 0.5 + float2(0.5, 0.5);
					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					float4 texCol = tex2D(_MainTex, i.uv);
					return float4(texCol.xyz * i.clr.xyz, texCol.w);
				}
				ENDCG
			}
		}
	}
}