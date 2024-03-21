Shader "ARVR/Diffuse Simple" {
	SubShader{
	  Tags { "RenderType" = "Opaque" }
	  CGPROGRAM
	  #pragma surface surf Lambert
	  struct Input {
		  float4 color : COLOR;
	  };
	  void surf(Input IN, inout SurfaceOutput o) {
		   o.Albedo = 1;
	   }
	   ENDCG
	}
		Fallback "Diffuse"
}

//fragment shader
//- manipulates vertices (position, UV, normal, tangent, binormals)
//- takes vertex multiplies the vertex by World x View x Projection

//surface shader
// - fills in the color for all points on the screen space (x[-1, 1], y[1, -1]) => [0,0] -> [1920, 1080]