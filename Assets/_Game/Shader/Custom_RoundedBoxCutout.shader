Shader "Custom/RoundedBoxCutout" {
	Properties {
		_OverlayColor ("Overlay Color (RGBA)", Vector) = (0,0,0,0.7)
		_BoxSize ("Half Box Size (x,y)", Vector) = (0.5,0.5,0,0)
		_CornerRadius ("Corner Radius", Float) = 0.1
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
}