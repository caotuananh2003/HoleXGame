Shader "Shader Graphs/PAR SG _ New Shield 1" {
	Properties {
		[NoScaleOffset] _Main_Texture ("Main_Texture", 2D) = "white" {}
		_Offset_Intensity ("Offset_Intensity", Range(0, 1)) = 0
		_Limit_Vertex_Scale ("Limit_Vertex_Scale", Range(0, 1)) = 0.3
		_V_Offset_Speed ("V_Offset_Speed", Vector) = (0,0,0,0)
		_Sweep_Color_Speed_Idle ("Sweep_Color_Speed_Idle", Vector) = (0,0.15,0,0)
		_Sweep_Color_Speed_Anim ("Sweep_Color_Speed_Anim", Vector) = (0,0.72,0,0)
		_Outline_Intensity ("Outline_Intensity", Range(0, 1)) = 0
		[HDR] _Polygon_Color ("Polygon_Color", Vector) = (0,0,0,0)
		[HDR] _Emission_Color ("Emission Color", Vector) = (0.5660378,0.5660378,0.5660378,1)
		[HDR] _Sweep_Color_Idle ("Sweep_Color_Idle", Vector) = (1,1,1,1)
		_Sweep_Color_Anim ("Sweep_Color_Anim", Vector) = (1,1,1,1)
		_Freshnel_Intensity ("Freshnel_Intensity", Float) = 1
		_Cutoff ("Cutoff", Range(0, 1)) = 1
		[NoScaleOffset] _SampleTexture2D_2b95e637457941e5a6b823bbaf54b395_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
		[NoScaleOffset] _SampleTexture2D_93b9de676f7447cc8667efd4b77042fd_Texture_1_Texture2D ("Texture2D", 2D) = "white" {}
		[HideInInspector] _CastShadows ("_CastShadows", Float) = 1
		[HideInInspector] _Surface ("_Surface", Float) = 1
		[HideInInspector] _Blend ("_Blend", Float) = 2
		[HideInInspector] _AlphaClip ("_AlphaClip", Float) = 0
		[HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1
		[HideInInspector] _DstBlend ("_DstBlend", Float) = 0
		[HideInInspector] _SrcBlendAlpha ("_SrcBlendAlpha", Float) = 1
		[HideInInspector] _DstBlendAlpha ("_DstBlendAlpha", Float) = 0
		[ToggleUI] [HideInInspector] _ZWrite ("_ZWrite", Float) = 0
		[HideInInspector] _ZWriteControl ("_ZWriteControl", Float) = 0
		[HideInInspector] _ZTest ("_ZTest", Float) = 4
		[HideInInspector] _Cull ("_Cull", Float) = 2
		[HideInInspector] _AlphaToMask ("_AlphaToMask", Float) = 0
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] _XRMotionVectorsPass ("_XRMotionVectorsPass", Float) = 1
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