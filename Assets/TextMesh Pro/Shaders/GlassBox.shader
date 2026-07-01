Shader "Custom/GlassBox"
{
    Properties
    {
        _Color ("Glass Color", Color) = (0.9, 0.95, 1, 0.2)
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Distortion ("Refraction Distortion", Range(0, 1)) = 0.1
        _FresnelPower ("Fresnel Power", Range(0, 5)) = 2.0
        _FresnelScale ("Fresnel Scale", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 300

        GrabPass { "_GrabTexture" }

        CGPROGRAM
        #pragma surface surf Glass alpha:fade
        #pragma target 3.0

        sampler2D _GrabTexture;
        sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _Color;
        float _Distortion;
        float _FresnelPower;
        float _FresnelScale;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 viewDir;
            float4 screenPos;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Normal map للانكسار
            fixed3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            
            // حساب انكسار الشاشة
            float2 offset = normal.xy * _Distortion;
            float2 uv = IN.screenPos.xy / IN.screenPos.w;
            uv += offset;
            
            fixed3 refractColor = tex2D(_GrabTexture, uv).rgb;
            
            // تأثير Fresnel
            float fresnel = pow(1.0 - saturate(dot(normalize(IN.viewDir), o.Normal)), _FresnelPower);
            fresnel *= _FresnelScale;
            
            // دمج اللون
            fixed3 finalColor = lerp(refractColor, _Color.rgb, _Color.a * fresnel);
            
            o.Albedo = finalColor;
            o.Normal = normal;
            o.Alpha = lerp(_Color.a * 0.3f, _Color.a, fresnel);
        }
        
        half4 LightingGlass (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * atten;
            c.a = s.Alpha;
            return c;
        }
        ENDCG
    }
    
    FallBack "Transparent/VertexLit"
}