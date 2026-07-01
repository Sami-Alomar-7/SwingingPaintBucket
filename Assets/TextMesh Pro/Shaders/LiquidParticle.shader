// خامة جزيء سائل لامع (Wet Liquid) متوافقة مع URP
// مظهر سائل واقعي: إضاءة منتشرة + لمعة براقة (Specular) + حافة فرينل (Fresnel) للبلل.
// اللون يُمرَّر لكل جزيء عبر MaterialPropertyBlock بالاسم "_Color".
Shader "SwingingPaintBucket/LiquidParticle"
{
    Properties
    {
        _Color ("Color", Color) = (0.85, 0.1, 0.1, 1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.92
        _SpecIntensity ("Specular Intensity", Range(0,4)) = 1.6
        _FresnelPower ("Fresnel Power", Range(0.5,8)) = 4.0
        _FresnelStrength ("Fresnel Strength", Range(0,2)) = 0.7
        _Translucency ("Translucency", Range(0,2)) = 0.45
        _LightWrap ("Light Wrap", Range(0,1)) = 0.35
        // مطلوبة للتوافق مع URP Lit كخامة بديلة (Fallback)
        _BaseColor ("Base Color", Color) = (0.85, 0.1, 0.1, 1)
        _Metallic ("Metallic", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
            };

            // SRP Batcher compatible — القيم تُستبدل لكل جزيء عبر MaterialPropertyBlock
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _BaseColor;
                float  _Smoothness;
                float  _SpecIntensity;
                float  _FresnelPower;
                float  _FresnelStrength;
                float  _Translucency;
                float  _LightWrap;
                float  _Metallic;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(IN.normalOS);
                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = nrmInputs.normalWS;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(GetCameraPositionWS() - IN.positionWS);

                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);
                float3 H = normalize(L + V);

                float NdotH = saturate(dot(N, H));

                float3 baseCol = _Color.rgb;

                // التفاف الضوء (Light Wrap): يلين التدرج بين الضوء والظل فيبدو السطح طرياً كالسائل
                float rawNdotL = dot(N, L);
                float NdotL = saturate((rawNdotL + _LightWrap) / (1.0 + _LightWrap));

                // إضاءة محيطة (Ambient) مع أرضية دنيا حتى لا يصبح الجانب السفلي أسود تماماً
                float3 ambient = SampleSH(N) + 0.15;
                float3 diffuse = baseCol * (ambient + mainLight.color.rgb * NdotL);

                // لمعة براقة (Blinn-Phong) تعطي إحساس السطح المبلل
                float specPower = exp2(_Smoothness * 11.0) + 2.0;
                float spec = pow(NdotH, specPower) * _SpecIntensity;
                float3 specular = mainLight.color.rgb * spec;
                // حافة فرينل: تفتيح الحواف لإيهام انكسار الضوء على سطح السائل
                float fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower) * _FresnelStrength;
                float3 fresnelCol = lerp(baseCol, (float3)1.0, 0.6) * fresnel;

                // شفافية ضوئية (Subsurface/Back-scatter): ضوء يعبر السائل من خلفه نحو الكاميرا
                float backScatter = pow(saturate(dot(V, -L)), 3.0) * _Translucency;
                float3 translucency = baseCol * mainLight.color.rgb * backScatter;

                float3 color = diffuse + specular + fresnelCol + translucency;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }

    // خامة بديلة آمنة إن فشل ترجمة الشيدر المخصص لأي سبب
    Fallback "Universal Render Pipeline/Lit"
}