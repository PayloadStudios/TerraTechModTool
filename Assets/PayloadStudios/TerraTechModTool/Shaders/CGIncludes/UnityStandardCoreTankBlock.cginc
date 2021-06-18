#include "UnityStandardCore.cginc"

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(half4, _DamageEmitSkin)
UNITY_INSTANCING_BUFFER_END(Props)

Texture2D _DamageCLUT;
SamplerState sampler_point_repeat;
float _RcpNumSkinsU;
float _RcpNumSkinsV;

half3 TankBlockAlbedo(float4 texcoords, float2 skinIndex)
{
#ifdef _SKINS
	half3 albedo = _Color.rgb * tex2D(_MainTex, texcoords.xy * float2(_RcpNumSkinsU, _RcpNumSkinsV) + float2(skinIndex.x * _RcpNumSkinsU, skinIndex.y * _RcpNumSkinsV)).rgb;
#else
	half3 albedo = _Color.rgb * tex2D(_MainTex, texcoords.xy).rgb;
#endif
	return albedo;
}

half2 TankBlockMetallicGloss(float2 uv, float2 skinIndex)
{
#ifdef _SKINS
	half2 mg = tex2D(_MetallicGlossMap, uv * float2(_RcpNumSkinsU, _RcpNumSkinsV) + float2(skinIndex.x * _RcpNumSkinsU, skinIndex.y * _RcpNumSkinsV)).ra;
#else
#ifdef _METALLICGLOSSMAP
	half2 mg = tex2D(_MetallicGlossMap, uv).ra;
#else
	half2 mg;
	mg.r = _Metallic;
	mg.g = _Glossiness;
#endif
#endif
	return mg;
}

half3 TankBlockEmission(float2 uv, float2 skinIndex)
{
#ifndef _EMISSION
	return 0;
#else
#ifdef _SKINS
	return tex2D(_EmissionMap, uv * float2(_RcpNumSkinsU, _RcpNumSkinsV) + float2(skinIndex.x * _RcpNumSkinsU, skinIndex.y * _RcpNumSkinsV)).rgb * _EmissionColor.rgb;
#else
	return tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb;
#endif
#endif
}

inline FragmentCommonData TankBlockMetallicSetup(float4 i_tex, float2 skinIndex)
{
	half2 metallicGloss = TankBlockMetallicGloss(i_tex.xy, skinIndex);
	half metallic = metallicGloss.x;
	half smoothness = metallicGloss.y; // this is 1 minus the square root of real roughness m.

	half oneMinusReflectivity;
	half3 specColor;
	half3 diffColor = DiffuseAndSpecularFromMetallic(TankBlockAlbedo(i_tex, skinIndex), metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

	FragmentCommonData o = (FragmentCommonData)0;
	o.diffColor = diffColor;
	o.specColor = specColor;
	o.oneMinusReflectivity = oneMinusReflectivity;
	o.smoothness = smoothness;
	return o;
}

inline FragmentCommonData TankBlockFragmentSetup(inout float4 i_tex, float2 skinIndex, float3 i_eyeVec, half3 i_viewDirForParallax, float4 tangentToWorld[3], float3 i_posWorld)
{
	i_tex = Parallax(i_tex, i_viewDirForParallax);

	half alpha = Alpha(i_tex.xy);
#if defined(_ALPHATEST_ON)
	clip(alpha - _Cutoff);
#endif

	FragmentCommonData o = TankBlockMetallicSetup(i_tex, skinIndex);
	o.normalWorld = PerPixelWorldNormal(i_tex, tangentToWorld);
	o.eyeVec = NormalizePerPixelNormal(i_eyeVec);
	o.posWorld = i_posWorld;

	// NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
	o.diffColor = PreMultiplyAlpha(o.diffColor, alpha, o.oneMinusReflectivity, /*out*/ o.alpha);
	return o;
}