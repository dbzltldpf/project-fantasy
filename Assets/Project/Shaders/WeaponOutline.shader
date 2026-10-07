// 무기 아웃라인 글로우 (Inverted Hull): 법선 방향으로 부풀린 뒷면만 HDR 가산 혼합 → Bloom으로 번지는 테두리, 밝기 맥동
Shader "ProjectFantasy/WeaponOutline"
{
    Properties
    {
        [HDR] _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _Width ("Width (m)", Float) = 0.006
        _PulseSpeed ("Pulse Speed", Float) = 2
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            // 뒷면만 그려 원본 바깥 테두리만 보이게, 깊이 미기록·가산 혼합
            Cull Front
            ZWrite Off
            Blend One One

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Width;
                float _PulseSpeed;
                float _PulseAmount;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            // 월드 단위로 부풀려 무기 크기·스케일과 무관한 두께
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(positionWS + normalWS * _Width);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half pulse = 1.0h + sin(_Time.y * _PulseSpeed) * _PulseAmount;
                return half4(_BaseColor.rgb * pulse, 1.0h);
            }
            ENDHLSL
        }
    }
}
