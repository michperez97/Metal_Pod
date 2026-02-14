Shader "MetalPod/Environment/Toxic/ElectricArc"
{
    Properties
    {
        [HDR] _Color ("Arc Color", Color) = (0.5, 0.7, 1, 1)
        [HDR] _CoreColor ("Core Color", Color) = (1, 1, 1, 1)
        _NoiseTex ("Noise", 2D) = "gray" {}
        _ArcSpeed ("Speed", Range(1, 20)) = 10
        _ArcThickness ("Thickness", Range(0.01, 0.2)) = 0.05
        _Intensity ("Intensity", Range(0, 5)) = 3
        _Active ("Active", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        Blend One One
        Cull Off
        ZWrite Off

        Pass
        {
            Name "ForwardAdd"
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
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST;
                float4 _Color;
                float4 _CoreColor;
                float _ArcSpeed;
                float _ArcThickness;
                float _Intensity;
                float _Active;
            CBUFFER_END

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _NoiseTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 noiseUV = float2(input.uv.x + _Time.y * _ArcSpeed * 0.05, input.uv.y);
                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;

                half centerLine = abs((input.uv.y - 0.5h) + (noise - 0.5h) * 0.3h);
                half core = 1.0h - smoothstep(0.0h, _ArcThickness, centerLine);
                half glow = 1.0h - smoothstep(_ArcThickness, _ArcThickness * 3.0h, centerLine);
                half flicker = 0.7h + 0.3h * sin(_Time.y * _ArcSpeed * 2.0h + noise * 10.0h);

                half active = saturate(_Active);
                half3 color = ((_CoreColor.rgb * core) + (_Color.rgb * glow)) * (_Intensity * flicker * active);
                half alpha = max(core, glow) * active;
                return half4(color * alpha, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
