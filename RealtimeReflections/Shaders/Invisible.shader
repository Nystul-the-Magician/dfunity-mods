Shader "Daggerfall/RealtimeReflections/Invisible" {
	SubShader
	{
		//Tags {"Queue" = "Geometry-1" }
		Lighting Off
		Pass
		{
			ZWrite Off
			ColorMask 0
		}
	}
}

