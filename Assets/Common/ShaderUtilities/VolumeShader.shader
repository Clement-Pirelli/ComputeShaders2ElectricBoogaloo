Shader "Unlit/VolumeShader"
{
    Properties
    {
        _MainTex("Texture", 3D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "white" {}

        _Alpha("Alpha", float) = 0.02
        _StepSize("Step Size", float) = 0.01
        _MaxSteps("Max Steps", int) = 100

        _LightStepSize("Light Step Size", float) = 0.1
        _LightMaxSteps("Max Light Steps", int) = 10
        _LightColor("Light Color", Color) = (1,1,1,1)
        _LightDirection("Light Direction", Vector) = (1,1,1, 1)
        _LightStrength("Light Strength", float) = 1.0

        _UsePowder("Use Powder Law", Range(0, 1)) = 1

        _DensityMultiplier("Density Multiplier", float) = 1.0
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            Blend One OneMinusSrcAlpha
            LOD 100

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                // Allowed floating point inaccuracy
                #define EPSILON 0.00001f

                struct appdata
                {
                    float4 vertex : POSITION;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float3 objectVertex : TEXCOORD0;
                    float3 vectorToSurface : TEXCOORD1;
                    float2 screenPosition : TEXCOORD2;
                };

                sampler3D _MainTex;
                float4 _MainTex_ST;


                sampler2D _NoiseTex;
                float4 _NoiseTex_ST;
                
                float _Alpha;
                
                float _StepSize;
                int _MaxSteps;
                
                float _LightStepSize;
                int _LightMaxSteps;
                float4 _LightColor;
                float3 _LightDirection;

                float _DensityMultiplier;
                float _LightStrength;

                int _UsePowder;

                const float notDenseEnough = .01;

                v2f vert(appdata v)
                {
                    v2f o;

                    // Vertex in object space this will be the starting point of raymarching
                    o.objectVertex = v.vertex;

                    // Calculate vector from camera to vertex in world space
                    float3 worldVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.vectorToSurface = worldVertex - _WorldSpaceCameraPos;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.screenPosition = ComputeScreenPos(o.vertex);
                    return o;
                }

                bool WithinUnitCube(float3 samplePosition)
                {
                    return max(abs(samplePosition.x), max(abs(samplePosition.y), abs(samplePosition.z))) < 0.5f + EPSILON;
                }

                float4 SampleCloud(float3 samplePosition)
                {
                    const float4 cloud = tex3D(_MainTex, samplePosition + float3(0.5f, 0.5f, 0.5f));
                    return float4(cloud.xyz, cloud.a * _Alpha);
                }

                struct LightDensityArgs 
                {
                    float3 startPosition;
                    float3 fixedStep;
                };

                float CalculateLightDensity(LightDensityArgs args)
                {
                    float result = .0;
                    float3 samplePoint = args.startPosition;
                    
                    [loop]
                    for (int i = 0; i < _LightMaxSteps; i++)
                    {
                        result += SampleCloud(samplePoint).a;
                        samplePoint += args.fixedStep;

                        if (!WithinUnitCube(samplePoint))
                        {
                            break;
                        }
                    }
                    return result;
                }


                fixed4 frag(v2f i) : SV_Target
                {
                    const float3 rayOrigin = i.objectVertex;
                    const float3 rayDirection = mul(unity_WorldToObject, float4(normalize(i.vectorToSurface), 1));

                    const float4 noise = frac(tex2D(_NoiseTex, i.screenPosition) + _Time);
                    const float noiseBasedOffset = (noise * _StepSize);

                    float3 samplePosition = rayOrigin + (rayDirection * noiseBasedOffset);
                    const float3 fixedStep = rayDirection * _StepSize;

                    float transmittance = 1.0;
                    float4 lightEnergy = float4(.0, .0, .0, .0);

                    LightDensityArgs lightDensityArgs;
                    const float3 lightDirection = mul(unity_WorldToObject, float4(normalize(_LightDirection.xyz), 0));
                    lightDensityArgs.fixedStep = lightDirection * _LightStepSize;


                    [loop]
                    for (int i = 0; i < _MaxSteps; i++)
                    {
                        const float4 cloud = SampleCloud(samplePosition);
                        const float density = cloud.a; 
                        if (density > notDenseEnough)
                        {
                            transmittance *= 1. - density;
                            lightDensityArgs.startPosition = samplePosition;
                            const float lightDensity = CalculateLightDensity(lightDensityArgs);
                            const float beer = exp(-lightDensity * _DensityMultiplier) * density * transmittance;
                            const float powder = _UsePowder > 0 ? 1.0 - beer : 1.0;
                            lightEnergy += beer * powder * float4(_LightColor.xyz * _LightStrength, .0) * float4(cloud.xyz, .0);
                            lightEnergy.a += density;
                        }
                        samplePosition += fixedStep;
                        if (!WithinUnitCube(samplePosition))
                        {
                            break;
                        }
                    }

                    return saturate(lightEnergy);
                }
                ENDCG
            }
        }
}