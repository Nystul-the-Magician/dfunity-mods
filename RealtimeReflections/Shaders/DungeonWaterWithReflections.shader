Shader "Daggerfall/RealtimeReflections/DungeonWaterWithReflections" {
	Properties {
		_transparency ("Transparency", Range (0.0,1.0)) = 0.6
		_horizonColor ("Horizon color", COLOR)  = ( .172 , .463 , .435 , 0)
		//_fresnelMultiplierReflection ("Fresnel Multiplier Reflection", Range(0.0,1.0)) = 0.02
		//_reflectionTextureIntensity ("Reflection Texture Intensity", Range(0.0,1.0)) = 0.25
		//_blendRatioHorizonColorToReflection ("Blend Ratio Horizon Color To Reflection", Range(0.0,1.0)) = 0.1
		//_blendRatioWaterColorToReflection ("Blend Ratio Water Color To Reflection", Range(0.0,1.0)) = 0.6
		_WaveScale ("Wave scale", Range (0.02,0.15)) = .07
		[NoScaleOffset] _ColorControl ("Reflective color (RGB) fresnel (A) ", 2D) = "" { }
		[NoScaleOffset] _BumpMap ("Waves Normalmap ", 2D) = "" { }
		WaveSpeed("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	uniform float4 _horizonColor;
	uniform float _transparency;
	//uniform float _fresnelMultiplierReflection;
	//uniform float _reflectionTextureIntensity;
	//uniform float _blendRatioHorizonColorToReflection;
	//uniform float _blendRatioWaterColorToReflection;
	static const float _fresnelMultiplierReflection = 0.02f;
	static const float _reflectionTextureIntensity = 0.25f;
	static const float _blendRatioHorizonColorToReflection = 0.1f;
	static const float _blendRatioWaterColorToReflection = 0.6f;

	uniform float4 WaveSpeed;
	uniform float _WaveScale;
	uniform float4 _WaveOffset;

	uniform sampler2D _CameraGBufferTexture0;
	uniform sampler2D _CameraGBufferTexture1;
	uniform sampler2D _CameraGBufferTexture2;
	uniform sampler2D _CameraGBufferTexture3;
	uniform sampler2D _ReflectionTex;
	uniform sampler2D _FinalReflectionTexture;
	uniform sampler2D _CameraReflectionsTexture;
	uniform sampler2D _TempTexture;

	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f {
		float4 pos : SV_POSITION;
		float2 bumpuv[2] : TEXCOORD0;
		float3 viewDir : TEXCOORD2;
		float4 screenPos : TEXCOORD4;
		UNITY_FOG_COORDS(3)
	};

	v2f vert(appdata v)
	{
		v2f o;
		float4 s;

		o.pos = UnityObjectToClipPos(v.vertex);

		// scroll bump waves
		float4 temp;
		float4 wpos = mul (unity_ObjectToWorld, v.vertex);
		temp.xyzw = wpos.xzxz * _WaveScale + _WaveOffset;
		o.bumpuv[0] = temp.xy * float2(.4, .45);
		o.bumpuv[1] = temp.wz;

		// object space view direction
		o.viewDir.xzy = normalize( WorldSpaceViewDir(v.vertex) );

		o.screenPos = ComputeScreenPos(o.pos);

		UNITY_TRANSFER_FOG(o,o.pos);
		return o;
	}

	ENDCG

	Subshader {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			sampler2D _BumpMap;
			sampler2D _ColorControl;

			half4 frag( v2f i ) : COLOR
			{
				half3 bump1 = UnpackNormal(tex2D(_BumpMap, i.bumpuv[0])).rgb;
				half3 bump2 = UnpackNormal(tex2D(_BumpMap, i.bumpuv[1])).rgb;
				half3 bump = (bump1 + bump2) * 0.5;

				half fresnel = abs(dot(i.viewDir, bump));
				half4 water = tex2D(_ColorControl, float2(fresnel,fresnel));

				half4 col;
				half3 reflCol;
				
				float2 screenPos = (i.screenPos / i.screenPos.w).xy;
				reflCol = lerp(tex2D(_ReflectionTex, screenPos + float2(fresnel, fresnel)*_fresnelMultiplierReflection).rgb*_reflectionTextureIntensity, _horizonColor.rgb, _blendRatioHorizonColorToReflection);

				col.rgb = lerp(water.rgb, reflCol, _blendRatioWaterColorToReflection); // water.a);
				col.a = _transparency;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}

}
