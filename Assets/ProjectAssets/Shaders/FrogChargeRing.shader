Shader "Project/Frog/ChargeRing"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.05, 0.8, 0.25, 0.0)
        _RingColor ("Ring Color", Color) = (0.05, 1.0, 0.35, 0.85)
        _ProgressColor ("Progress Color", Color) = (1.0, 0.25, 0.15, 0.95)
        _InnerRadius ("Inner Radius", Range(0.0, 1.0)) = 0.68
        _OuterRadius ("Outer Radius", Range(0.0, 1.0)) = 0.98
        _Softness ("Softness", Range(0.001, 0.15)) = 0.03
        _Charge ("Charge", Range(0.0, 1.0)) = 0
        _Active ("Active", Range(0.0, 1.0)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _BaseColor;
            fixed4 _RingColor;
            fixed4 _ProgressColor;
            float _InnerRadius;
            float _OuterRadius;
            float _Softness;
            float _Charge;
            float _Active;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 p = i.uv * 2.0 - 1.0;
                float r = length(p);

                float inner = smoothstep(_InnerRadius - _Softness, _InnerRadius + _Softness, r);
                float outer = 1.0 - smoothstep(_OuterRadius - _Softness, _OuterRadius + _Softness, r);
                float ringMask = saturate(inner * outer);

                float angle = atan2(p.x, p.y);
                float angle01 = frac((angle + UNITY_PI) / (2.0 * UNITY_PI));
                float fillMask = step(angle01, saturate(_Charge));

                fixed4 ringColor = lerp(_RingColor, _ProgressColor, fillMask);
                fixed4 finalColor = lerp(_BaseColor, ringColor, ringMask);
                finalColor.a *= ringMask * saturate(_Active);

                return finalColor;
            }
            ENDCG
        }
    }
}
