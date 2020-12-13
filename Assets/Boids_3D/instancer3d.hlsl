
float4x4 transform;
float3 position;

int resolution;
float spacing;
float size;

#define COLOR_TYPE_FLAT 0
#define COLOR_TYPE_DIRECTION 1
#define COLOR_TYPE_SPEED 2

int colorType;
float4 flatColor;

float maxSpeed;
float4 noSpeedColor;
float4 fullSpeedColor;

struct Agent
{
	float3 position;
	float3 direction;
};

StructuredBuffer<Agent> agents;

float4 GetColor(Agent agent)
{
	[branch] switch (colorType)
	{
	case COLOR_TYPE_FLAT:
		return flatColor;
	case COLOR_TYPE_DIRECTION:
		const float3 agentDirection = abs(normalize(agent.direction));
		return float4(agentDirection.r, agentDirection.g, agentDirection.b, 1);
	case COLOR_TYPE_SPEED:
		const float speed01 = length(agent.direction) / maxSpeed;
		return lerp(noSpeedColor, fullSpeedColor, speed01);
	}
	return float4(1, 0, 1, 1);
}

PackedVaryingsType InstancedVert(AttributesMesh inputMesh, uint instanceID : SV_InstanceID)
{
#ifdef ATTRIBUTES_NEED_COLOR
	inputMesh.color = GetColor(agents[instanceID]);
#endif

	const float3 position = agents[instanceID].position / (float)resolution;

	float3 pos = inputMesh.positionOS;
	pos.xyz *= size;

	inputMesh.positionOS = mul(transform, pos + (position) * spacing).xyz + position;

	VaryingsType vt;
	vt.vmesh = VertMesh(inputMesh);
	return PackVaryingsType(vt);
}



void InstancedFrag(PackedVaryingsToPS packedInput,
	OUTPUT_GBUFFER(outGBuffer)
#ifdef _DEPTHOFFSET_ON
	, out float outputDepth : SV_Depth
#endif
)
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
	FragInputs input = UnpackVaryingsMeshToFragInputs(packedInput.vmesh);

	// input.positionSS is SV_Position
	PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);

#ifdef VARYINGS_NEED_POSITION_WS
	float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
#else
	// Unused
	float3 V = float3(1.0, 1.0, 1.0); // Avoid the division by 0
#endif

	SurfaceData surfaceData;
	BuiltinData builtinData;
	GetSurfaceAndBuiltinData(input, V, posInput, surfaceData, builtinData);


	///////////////////////////////////////////////
	// Workshop Customize
	surfaceData.baseColor = input.color.rgb;
	///////////////////////////////////////////////


	ENCODE_INTO_GBUFFER(surfaceData, builtinData, posInput.positionSS, outGBuffer);

#ifdef _DEPTHOFFSET_ON
	outputDepth = posInput.deviceDepth;
#endif
}