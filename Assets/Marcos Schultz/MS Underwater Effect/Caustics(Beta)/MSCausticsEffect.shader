Shader "UnderWaterEffect/MSCausticsEffect" {
	Properties {
		[HideInInspector]_Color ("Color", Color) = (1,1,1,0)
		[HideInInspector]_Multiply ("Intensity", Range (1, 5)) = 1.5
		[HideInInspector]_Diffraction ("Diffraction", Range (0.0, 1.0)) = 0.1

		_MainTex ("Caustic Texture", 2D) = "black" { }
		_NoiseTex ("Noise Texture", 2D) = "black" { }

		[HideInInspector]_Height ("Water Height", Float) = 1.0
		[HideInInspector]_EdgeBlend ("Edge Blend", Range (0.1, 100)) = 1.0
		[HideInInspector]_DepthBlend ("Depth Blend", Float) = 10.0
		[HideInInspector]_DepthFade ("Depth Fade", Float) = 10.0
	 }
	 
	 Subshader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent+100" }
		Pass {
			ZWrite Off
			Offset -1, -1
			Blend DstColor One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
		 
			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uvNoise : TEXCOORD1;
				float3 wPos : TEXCOORD2; 
				UNITY_FOG_COORDS(3)
			};

			sampler2D _MainTex;
			sampler2D _NoiseTex;
			float4 _MainTex_ST;
			float4 _NoiseTex_ST;
			float4 _Color;
			float _Diffraction;
			float4x4 unity_Projector;
			float _Distortion;
			float _Height;
			float _DepthBlend;
			float _DepthFade;
			float _EdgeBlend;
			float _Multiply;

			v2f vert (appdata_tan v) {
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.uv = TRANSFORM_TEX (mul (unity_Projector, v.vertex).xy, _MainTex);
				o.uvNoise = TRANSFORM_TEX (mul (unity_Projector, v.vertex).xy, _NoiseTex);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			fixed4 frag (v2f i) : COLOR {
				float dist = distance(_WorldSpaceCameraPos, i.wPos);
				fixed4 noise = tex2D(_NoiseTex, i.uvNoise-frac(_SinTime.y*0.01));
				noise = noise + (_Time.y*0.05);
				float distLOD = 2; //(dist/100);
				float height = i.wPos.y-_Height;
				float heightBlend = height/-_DepthBlend;
				fixed cAr = tex2Dlod (_MainTex, float4(i.uv + frac(fixed2(_Diffraction, _Diffraction))+noise,0,heightBlend*100+distLOD));
				fixed cAg = tex2Dlod (_MainTex, float4(i.uv + frac(fixed2(_Diffraction, -_Diffraction))+noise,0,heightBlend*100+distLOD));
				fixed cAb = tex2Dlod (_MainTex, float4(i.uv + frac(fixed2(-_Diffraction, -_Diffraction))+noise,0,heightBlend*100+distLOD));
				fixed4 cA = fixed4(cAr, cAg, cAb,1);
				fixed cBr = tex2Dlod (_MainTex, float4(i.uv - frac(fixed2(_Diffraction, _Diffraction)+noise),0,heightBlend*100+distLOD));
				fixed cBg = tex2Dlod (_MainTex, float4(i.uv - frac(fixed2(_Diffraction, -_Diffraction)+noise),0,heightBlend*100+distLOD));
				fixed cBb = tex2Dlod (_MainTex, float4(i.uv - frac(fixed2(-_Diffraction, -_Diffraction)+noise),0,heightBlend*100+distLOD));
				fixed4 cB = fixed4(cBr, cBg, cBb, 1);
				cA = lerp(cA,cB,0.49);
				
				if (i.wPos.y<=_Height) 
					cA = clamp(lerp(cA,fixed4(0,0,0,0),heightBlend*_DepthFade),0,1);
				else 
					cA = lerp(cA,fixed4(0,0,0,0),height/_EdgeBlend);

				float distDiff = (dist-1000)/200;
				if (dist>1000)
					cA = lerp(saturate(cA-distDiff),0,distDiff);
				cA = saturate(cA * _Color * _Multiply);
				UNITY_APPLY_FOG_COLOR(i.fogCoord, cA, fixed4(0,0,0,0));				
				return cA;
			}
			ENDCG
		}
	}
}