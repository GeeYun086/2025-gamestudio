#ifndef OUTLINESHADER_INCLUDED
#define OUTLINESHADER_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
float _Thickness;
            
struct Attributes
{
    float4 posO : POSITION;
    float4 posN : NORMAL;
};
            
struct VertexOutput
{
    float4 pos: SV_POSITION;
};

VertexOutput Vertex(Attributes input)
{
    VertexOutput output = (VertexOutput)0;
    #if VISIBLE
    float len = dot(input.posN,input.posO.xyz);
    float3 direction= input.posN*len - input.posO.xyz;
    float3 posOS = input.posO.xyz + direction*_Thickness;;
    output.pos = GetVertexPositionInputs(posOS).positionCS;
    #else
    output.pos = GetVertexPositionInputs(input.posO.xyz).positionCS;
    #endif
    return output;
}

float4 Fragment(VertexOutput input) : SV_Target
{
    return 0;
}
#endif
