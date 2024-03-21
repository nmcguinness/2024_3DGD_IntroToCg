Shader "ARVR/Screen Space Texture" {
	Properties{
	  _MainTex("Main Texture", 2D) = "white" {}
	  _Detail("Screen Space Texture", 2D) = "gray" {}
	  _ScreenSpaceUMultiplier("Screen Space U-Value Multipler", Range(0,20)) = 4.0
	  _ScreenSpaceVMultiplier("Screen Space V-Value Multipler", Range(0,20)) = 6.0
	}
		SubShader{
		  Tags { "RenderType" = "Opaque" }
		  CGPROGRAM
		  #pragma surface surf Lambert  //N.L
		  struct Input {
			  float2 uv_MainTex;
			  float4 screenPos;  //new variable
		  };
		  sampler2D _MainTex;
		  sampler2D _Detail;
		  fixed _ScreenSpaceUMultiplier, _ScreenSpaceVMultiplier;   //float(32), half(16), fixed(11)

		  void surf(Input IN, inout SurfaceOutput o) {
			  o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
			  float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			  screenUV *= float2(_ScreenSpaceUMultiplier, _ScreenSpaceVMultiplier);
			  o.Albedo *= tex2D(_Detail, screenUV).rgb;
		  }
		  ENDCG
	  }
		  Fallback "Diffuse"
}