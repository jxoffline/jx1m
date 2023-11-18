Shader "SupGames/Mobile/PostProcess"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"
	#define lum fixed3(0.212673h, 0.715152h, 0.072175h)
	#define hal fixed3(0.5h, 0.5h, 0.5h)

	struct appdata {
		fixed4 pos : POSITION;
		fixed2 uv : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2fb {
		fixed4 pos : SV_POSITION;
		fixed4 uv : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};
	struct v2f {
		fixed4 pos : SV_POSITION;
		fixed4 uv  : TEXCOORD0;
		fixed4 uv1 : TEXCOORD1;
		fixed4 uv2 : TEXCOORD2;
		fixed2 uv3  : TEXCOORD3;
		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};

	uniform UNITY_DECLARE_TEX3D(_LutTex);
	uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
	uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_MaskTex);
	uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_BlurTex);
	uniform fixed _LutAmount;
	uniform fixed4 _BloomColor;
	uniform fixed _BlurAmount;
	uniform fixed _BloomDiffuse;
	uniform fixed4 _Color;
	uniform fixed4 _BloomData;
	uniform fixed _Contrast;
	uniform fixed _Brightness;
	uniform fixed _Saturation;
	uniform fixed _CentralFactor;
	uniform fixed _SideFactor;
	uniform fixed _Offset;
	uniform fixed _FishEye;
	uniform fixed _LensDistortion;
	uniform fixed4 _VignetteColor;
	uniform fixed _VignetteAmount;
	uniform fixed _VignetteSoftness;
	uniform fixed4 _MainTex_TexelSize;

	v2fb vertBlur(appdata i)
	{
		v2fb o;
		UNITY_SETUP_INSTANCE_ID(i);
		UNITY_INITIALIZE_OUTPUT(v2fb, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.pos = UnityObjectToClipPos(i.pos);
#if defined(BLOOM) && !defined(BLUR)
		fixed2 offset = _MainTex_TexelSize.xy * _BloomDiffuse;
#else
		fixed2 offset = _MainTex_TexelSize.xy * _BlurAmount;
#endif
		o.uv = fixed4(i.uv - offset, i.uv + offset);
		return o;
	}

	v2f vert(appdata i)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(i);
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.pos = UnityObjectToClipPos(i.pos);
		o.uv.xy = UnityStereoTransformScreenSpaceTex(i.uv);
		o.uv.zw = i.uv;
		o.uv1 = fixed4(o.uv.xy - _MainTex_TexelSize.xy, o.uv.xy + _MainTex_TexelSize.xy);
		o.uv2.x = o.uv.x - _Offset * _MainTex_TexelSize.x - 0.5h;
		o.uv2.y = o.uv.x + _Offset * _MainTex_TexelSize.x - 0.5h;
		o.uv2.zw = i.uv - 0.5h;
		o.uv3 = o.uv.xy - 0.5h;
		return o;
	}

	fixed4 fragBlur(v2fb i) : SV_Target
	{ 
		UNITY_SETUP_INSTANCE_ID(i);
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		fixed4 b = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv.xy);
		b += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv.xw);
		b += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv.zy);
		b += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv.zw);
		return b * 0.25h;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		UNITY_SETUP_INSTANCE_ID(i);
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		fixed q = dot(i.uv2.zw, i.uv2.zw);
		fixed q2 = sqrt(q);

#ifdef DISTORTION
		fixed q3 = q * _LensDistortion * q2;
		i.uv.xy = (1.0h + q3) * i.uv3 + hal;
#endif

		fixed4 c = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv.xy);

#if defined(BLUR) || defined(BLOOM)
		fixed4 b = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_BlurTex, i.uv.xy);
#endif

#ifdef BLUR
		fixed4 m = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MaskTex, i.uv.zw);
#endif

#ifdef CHROMA
		fixed r = dot(i.uv2.xw, i.uv2.xw);
#ifdef DISTORTION
		fixed2 r2 = (1.0h + r * _FishEye * sqrt(r) + q3) * i.uv2.xw + hal;
#else
		fixed2 r2 = (1.0h + r * _FishEye * sqrt(r)) * i.uv2.xw + hal;
#endif
		c.r = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, r2).r;
#ifdef BLUR 
		b.r = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_BlurTex, r2).r;
#endif
		r = dot(i.uv2.yw, i.uv2.yw);
#ifdef DISTORTION
		r2 = (1.0h - r * _FishEye * sqrt(r) + q3) * i.uv2.yw + hal;
#else
		r2 = (1.0h - r * _FishEye * sqrt(r)) * i.uv2.yw + hal;
#endif
		c.b = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, r2).b;
#ifdef BLUR
		b.b = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_BlurTex, r2).b;
#endif
#endif
#if !defined(UNITY_NO_LINEAR_COLORSPACE)
		c.rgb = sqrt(c.rgb);
#if defined(BLOOM)|| defined(BLUR)
		b.rgb = sqrt(b.rgb);
#endif
#endif
#ifdef SHARPEN
		c *= _CentralFactor;
		c -= UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv1.xy) * _SideFactor;
		c -= UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv1.xw) * _SideFactor;
		c -= UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv1.zy) * _SideFactor;
		c -= UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv1.zw) * _SideFactor;
#endif

#ifdef LUT
		c = lerp(c, UNITY_SAMPLE_TEX3D(_LutTex, c.rgb * 0.9375h + 0.03125h), _LutAmount);
#if defined(BLOOM)|| defined(BLUR)
		b = lerp(b, UNITY_SAMPLE_TEX3D(_LutTex, b.rgb * 0.9375h + 0.03125h), _LutAmount);
#endif
#endif

#ifdef BLUR
		c = lerp(c, b, m.r);
#endif
#ifdef BLOOM
		fixed br = max(b.r, max(b.g, b.b));
		fixed soft = clamp(br - _BloomData.y, 0.0h, _BloomData.z);
		b *= max(soft * soft * _BloomData.w, br - _BloomData.x) / max(br, 0.00001h) * _BloomColor;
#if !defined(UNITY_NO_LINEAR_COLORSPACE)
		b.rgb *= b.rgb;
#endif
		c += b;
#endif

#ifdef FILTER
		c.rgb = _Contrast * c.rgb  + _Brightness;
		c.rgb = lerp(dot(c.rgb, lum), c.rgb, _Saturation) * _Color.rgb;
#endif
		c.rgb = lerp(_VignetteColor.rgb, c.rgb, smoothstep(_VignetteAmount, _VignetteSoftness, q2));

#if !defined(UNITY_NO_LINEAR_COLORSPACE)
		c.rgb *= c.rgb;
#endif
		return c;
	}
	ENDCG


	Subshader
	{

		Pass //0
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  CGPROGRAM
		  #pragma vertex vertBlur
		  #pragma fragment fragBlur
		  #pragma shader_feature_local BLOOM
		  #pragma shader_feature_local BLUR
		  #pragma fragmentoption ARB_precision_hint_fastest
		  ENDCG
		}

		Pass //1
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  CGPROGRAM
		  #pragma vertex vert
		  #pragma fragment frag
		  #pragma shader_feature_local BLOOM
		  #pragma shader_feature_local BLUR
		  #pragma shader_feature_local CHROMA
		  #pragma shader_feature_local LUT
		  #pragma shader_feature_local FILTER
		  #pragma shader_feature_local SHARPEN
		  #pragma shader_feature_local DISTORTION
		  #pragma fragmentoption ARB_precision_hint_fastest
		  ENDCG
		}
	}
	Fallback off
}