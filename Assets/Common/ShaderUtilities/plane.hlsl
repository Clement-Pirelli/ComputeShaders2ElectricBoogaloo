struct Plane
{
	float3 normal;
	float3 origin;
};

bool culledByPlane(float3 position, Plane plane)
{
	return dot(normalize(position - plane.origin), plane.normal) < .0;
}