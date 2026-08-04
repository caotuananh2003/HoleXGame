Shader "Shader Graphs/StylizedWaterInteractive" {
	Properties {
		Color_27D4D743 ("Main Tint", Vector) = (0.2735849,0.8226658,1,0)
		[NoScaleOffset] Texture2D_37AAFE25 ("Noise Texture", 2D) = "white" {}
		[NoScaleOffset] Texture2D_B66AF0E1 ("Distortion Texture", 2D) = "white" {}
		Vector1_80CAEB23 ("Noise/Distort Scale", Range(0.01, 20)) = 0.3
		Vector1_8E5091B8 ("NoiseDistortion", Float) = -0.33
		Vector1_3B48FD24 ("Foam Thickness", Float) = -0.27
		Vector1_39D35881 ("Foam Cutoff", Range(0, 1)) = 1
		Vector1_981A3A67 ("Foam Cutoff Softness", Range(0, 1)) = 0.3
		Color_51A4B6EC ("Edge Color", Vector) = (0,0.7135715,1,0)
		Color_C347D0B ("Depth Color", Vector) = (0.4549662,0.6634097,0.6792453,0)
		Vector1_5AE4170F ("Noise Scroll Speed", Float) = 0.15
		Vector1_2E11270D ("Refraction Distortion", Float) = 0.03
		Vector1_ECB3F2B8 ("Ripple Cutoff", Range(0, 1)) = 0.4
		Vector1_C1C7D298 ("Depth Offset Coloring", Range(0, 1)) = 0.767
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
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
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}