Shader "ARVR/Unlit Textured"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			// vertex shader inputs
			struct appdata
			{
				float4 vertex : POSITION;	// vertex position
				float2 uv : TEXCOORD0;		// texture coordinate
			};

	// vertex shader outputs ("vertex to fragment")
	struct v2f
	{
		float2 uv : TEXCOORD0;			// texture coordinate
		float4 vertex : SV_POSITION;	// clip space position
	};

	// texture we will sample
	sampler2D _MainTex;

	// vertex shader
	v2f vert(appdata v)
	{
		v2f o;
		// transform position to clip space by multiplying with model*view*projection matrix
		o.vertex = UnityObjectToClipPos(v.vertex);
		// just pass the texture coordinate
		o.uv = v.uv;
		return o;
	}

	// color ("SV_Target" semantic)
	fixed4 frag(v2f i) : SV_Target
	{
		// sample texture and return it
		fixed4 col = tex2D(_MainTex, i.uv);
		return col;
	}
	ENDCG
}
	}
}