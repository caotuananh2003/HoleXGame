Shader "Watermelon/Toon/Lit/Toon Lit Float (Legacy)" {
	Properties {
		[NoScaleOffset] _MainTex ("Main texture", 2D) = "white" {}
		_Color ("Color", Vector) = (1,1,1,1)
		[NoScaleOffset] _AlbedoTex ("DetailAlbedo", 2D) = "white" {}
		[ToggleUI] _Albedo ("Albedo", Float) = 0
		_RampMin ("RampMin", Vector) = (0,0,0,1)
		_RampMax ("RampMax", Vector) = (1,1,1,1)
		[ToggleUI] _Receive_Shadows ("Receive Shadows", Float) = 1
		_SColor ("Shadow Color", Vector) = (0,0,0,1)
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		_Occlusion ("Occlusion", Range(0, 1)) = 0
		_Albedo_Color ("Albedo Color", Vector) = (1,1,1,1)
		[ToggleUI] _Use_Ramp_Texture ("Use Ramp Texture", Float) = 0
		[NoScaleOffset] _Ramp ("Ramp", 2D) = "white" {}
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
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
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}