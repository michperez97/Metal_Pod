Shader "MetalPod/Environment/Lava/VolcanicRock"
{
    Properties
    {
        [MainTexture] _MainTex ("Rock Texture", 2D) = "white" {}
        _EmissiveMask ("Vein Mask", 2D) = "black" {}
        _RockColor ("Rock Color", Color) = (0.05, 0.03, 0.02, 1)
        [HDR] _VeinColor ("Vein Color", Color) = (1, 0.3, 0, 1)
        _VeinIntensity ("Vein Intensity", Range(0, 8)) = 4
        _PulseSpeed ("Pulse Speed", Range(0, 3)) = 1
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.3
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
                float4 _RockColor;
                float4 _VeinColor;
                float _VeinIntensity;
                float _PulseSpeed;
                float _PulseAmount;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_EmissiveMask);
            SAMPLER(sampler_EmissiveMask);

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
                half3 rockTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).rgb;
                half veinMask = SAMPLE_TEXTURE2D(_EmissiveMask, sampler_EmissiveMask, input.uv).r;

                Light mainLight = GetMainLight();
                half3 n = normalize(input.normalWS);
                half ndotl = saturate(dot(n, mainLight.direction));
                half3 litRock = rockTex * _RockColor.rgb * (0.35h + ndotl * 0.65h);

                float pulse = 1.0 + (sin(_Time.y * _PulseSpeed) * _PulseAmount);
                half3 veins = _VeinColor.rgb * (_VeinIntensity * pulse * veinMask);

                return half4(litRock + veins, 1.0h);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
