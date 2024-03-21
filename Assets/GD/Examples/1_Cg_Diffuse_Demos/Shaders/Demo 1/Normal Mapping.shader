Shader "ARVR/Diffuse Bump" {
	Properties{
	  _MainTex("Texture", 2D) = "white" {}
	  _BumpMap("Bumpmap", 2D) = "bump" {}
	}
		SubShader{
		  Tags { "RenderType" = "Opaque" }
		  CGPROGRAM
		  #pragma surface surf Lambert
		  struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
		  };
		  sampler2D _MainTex;
		  sampler2D _BumpMap;

		  void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;

			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		  }

		  ENDCG
	}
		Fallback "Diffuse"
}

//[-1, 1] => [0, 1]

//N = (-0.5, 0.5, 0)  => (0.5, 1.5, 1) => (0.25, 0.75, 0.5)