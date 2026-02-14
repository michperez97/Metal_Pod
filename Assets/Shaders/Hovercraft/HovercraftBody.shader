Shader "MetalPod/Hovercraft/HovercraftBody"
{
    Properties
    {
        [MainTexture] _MainTex ("Base Texture", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _MaskTex ("Color Mask", 2D) = "gray" {}
        _PrimaryColor ("Primary", Color) = (0.83, 0.63, 0.09, 1)
        _SecondaryColor ("Secondary", Color) = (0.17, 0.17, 0.17, 1)
        _AccentColor ("Accent", Color) = (1, 0.53, 0, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0.7
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        _DamageAmount ("Damage", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

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
                float4 _PrimaryColor;
                float4 _SecondaryColor;
                float4 _AccentColor;
                float _Metallic;
                float _Smoothness;
                float _DamageAmount;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

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

            half3 ApplyDamage(half3 color, half damageAmount)
            {
                half grayscale = dot(color, half3(0.299h, 0.587h, 0.114h));
                color = lerp(color, grayscale.xxx, damageAmount * 0.55h);
                color *= lerp(1.0h, 0.45h, damageAmount);
                return color;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).rgb;
                half3 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv).rgb;

                half remainder = saturate(1.0h - (mask.r + mask.g + mask.b));
                half3 tint = _PrimaryColor.rgb * mask.r + _SecondaryColor.rgb * mask.g + _AccentColor.rgb * mask.b + remainder.xxx;
                half3 albedo = baseTex * tint;
                albedo = ApplyDamage(albedo, saturate(_DamageAmount));

                half3 n = normalize(input.normalWS);
                half3 v = normalize(input.viewDirWS);
                Light mainLight = GetMainLight();
                half3 l = normalize(mainLight.direction);
                half3 h = normalize(l + v);

                half ndotl = saturate(dot(n, l));
                half ndoth = saturate(dot(n, h));

                half specPower = lerp(8.0h, 64.0h, _Smoothness);
                half specular = pow(ndoth, specPower) * lerp(0.12h, 1.0h, _Metallic);

                half3 lit = albedo * (0.25h + ndotl * 0.75h) + specular.xxx;
                return half4(lit, 1.0h);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
