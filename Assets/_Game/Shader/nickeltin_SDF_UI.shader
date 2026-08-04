Shader "nickeltin/SDF/UI" {
	Properties {
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		[PerRendererData] _Color ("Tint", Vector) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		_MainColor ("Main Color", Vector) = (1,1,1,1)
		[Toggle(OUTLINE_ON)] _EnableOutline ("Enable Outline", Float) = 0
		_OutlineColor ("Outline Color", Vector) = (0,0,0,1)
		_OutlineWidth ("Outline Width", Range(0, 1)) = 0.1
		_OutlineSoftness ("Outline Softness", Range(0, 1)) = 0.1
		_ShadowColor ("Shadow Color", Vector) = (1,1,1,1)
		_ShadowSoftness ("Shadow Softness", Range(0, 1)) = 0
		_DistanceSoftness ("Distance Softness", Range(0, 1)) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "UI/Default"
	//CustomEditor "nickeltin.SDF.Editor.SDFShaderGUI"
}