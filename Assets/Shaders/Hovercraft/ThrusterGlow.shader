Shader "MetalPod/Hovercraft/ThrusterGlow"
{
    Properties
    {
        [MainTexture] _MainTex ("Particle Texture", 2D) = "white" {}
        [HDR] _Color ("Tint Color", Color) = (0, 0.87, 1, 1)
        _Intensity ("Intensity", Range(0, 5)) = 2
        _SoftParticleFade ("Soft Particle Fade", Range(0.01, 5)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
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
            #pragma multi_compile_fragment _ SOFTPARTICLES_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

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
                float4 screenPos : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Intensity;
                float _SoftParticleFade;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                output.screenPos = ComputeScreenPos(output.positionHCS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 color = tex * input.color * _Color;
                color.rgb *= _Intensity;

                #if defined(SOFTPARTICLES_ON)
                    float2 screenUV = input.screenPos.xy / input.screenPos.w;
                    float sceneDepth = SampleSceneDepth(screenUV);
                    float sceneEye = LinearEyeDepth(sceneDepth, _ZBufferParams);
                    float partEye = LinearEyeDepth(input.screenPos.z / input.screenPos.w, _ZBufferParams);
                    float fade = saturate((sceneEye - partEye) / max(_SoftParticleFade, 1e-4));
                    color.a *= fade;
                #endif

                color.rgb *= color.a;
                return color;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
