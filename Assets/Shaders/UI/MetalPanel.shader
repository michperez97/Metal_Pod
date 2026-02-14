Shader "MetalPod/UI/MetalPanel"
{
    Properties
    {
        [MainTexture] _MainTex ("Metal Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0.18, 0.18, 0.18, 1)
        _RivetTex ("Rivet Overlay", 2D) = "black" {}
        _EdgeGlow ("Edge Glow", Range(0, 1)) = 0
        [HDR] _EdgeColor ("Edge Color", Color) = (1, 0.53, 0, 0.5)
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
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "UIForward"
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
                float4 _MainTex_ST;
                float4 _RivetTex_ST;
                float4 _Color;
                float4 _EdgeColor;
                float _EdgeGlow;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_RivetTex);
            SAMPLER(sampler_RivetTex);

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half rivet = SAMPLE_TEXTURE2D(_RivetTex, sampler_RivetTex, TRANSFORM_TEX(input.uv, _RivetTex)).r;

                float2 edge = abs(input.uv * 2.0 - 1.0);
                float edgeMask = smoothstep(0.78, 0.98, max(edge.x, edge.y));

                half3 panel = baseTex.rgb * _Color.rgb;
                panel += rivet * 0.12h;
                panel += _EdgeColor.rgb * (_EdgeGlow * edgeMask * _EdgeColor.a);

                half alpha = baseTex.a * _Color.a * input.color.a;
                return half4(panel * input.color.rgb, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
