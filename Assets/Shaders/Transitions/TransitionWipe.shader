Shader "MetalPod/Transitions/Wipe"
{
    Properties
    {
        _Progress ("Progress", Range(0, 1)) = 0
        _Direction ("Direction", Vector) = (1, 0, 0, 0)
        _Softness ("Edge Softness", Range(0, 0.5)) = 0.1
        _Color ("Color", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            float _Progress;
            float2 _Direction;
            float _Softness;
            float4 _Color;

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = i.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Project UV onto direction to get 0..1 gradient
                float2 dir = normalize(_Direction);
                float2 centeredUV = i.uv - 0.5;
                float proj = dot(centeredUV, dir) + 0.5; // Remap to 0..1

                // Wipe edge
                float edge = smoothstep(_Progress - _Softness, _Progress + _Softness, proj);
                float alpha = 1.0 - edge;

                return half4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
