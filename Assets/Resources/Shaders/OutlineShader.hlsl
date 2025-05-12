#ifndef OUTLINESHADER_INCLUDED
#define OUTLINESHADER_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    //object space
struct Attributes {
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
};

    //clip space
struct VertexOutput {
    float4 positionCS : SV_POSITION;
};

float _Thickness;
float4 _Color;

VertexOutput Vertex(Attributes input){
    VertexOutput output = (VertexOutput)0;
#ifdef VISIBLE
    float3 normalOS = input.normalOS;

    float3 normal= normalize(input.positionOS.xyz)*_Thickness;
    float3 posOS = input.positionOS.xyz + normal;
#else
    float3 normal= normalize(input.positionOS.xyz)*-0.8;
    float3 posOS = input.positionOS.xyz + normal;
#endif
    output.positionCS = GetVertexPositionInputs(posOS).positionCS;
    return output;
}

float4 Fragment(VertexOutput input) : SV_Target {
    return _Color;
}

#endif