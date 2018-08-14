Shader "Ocean Shaders/Ocean"
{
	Properties
	{
		_Shininess("Shininess", Range(0.03, 10)) = 1.0
		_MyColor("Shine Color", Color) = (255,255,0,255)

		_Color("Main Color", Color) = (1,1,1,1)

		_MainTex("Texture", 2D) = "white" {}
		_MainTex2("Texture 2", 2D) = "white" {}
		_Blend_Strength("Blend Strength", range(-100,200)) = 0.5

		_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpMap_Strength("Normal Strength", range (-30,30)) = 0.5

		_Fog_Strength("Fog Strength", range(0,1000)) = 450.0

		_ColorTint("Tint", Color) = (0.0, 125.0, 200.0, 255.0)
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Cull Off	//	Render both sides
		CGPROGRAM

		#pragma surface surf Lambert finalcolor:mycolor vertex:myvert
		#pragma multi_compile_fog

			sampler2D _MainTex;
			sampler2D _MainTex2;

			float _Blend_Strength;

			//FOG
			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			float _Fog_Strength;

			sampler2D _BumpMap;
			float _BumpMap_Strength;

			fixed4 _Color;

			float _Shininess;
			fixed4 _MyColor;

			fixed4 _ColorTint;

	struct Input
	{
		float2 uv_MainTex;
		float2 uv_MainTex2;

		float2 uv_BumpMap;
		float3 viewDir;

		//FOG
		half fog;
	};

	half _Blend;

	void myvert(inout appdata_full v, out Input data) 
	{
		UNITY_INITIALIZE_OUTPUT(Input, data);
		float pos = length(UnityObjectToViewPos(v.vertex).xyz);
		float diff = unity_FogEnd.x - unity_FogStart.x;
		float invDiff = 1.0f / diff;
		data.fog = clamp((unity_FogEnd.x - pos) * invDiff, 0.0, 1.0);
	}

	void mycolor(Input IN, SurfaceOutput o, inout fixed4 color)
	{
		//	Material color = Tint
		color *= _ColorTint;

		//FOG
		#ifdef UNITY_PASS_FORWARDADD
			UNITY_APPLY_FOG_COLOR(IN.fog, color, float4(0, 0, 0, 0));
		#else
			UNITY_APPLY_FOG_COLOR(IN.fog / _Fog_Strength, color, unity_FogColor );
		#endif
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		//	Blend 2 textures
		fixed4 c = lerp(tex2D(_MainTex, IN.uv_MainTex), tex2D(_MainTex2, IN.uv_MainTex), _Blend_Strength) * _Color;

		//	Normal map
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		half factor = dot(normalize(IN.viewDir), o.Normal * _BumpMap_Strength);

		//	Color
		o.Albedo = c.rgb + _MyColor * (_Shininess - factor * _Shininess);

		//	Emission
		o.Emission.rgb = _MyColor * (_Shininess - factor * _Shininess);

		o.Alpha = c.a;

		o.Specular = _Shininess;
	}
	ENDCG
	}
		FallBack "Diffuse"
}