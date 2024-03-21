Shader "ARVR/Diffuse Alpha"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Alpha("Alpha", Range(0,1)) = 0.5
	}
		SubShader
		{
			//add object to the correct queue for transparent rendering
			Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
			Cull Off

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types, enable alpha
			#pragma surface surf Standard alpha:fade

			sampler2D _MainTex;
			sampler2D _OpacityTex;

			struct Input
			{
				float2 uv_MainTex;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
			half _Alpha;

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = _Alpha;
				//or make the effect time based
				//o.Alpha = abs(_SinTime);
			}
			ENDCG
		}

			FallBack "Diffuse"
}