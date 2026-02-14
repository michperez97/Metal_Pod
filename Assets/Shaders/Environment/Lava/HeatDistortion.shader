Shader "MetalPod/Environment/Lava/HeatDistortion"
{
    Properties
    {
        _DistortionStrength ("Distortion", Range(0, 0.1)) = 0.02
        _DistortionSpeed ("Speed", Range(0.1, 5.0)) = 1.0
        _NoiseTex ("Noise", 2D) = "gray" {}
        _Intensity ("Intensity", Range(0, 1)) = 0
        _HeatTint ("Heat Tint", Color) = (1, 0.45, 0.1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "FullScreenHeatDistortion"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST;
                float4 _HeatTint;
                float _DistortionStrength;
                float _DistortionSpeed;
                float _Intensity;
            CBUFFER_END

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            float _HeatDistortionIntensity;
            float _MetalPodHeatDistortion;

            half4 Frag(Varyings input) : SV_Target
            {
                float intensity = saturate(max(_Intensity, max(_HeatDistortionIntensity, _MetalPodHeatDistortion)));
                float2 noiseUV = TRANSFORM_TEX(input.texcoord, _NoiseTex) + float2(0.0, _Time.y * _DistortionSpeed);
                float2 noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).rg * 2.0 - 1.0;

                float2 offset = noise * (_DistortionStrength * intensity);
                half4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord + offset);

                half tintAmount = intensity * 0.2h;
                scene.rgb = lerp(scene.rgb, scene.rgb + (_HeatTint.rgb * 0.35h), tintAmount);
                scene.a = 1.0h;
                return scene;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
