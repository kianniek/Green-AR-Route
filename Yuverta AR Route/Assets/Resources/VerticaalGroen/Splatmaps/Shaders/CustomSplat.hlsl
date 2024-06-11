#pragma kernel CustomSplat_float

#include <UnityCG.cginc>
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

void CustomSplat_float(
    float2 uv2_SplatTex,
    float4 _SplatTex_TexelSize,
    float _SplatEdgeBumpWidth,
    float _SplatEdgeBump,
    float _Clip,
    float _SplatTileBump,
    Texture2D (_SplatTex),
    sampler2D (sampler_SplatTex),
    Texture2D(_SplatTileNormalTex),
    sampler2D(sampler_SplatTileNormalTex),
    Texture2D(_WorldTangentTex),
    sampler2D(sampler_WorldTangentTex),
    Texture2D(_WorldBinormalTex),
    sampler2D(sampler_WorldBinormalTex),
    Texture2D(_BumpTex),
    sampler2D(sampler_BumpTex),
    Texture2D(_MainTex),
    sampler2D(sampler_MainTex),
    float4 _Color,
    float _Metallic,
    float _Glossiness,
    float3 worldPos,
    float3 worldNormal,
    float3 viewDir,
    out float3 Albedo,
    out float3 Normal,
    out float Metallic,
    out float Smoothness,
    out float Alpha
) {
    float4 splatSDF = SAMPLE_TEXTURE2D(_SplatTex, sampler_SplatTex, uv2_SplatTex);
    float4 splatSDFx = SAMPLE_TEXTURE2D(_SplatTex, sampler_SplatTex, uv2_SplatTex + float2(_SplatTex_TexelSize.x, 0));
    float4 splatSDFy = SAMPLE_TEXTURE2D(_SplatTex, sampler_SplatTex, uv2_SplatTex + float2(0, _SplatTex_TexelSize.y));
    half splatDDX = length(ddx(uv2_SplatTex * _SplatTex_TexelSize.zw));
    half splatDDY = length(ddy(uv2_SplatTex * _SplatTex_TexelSize.zw));
    half clipDist = sqrt(splatDDX * splatDDX + splatDDY * splatDDY);
    half clipDistHard = max(clipDist * 0.01, 0.01);
    half clipDistSoft = 0.01 * _SplatEdgeBumpWidth;
    float4 splatMask = smoothstep((_Clip - 0.01) - clipDistHard, (_Clip - 0.01) + clipDistHard, splatSDF);
    float splatMaskTotal = max(max(splatMask.x, splatMask.y), max(splatMask.z, splatMask.w));
    float4 splatMaskInside = smoothstep(_Clip - clipDistSoft, _Clip + clipDistSoft, splatSDF);
    splatMaskInside = max(max(splatMaskInside.x, splatMaskInside.y), max(splatMaskInside.z, splatMaskInside.w));
    float4 offsetSplatX = splatSDF - splatSDFx;
    float4 offsetSplatY = splatSDF - splatSDFy;
    float2 offsetSplat = lerp(float2(offsetSplatX.x, offsetSplatY.x), float2(offsetSplatX.y, offsetSplatY.y), splatMask.y);
    offsetSplat = lerp(offsetSplat, float2(offsetSplatX.z, offsetSplatY.z), splatMask.z);
    offsetSplat = lerp(offsetSplat, float2(offsetSplatX.w, offsetSplatY.w), splatMask.w);
    offsetSplat = normalize(float3(offsetSplat, 0.0001)).xy;
    offsetSplat = offsetSplat * (1.0 - splatMaskInside) * _SplatEdgeBump;
    float2 splatTileNormalTex = SAMPLE_TEXTURE2D(_SplatTileNormalTex, sampler_SplatTileNormalTex, uv2_SplatTex * 10.0).xy;
    offsetSplat += (splatTileNormalTex.xy - 0.5) * _SplatTileBump * 0.2;
    float3 worldTangentTex = SAMPLE_TEXTURE2D(_WorldTangentTex, sampler_WorldTangentTex, uv2_SplatTex).xyz * 2.0 - 1.0;
    float3 worldBinormalTex = SAMPLE_TEXTURE2D(_WorldBinormalTex, sampler_WorldBinormalTex, uv2_SplatTex).xyz * 2.0 - 1.0;
    float3 offsetSplatWorld = offsetSplat.x * worldTangentTex + offsetSplat.y * worldBinormalTex;
    float3 worldTangent = normalize(cross(worldNormal, worldBinormalTex));
    float3 worldBinormal = normalize(cross(worldNormal, worldTangent));
    float2 offsetSplatLocal = float2(0, 0);
    offsetSplatLocal.x = dot(worldTangent, offsetSplatWorld);
    offsetSplatLocal.y = dot(worldBinormal, offsetSplatWorld);
    float4 normalMap = SAMPLE_TEXTURE2D(_BumpTex, sampler_BumpTex, uv2_SplatTex);
    normalMap.xyz = UnpackNormal(normalMap);
    float3 tanNormal = normalMap.xyz;
    tanNormal.xy += offsetSplatLocal * splatMaskTotal;
    tanNormal = normalize(tanNormal);
    float4 MainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv2_SplatTex);
    float4 c = MainTex * _Color;
    c.xyz = lerp(c.xyz, float3(1.0, 0.5, 0.0), splatMask.x);
    c.xyz = lerp(c.xyz, float3(1.0, 0.0, 0.0), splatMask.y);
    c.xyz = lerp(c.xyz, float3(0.0, 1.0, 0.0), splatMask.z);
    c.xyz = lerp(c.xyz, float3(0.0, 0.0, 1.0), splatMask.w);
    Albedo = c.rgb;
    Normal = tanNormal;
    Metallic = _Metallic;
    Smoothness = lerp(_Glossiness, 0.7, splatMaskTotal);
    Alpha = c.a;
}
