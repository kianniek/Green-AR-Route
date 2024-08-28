Shader "TNTC/TexturePainter"
{
    Properties
    {
        _PainterColor ("Painter Color", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float3 _PainterPosition;
            float _Radius;
            float _Hardness;
            float _Strength;
            float4 _PainterColor;
            float _PrepareUV;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            float mask(float3 position, float3 center, float radius, float hardness)
            {
                float m = distance(center, position);
                return 1 - smoothstep(radius * hardness, radius, m);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                float4 uv = float4(0, 0, 0, 1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * float2(2, 2) - float2(1, 1));
                o.vertex = uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_PrepareUV > 0)
                {
                    return float4(0, 0, 0, 1);
                }

                float4 currentColor = tex2D(_MainTex, i.uv);
                float f = mask(i.worldPos, _PainterPosition, _Radius, _Hardness);
                float edge = f * _Strength;

                float goldenRatioConjugate = 0.618f;
                float treshold = 1 - goldenRatioConjugate; // 0.382

                // Check if the area is unpainted (black)
                bool isBlack = currentColor.r < treshold && currentColor.g < treshold && currentColor.b < treshold;
                bool isPaintingRed = _PainterColor.r > treshold;
                bool isPaintingGreen = _PainterColor.g > treshold;

                bool isRed = currentColor.r > treshold;
                bool isGreen = currentColor.g > treshold;

                if (isPaintingRed && !isGreen) // Red layer
                {
                    currentColor = lerp(currentColor, _PainterColor, edge);
                }

                if (isPaintingGreen && isRed) // Green layer
                {
                    currentColor = lerp(currentColor, _PainterColor, edge);
                }

                return currentColor; // No color change if conditions are not met
            }
            ENDCG
        }
    }
}