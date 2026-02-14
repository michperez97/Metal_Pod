Shader "MetalPod/UI/GlowBorder"
{
    Properties
    {
        [HDR] _Color ("Glow Color", Color) = (1, 0.53, 0, 1)
        _GlowWidth ("Width", Range(0.01, 0.1)) = 0.02
        _PulseSpeed ("Pulse", Range(0, 5)) = 2
        _Intensity ("Intensity", Range(0, 3)) = 1.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }
        Cull Off
        ZWrite Off
        Blend SrcAlpha One

        Pass
        {
            Name "UIGlow"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _GlowWidth;
                float _PulseSpeed;
                float _Intensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float distToEdge = min(min(input.uv.x, 1.0 - input.uv.x), min(input.uv.y, 1.0 - input.uv.y));
                float border = 1.0 - smoothstep(_GlowWidth, _GlowWidth * 2.0, distToEdge);
                float pulse = 0.75 + 0.25 * sin(_Time.y * _PulseSpeed);

                half alpha = saturate(border * _Color.a * _Intensity * pulse * input.color.a);
                half3 glow = _Color.rgb * alpha;
                return half4(glow, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
