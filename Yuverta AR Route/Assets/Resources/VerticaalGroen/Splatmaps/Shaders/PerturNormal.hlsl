//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

float3x3 CotangentFrame_float(float3 N, float3 p, float2 uv)
{
    float3 dp1 = ddx(p);
    float3 dp2 = ddy(p);
    float2 duv1 = ddx(uv);
    float2 duv2 = ddy(uv);

    float3 dp2perp = cross(dp2, N);
    float3 dp1perp = cross(N, dp1);
    float3 T = dp2perp * duv1.x + dp1perp * duv2.x;
    float3 B = dp2perp * duv1.y + dp1perp * duv2.y;

    float invmax = rsqrt(max(dot(T,T), dot(B,B)));
    return float3x3(normalize(T * invmax), normalize(B * invmax), N);
}

void PerturbNormal_float(float3 normal, float2 offset, out float3 Out)
{
    float angle = length(offset) * 3.14159265359; // Simple rotation based on offset magnitude
    float3x3 rotationMatrix = float3x3(
        cos(angle), 0, sin(angle),
        0, 1, 0,
        -sin(angle), 0, cos(angle)
    );
    Out = mul(rotationMatrix, normal);
}

void perturbNormal_float( float3 localNormal, float3 N, float3 V, float2 uv, out float3 Out)
{
    // assume N, the interpolated vertex normal and 
    // V, the view vector (vertex to eye)
    float3x3 TBN = CotangentFrame_float( N, -V, uv );
    Out = normalize( mul( TBN, localNormal ) );
}

void BlendNormals_float(float3 baseNormal, float3 addedNormal, float blendFactor, out float3 Out)
{
    float3 blendedNormal = normalize(lerp(baseNormal, addedNormal, blendFactor));
    Out = blendedNormal;
}

void CalculateMask_float(float4 sdfValues, float edgeWidth, float clipDist, out float Out)
{
    float mask = smoothstep(clipDist - edgeWidth, clipDist + edgeWidth, max(max(sdfValues.x, sdfValues.y), max(sdfValues.z, sdfValues.w)));
    Out = mask;
}

#endif //MYHLSLINCLUDE_INCLUDED


