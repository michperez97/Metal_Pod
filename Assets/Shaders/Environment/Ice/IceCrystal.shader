Shader "MetalPod/Environment/Ice/IceCrystal"
{
    Properties
    {
        _Color ("Color", Color) = (0.6, 0.8, 1, 0.5)
        _Refraction ("Refraction", Range(0, 0.1)) = 0.02
        _Smoothness ("Smoothness", Range(0, 1)) = 0.98
        _FresnelPower ("Fresnel", Range(1, 6)) = 3
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite Off

        Pass
        {
            Name "ForwardTransparent"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Refraction;
                float _Smoothness;
                float _FresnelPower;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 n = normalize(input.normalWS);
                half3 v = normalize(input.viewDirWS);
                half fresnel = pow(1.0h - saturate(dot(n, v)), _FresnelPower);

                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionHCS);
                float2 refractOffset = n.xy * (_Refraction * (0.5h + fresnel));
                half3 scene = SampleSceneColor(screenUV + refractOffset).rgb;

                half3 sparkle = fresnel * _Smoothness.xxx;
                half3 tint = lerp(scene, _Color.rgb, 0.25h + fresnel * 0.5h);
                half alpha = saturate(_Color.a + fresnel * 0.25h);

                return half4(tint + sparkle * 0.2h, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
