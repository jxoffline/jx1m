// UI Editable properties
uniform sampler2D	_FaceTex;					// Alpha : Signed Distance
uniform float		_FaceUVSpeedX;
uniform float		_FaceUVSpeedY;
uniform fixed4		_FaceColor;					// RGBA : Color + Opacity
uniform float		_FaceDilate;				// v[ 0, 1]
uniform float		_OutlineSoftness;			// v[ 0, 1]

uniform sampler2D	_OutlineTex;				// RGBA : Color + Opacity
uniform float		_OutlineUVSpeedX;
uniform float		_OutlineUVSpeedY;
uniform fixed4		_OutlineColor;				// RGBA : Color + Opacity
uniform float		_OutlineWidth;				// v[ 0, 1]

uniform float		_Bevel;						// v[ 0, 1]
uniform float		_BevelOffset;				// v[-1, 1]
uniform float		_BevelWidth;				// v[-1, 1]
uniform float		_BevelClamp;				// v[ 0, 1]
uniform float		_BevelRoundness;			// v[ 0, 1]

uniform sampler2D	_BumpMap;					// Normal map
uniform float		_BumpOutline;				// v[ 0, 1]
uniform float		_BumpFace;					// v[ 0, 1]

uniform samplerCUBE	_Cube;						// Cube / sphere map
uniform fixed4 		_ReflectFaceColor;			// RGB intensity
uniform fixed4		_ReflectOutlineColor;
//uniform float		_EnvTiltX;					// v[-1, 1]
//uniform float		_EnvTiltY;					// v[-1, 1]
uniform float3      _EnvMatrixRotation;
uniform float4x4	_EnvMatrix;

uniform fixed4		_SpecularColor;				// RGB intensity
uniform float		_LightAngle;				// v[ 0,Tau]
uniform float		_SpecularPower;				// v[ 0, 1]
uniform float		_Reflectivity;				// v[ 5, 15]
uniform float		_Diffuse;					// v[ 0, 1]
uniform float		_Ambient;					// v[ 0, 1]

uniform fixed4		_UnderlayColor;				// RGBA : Color + Opacity
uniform float		_UnderlayOffsetX;			// v[-1, 1]
uniform float		_UnderlayOffsetY;			// v[-1, 1]
uniform float		_UnderlayDilate;			// v[-1, 1]
uniform float		_UnderlaySoftness;			// v[ 0, 1]

uniform fixed4 		_GlowColor;					// RGBA : Color + Intesity
uniform float 		_GlowOffset;				// v[-1, 1]
uniform float 		_GlowOuter;					// v[ 0, 1]
uniform float 		_GlowInner;					// v[ 0, 1]
uniform float 		_GlowPower;					// v[ 1, 1/(1+4*4)]

// API Editable properties
uniform float 		_ShaderFlags;
uniform float		_WeightNormal;
uniform float		_WeightBold;

uniform float		_ScaleRatioA;
uniform float		_ScaleRatioB;
uniform float		_ScaleRatioC;

uniform float		_VertexOffsetX;
uniform float		_VertexOffsetY;

//uniform float		_UseClipRect;
uniform float		_MaskID;
uniform sampler2D	_MaskTex;
uniform float4		_MaskCoord;
uniform float4		_ClipRect;	// bottom left(x,y) : top right(z,w)
//uniform float		_MaskWipeControl;
//uniform float		_MaskEdgeSoftness;
//uniform fixed4		_MaskEdgeColor;
//uniform bool		_MaskInverse;

uniform float		_MaskSoftnessX;
uniform float		_MaskSoftnessY;

// Font Atlas properties
uniform sampler2D	_MainTex;
uniform float		_TextureWidth;
uniform float		_TextureHeight;
uniform float 		_GradientScale;
uniform float		_ScaleX;
uniform float		_ScaleY;
uniform float		_PerspectiveFilter;
uniform float		_Sharpness;



float2 UnpackUV(float uv)
{ 
	float2 output;
	output.x = floor(uv / 4096);
	output.y = uv - 4096 * output.x;

	return output * 0.001953125;
}

fixed4 GetColor(half d, fixed4 faceColor, fixed4 outlineColor, half outline, half softness)
{
	half faceAlpha = 1-saturate((d - outline * 0.5 + softness * 0.5) / (1.0 + softness));
	half outlineAlpha = saturate((d + outline * 0.5)) * sqrt(min(1.0, outline));

	faceColor.rgb *= faceColor.a;
	outlineColor.rgb *= outlineColor.a;

	faceColor = lerp(faceColor, outlineColor, outlineAlpha);

	faceColor *= faceAlpha;

	return faceColor;
}

float3 GetSurfaceNormal(float4 h, float bias)
{
	bool raisedBevel = step(1, fmod(_ShaderFlags, 2));

	h += bias+_BevelOffset;

	float bevelWidth = max(.01, _OutlineWidth+_BevelWidth);

  // Track outline
	h -= .5;
	h /= bevelWidth;
	h = saturate(h+.5);

	if(raisedBevel) h = 1 - abs(h*2.0 - 1.0);
	h = lerp(h, sin(h*3.141592/2.0), _BevelRoundness);
	h = min(h, 1.0-_BevelClamp);
	h *= _Bevel * bevelWidth * _GradientScale * -2.0;

	float3 va = normalize(float3(1.0, 0.0, h.y - h.x));
	float3 vb = normalize(float3(0.0, -1.0, h.w - h.z));

	return cross(va, vb);
}

float3 GetSurfaceNormal(float2 uv, float bias, float3 delta)
{
	// Read "height field"
  float4 h = {tex2D(_MainTex, uv - delta.xz).a,
				tex2D(_MainTex, uv + delta.xz).a,
				tex2D(_MainTex, uv - delta.zy).a,
				tex2D(_MainTex, uv + delta.zy).a};

	return GetSurfaceNormal(h, bias);
}

float3 GetSpecular(float3 n, float3 l)
{
	float spec = pow(max(0.0, dot(n, l)), _Reflectivity);
	return _SpecularColor.rgb * spec * _SpecularPower;
}

float4 GetGlowColor(float d, float scale)
{
	float glow = d - (_GlowOffset*_ScaleRatioB) * 0.5 * scale;
	float t = lerp(_GlowInner, (_GlowOuter * _ScaleRatioB), step(0.0, glow)) * 0.5 * scale;
	glow = saturate(abs(glow/(1.0 + t)));
	glow = 1.0-pow(glow, _GlowPower);
	glow *= sqrt(min(1.0, t)); // Fade off glow thinner than 1 screen pixel
	return float4(_GlowColor.rgb, saturate(_GlowColor.a * glow * 2));
}

float4 BlendARGB(float4 overlying, float4 underlying)
{
	overlying.rgb *= overlying.a;
	underlying.rgb *= underlying.a;
	float3 blended = overlying.rgb + ((1-overlying.a)*underlying.rgb);
	float alpha = underlying.a + (1-underlying.a)*overlying.a;
	return float4(blended, alpha);
}

