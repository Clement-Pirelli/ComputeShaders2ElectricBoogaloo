
float4x4 transform;
float3 position;

int resolution;
float spacing;
float size;
float sizeY;

Texture2D<float4> inputTexture;

PackedVaryingsType InstancedVert(AttributesMesh inputMesh, uint instanceID : SV_InstanceID)
{
	// Instance object space
	float4 pos = 0;
    pos.xyz = inputMesh.positionOS;

	const int3 id = int3(
		instanceID % resolution,
		floor(instanceID / (float)resolution),
		0);

	const float4 color = saturate(inputTexture[id.xy]);

	const float height = sizeY * (color.r * 1.5 + color.g * 1.7 + color.b * 1.3); //todo: mess around with this
	
	pos.y *= height;
	pos.y += height / 2.0;

	// SET COLOR
#ifdef ATTRIBUTES_NEED_COLOR
	inputMesh.color = color;
#endif


	// Grid Position
	const float halfResolution = resolution / 2.0;
	const float4 gridPosition = float4(id.x - halfResolution, 0, id.y - halfResolution, 0) * spacing;


	pos.xz *= size;

	// SET POSITION
	inputMesh.positionOS = mul(transform, pos + gridPosition).xyz + position;

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