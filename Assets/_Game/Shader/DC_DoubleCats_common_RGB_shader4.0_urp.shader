Shader "DC/DoubleCats_common_RGB_shader4.0_urp" {
	Properties {
		[HideInInspector] _EmissionColor ("Emission Color", Vector) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff ("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin] _blend ("blend", Range(0, 1)) = 1
		[NoScaleOffset] _Base_Tex ("Base_Tex", 2D) = "white" {}
		_Base_uv ("Base_uv", Vector) = (1,1,0,0)
		_Base_speed ("Base_speed", Vector) = (1,0,0,0)
		[Toggle(_USE_CUSTOM1_ZW_BASE_UV_ON)] _use_custom1_zw_base_uv ("use_custom1_zw_base_uv", Float) = 0
		_Base_distort_power ("Base_distort_power", Range(0, 1)) = 0
		_Base_costart ("Base_costart", Range(0, 12)) = 1
		[HDR] _R_light_color ("R_light_color", Vector) = (1,1,1,0)
		[HDR] _R_dark_clolor ("R_dark_clolor", Vector) = (0,0,0,0)
		[Toggle(_R_USE_ONE_ON)] _R_Use_one ("R_Use_one", Float) = 0
		[NoScaleOffset] _Color_Tex ("Color_Tex", 2D) = "white" {}
		_Color_uv ("Color_uv", Vector) = (1,1,0,0)
		_color_speed ("color_speed", Vector) = (1,0,0,0)
		_Color_disort ("Color_disort", Range(0, 1)) = 0
		_color_change_HSV ("color_change_HSV", Vector) = (0,0,0,0)
		[Toggle(_USE_CUSTOM2_X_COLOR_SAT_ON)] _use_custom2_x_Color_sat ("use_custom2_x_Color_sat", Float) = 0
		_All_power ("All_power", Range(0, 50)) = 1
		[HDR] _AllColor ("AllColor", Vector) = (1,1,1,0)
		[Enum(R,0,G,1,B,2,A,3)] _Choose_alpha ("Choose_alpha", Float) = 3
		_Alpha ("Alpha", Range(0, 10)) = 1
		_Base_Alpha_costart ("Base_Alpha_costart", Range(0.7, 3)) = 0
		_Distance ("Distance", Range(0, 2)) = 0
		[NoScaleOffset] _MaskTex ("MaskTex", 2D) = "white" {}
		_Mask_uv ("Mask_uv", Vector) = (1,1,0,0)
		_Mask_speed ("Mask_speed", Vector) = (1,0,0,0)
		_Mask_power ("Mask_power", Range(0, 12)) = 1
		_Mask_costart ("Mask_costart", Range(0, 12)) = 1
		_Mask_distort_power ("Mask_distort_power", Range(0, 1)) = 0
		[Toggle(_USE_CUSTOM2_ZW_MASK_UV_ON)] _use_custom2_zw_mask_uv ("use_custom2_zw_mask_uv", Float) = 0
		[NoScaleOffset] _DissolutionTex ("DissolutionTex", 2D) = "white" {}
		_Dissolve_uv ("Dissolve_uv", Vector) = (0,0,0,0)
		_Dissolve_speed ("Dissolve_speed", Vector) = (0,0,0,0)
		[Toggle(_MASK_INVERT_ON)] _Mask_invert ("Mask_invert", Float) = 0
		_Hardness ("Hardness", Range(0, 111)) = 22
		_Dissolve ("Dissolve", Range(0, 1)) = 1
		_Disslove_distort_power ("Disslove_distort_power", Float) = 0
		[Toggle(_USE_CUSTOM1_X_DISSOLVE_ON)] _use_custom1_x_dissolve ("use_custom1_x_dissolve", Float) = 0
		_side ("side", Range(0, 0.1)) = 0
		[HDR] _Side_Color ("Side_Color", Vector) = (1,1,1,0)
		_Side_Color_power ("Side_Color_power", Range(0, 55)) = 1
		[NoScaleOffset] _FlowTex ("FlowTex", 2D) = "white" {}
		_Distort_uv1 ("Distort_uv1", Vector) = (1,1,0,0)
		_Distort_speed1 ("Distort_speed1", Vector) = (1,0,0,0)
		_Distort_repair ("Distort_repair", Vector) = (0,0,0,0)
		_Distort_power ("Distort_power", Range(-2, 2)) = 0
		[Toggle(_USE_CUSTOM1_Y_DISTORT_ON)] _use_custom1_y_distort ("use_custom1_y_distort", Float) = 0
		_Distort_mask ("Distort_mask", 2D) = "white" {}
		_Alpha_mask ("Alpha_mask", 2D) = "white" {}
		[Enum(R,0,G,1,B,2,A,3)] _mask_Choose_channel ("mask_Choose_channel", Float) = 0
		[Toggle(_USE_FRENSEL_ON)] _use_frensel ("use_frensel", Float) = 0
		[Toggle(_FRENSEL_FLIP_ON)] _frensel_flip ("frensel_flip", Float) = 0
		_frensel ("frensel", Range(-0.01, 1)) = 0
		_frensel_edge ("frensel_edge", Range(0, 1)) = 0
		[Enum(Off,0,on,1)] _Zwrite ("Zwrite", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _Ztest ("Ztest", Float) = 4
		[ASEEnd] [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
		[HideInInspector] _texcoord ("", 2D) = "white" {}
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
	//CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
}