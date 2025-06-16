Shader "Outlines/OutlineStencilShader"
{
    Properties
    {
        _Thickness("Thickness", float) =1
        [HDR]_Color("Color",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" "Queue"="Geometry-1"
        }

        Pass
        {
            Name "Stencil"
            Blend Zero One
            ZWrite Off
            cull front
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex Vertex
            #pragma fragment Fragment

            #pragma shader_feature_local VISIBLE

            #include "OutlineShader.hlsl"
            ENDHLSL
        }
    }
}