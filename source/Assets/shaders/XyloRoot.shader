Shader "XyloRoot"
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
		Fog { Color(0, 0.0, 0.0, 0.0) }
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
				sampler2D _PalTex;
				uniform float _waterPosition;
				#if defined(SHADER_API_PSSL)
				sampler2D _GrabTexture;
				#else
				sampler2D _GrabTexture : register(s0);
				#endif
				uniform float _RAIN;
				uniform float4 _spriteRect;
				uniform float2 _screenSize;
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
					float2 textCoord = (floor(i.scrPos * _screenSize) / _screenSize - _spriteRect.xy) / (_spriteRect.zw - _spriteRect.xy);
					float4 levCol = tex2D(_LevelTex, textCoord);
					float terrainDpth = ((uint)round(levCol.x * 255.0) % 30) / 30.0;
					if (all(levCol == float3(1.0, 1.0, 1.0)))
						return float4(0.0, 0.0, 0.0, 0.0);
					if (terrainDpth < 42.0 && terrainDpth > 6.0 / 30.0)
					{
						float4 grabTexCol = tex2D(_GrabTexture, i.scrPos);
						if (any(grabTexCol.xyz > float3(1.0 / 255.0, 0.0, 0.0)))
							return float4(0.0, 0.0, 0.0, 0.0);
					}
					terrainDpth = max(terrainDpth - 0.18, 0.0) / 0.82;
					float4 textCol = tex2D(_MainTex, i.uv + normalize(i.uv - float2(0.5, 0.5)) * terrainDpth * 0.12);
					float4 setCol = lerp(tex2D(_PalTex, float2(2.5 / 32.0, 7.5 / 8.0)), float4(0.5, 0.5, 0.7, 1.0) * tex2D(_PalTex, float2(30.5 / 32.0, 7.5 / 8.0)).x, pow(terrainDpth, 0.45) * 0.25);
					float alpha = 1.0 - textCol.x;
					alpha *= 0.4 + 0.6 * tex2D(_NoiseTex, float2(textCoord.x * 8.0, textCoord.y * 4.0)).x;
					if (alpha > 0.5)
						alpha = pow(alpha, 0.7);
					alpha = max(0.0, alpha - terrainDpth * 0.2);
					if (alpha < 0.1)
						return float4(0.0, 0.0, 0.0, 0.0);
					if (alpha < 0.3)
						return float4(setCol.xyz, 0.2 * i.clr.w);
					if (alpha > 0.6)
						return float4(setCol.xyz, 1.0 * i.clr.w);
					return float4(setCol.xyz, 0.5 * i.clr.w);
				}
				ENDCG
			}
		} 
	}
}