Shader "Custom/KawaseLightStreak" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Gain ("Gain", Float) = 0.1
		_Dir ("Streak Dir", Vector) = (1, 0, 0, 0)
		_Offset ("Pixel OFfset", Float) = 1
		_Atten ("Attenuation", Float) = 0.95
	}
	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }
		
		CGINCLUDE
		sampler2D _MainTex;
		float4 _MainTex_TexelSize;
		float _Gain;
		float4 _Dir;
		float _Offset;
		float _Atten;

		struct Input {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};
		struct Inter {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};
		
		Inter vert(Input IN) {
			Inter OUT;
			OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
			#if UNITY_UV_STARTS_AT_TOP && defined(FLIP_UV_Y_ON)
			if (_MainTex_TexelSize.y < 0)
				IN.uv.y = 1 - IN.uv.y;
			#endif
			OUT.uv = IN.uv;
			return OUT;
		}
		ENDCG
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			float4 frag(Inter IN) : COLOR {
				float4 c = tex2D(_MainTex, IN.uv);
				return float4(c.rgb * c.a * _Gain, c.a);
			}
			ENDCG
		}
		
		Pass {
			CGPROGRAM
			#define FLIP_UV_Y_ON
			#pragma vertex vert
			#pragma fragment frag

			float4 frag(Inter IN) : COLOR {
				float2 dx = _MainTex_TexelSize.xy;
				float atten2 = _Atten * _Atten;
				float atten3 = atten2 * _Atten;
				return tex2D(_MainTex, IN.uv) 
					+ _Atten * tex2D(_MainTex, IN.uv + _Offset * _Dir.xy * dx)
					+ atten2 * tex2D(_MainTex, IN.uv + 2 * _Offset * _Dir.xy * dx)
					+ atten3 * tex2D(_MainTex, IN.uv + 3 * _Offset * _Dir.xy * dx);
			}
			ENDCG
		}
		
		Pass {
			Blend One One

			CGPROGRAM
			#define FLIP_UV_Y_ON
			#pragma vertex vert
			#pragma fragment frag
			
			float4 frag(Inter IN) : COLOR {
				return tex2D(_MainTex, IN.uv);
			}			
			ENDCG
		}

		
		Pass {
			CGPROGRAM
			#define FLIP_UV_Y_ON
			#pragma vertex vert
			#pragma fragment frag
			
			float4 frag(Inter IN) : COLOR {
				return tex2D(_MainTex, IN.uv);
			}
			ENDCG
		}
	} 
}
