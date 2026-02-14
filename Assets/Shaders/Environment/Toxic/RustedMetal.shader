Shader "MetalPod/Environment/Toxic/RustedMetal"
{
    Properties
    {
        [MainTexture] _MainTex ("Metal Texture", 2D) = "white" {}
        _RustMask ("Rust Mask", 2D) = "black" {}
        _MetalColor ("Metal Color", Color) = (0.5, 0.5, 0.5, 1)
        _RustColor ("Rust Color", Color) = (0.6, 0.3, 0.1, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0.8
        _RustMetallic ("Rust Metallic", Range(0, 1)) = 0.1
        _Smoothness ("Smoothness", Range(0, 1)) = 0.4
        _RustSmoothness ("Rust Smoothness", Range(0, 1)) = 0.1
        _BumpMap ("Normal", 2D) = "bump" {}
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
                float3 viewDirWS : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MetalColor;
                float4 _RustColor;
                float _Metallic;
                float _RustMetallic;
                float _Smoothness;
                float _RustSmoothness;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_RustMask);
            SAMPLER(sampler_RustMask);

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).rgb;
                half rustMask = SAMPLE_TEXTURE2D(_RustMask, sampler_RustMask, input.uv).r;

                half3 albedoTint = lerp(_MetalColor.rgb, _RustColor.rgb, rustMask);
                half smoothness = lerp(_Smoothness, _RustSmoothness, rustMask);
                half metallic = lerp(_Metallic, _RustMetallic, rustMask);

                half3 n = normalize(input.normalWS);
                half3 v = normalize(input.viewDirWS);
                Light mainLight = GetMainLight();
                half3 l = normalize(mainLight.direction);
                half3 h = normalize(l + v);

                half ndotl = saturate(dot(n, l));
                half ndoth = saturate(dot(n, h));
                half specPower = lerp(8.0h, 64.0h, smoothness);
                half spec = pow(ndoth, specPower) * lerp(0.15h, 1.0h, metallic);

                half3 lit = baseTex * albedoTint * (0.25h + ndotl * 0.75h) + spec.xxx;
                return half4(lit, 1.0h);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
