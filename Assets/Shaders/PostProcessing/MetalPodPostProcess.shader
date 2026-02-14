Shader "MetalPod/PostProcessing/MetalPodPostProcess"
{
    Properties
    {
        _NoiseTex ("Noise", 2D) = "gray" {}
        _HeatStrength ("Heat Strength", Range(0, 0.1)) = 0.02
        _FrostColor ("Frost Color", Color) = (0.8, 0.9, 1, 1)
        _ToxicTint ("Toxic Tint", Color) = (0.3, 1, 0.2, 0.15)
        _DamageColor ("Damage Color", Color) = (0.5, 0, 0, 1)
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
            Name "CombinedPost"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST;
                float4 _FrostColor;
                float4 _ToxicTint;
                float4 _DamageColor;
                float _HeatStrength;
            CBUFFER_END

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            float _HeatDistortionIntensity;
            float _FrostIntensity;
            float _ToxicIntensity;
            float _DamageFlashIntensity;
            float _DamageVignetteIntensity;

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 noiseUV = TRANSFORM_TEX(uv, _NoiseTex) + float2(0, _Time.y * 0.5);
                float2 noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).rg * 2.0 - 1.0;

                uv += noise * (_HeatStrength * saturate(_HeatDistortionIntensity));
                half4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                half toxic = saturate(_ToxicIntensity);
                scene.rgb = lerp(scene.rgb, scene.rgb + _ToxicTint.rgb, toxic * _ToxicTint.a);

                float2 centered = input.texcoord * 2.0 - 1.0;
                half edge = smoothstep(0.55h, 1.0h, length(centered));
                half frost = edge * saturate(_FrostIntensity);
                scene.rgb = lerp(scene.rgb, scene.rgb + _FrostColor.rgb * 0.35h, frost);

                scene.rgb += _DamageColor.rgb * (_DamageFlashIntensity * 0.35h);
                scene.rgb = lerp(scene.rgb, _DamageColor.rgb, edge * saturate(_DamageVignetteIntensity) * 0.35h);
                scene.a = 1.0h;
                return scene;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
