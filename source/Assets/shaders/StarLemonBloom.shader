Shader "StarLemonBloom"
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
		Fog { Color(0, 0, 0, 0) }
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
			GrabPass {}
		
			Pass 
			{
				CGPROGRAM
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				sampler2D _MainTex;
				sampler2D _LevelTex;
				sampler2D _NoiseTex;
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
					float4  pos : SV_POSITION;
				    float2  uv : TEXCOORD0;
					float2 scrPos : TEXCOORD1;
					float4 clr : COLOR;
				};

				v2f vert (appdata_full v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.scrPos = ComputeScreenPos(o.pos);
					o.clr = v.color;
					return o;
				}

				half4 frag (v2f i) : SV_Target
				{
					if (tex2D(_MainTex, i.uv).w == 0.0)
						return half4(0.0, 0.0, 0.0, 0.0);
					float2 textCoord = i.scrPos;
					textCoord.x -= _spriteRect.x;
					textCoord.y -= _spriteRect.y;
					textCoord.x /= _spriteRect.z - _spriteRect.x;
					textCoord.y /= _spriteRect.w - _spriteRect.y;
					half2 screenPos = half2(i.scrPos.x, i.scrPos.y);
					half4 getCol = half4(0.0, 0.0, 0.0, 1.0);
					half4 texcol = half4(0.0, 0.0, 0.0, 1.0);
					half4 goalColor = half4(1.0, 0.0, 0.0, 1.0);
					float div = 0.0;
					float coef = 1.0;
					float fI = 0.0;
					float blurAmount = 0.0024;
					float horFac = _screenSize.y / _screenSize.x;
				    half add = 0;
					for (int j = 0; j < 4; j++)
					{
						fI++;
						coef *= 0.92;
						texcol = tex2D(_GrabTexture, float2(screenPos.x, screenPos.y + fI * blurAmount)) * coef;
						getCol += texcol;
						texcol = tex2D(_GrabTexture, float2(screenPos.x - fI * blurAmount * horFac, screenPos.y)) * coef;
						getCol += texcol;
						texcol = tex2D(_GrabTexture, float2(screenPos.x + fI * blurAmount * horFac, screenPos.y)) * coef;
						getCol += texcol;
						texcol = tex2D(_GrabTexture, float2(screenPos.x, screenPos.y - fI * blurAmount)) * coef;
						getCol += texcol;
						div += 4.0 * coef;
					}
					 getCol /= div;
					 getCol *= i.clr.w * lerp(1, 0.5, distance(i.uv, half2(0.5, 0.5)) * 2.0);
					 half4 grabCol = tex2D(_GrabTexture, float2(screenPos.x, screenPos.y));
					 grabCol.x = max(grabCol.x, getCol.x);
					 grabCol.y = max(grabCol.y, getCol.y);
					 grabCol.z = max(grabCol.z, getCol.z);
					 getCol.x = pow(getCol.x, 1.5);
					 getCol.y = pow(getCol.y, 1.5);
					 getCol.z = pow(getCol.z, 1.5);
					 return grabCol + getCol;
				}
				ENDCG
			}
		} 
	}
}