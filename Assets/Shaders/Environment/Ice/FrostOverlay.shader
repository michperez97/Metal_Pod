Shader "MetalPod/Environment/Ice/FrostOverlay"
{
    Properties
    {
        _FrostTex ("Frost Texture", 2D) = "white" {}
        _FrostIntensity ("Intensity", Range(0, 1)) = 0
        _FrostColor ("Frost Color", Color) = (0.8, 0.9, 1, 1)
        _VignetteSize ("Vignette", Range(0, 1)) = 0.3
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
            Name "FullScreenFrostOverlay"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _FrostTex_ST;
                float4 _FrostColor;
                float _FrostIntensity;
                float _VignetteSize;
            CBUFFER_END

            TEXTURE2D(_FrostTex);
            SAMPLER(sampler_FrostTex);

            half4 Frag(Varyings input) : SV_Target
            {
                half4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);

                float2 centered = input.texcoord * 2.0 - 1.0;
                float radial = saturate(length(centered));
                float edgeMask = smoothstep(1.0 - _VignetteSize, 1.0, radial);

                float2 frostUV = TRANSFORM_TEX(input.texcoord, _FrostTex) + float2(_Time.y * 0.02, _Time.y * 0.01);
                half frostSample = SAMPLE_TEXTURE2D(_FrostTex, sampler_FrostTex, frostUV).r;
                half frostMask = saturate(edgeMask * frostSample * _FrostIntensity);

                scene.rgb = lerp(scene.rgb, scene.rgb + (_FrostColor.rgb * 0.45h), frostMask);
                scene.a = 1.0h;
                return scene;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
