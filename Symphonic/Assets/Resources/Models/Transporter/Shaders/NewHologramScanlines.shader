Shader "MiDaEm/HologramScanlines"
{
	Properties
	{
		_Color("Color", Color) = (0.3098039,0.8,1,1)
		_BlinkSpeed("BlinkSpeed", Float) = 5
		_Scanlines("Scanlines", Float) = 20
		_MinAlphaScanlines("MinAlphaScanlines", Range( 0 , 1)) = 0.6
		_MaxAlphaScanlines("MaxAlphaScanlines", Range( 0 , 1)) = 1
		_SpeedScanline("SpeedScanline", Float) = 0.5
		_HardnessScanline("HardnessScanline", Float) = 1
		_RandomBlinkTexture("RandomBlinkTexture", 2D) = "white" {}
		_MinAlphaBlink("MinAlphaBlink", Range( 0 , 1)) = 0.5
		_MaxAlphaBlink("MaxAlphaBlink", Range( 0 , 1)) = 1
		_Alpha("Alpha", 2D) = "white" {}
		_USpeed("USpeed", Float) = 0
		_VSpeed("VSpeed", Float) = 1
		_MaskBorder("MaskBorder", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit alpha:fade keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color;
		uniform sampler2D _Alpha;
		uniform float4 _Alpha_ST;
		uniform float _USpeed;
		uniform float _VSpeed;
		uniform float _MinAlphaScanlines;
		uniform float _MaxAlphaScanlines;
		uniform float _Scanlines;
		uniform float _SpeedScanline;
		uniform float _HardnessScanline;
		uniform float _MinAlphaBlink;
		uniform float _MaxAlphaBlink;
		uniform sampler2D _RandomBlinkTexture;
		uniform float _BlinkSpeed;
		uniform sampler2D _MaskBorder;
		uniform float4 _MaskBorder_ST;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Emission = _Color.rgb;
			float2 uv_Alpha = i.uv_texcoord * _Alpha_ST.xy + _Alpha_ST.zw;
			float4 appendResult13 = (float4(_USpeed , _VSpeed , 0.0 , 0.0));
			float clampResult29 = clamp( pow( (0.0 + (sin( ( _Scanlines * ( uv_Alpha.y - ( _Time.x * _SpeedScanline ) ) * 6.28318548202515 ) ) - -1.0) * (1.0 - 0.0) / (1.0 - -1.0)) , _HardnessScanline ) , 0.0 , 1.0 );
			float lerpResult16 = lerp( _MinAlphaScanlines , _MaxAlphaScanlines , clampResult29);
			float temp_output_22_0 = ( _BlinkSpeed * ( _Time.y / 15.0 ) );
			float4 appendResult21 = (float4(temp_output_22_0 , temp_output_22_0 , 0.0 , 0.0));
			float lerpResult17 = lerp( _MinAlphaBlink , _MaxAlphaBlink , tex2D( _RandomBlinkTexture, appendResult21.xy ).r);
			float2 uv_MaskBorder = i.uv_texcoord * _MaskBorder_ST.xy + _MaskBorder_ST.zw;
			o.Alpha = ( ( tex2D( _Alpha, ( float4( uv_Alpha, 0.0 , 0.0 ) + ( appendResult13 * _Time.y ) ).xy ).a * ( lerpResult16 * lerpResult17 ) ) * tex2D( _MaskBorder, uv_MaskBorder ).a );
		}

		ENDCG
	}

}
