Shader "Unlit/ParticlesShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include "UnityCG.cginc"
            struct Agent
            {
                float2 position;
                float angle;
                int neighbors;
            };
            StructuredBuffer<Agent> agentsBuffer;
            int domainSize;
            float scale;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 color : TEXCOORD0;
            };


            v2f InstancedVert(appdata data, uint instanceID : SV_InstanceID)
            {
                v2f o;
                const float3 a = float3(0.5, 0.5, 0.5),
                    b = float3(0.5, 0.5, 0.5),
                    c = float3 (1.0, 1.0, 1.0),
                    d = float3(0.0, 0.10, 0.20);

                const Agent agent = agentsBuffer[instanceID];

                o.color = a + b * cos(6.28318 * (c * (agent.neighbors / 10.0f) + d));
            
                const float3 position = float3(agent.position, .0) / (float)domainSize;
            
                const float3 pos = data.vertex.xyz * scale;
            
                o.vertex = float4(pos + position, 1.0);

                return o;
            }
            
            
            
            fixed4 InstancedFrag(v2f input) : SV_Target
            {
                return float4(input.color, 1.0);
            }


            #pragma vertex InstancedVert
            #pragma fragment InstancedFrag

            ENDHLSL
        }
    }
}
