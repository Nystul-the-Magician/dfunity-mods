//RealtimeReflections for Daggerfall-Unity
//http://www.reddit.com/r/dftfu
//http://www.dfworkshop.net/
//Author: Michael Rauter (a.k.a. Nystul)
//License: MIT License (http://www.opensource.org/licenses/mit-license.php)

Shader "Daggerfall/RealtimeReflections/TilemapTextureArrayWithReflections" {
	Properties {
		// These params are required to stop terrain system throwing errors
		// However we won't be using them as Unity likes to init these textures
		// and will overwrite any assignments we already made
		// TODO: Combine splat painting with tilemapping
		[HideInInspector] _MainTex("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
		[HideInInspector] _SplatTex3("Layer 3 (A)", 2D) = "white" {}
		[HideInInspector] _SplatTex2("Layer 2 (B)", 2D) = "white" {}
		[HideInInspector] _SplatTex1("Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _SplatTex0("Layer 0 (R)", 2D) = "white" {}

		// These params are used for our shader
		_TileTexArr("Tile Texture Array", 2DArray) = "" {}
		_TileNormalMapTexArr("Tileset NormalMap Texture Array (RGBA)", 2DArray) = "" {}
		_TileMetallicGlossMapTexArr ("Tileset MetallicGlossMap Texture Array (RGBA)", 2DArray) = "" {}
		_TilemapTex("Tilemap (R)", 2D) = "red" {}
		_ReflectionGroundTex("Reflection Texture Ground Reflection", 2D) = "black" {}
		_ReflectionSeaTex("Reflection Texture Sea Reflection", 2D) = "black" {}		
		_TilemapDim("Tilemap Dimension (in tiles)", Int) = 128
		_MaxIndex("Max Tileset Index", Int) = 255
		_SeaLevelHeight("sea level height", Float) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma target 3.5
		#pragma surface surf Standard
		#pragma glsl

		#pragma multi_compile __ _NORMALMAP

		UNITY_DECLARE_TEX2DARRAY(_TileTexArr);

		#ifdef _NORMALMAP
			UNITY_DECLARE_TEX2DARRAY(_TileNormalMapTexArr);
		#endif

		UNITY_DECLARE_TEX2DARRAY(_TileMetallicGlossMapTexArr);
			
		sampler2D _TilemapTex;
		sampler2D _ReflectionGroundTex;
		sampler2D _ReflectionSeaTex;
		int _TilemapDim;
		int _MaxIndex;
		float _SeaLevelHeight;

		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)
		#define UNITY_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) tex.SampleGrad (sampler##tex,coord,dx,dy)
		#else
		#if defined(UNITY_COMPILER_HLSL2GLSL) || defined(SHADER_TARGET_SURFACE_ANALYSIS)
		#define UNITY_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) texCUBEgrad(tex,coord,dx,dy)
		#endif
		#endif

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldPos;
			float3 worldNormal;
			float4 screenPos;
			INTERNAL_DATA
		};

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			// Get offset to tile in atlas
			int index = tex2D(_TilemapTex, IN.uv_MainTex).x * _MaxIndex;

			// Offset to fragment position inside tile
			float2 uv = fmod(IN.uv_MainTex * _TilemapDim, 1.0f);
	
			// compute all 4 posible configurations of terrain tiles (normal, rotated, flipped, rotated and flipped)
			// normal texture tiles (no operation required) are those with index % 4 == 0
			// rotated texture tiles are those with (index+1) % 4 == 0
			// flipped texture tiles are those with (index+2) % 4 == 0
			// rotated and flipped texture tiles are those with (index+3) % 4 == 0
			// so correct uv coordinates according to index 
			if (((uint)index+1) % 4 == 0)
			{
				uv = float2(1.0f - uv.y, uv.x);
			}
			else if (((uint)index+2) % 4 == 0)
			{
				uv = 1.0f - uv;
			}
			else if (((uint)index+3) % 4 == 0)
			{
				uv = float2(uv.y, 1.0f - uv.x);
			}

			// Sample based on gradient and set output
			float3 uv3 = float3(uv, ((uint)index)/4); // compute correct texture array index from index
			
			//half4 c = UNITY_SAMPLE_TEX2DARRAY_GRAD(_TileTexArr, uv3, ddx(uv3), ddy(uv3)); // (see https://forum.unity3d.com/threads/texture2d-array-mipmap-troubles.416799/)

			// since there is currently no UNITY_SAMPLE_TEX2DARRAY_GRAD function in unity, this is used as workaround
			// mip map level is selected manually dependent on fragment's distance from camera
			
			float dist = distance(IN.worldPos.xyz, _WorldSpaceCameraPos.xyz);
			
			half4 c;
			if (dist < 10.0f)
				c = UNITY_SAMPLE_TEX2DARRAY_LOD(_TileTexArr, uv3, 0);
			else if (dist < 25.0f)
				c = UNITY_SAMPLE_TEX2DARRAY_LOD(_TileTexArr, uv3, 1);
			else if (dist < 150.0f)
				c = UNITY_SAMPLE_TEX2DARRAY_LOD(_TileTexArr, uv3, 2);
			else if (dist < 500.0f)
				c = UNITY_SAMPLE_TEX2DARRAY_LOD(_TileTexArr, uv3, 3);
			else if (dist < 750.0f)
				c = UNITY_SAMPLE_TEX2DARRAY_LOD(_TileTexArr, uv3, 4);
			else if (dist < 2500.0f)
				c = UNITY_SAMPLE_TEX2DARRAY_LOD(_TileTexArr, uv3, 5);
			else if (dist < 5000.0f)
				c = UNITY_SAMPLE_TEX2DARRAY_LOD(_TileTexArr, uv3, 6);
			else if (dist < 10000.0f)
				c = UNITY_SAMPLE_TEX2DARRAY_LOD(_TileTexArr, uv3, 7);
			else
				c = UNITY_SAMPLE_TEX2DARRAY_LOD(_TileTexArr, uv3, 8);

			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			
			half4 metallicGloss = UNITY_SAMPLE_TEX2DARRAY_GRAD(_TileMetallicGlossMapTexArr, uv3, ddx(uv3), ddy(uv3));
			float roughness = (1.0 - metallicGloss.a) * 4.0f;
			half3 refl;
			if (IN.worldPos.y > _SeaLevelHeight + 0.01f)
			{
				refl = tex2Dlod(_ReflectionGroundTex, float4(screenUV, 0.0f, roughness)).rgb; // 4th component is blurring of reflection
			}
			else
			{				
				refl = tex2Dlod(_ReflectionSeaTex, float4(screenUV, 0.0f, roughness)).rgb;				
			}

			#ifdef _NORMALMAP
				o.Normal = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY_GRAD(_TileNormalMapTexArr, uv3, ddx(uv3), ddy(uv3)));
			#endif			
			//float3 worldNormal = normalize(WorldNormalVector(IN, o.Normal));

			//float reflAmount;
			//const float3 upVec = float3(0.0f, 1.0f, 0.0f);
			//if (dot(worldNormal, upVec) > 0.99f)
			//{
			//	reflAmount = metallicGloss.r;
			//}
			//else
			//{
			//	reflAmount = 0.0f;
			//}

			float reflAmount = metallicGloss.r;

			c.rgb = c.rgb * (1.0f - reflAmount) + reflAmount * refl.rgb;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Metallic = 0.0f;
		}
		ENDCG
	} 
	FallBack "Standard"
}
