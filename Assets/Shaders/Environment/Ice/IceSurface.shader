Shader "MetalPod/Environment/Ice/IceSurface"
{
    Properties
    {
        [MainTexture] _MainTex ("Ice Texture", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _IceColor ("Ice Color", Color) = (0.7, 0.85, 1, 0.9)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.95
        _FresnelPower ("Fresnel Power", Range(1, 5)) = 3
        _SubsurfaceColor ("Subsurface Color", Color) = (0.3, 0.5, 0.8, 1)
        _SubsurfaceIntensity ("Subsurface", Range(0, 1)) = 0.3
        _ReflectionStrength ("Reflection", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _IceColor;
                float4 _SubsurfaceColor;
                float _Smoothness;
                float _FresnelPower;
                float _SubsurfaceIntensity;
                float _ReflectionStrength;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 iceTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).rgb;
                half3 normalTS = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv).xyz * 2.0h - 1.0h;
                half3 normalWS = normalize(input.normalWS + normalTS * 0.35h);
                half3 viewDir = normalize(input.viewDirWS);

                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half ndotl = saturate(dot(normalWS, lightDir));

                half fresnel = pow(1.0h - saturate(dot(normalWS, viewDir)), _FresnelPower);
                half3 baseColor = iceTex * _IceColor.rgb;

                half3 halfVector = normalize(lightDir + viewDir);
                half spec = pow(saturate(dot(normalWS, halfVector)), lerp(8.0h, 64.0h, _Smoothness)) * _Smoothness;
                half3 subsurface = _SubsurfaceColor.rgb * _SubsurfaceIntensity * (1.0h - ndotl);
                half3 reflection = fresnel * _ReflectionStrength.xxx;

                half3 lit = baseColor * (0.35h + ndotl * 0.65h) + subsurface + reflection + spec;
                half alpha = saturate(_IceColor.a + fresnel * 0.2h);
                return half4(lit, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
