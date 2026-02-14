Shader "MetalPod/UI/ScreenBlur"
{
    Properties
    {
        _BlurAmount ("Blur", Range(0, 10)) = 3
        _TintColor ("Tint", Color) = (0, 0, 0, 0.5)
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
            Name "FullScreenBlur"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _TintColor;
                float _BlurAmount;
            CBUFFER_END

            half4 Frag(Varyings input) : SV_Target
            {
                float2 texel = (_BlurAmount / max(_ScreenParams.xy, 1.0));
                float2 uv = input.texcoord;

                half4 center = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                half4 x1 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(texel.x, 0));
                half4 x2 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(texel.x, 0));
                half4 y1 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, texel.y));
                half4 y2 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(0, texel.y));

                half4 blurred = center * 0.4h + (x1 + x2 + y1 + y2) * 0.15h;
                half3 tinted = lerp(blurred.rgb, _TintColor.rgb, _TintColor.a);
                return half4(tinted, 1.0h);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
