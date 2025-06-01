Shader "Outlines/OutlineShader"
{
    Properties
    {
        _Thickness("Thickness", float) =1
        [HDR]_Color("Color",Color) = (1,1,1,1)
        _BaseMap("BaseMap",2D) ="white"
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        Pass
        {
            Name "Stencil"
            //Blend Zero One
            ZWrite Off
            
            /*Stencil{
                Ref 1
                Comp Always
                Pass Replace
            }*/
            
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex Vertex
            #pragma fragment Fragment

            #pragma shader_feature_local VISIBLE

            #include "OutlineShader.hlsl"
            
            ENDHLSL
        }
        
        Pass
        {
            Name "Outlines"

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
                float2 uv: TEXCOORD0;
            };
            
            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv: TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            CBUFFER_END
            
            
            VertexOutput Vertex(Attributes input)
            {
                VertexOutput output;
                float3 poso = input.posO.xyz * 1.1;
                output.pos = GetVertexPositionInputs(poso).positionCS;
                output.uv = TRANSFORM_TEX(input.uv,_BaseMap);
                return output;
            }
            
            half4 Fragment(VertexOutput input) : SV_Target
            {
                /*half4 color = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv);
                return color;*/
                return _Color;
            }

            ENDHLSL
        }
    }
}
