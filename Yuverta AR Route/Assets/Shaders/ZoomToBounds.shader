Shader "Custom/ZoomToBounds"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MinBound ("Min Bound", Vector) = (0, 0, 0, 0)
        _MaxBound ("Max Bound", Vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

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
            float4 _MinBound;
            float4 _MaxBound;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Calculate UV center and range
                float2 uvCenter = lerp(_MinBound.xy, _MaxBound.xy, 0.5);
                float2 uvRange = _MaxBound.xy - _MinBound.xy;

                // Calculate the zoom factor based on the bounds
                float zoomX = 1.0 / uvRange.x;
                float zoomY = 1.0 / uvRange.y;
                float zoom = min(zoomX, zoomY);

                // Apply scaling for zoom
                float2 scaledUV = (v.uv - uvCenter) * zoom + uvCenter;

                // Clamp UVs to min/max bounds
                o.uv = clamp(scaledUV, _MinBound.xy, _MaxBound.xy);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}