Shader "MetalPod/Hovercraft/DamageOverlay"
{
    Properties
    {
        _FlashColor ("Flash Color", Color) = (1, 0, 0, 0.3)
        _FlashIntensity ("Flash", Range(0, 1)) = 0
        _VignetteIntensity ("Vignette", Range(0, 1)) = 0
        _VignetteColor ("Vignette Color", Color) = (0.5, 0, 0, 1)
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
            Name "FullScreenDamageOverlay"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _FlashColor;
                float4 _VignetteColor;
                float _FlashIntensity;
                float _VignetteIntensity;
            CBUFFER_END

            float _DamageFlashIntensity;
            float _DamageVignetteIntensity;

            half4 Frag(Varyings input) : SV_Target
            {
                half4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);

                float flash = saturate(max(_FlashIntensity, _DamageFlashIntensity));
                float vignetteIntensity = saturate(max(_VignetteIntensity, _DamageVignetteIntensity));

                float2 centered = input.texcoord * 2.0 - 1.0;
                float vignette = smoothstep(0.35, 1.0, length(centered)) * vignetteIntensity;

                half3 color = scene.rgb;
                color += _FlashColor.rgb * (_FlashColor.a * flash);
                color = lerp(color, _VignetteColor.rgb, vignette * _VignetteColor.a);

                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
