Shader "MetalPod/Transitions/Dissolve"
{
    Properties
    {
        _Progress ("Progress", Range(0, 1)) = 0
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _EdgeColor ("Edge Color", Color) = (1, 0.3, 0, 1)
        _BaseColor ("Base Color", Color) = (0, 0, 0, 1)
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.08
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

            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            float _Progress;
            float4 _EdgeColor;
            float4 _BaseColor;
            float _EdgeWidth;

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = i.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv).r;

                // Dissolve threshold
                float threshold = _Progress;
                float diff = noise - threshold;

                // Alpha: fully opaque where dissolved
                float alpha = step(diff, 0.0);

                // Edge glow
                float edgeMask = 1.0 - smoothstep(0.0, _EdgeWidth, abs(diff));

                half3 color = lerp(_BaseColor.rgb, _EdgeColor.rgb, edgeMask * alpha);

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
