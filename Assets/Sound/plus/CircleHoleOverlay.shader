Shader "Unlit/CircleHoleOverlay"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 1)              // 검은 오버레이 색
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)       // 화면 기준 중심 (0~1)
        _Radius ("Radius", Float) = 0.5                     // 구멍 반지름
        _Smoothness ("Smoothness", Float) = 0.01            // 가장자리 부드럽게
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos        : SV_POSITION;
                float4 screenPos  : TEXCOORD0;   // 화면 좌표용
            };

            fixed4 _Color;
            float2 _Center;
            float  _Radius;
            float  _Smoothness;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);   // 클립 → 화면 좌표
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 0~1 화면 좌표
                float2 uv = (i.screenPos.xy / i.screenPos.w);

                // 중심 기준 좌표
                float2 p = uv - _Center;

                // 화면 비율 (가로 / 세로)
                float aspect = _ScreenParams.x / _ScreenParams.y;

                // ❗ 가로 방향 스케일을 늘려서 원 모양 맞추기
                p.x *= aspect;

                // 중심에서의 거리
                float d = length(p);

                // t = 0 → 투명(구멍 안), 1 → 불투명(검정)
                float t = smoothstep(_Radius, _Radius + _Smoothness, d);

                fixed4 col = _Color;
                col.a *= t;
                return col;
            }

            ENDCG
        }
    }
}
