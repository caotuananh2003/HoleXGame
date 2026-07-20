Shader "OccaSoftware/Outline Objects" {
	Properties {
		[HDR] _OutlineColor ("Outline Color", Vector) = (0,0,0,1)
		_OutlineThickness ("Outline Thickness", Float) = 0.1
		_CompleteFalloffDistance ("Complete Falloff Distance", Float) = 30
		_NoiseTexture ("Noise Texture", 2D) = "white" {}
		_NoiseFrequency ("Noise Frequency", Float) = 5
		_NoiseFramerate ("Noise Framerate", Float) = 12
		[Toggle(_USE_VERTEX_COLOR_ENABLED)] _USE_VERTEX_COLOR_ENABLED ("Use Vertex Color (R) for Outline Thickness?", Float) = 0
		[Toggle(_ATTENUATE_BY_DISTANCE_ENABLED)] _ATTENUATE_BY_DISTANCE_ENABLED ("Attenuate Outline Thickness by Camera Distance?", Float) = 0
		[Toggle(_RANDOM_OFFSETS_ENABLED)] _RANDOM_OFFSETS_ENABLED ("Randomly offset the sample position", Float) = 0
		[Toggle(_USE_SMOOTHED_NORMALS_ENABLED)] _USE_SMOOTHED_NORMALS_ENABLED ("Use Smoothed Normals (UV3)", Float) = 0
		_Surface ("__surface", Float) = 0
		_QueueOffset ("Queue offset", Float) = 0
		[HideInInspector] _ZWrite ("__zw", Float) = 1
		[HideInInspector] _SrcBlend ("__src", Float) = 1
		[HideInInspector] _DstBlend ("__dst", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
	//CustomEditor "OccaSoftware.OutlineObjects.Editor.OutlineObjectsShaderGUI"
}