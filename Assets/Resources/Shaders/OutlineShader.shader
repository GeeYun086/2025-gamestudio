Shader"Outlines/OutlineShader"
{
    Properties
    {
        [HDR]_Color("Color",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" "Queue"="Geometry"
        }

        Pass
        {
            Name "Outline"
            Stencil
            {
                Ref 0
                Comp Equal
            }
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _Color;

            struct Attributes
            {
                float4 posO : POSITION;
            };

            struct VertexOutput
            {
                float4 pos: SV_POSITION;
            };

            VertexOutput Vertex(Attributes input)
            {
                VertexOutput output = (VertexOutput)0;
                output.pos = GetVertexPositionInputs(input.posO.xyz).positionCS;
                return output;
            }

            float4 Fragment(VertexOutput input) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}