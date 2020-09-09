// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//RealtimeReflections for Daggerfall-Unity
//http://www.reddit.com/r/dftfu
//http://www.dfworkshop.net/
//Author: Michael Rauter (a.k.a. Nystul)
//License: MIT License (http://www.opensource.org/licenses/mit-license.php)

// used as replacement shader to create texture uv sampling positions for every fragment
Shader "Daggerfall/RealtimeReflections/CreateLookupReflectionTextureCoordinates" {
    Properties
    {
            _MainTex ("Base (RGB)", 2D) = "white" {}
    }
		
	CGINCLUDE

	#include "UnityCG.cginc"               

	uniform sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;

	uniform float _GroundLevelHeight;
	uniform float _LowerLevelHeight;     

    struct v2f
    {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
			float4 parallaxCorrectedScreenPos : TEXCOORD1;
            float2 uv2 : TEXCOORD2;		
    };

    v2f vert( appdata_full v )
    {
            v2f o;
			UNITY_INITIALIZE_OUTPUT(v2f, o);

            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord.xy;
            o.uv2 = v.texcoord.xy;
            #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0)
                        o.uv2.y = 1-o.uv2.y;
            #endif				
					
			float4 posWorldSpace = mul(unity_ObjectToWorld, v.vertex);
			o.parallaxCorrectedScreenPos = ComputeScreenPos(mul(UNITY_MATRIX_VP, posWorldSpace));
			if ((abs(posWorldSpace.y - _GroundLevelHeight) > 0.01f) && (abs(posWorldSpace.y - _LowerLevelHeight) > 0.01f))
			{
				// parallax-correct reflection position
				o.parallaxCorrectedScreenPos = ComputeScreenPos(mul(UNITY_MATRIX_VP, posWorldSpace - float4(0.0f, (posWorldSpace.y - _GroundLevelHeight) * 1.0f, 0.0f, 0.0f)));
			}			
						
            return o;
    }
				
    float4 frag(v2f IN) : SV_Target
    {
			float2 parallaxCorrectedScreenPos = IN.parallaxCorrectedScreenPos.xy / IN.parallaxCorrectedScreenPos.w;			
            return float4(parallaxCorrectedScreenPos.x, parallaxCorrectedScreenPos.y, 0.0f, 0.0f);
    }

	ENDCG

	SubShader
	{
		ZTest LEqual Cull Back ZWrite On

		Pass
		{
			CGPROGRAM
			#pragma exclude_renderers gles xbox360 ps3
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			ENDCG
		}
	}	

    Fallback "None"
}
