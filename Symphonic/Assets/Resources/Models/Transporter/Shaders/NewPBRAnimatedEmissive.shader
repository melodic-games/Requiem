Shader "MiDaEm/PBR_Ani_Em"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Metallic("Metallic", 2D) = "white" {}
		[Normal]_Normal("Normal", 2D) = "white" {}
		_Speed("Speed", Float) = 5
		_Color_Em("Color_Em", Color) = (0,0.7529413,1,1)
		_EmissiveAnimated("EmissiveAnimated", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float _Speed;
		uniform sampler2D _EmissiveAnimated;
		uniform float4 _EmissiveAnimated_ST;
		uniform float4 _Color_Em;
		uniform sampler2D _Metallic;
		uniform float4 _Metallic_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			o.Albedo = tex2D( _Albedo, uv_Albedo ).rgb;
			float2 uv_EmissiveAnimated = i.uv_texcoord * _EmissiveAnimated_ST.xy + _EmissiveAnimated_ST.zw;
			o.Emission = ( (0.0 + (sin( ( _Time.x * _Speed ) ) - -1.0) * (1.0 - 0.0) / (1.0 - -1.0)) * ( tex2D( _EmissiveAnimated, uv_EmissiveAnimated ) * _Color_Em ) ).rgb;
			float2 uv_Metallic = i.uv_texcoord * _Metallic_ST.xy + _Metallic_ST.zw;
			float4 tex2DNode2 = tex2D( _Metallic, uv_Metallic );
			o.Metallic = tex2DNode2.r;
			o.Smoothness = tex2DNode2.a;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"

}
