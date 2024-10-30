Shader "MiniLeviEelBody"
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
				uniform half4 _MiniLeviColorA;
				uniform half4 _MiniLeviColorB;
				uniform half4 _MiniLeviColorHead;
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
					half n = tex2D(_NoiseTex, half2(lerp(i.uv.x * lerp(3.0, 1.0, i.uv.y), 0.5, pow(i.uv.y, 0.15)), (i.uv.y + i.clr.w) * 30.0));
					n = 0.5 - sin(n * 3.14 * 6.0) * 0.5;
					n = pow(n, 4.0);
					n -= pow(abs(i.uv.x - 0.5) * 2.0, 5.0);
					n = lerp(n, 1.0 - abs(i.uv.x - 0.5) * 2.0, pow(i.uv.y, 0.75));
					if (i.uv.y > 0.3)
						n -= (i.uv.y - 0.3) * 5.0;
					if (n > 0.7) 
						return half4(i.clr.xyz, 1.0);
					half jagg = 0;
					jagg = tex2D(_NoiseTex, half2(i.uv.y * 20.0 + i.clr.w * (i.uv.x < 0.5 ? -1.0 : 1.0) * 0.2, ((i.uv.y * lerp(3.0, 1.0, pow(i.uv.y, 0.5))) + i.clr.w) * 30.0)) * min(i.uv.y + 0.1, 1.0);
					jagg -= pow(1.0 - abs(i.uv.x - 0.5) * 2.0, 1.0);
					if (jagg > 0.0)
						return half4(0.0, 0.0, 0.0, 0.0);
					return lerp(_MiniLeviColorHead, lerp(_MiniLeviColorA, _MiniLeviColorB, pow(i.uv.y, 2)), clamp((i.uv.y - 0.015) * 60.0, 0.0, 1.0));
				}
				ENDCG
			}
		} 
	}
}