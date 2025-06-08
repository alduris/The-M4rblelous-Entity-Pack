Shader "LBRottenTentaclePlant"
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
				#include "_RippleClip.cginc"
				sampler2D _MainTex;
				sampler2D _NoiseTex;
				sampler2D _PalTex;
				float4 _MainTex_ST;
				uniform float4 _LBCustom_RottenTentacleBlack;
				
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
				
				float circle(float2 uv, float2 c, float2 rad, float ef)
				{
					float eyeRad = 0.5 * ef;
					float2 ps = uv - c;
					float sdist = (ps.x * ps.x) / (rad.x * rad.x) + (ps.y * ps.y) / (rad.y * rad.y);
					if (sdist < 1.0)
					{
						if (sdist < eyeRad)
							return ef;
						return eyeRad;
					}
					return 0.0;
				}

				float random(float2 uv)
				{
					return frac(sin(dot(uv, float2(34.9002, 25.0938))) * 10938.8593987);
				}

				float4 frag(v2f i) : SV_Target
				{
					rippleClip(i.scrPos);
					i.uv *= float2(2.0, 5.0);
					float2 flr = floor(i.uv);
					float rnd = random(flr);
					float4 fragCol;
					if (rnd > 0.1)
					{
						float2 rad = float2(0.25, 0.05);
						fragCol = float4(lerp(_LBCustom_RottenTentacleBlack.xyz, i.clr.xyz, circle(i.uv * float2(0.5, 0.2), flr * float2(0.5, 0.2) + rad, rad * (0.5 + 0.5 * random(ceil(i.uv))), rnd)), 1.0);
					}
					else
						fragCol = float4(_LBCustom_RottenTentacleBlack.xyz, 1.0);
					i.uv *= float2(0.5, 0.2);
					float lim = 0.25 * i.clr.w;
					if (all(fragCol.xyz == _LBCustom_RottenTentacleBlack.xyz) && (i.uv.x < lim || i.uv.x > 1.0 - lim))
						return float4(0.0, 0.0, 0.0, 0.0);
					return fragCol;
				}
				ENDCG
			}
		} 
	}
}