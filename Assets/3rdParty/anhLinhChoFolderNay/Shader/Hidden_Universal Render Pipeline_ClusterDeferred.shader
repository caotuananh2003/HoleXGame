Shader "Hidden/Universal Render Pipeline/ClusterDeferred" {
	Properties {
		_LitStencilRef ("LitStencilRef", Float) = 0
		_LitStencilReadMask ("LitStencilReadMask", Float) = 0
		_LitStencilWriteMask ("LitStencilWriteMask", Float) = 0
		_SimpleLitStencilRef ("SimpleLitStencilRef", Float) = 0
		_SimpleLitStencilReadMask ("SimpleLitStencilReadMask", Float) = 0
		_SimpleLitStencilWriteMask ("SimpleLitStencilWriteMask", Float) = 0
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
	Fallback "Hidden/Universal Render Pipeline/FallbackError"
}