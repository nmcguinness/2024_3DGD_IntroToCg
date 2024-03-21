Shader "ARVR/Simple Toon Shader" {
	Properties
	{
	  _MainTex("Texture", 2D) = "white"{}
	  _RampTex("Ramp", 2D) = "black"{}
	   _RimColor("Rim Color", Color) = (0.26,0.19,0.16, 0.0)
	  _RimThreshold("Rim Threshold", Range(0,0.99)) = 0.1
	}

		SubShader
	   {
		   Tags { "RenderType" = "Opaque" }

		   CGPROGRAM
		   // Use the LightingToon function instead
		   #pragma surface surf Toon

		   struct Input
		   {
			   float2 uv_MainTex;
			   float3 viewDir;
		   };

		   sampler2D _MainTex;
		   sampler2D _RampTex;
		   float4 _RimColor;
		   float _RimThreshold;

		   void surf(Input IN, inout SurfaceOutput o)
		   {
			   o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;

			   if (dot(normalize(IN.viewDir), o.Normal) <= _RimThreshold)
				   o.Albedo.rgb = _RimColor.rgb;
		   }

		   fixed4 LightingToon(SurfaceOutput o, fixed3 lightDir, fixed atten)
		   {
			   half NdotL = dot(o.Normal, lightDir);
			   NdotL = tex2D(_RampTex, fixed2(NdotL, 0.5));
			   half4 color;
			   color.rgb = o.Albedo * _LightColor0.rgb * (NdotL * atten);
			   color.a = o.Alpha;
			   return color;
		   }

		   ENDCG
	   }
		   FallBack "Diffuse"
}