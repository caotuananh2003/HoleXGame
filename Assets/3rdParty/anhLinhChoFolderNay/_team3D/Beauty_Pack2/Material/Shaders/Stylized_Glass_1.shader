Shader "Shader Graphs/Stylized_Glass_1" {
	Properties {
		_Glass_Main_Color ("Glass Main Color", Vector) = (0.6337219,0.9528302,0.9110626,1)
		_Light_Spec_Cutoff ("Light Spec Cutoff", Range(0, 1)) = 0.5
		_Light_Spec_Cutoff_Smoothness ("Light Spec Cutoff Smoothness", Range(0, 1)) = 0.05
		_View_Spec_Cutoff ("View Spec Cutoff", Range(0, 1)) = 0.5
		_Specular_Color ("Specular Color", Vector) = (0.6941177,1,0.9764706,1)
		_Rim_Power ("Rim Power", Range(1, 20)) = 1
		_Outer_Rim_Smoothness ("Outer Rim Smoothness", Range(0, 3)) = 1
		_Outer_Rim_Color ("Outer Rim Color", Vector) = (0.9882354,1,0.5882353,1)
		_Inner_Rim_Power ("Inner Rim Power", Range(1, 20)) = 1
		_Inner_Rim_Smothness ("Inner Rim Smothness", Float) = 0
		_Inter_Glow_Color ("Inter Glow Color", Vector) = (0.7144657,0.7924528,0.5046279,1)
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