Shader "Custom/GlassShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,0.1)
        _BlurSize ("Blur Size", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        LOD 100

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _BlurSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 col = tex2D(_MainTex, uv) * _Color;

                // Simple blur effect (horizontal and vertical pass)
                col += tex2D(_MainTex, uv + float2(_BlurSize, 0.0)) * 0.25;
                col += tex2D(_MainTex, uv - float2(_BlurSize, 0.0)) * 0.25;
                col += tex2D(_MainTex, uv + float2(0.0, _BlurSize)) * 0.25;
                col += tex2D(_MainTex, uv - float2(0.0, _BlurSize)) * 0.25;

                return col;
            }
            ENDCG
        }
    }
}
