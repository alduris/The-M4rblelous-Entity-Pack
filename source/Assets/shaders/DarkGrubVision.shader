Shader "DarkGrubVision"
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
				sampler2D _NoiseTex2;
				sampler2D _PalTex;
				#if defined(SHADER_API_PSSL)
				sampler2D _GrabTexture;
				#else
				sampler2D _GrabTexture : register(s0);
				#endif
				uniform float _RAIN;
				uniform float4 _spriteRect;
				uniform float2 _screenSize;
				uniform float _waterDepth;
				uniform float _hologramThreshold;
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
					o.pos = UnityObjectToClipPos (v.vertex);
					o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
					o.scrPos = ComputeScreenPos(o.pos);
					o.clr = v.color;
					return o;
				}

				#if defined(SHADER_API_PSSL)
				float DepthAtTextCoord(float2 txc, float2 scrPos)
				#else
				float DepthAtTextCoord(float2 txc, float2 scrPos) : FLOAT
				#endif
				{
					float4 texcol = tex2D(_LevelTex, txc);
					float d = fmod(round(texcol.x * 255.0) - 1.0, 30.0) / 30.0;
					if (all(texcol.xyz == float3(1.0, 1.0, 1.0)))
						d = 1.0;
					if (d > 6.0 / 30.0)
					{
						float4 grabColor = tex2D(_GrabTexture, scrPos.xy);
						if (any(grabColor > float3(1.0 / 255.0, 0.0, 0.0)))
							return 6.0 / 30.0;
					}
					return d;
				}

				float4 frag(v2f i) : SV_Target
				{
					float2 textCoord = (floor(i.scrPos * _screenSize) / _screenSize - _spriteRect.xy) / (_spriteRect.zw - _spriteRect.xy);
					float light = 0.0;
					float centerDist = clamp(distance(float2(0.5, 0.5), i.uv) * 2.0 + (1.0 - i.clr.w), 0.0, 1.0);
					float dpth = DepthAtTextCoord(textCoord, i.scrPos);
					if (dpth >= 0.999)
						return float4(0.0, 0.0, 0.0, 0.0);
					i.uv += ((textCoord - float2(0.5, 0.66)) * 0.4 + (i.uv - float2(0.5, 0.5)) * 0.4) * dpth;
					dpth = DepthAtTextCoord(textCoord, i.scrPos);
					if ((dpth > 3.0 / 30.0 && dpth < 7.0 / 30.0) || (dpth > 14.0 / 30.0 && dpth < 17.0 / 30.0) || (dpth > 22.0 / 30.0 && dpth < 24.0 / 30.0))
					{
						if (DepthAtTextCoord(textCoord + float2(-1.0 / 1400.0, 0.0), i.scrPos + float2(-1.0 / _screenSize.x, 0.0)) > dpth
						 || DepthAtTextCoord(textCoord + float2(1.0 / 1400.0, 0.0), i.scrPos + float2(1.0 / _screenSize.x, 0.0)) > dpth
						 || DepthAtTextCoord(textCoord + float2(0.0, 1.0 / 800.0), i.scrPos + float2(0.0, 1.0 / _screenSize.y)) > dpth
						 || DepthAtTextCoord(textCoord + float2(0.0, -1.0 / 800.0), i.scrPos + float2(0.0, -1.0 / _screenSize.y)) > dpth)
						 light = dpth < 7.0 / 30.0 ? 1.0 : 0.35;
						 centerDist = pow(centerDist, 2.0);
					}
					float h = tex2D(_NoiseTex2, float2(textCoord.x * 4.0, textCoord.y * 8.0 - _RAIN * 10.0)).x * 2.0 - pow(i.clr.w, 2.0) * (1.0 - pow(centerDist, 2.0));
					if (fmod(round((textCoord.y - _RAIN * 0.15) * 400.0), 3) == 0.0)
						h += lerp(0.6, 0.15, light);
					if (h > _hologramThreshold * i.clr.w)
						return float4(0.0, 0.0, 0.0, 0.0);
					light *= 1.0 - centerDist;
					return float4(i.clr.xyz, light * 0.6);
				}
				ENDCG
			}
		} 
	}
}