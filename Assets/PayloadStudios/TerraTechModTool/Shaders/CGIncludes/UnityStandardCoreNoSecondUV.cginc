// helper file for implementing standard shader passes which only use one UV set
// (this is useful for reducing the number of vertex attributes, for batching purposes)

#include "UnityStandardCore.cginc"

struct VertexInputNoSecondUV
{
	float4 vertex	: POSITION;
	half3 normal	: NORMAL;
	float2 uv0		: TEXCOORD0;
	//				float2 uv1		: TEXCOORD1;
//#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
	//				float2 uv2		: TEXCOORD2;
//#endif
#ifdef _TANGENT_TO_WORLD
	half4 tangent	: TANGENT;
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutputDeferredInst
{
	VertexOutputDeferred vod;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexOutputDeferredInst vertDeferredInst(VertexInput v)
{
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutputDeferredInst o;
	o.vod = vertDeferred(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);

	return o;
}

VertexOutputForwardBase vertForwardBaseNoSecondUVWrapper(VertexInputNoSecondUV v)
{
	VertexInput wrappedInput;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, wrappedInput);
	wrappedInput.vertex = v.vertex;
	wrappedInput.normal = v.normal;
	wrappedInput.uv0 = v.uv0;
	wrappedInput.uv1 = float2(0.0f, 0.0f);
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
	wrappedInput.uv2 = float2(0.0f, 0.0f);
#endif
#ifdef _TANGENT_TO_WORLD
	wrappedInput.tangent = v.tangent;
#endif
	return vertForwardBase(wrappedInput);
}

VertexOutputForwardAdd vertForwardAddNoSecondUVWrapper(VertexInputNoSecondUV v)
{
	VertexInput wrappedInput;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, wrappedInput);
	wrappedInput.vertex = v.vertex;
	wrappedInput.normal = v.normal;
	wrappedInput.uv0 = v.uv0;
	wrappedInput.uv1 = float2(0.0f, 0.0f);
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
	wrappedInput.uv2 = float2(0.0f, 0.0f);
#endif
#ifdef _TANGENT_TO_WORLD
	wrappedInput.tangent = v.tangent;
#endif
	return vertForwardAdd(wrappedInput);
}

VertexOutputDeferred vertDeferredNoSecondUVWrapper(VertexInputNoSecondUV v)
{
	VertexInput wrappedInput;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, wrappedInput);
	wrappedInput.vertex = v.vertex;
	wrappedInput.normal = v.normal;
	wrappedInput.uv0 = v.uv0;
	wrappedInput.uv1 = float2(0.0f, 0.0f);
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
	wrappedInput.uv2 = float2(0.0f, 0.0f);
#endif
#ifdef _TANGENT_TO_WORLD
	wrappedInput.tangent = v.tangent;
#endif
	return vertDeferred(wrappedInput);
}

VertexOutputDeferredInst vertDeferredInstNoSecondUVWrapper(VertexInputNoSecondUV v)
{
	VertexInput wrappedInput;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, wrappedInput);
	wrappedInput.vertex = v.vertex;
	wrappedInput.normal = v.normal;
	wrappedInput.uv0 = v.uv0;
	wrappedInput.uv1 = float2(0.0f, 0.0f);
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
	wrappedInput.uv2 = float2(0.0f, 0.0f);
#endif
#ifdef _TANGENT_TO_WORLD
	wrappedInput.tangent = v.tangent;
#endif
	return vertDeferredInst(wrappedInput);
}
