Shader "ARVR/Opacity From Noise"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Diffuse", 2D) = "white" {}
		_OpacityTex("Opacity", 2D) = "white" {}  //Add opacity texture
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		_OpacityCutOff("Opacity Cut Off", Range(0,1)) = 0.5
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
			sampler2D _OpacityTex;     //Add any variable(s) required for this texture
			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
			fixed _OpacityCutOff;

			struct Input
			{
				float2 uv_MainTex;
				float2 uv_OpacityTex;  //Read the uv values (apply any tiling and offset from inspector)
			};

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;

				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;

				fixed4 alpha = tex2D(_OpacityTex, IN.uv_OpacityTex);
				o.Alpha = (_OpacityCutOff <= alpha.r) ? 1 : 0;  //ternary operator
			}
			ENDCG
		}

			FallBack "Diffuse"
}

//Steps
//1. Add opacity texture
//2. Add any variable(s) required for this texture
//3. Read the texture color
//4. Use the color to set the o.Alpha in the surface shader!