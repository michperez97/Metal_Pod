Shader "MetalPod/Environment/Lava/LavaSurface"
{
    Properties
    {
        [MainTexture] _MainTex ("Lava Texture", 2D) = "white" {}
        _NoiseTex ("Flow Noise", 2D) = "gray" {}
        _FlowSpeed ("Flow Speed", Range(0.1, 2.0)) = 0.5
        _FlowDirection ("Flow Direction", Vector) = (1, 0, 0, 0)
        [HDR] _EmissiveColor ("Emissive Color", Color) = (1, 0.3, 0, 1)
        _EmissiveIntensity ("Emissive Intensity", Range(1, 10)) = 5
        _CrustColor ("Crust Color", Color) = (0.1, 0.05, 0.02, 1)
        _CrustThreshold ("Crust Threshold", Range(0, 1)) = 0.4
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
                float4 _FlowDirection;
                float4 _EmissiveColor;
                float4 _CrustColor;
                float _FlowSpeed;
                float _EmissiveIntensity;
                float _CrustThreshold;
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
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 flowOffset = _FlowDirection.xy * (_Time.y * _FlowSpeed);
                float2 flowUV = input.uv + flowOffset;
                float2 noiseUV = TRANSFORM_TEX(input.uv, _NoiseTex) + float2(_Time.y * _FlowSpeed * 0.35, _Time.y * _FlowSpeed * 0.2);

                half3 lavaBase = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, flowUV).rgb;
                half noiseSample = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;

                float breathing = sin(_Time.y * 1.7) * 0.05;
                float hotMask = saturate((noiseSample + breathing - _CrustThreshold) * 6.0 + 0.5);

                half3 crust = lavaBase * _CrustColor.rgb;
                half3 molten = lavaBase * _EmissiveColor.rgb * _EmissiveIntensity;
                half3 color = lerp(crust, molten, hotMask);

                Light mainLight = GetMainLight();
                half ndotl = saturate(dot(normalize(input.normalWS), mainLight.direction));
                color *= (0.65h + 0.35h * ndotl);

                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
