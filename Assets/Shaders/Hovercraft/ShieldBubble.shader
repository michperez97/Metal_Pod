Shader "MetalPod/Hovercraft/ShieldBubble"
{
    Properties
    {
        [HDR] _Color ("Shield Color", Color) = (0, 0.5, 1, 0.3)
        _FresnelPower ("Fresnel", Range(1, 5)) = 2
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 2
        _HexPattern ("Hex Pattern", 2D) = "white" {}
        _PatternScale ("Pattern Scale", Range(1, 20)) = 10
        _HitPoint ("Hit Point", Vector) = (0, 0, 0, 0)
        _HitIntensity ("Hit Intensity", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        Blend SrcAlpha One
        Cull Back
        ZWrite Off

        Pass
        {
            Name "ForwardShield"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _HexPattern_ST;
                float4 _Color;
                float4 _HitPoint;
                float _FresnelPower;
                float _PulseSpeed;
                float _PatternScale;
                float _HitIntensity;
            CBUFFER_END

            TEXTURE2D(_HexPattern);
            SAMPLER(sampler_HexPattern);

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _HexPattern);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                half3 viewDir = normalize(input.viewDirWS);
                half fresnel = pow(1.0h - saturate(dot(normalWS, viewDir)), _FresnelPower);

                float2 patternUV = input.uv * _PatternScale + float2(_Time.y * 0.05, _Time.y * 0.03);
                half hex = SAMPLE_TEXTURE2D(_HexPattern, sampler_HexPattern, patternUV).r;

                float hitDistance = distance(input.positionWS, _HitPoint.xyz);
                half ripple = saturate(1.0h - abs(hitDistance - (_Time.y * 6.0h))) * _HitIntensity;
                half pulse = 0.85h + 0.15h * sin(_Time.y * _PulseSpeed);

                half alpha = saturate((fresnel * 0.8h + hex * 0.3h + ripple) * _Color.a * pulse);
                half3 color = _Color.rgb * (fresnel + hex * 0.25h) * pulse + ripple.xxx;
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
