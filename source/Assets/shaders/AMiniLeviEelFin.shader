Shader "AMiniLeviEelFin"
{
	Properties
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	Category
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
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
				sampler2D _MainTex;
				sampler2D _NoiseTex;
				uniform half4 _AMiniLeviColorA;
				uniform half4 _AMiniLeviColorB;
				uniform half4 _AMiniLeviColorHead;
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

				half4 frag(v2f i) : SV_Target
				{
					half n = tex2D(_NoiseTex, half2(i.clr.y, i.uv.y * lerp(1.0, 3.0, i.clr.x) * 3.0));
					n = min(1.0, n + 0.1);
					n -= pow(i.uv.x, 1.5);
					if (n <= 0.0)
						return half4(0.0, 0.0, 0.0, 0.0);
					return lerp(lerp(_AMiniLeviColorHead, _AMiniLeviColorA, i.clr.z), _AMiniLeviColorB, pow(i.uv.y, 2.0));
				}
				ENDCG
			}
		} 
	}
}