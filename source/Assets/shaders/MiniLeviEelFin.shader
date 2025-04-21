Shader "MiniLeviEelFin"
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
				sampler2D _NoiseTex;
				uniform float4 _MiniLeviColorA;
				uniform float4 _MiniLeviColorB;
				uniform float4 _MiniLeviColorHead;
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
					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					float n = min(1.0, tex2D(_NoiseTex, float2(i.clr.y, i.uv.y * lerp(1.0, 3.0, i.clr.x) * 3.0)) + 0.1) - pow(i.uv.x, 1.5);
					if (n <= 0.0)
						return float4(0.0, 0.0, 0.0, 0.0);
					return lerp(lerp(_MiniLeviColorHead, _MiniLeviColorA, i.clr.z), _MiniLeviColorB, pow(i.uv.y, 2.0));
				}
				ENDCG
			}
		} 
	}
}