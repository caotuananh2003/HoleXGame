Shader "UI/RadialRounded" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_Thickness ("Thickness", Range(0, 1)) = 0.1
		_Fill ("Fill Amount", Range(0, 1)) = 0.75
		_Cap ("Cap Size", Range(0, 0.2)) = 0.05
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
}