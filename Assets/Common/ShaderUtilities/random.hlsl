struct RandState
{
	uint state;
};

uint wangHash(uint seed)
{
	seed = (seed ^ 61u) ^ (seed >> 16u);
	seed *= 9u;
	seed = seed ^ (seed >> 4u);
	seed *= 0x27d4eb2du;
	seed = seed ^ (seed >> 15u);
	return 1u + seed;
}

uint xorshift32(inout uint state)
{
	state ^= state << 13u;
	state ^= state >> 17u;
	state ^= state << 5u;
	return state;
}

float rand(inout uint state)
{
	return float(xorshift32(state)) * (1.0 / 4294967296.0);
}

float2 rand2(inout uint state)
{
	return float2(rand(state), rand(state));
}

float3 rand3(inout uint state)
{
	return float3(rand2(state), rand(state));
}

float3 randDir(inout uint state)
{
	const float pi = 3.141592;
	//from criver
	const float2 r = rand2(state);
	const float cos_theta = 2.0 * r.x - 1.0;
	const float phi = 2.0 * pi * r.y;
	const float sin_theta = sqrt(1.0 - cos_theta * cos_theta);
	const float sin_phi = sin(phi);
	const float cos_phi = cos(phi);

	return float3(sin_theta * cos_phi, cos_theta, sin_theta * sin_phi);
}