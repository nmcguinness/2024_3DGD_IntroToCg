Shader "ARVR/Diffuse Texture" {
	Properties{
		 _MainTex("Texture", 2D) = "white" {}
	}
		SubShader{
		  Tags { "RenderType" = "Transparent"}
		  CGPROGRAM
		  #pragma surface surf Lambert
		  struct Input {
			  float2 uv_MainTex;
		  };

	  sampler2D _MainTex;

	  void surf(Input IN, inout SurfaceOutput o) {
		  //Cg and HLSL we can swizzle (float4, fixed4, half4) - swizzle means to interchange rgb e.g. grb, bgr
		  o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
	  }
	  ENDCG
	}
		Fallback "Diffuse"
}