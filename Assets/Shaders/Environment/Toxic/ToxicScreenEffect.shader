Shader "MetalPod/Environment/Toxic/ToxicScreenEffect"
{
    Properties
    {
        _Intensity ("Intensity", Range(0, 1)) = 0
        _TintColor ("Tint", Color) = (0.3, 1, 0.2, 0.15)
        _AberrationStrength ("Aberration", Range(0, 0.02)) = 0.005
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
            Name "FullScreenToxic"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _TintColor;
                float _Intensity;
                float _AberrationStrength;
            CBUFFER_END

            float _ToxicIntensity;

            half4 Frag(Varyings input) : SV_Target
            {
                float intensity = saturate(max(_Intensity, _ToxicIntensity));
                float aberration = _AberrationStrength * intensity;
                float2 uv = input.texcoord;

                half4 center = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                half4 redShift = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(aberration, 0));
                half4 blueShift = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(aberration, 0));

                half3 chroma = half3(redShift.r, center.g, blueShift.b);
                half3 tinted = lerp(chroma, chroma + _TintColor.rgb, intensity * _TintColor.a);
                return half4(tinted, 1.0h);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
