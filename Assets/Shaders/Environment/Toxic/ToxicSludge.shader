Shader "MetalPod/Environment/Toxic/ToxicSludge"
{
    Properties
    {
        [MainTexture] _MainTex ("Sludge Texture", 2D) = "white" {}
        _NoiseTex ("Bubble Noise", 2D) = "gray" {}
        _SludgeColor ("Color", Color) = (0.2, 0.8, 0.1, 1)
        _EmissiveIntensity ("Glow", Range(0, 5)) = 3
        _BubbleSpeed ("Bubble Speed", Range(0.1, 3)) = 1
        _BubbleScale ("Bubble Scale", Range(0.5, 5)) = 2
        _FlowSpeed ("Flow Speed", Range(0.01, 0.5)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
                float4 _SludgeColor;
                float _EmissiveIntensity;
                float _BubbleSpeed;
                float _BubbleScale;
                float _FlowSpeed;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 flowUV = input.uv + float2(_Time.y * _FlowSpeed, _Time.y * _FlowSpeed * 0.3);
                half3 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, flowUV).rgb;

                float2 bubbleUV = TRANSFORM_TEX(input.uv * _BubbleScale, _NoiseTex) + float2(0.0, _Time.y * _BubbleSpeed);
                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, bubbleUV).r;

                half bubbleMask = smoothstep(0.6h, 0.95h, noise);
                half colorVariation = lerp(0.8h, 1.2h, noise);

                half3 sludgeBase = baseTex * _SludgeColor.rgb * colorVariation;
                half3 glow = _SludgeColor.rgb * (_EmissiveIntensity * (0.5h + bubbleMask * 0.5h));

                return half4(sludgeBase + glow, 1.0h);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
