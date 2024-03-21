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
		 // o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
		 
		  //lets add linear filtering to this shader (i.e. 5 point star filtering)
		  float2 pixelOffset = 8*float2(1.0/1920, 1.0/1080);

		  //lets read the north pixel
		 float4 centrePixel = tex2D(_MainTex, IN.uv_MainTex + float2(0, 1) * pixelOffset)
				 + tex2D(_MainTex, IN.uv_MainTex + float2(0, 1) * pixelOffset)
				 + tex2D(_MainTex, IN.uv_MainTex + float2(0, -1) * pixelOffset)
				 + tex2D(_MainTex, IN.uv_MainTex + float2(1, 0) * pixelOffset)
				 + tex2D(_MainTex, IN.uv_MainTex + float2(-1, 0) * pixelOffset);

		centrePixel = centrePixel / 5;
		 o.Albedo = centrePixel.rgb;

	  }
	  ENDCG
	}
		Fallback "Diffuse"
}