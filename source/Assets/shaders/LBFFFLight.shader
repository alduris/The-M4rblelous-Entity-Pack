Shader "LBFFFLight"
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
				#pragma target 4.0
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				//#include "_ShaderFix.cginc" -> unused code
				#include "_RippleClip.cginc"
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
					return o;
				}
				
				float random(float2 uv)
				{
					return frac(sin(dot(uv, float2(34.9002, 25.0938))) * 10938.8593987);
				}

				float4 frag(v2f i) : SV_Target
				{
					rippleClip(i.scrPos);
					float dst = distance(i.uv, float2(0.5, 0.5));
					if (dst < 0.075)
						return float4(lerp(i.clr.xyz, float3(1.0, 1.0, 1.0), min(1.0, i.clr.w * 8.0)), i.clr.w * 0.4 + 0.6);
					return float4(i.clr.xyz * (0.75 + 0.25 * random(i.uv)), lerp(i.clr.w, 0.0, dst * 2.0));
				}
				ENDCG
			}
		} 
	}
}