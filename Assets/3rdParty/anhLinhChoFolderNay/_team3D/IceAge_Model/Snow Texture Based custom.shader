Shader "Toony Colors Pro 2/Examples/Material Layers/Snow (Texture Based)" {
	Properties {
		[TCP2HeaderHelp(Base)] _BaseColor ("Color", Vector) = (1,1,1,1)
		[TCP2ColorNoAlpha] _HColor ("Highlight Color", Vector) = (0.75,0.75,0.75,1)
		[TCP2ColorNoAlpha] _SColor ("Shadow Color", Vector) = (0.2,0.2,0.2,1)
		_BaseMap ("Albedo", 2D) = "white" {}
		[TCP2Separator] [TCP2Header(Ramp Shading)] _RampThreshold ("Threshold", Range(0.01, 1)) = 0.5
		_RampSmoothing ("Smoothing", Range(0.001, 1)) = 0.5
		[TCP2Separator] [TCP2HeaderHelp(Specular)] [TCP2ColorNoAlpha] [HDR] _SpecularColor ("Specular Color", Vector) = (0.5,0.5,0.5,1)
		_SpecularRoughnessPBR ("Roughness", Range(0, 1)) = 0.5
		[TCP2Separator] [TCP2HeaderHelp(Emission)] [TCP2ColorNoAlpha] [HDR] _Emission ("Emission Color", Vector) = (0,0,0,1)
		[TCP2Separator] [TCP2HeaderHelp(Rim Lighting)] [TCP2ColorNoAlpha] [HDR] _RimColor ("Rim Color", Vector) = (0.8,0.8,0.8,0.5)
		_RimMin ("Rim Min", Range(0, 2)) = 0.5
		_RimMax ("Rim Max", Range(0, 2)) = 1
		[TCP2Separator] [TCP2HeaderHelp(Normal Mapping)] [NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
		_BumpScale ("Scale", Float) = 1
		[TCP2Separator] [TCP2Vector4Floats(Contrast X,Contrast Y,Contrast Z,Smoothing,1,16,1,16,1,16,0.05,10)] _TriplanarSamplingStrength ("Triplanar Sampling Parameters", Vector) = (8,8,8,0.5)
		[TCP2Separator] [TCP2HeaderHelp(MATERIAL LAYERS)] [TCP2Separator] [TCP2Header(Snow)] _layer_8f0527 ("Source Texture", 2D) = "white" {}
		_contrast_snow ("Contrast", Range(0, 1)) = 0.5
		_NoiseTexture_snow ("Noise Texture", 2D) = "gray" {}
		_NoiseStrength_snow ("Noise Strength", Range(0, 1)) = 0.1
		_BumpMap_snow ("Normal Map", 2D) = "bump" {}
		_BumpScale_snow ("Scale", Float) = 1
		_Albedo_snow ("Albedo", Vector) = (1,1,1,1)
		_RampSmoothing_snow ("Smoothing", Range(0.001, 1)) = 0.5
		[TCP2ColorNoAlpha] _SColor_snow ("Shadow Color", Vector) = (0.2,0.2,0.2,1)
		[ToggleOff(_RECEIVE_SHADOWS_OFF)] _ReceiveShadowsOff ("Receive Shadows", Float) = 1
		[HideInInspector] __dummy__ ("unused", Float) = 0
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
	Fallback "Hidden/InternalErrorShader"
	//CustomEditor "ToonyColorsPro.ShaderGenerator.MaterialInspector_SG2"
}