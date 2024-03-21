Shader "ARVR/Normal To Color"
{
	Properties{
	}
	SubShader{
	  Tags { "RenderType" = "Opaque" }
	  CGPROGRAM
	  #pragma surface surf Lambert vertex:vert
	  struct Input {
		  float3 customColor;
	  };
	  void vert(inout appdata_full v, out Input o) {
		   UNITY_INITIALIZE_OUTPUT(Input,o);   //Po x W x V x P => Pw

		   o.customColor = abs(UnityObjectToWorldNormal(v.normal)); //mul(World, v.normal)
		  // o.customColor = abs(v.normal);
		 }
		 sampler2D _MainTex;
		 void surf(Input IN, inout SurfaceOutput o) {
			 o.Albedo = IN.customColor;
		 }
		 ENDCG
	}
		Fallback "Diffuse"
}