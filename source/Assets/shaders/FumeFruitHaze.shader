Shader "FumeFruitHaze"
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
				sampler2D _LevelTex;
				sampler2D _NoiseTex;
				sampler2D _NoiseTex2;
				sampler2D _CloudsTex;
				sampler2D _PalTex;
				#if defined(SHADER_API_PSSL)
				sampler2D _GrabTexture;
				#else
				sampler2D _GrabTexture : register(s0);
				#endif
				uniform float _RAIN;
				uniform float4 _spriteRect;
				uniform float2 _screenSize;
				uniform float _waterLevel;
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
					float2 textCoord = (floor(i.scrPos * _screenSize) / _screenSize -_spriteRect.xy) / (_spriteRect.zw - _spriteRect.xy);
					float dist = clamp(1.0 - distance(i.uv, float2(0.5, 0.5)) * 2.0, 0.0, 1.0);
					float h = lerp(sin((tex2D(_NoiseTex, float2(textCoord.x * 1.5, textCoord.y - _RAIN * 0.1)).x + _RAIN * 0.2) * 15.7), 1.0, pow(dist, 0.35)) - tex2D(_NoiseTex2, float2(textCoord.x * 15.2, -0.2 * _RAIN + textCoord.y * 7.6)).x * (1.7 - i.clr.w);
					h += pow(tex2D(_CloudsTex, -0.06 * h * (float2(0.5, 0.66) - textCoord) + float2(textCoord.x * 3.0, textCoord.y * 3.0 -_RAIN * 0.15)).x * 1.5, 2.0);
					h *= dist * i.clr.w;
					float4 levCol = tex2D(_LevelTex, textCoord);
					float dp = ((uint)(levCol.x * 254.0) % 30) / 30.0;
					if (all(levCol.xyz == float3(1.0, 1.0, 1.0)))
						dp = 0.72; // not 1 -> errors
					// 6 / 30 = 0.2
					if (dp > 0.2 && any(tex2D(_GrabTexture, i.scrPos.xy).xyz > float3(1.0 / 255.0, 0.0, 0.0)))
						dp = 0.2;
					h *= clamp(((_waterLevel + 0.16) - (1.0 - i.scrPos.y)) * 10.0, 0.0, 1.0);
					if (h * (0.2 + 0.8 * pow(dp, 0.6)) < 0.2)
						return float4(0.0, 0.0, 0.0, 0.0);
					h = 0.2 + pow(h, 0.5) - dp * 0.3;
					float3 skyCol = tex2D(_PalTex, float2(0.15625, 0.9375));
					float3 centerCol = lerp(skyCol, float3(0.1, 0.1, 0.1), 0.5 + 0.5 * i.clr.x);
					if (h < 0.5)
						return float4(lerp(skyCol, centerCol, round(h * 6.0) / 3.0), 1);
					return float4(lerp(centerCol, float3(0.15, 0.15, 0.15), round(pow((h - 0.5) * i.clr.w, 0.5) * 6.0) / 4.0), 1.0);
				}
				ENDCG
			}
		}
	}
}