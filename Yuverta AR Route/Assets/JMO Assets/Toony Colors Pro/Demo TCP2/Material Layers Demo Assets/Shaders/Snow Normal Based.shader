// Toony Colors Pro+Mobile 2
// (c) 2014-2023 Jean Moreno

Shader "Toony Colors Pro 2/Examples/Material Layers/Snow (Normal Based)"
{
	Properties
	{
		[TCP2HeaderHelp(Base)]
		_BaseColor ("Color", Color) = (1,1,1,1)
		[TCP2ColorNoAlpha] _HColor ("Highlight Color", Color) = (0.75,0.75,0.75,1)
		[TCP2ColorNoAlpha] _SColor ("Shadow Color", Color) = (0.2,0.2,0.2,1)
		_BaseMap ("Albedo", 2D) = "white" {}
		[TCP2Separator]

		[TCP2Header(Ramp Shading)]
		
		_RampThreshold ("Threshold", Range(0.01,1)) = 0.5
		_RampSmoothing ("Smoothing", Range(0.001,1)) = 0.5
		[TCP2Separator]

		[TCP2HeaderHelp(Reflections)]
		[Toggle(TCP2_REFLECTIONS)] _UseReflections ("Enable Reflections", Float) = 0
		[TCP2ColorNoAlpha] _ReflectionColor ("Color", Color) = (1,1,1,1)
		_ReflectionSmoothness ("Smoothness", Range(0,1)) = 0.5
		
		[NoScaleOffset] _Cube ("Reflection Cubemap", Cube) = "black" {}
		[TCP2ColorNoAlpha] _ReflectionCubemapColor ("Color", Color) = (1,1,1,1)
		_ReflectionCubemapRoughness ("Cubemap Roughness", Range(0,1)) = 0.5
		_PlanarNormalsInfluence ("Reflection Normal Influence", Range(0,1)) = 0.1
		[HideInInspector] _ReflectionTex ("Planar Reflection RenderTexture", 2D) = "white" {}
		_FresnelMin ("Fresnel Min", Range(0,2)) = 0
		_FresnelMax ("Fresnel Max", Range(0,2)) = 1.5
		[TCP2Separator]
		
		[TCP2HeaderHelp(Vertex Displacement)]
		_DisplacementTex ("Displacement Texture", 2D) = "black" {}
		 _DisplacementStrength ("Displacement Strength", Range(-1,1)) = 0.01
		[TCP2Separator]
		
		[TCP2HeaderHelp(Normal Mapping)]
		[Toggle(_NORMALMAP)] _UseNormalMap ("Enable Normal Mapping", Float) = 0
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_BumpScale ("Scale", Float) = 1
		[TCP2Separator]
		
		[TCP2Separator]
		[TCP2HeaderHelp(MATERIAL LAYERS)]

		[TCP2Separator]
		[TCP2Header(Snow)]
		_NormalSnowThreshold ("Normal Snow Threshold", Float) = 1
		_NormalThresholdBlending ("Normal Threshold Blending", Range(0,1)) = 0.5
		_SnowNoiseTexture ("Snow Noise Texture", 2D) = "gray" {}
		 _SnowNoiseStrength ("Snow Noise Strength", Range(0,1)) = 0.1
		_DisplacementTex_snow ("Displacement Texture", 2D) = "black" {}
		 _DisplacementStrength_snow ("Displacement Strength", Range(-1,1)) = 0.01
		_BumpScale_snow ("Scale", Float) = 1
		_Albedo_snow ("Albedo", Color) = (1,1,1,1)
		_BaseColor_snow ("Color", Color) = (1,1,1,1)
		_RampSmoothing_snow ("Smoothing", Range(0.001,1)) = 0.5
		[TCP2ColorNoAlpha] _SColor_snow ("Shadow Color", Color) = (0.2,0.2,0.2,1)

		[TCP2Separator]
		[TCP2Header(Rain)]
		[NoScaleOffset] _WetnessMap ("Wetness Map", 2D) = "black" {}
		 _NormalThreshold_rain ("Normal Threshold", Float) = 1
		_contrast_rain ("Contrast", Range(0,1)) = 0.5
		_ReflectionSmoothness_rain ("Smoothness", Range(0,1)) = 0.5
		_ReflectionCubemapRoughness_rain ("Cubemap Roughness", Range(0,1)) = 0.5
		[TCP2ColorNoAlpha] _ReflectionCubemapColor_rain ("Color", Color) = (1,1,1,1)
		_PlanarNormalsInfluence_rain ("Reflection Normal Influence", Range(0,1)) = 0.1
		_FresnelMin_rain ("Fresnel Min", Range(0,2)) = 0
		_FresnelMax_rain ("Fresnel Max", Range(0,2)) = 1.5
		[TCP2ColorNoAlpha] _ReflectionColor_rain ("Color", Color) = (1,1,1,1)

		// Avoid compile error if the properties are ending with a drawer
		[HideInInspector] __dummy__ ("unused", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"RenderPipeline" = "UniversalPipeline"
			"RenderType"="Opaque"
		}

		HLSLINCLUDE
		#define fixed half
		#define fixed2 half2
		#define fixed3 half3
		#define fixed4 half4

		#if UNITY_VERSION >= 202020
			#define URP_10_OR_NEWER
		#endif
		#if UNITY_VERSION >= 202120
			#define URP_12_OR_NEWER
		#endif
		#if UNITY_VERSION >= 202220
			#define URP_14_OR_NEWER
		#endif

		// Texture/Sampler abstraction
		#define TCP2_TEX2D_WITH_SAMPLER(tex)						TEXTURE2D(tex); SAMPLER(sampler##tex)
		#define TCP2_TEX2D_NO_SAMPLER(tex)							TEXTURE2D(tex)
		#define TCP2_TEX2D_SAMPLE(tex, samplertex, coord)			SAMPLE_TEXTURE2D(tex, sampler##samplertex, coord)
		#define TCP2_TEX2D_SAMPLE_LOD(tex, samplertex, coord, lod)	SAMPLE_TEXTURE2D_LOD(tex, sampler##samplertex, coord, lod)

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

		// Uniforms

		// Shader Properties
		TCP2_TEX2D_WITH_SAMPLER(_DisplacementTex);
		TCP2_TEX2D_WITH_SAMPLER(_DisplacementTex_snow);
		TCP2_TEX2D_WITH_SAMPLER(_BumpMap);
		TCP2_TEX2D_WITH_SAMPLER(_BaseMap);

		TCP2_TEX2D_WITH_SAMPLER(_SnowNoiseTexture);
		TCP2_TEX2D_WITH_SAMPLER(_WetnessMap);
		samplerCUBE _Cube;
		sampler2D _ReflectionTex;

		CBUFFER_START(UnityPerMaterial)
			
			// Shader Properties
			float4 _DisplacementTex_ST;
			float _DisplacementStrength;
			float4 _DisplacementTex_snow_ST;
			float _DisplacementStrength_snow;
			float4 _BumpMap_ST;
			float _BumpScale;
			float _BumpScale_snow;
			float4 _BaseMap_ST;
			fixed4 _Albedo_snow;
			fixed4 _BaseColor;
			fixed4 _BaseColor_snow;
			float _RampThreshold;
			float _RampSmoothing;
			float _RampSmoothing_snow;
			fixed4 _SColor;
			fixed4 _SColor_snow;
			fixed4 _HColor;
			float _ReflectionSmoothness;
			float _ReflectionSmoothness_rain;
			float _ReflectionCubemapRoughness;
			float _ReflectionCubemapRoughness_rain;
			fixed4 _ReflectionCubemapColor;
			fixed4 _ReflectionCubemapColor_rain;
			float _PlanarNormalsInfluence;
			float _PlanarNormalsInfluence_rain;
			float _FresnelMin;
			float _FresnelMin_rain;
			float _FresnelMax;
			float _FresnelMax_rain;
			fixed4 _ReflectionColor;
			fixed4 _ReflectionColor_rain;
			float _NormalSnowThreshold;
			float _NormalThresholdBlending;
			float4 _SnowNoiseTexture_ST;
			float _SnowNoiseStrength;
			float _NormalThreshold_rain;
			float _contrast_rain;
		CBUFFER_END

		#if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_DOTS_INSTANCING_ENABLED)
			#define unity_ObjectToWorld UNITY_MATRIX_M
			#define unity_WorldToObject UNITY_MATRIX_I_M
		#endif

		// Built-in renderer (CG) to SRP (HLSL) bindings
		#define UnityObjectToClipPos TransformObjectToHClip
		#define _WorldSpaceLightPos0 _MainLightPosition
		
		ENDHLSL

		Pass
		{
			Name "Main"
			Tags
			{
				"LightMode"="UniversalForward"
			}

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard SRP library
			// All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 3.0

			// -------------------------------------
			// Material keywords
			#pragma multi_compile _RECEIVE_SHADOWS_OFF

			// -------------------------------------
			// Universal Render Pipeline keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK

			// -------------------------------------

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			#pragma vertex Vertex
			#pragma fragment Fragment

			//--------------------------------------
			// Toony Colors Pro 2 keywords
			#pragma shader_feature_local_fragment TCP2_REFLECTIONS
			#pragma shader_feature_local _NORMALMAP

			// vertex input
			struct Attributes
			{
				float4 vertex       : POSITION;
				float3 normal       : NORMAL;
				float4 tangent      : TANGENT;
				float4 texcoord0 : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			// vertex output / fragment input
			struct Varyings
			{
				float4 positionCS     : SV_POSITION;
				float3 normal         : NORMAL;
				float4 worldPosAndFog : TEXCOORD0;
			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord    : TEXCOORD1; // compute shadow coord per-vertex for the main light
			#endif
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				half3 vertexLights : TEXCOORD2;
			#endif
				float4 screenPosition : TEXCOORD3;
				float3 pack1 : TEXCOORD4; /* pack1.xyz = tangent */
				float3 pack2 : TEXCOORD5; /* pack2.xyz = bitangent */
				float2 pack3 : TEXCOORD6; /* pack3.xy = texcoord0 */
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings Vertex(Attributes input)
			{
				Varyings output = (Varyings)0;

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 worldNormalUv = mul(unity_ObjectToWorld, float4(input.normal, 1.0)).xyz;

				// Texture Coordinates
				output.pack3.xy = input.texcoord0.xy;
				// Sampled in Custom Code
				float4 imp_100 = _SnowNoiseStrength;
				// Shader Properties Sampling
				float3 __vertexDisplacement = ( input.normal.xyz * TCP2_TEX2D_SAMPLE_LOD(_DisplacementTex, _DisplacementTex, output.pack3.xy * _DisplacementTex_ST.xy + _DisplacementTex_ST.zw, 0).rgb * _DisplacementStrength );
				float3 __vertexDisplacement_snow = ( input.normal.xyz * TCP2_TEX2D_SAMPLE_LOD(_DisplacementTex_snow, _DisplacementTex_snow, output.pack3.xy * _DisplacementTex_snow_ST.xy + _DisplacementTex_snow_ST.zw, 0).rgb * _DisplacementStrength_snow );
				float __layer_snow = saturate(  worldNormalUv.y + _NormalSnowThreshold );
				float __contrast_snow = ( _NormalThresholdBlending );
				float __noise_snow = (  saturate( TCP2_TEX2D_SAMPLE_LOD(_SnowNoiseTexture, _SnowNoiseTexture, output.pack3.xy * _SnowNoiseTexture_ST.xy + _SnowNoiseTexture_ST.zw, -1).r * imp_100 ) - imp_100 / 2.0 );

				// Material Layers Blending
				 __vertexDisplacement = __vertexDisplacement + __vertexDisplacement_snow * saturate(((__layer_snow + __noise_snow) + (__contrast_snow * 0.5 - 0.5)) / __contrast_snow);

				input.vertex.xyz += __vertexDisplacement;
				float3 worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);
			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				output.shadowCoord = GetShadowCoord(vertexInput);
			#endif
				float4 clipPos = vertexInput.positionCS;

				float4 screenPos = ComputeScreenPos(clipPos);
				output.screenPosition.xyzw = screenPos;

				VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normal, input.tangent);
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				// Vertex lighting
				output.vertexLights = VertexLighting(vertexInput.positionWS, vertexNormalInput.normalWS);
			#endif

				// world position
				output.worldPosAndFog = float4(vertexInput.positionWS.xyz, 0);

				// normal
				output.normal = normalize(vertexNormalInput.normalWS);

				// tangent
				output.pack1.xyz = vertexNormalInput.tangentWS;
				output.pack2.xyz = vertexNormalInput.bitangentWS;

				// clip position
				output.positionCS = vertexInput.positionCS;

				return output;
			}

			half4 Fragment(Varyings input
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				float3 positionWS = input.worldPosAndFog.xyz;
				float3 normalWS = normalize(input.normal);
				half3 viewDirWS = SafeNormalize(GetCameraPositionWS() - positionWS);
				half3 tangentWS = input.pack1.xyz;
				half3 bitangentWS = input.pack2.xyz;
				#if defined(_NORMALMAP)
				half3x3 tangentToWorldMatrix = half3x3(tangentWS.xyz, bitangentWS.xyz, normalWS.xyz);
				#endif

				// Sampled in Custom Code
				float4 imp_101 = _SnowNoiseStrength;
				// Shader Properties Sampling
				float4 __normalMap = ( TCP2_TEX2D_SAMPLE(_BumpMap, _BumpMap, input.pack3.xy * _BumpMap_ST.xy + _BumpMap_ST.zw).rgba );
				float __bumpScale = ( _BumpScale );
				float __bumpScale_snow = ( _BumpScale_snow );
				float4 __albedo = ( TCP2_TEX2D_SAMPLE(_BaseMap, _BaseMap, input.pack3.xy * _BaseMap_ST.xy + _BaseMap_ST.zw).rgba );
				float4 __albedo_snow = ( _Albedo_snow.rgba );
				float4 __mainColor = ( _BaseColor.rgba );
				float4 __mainColor_snow = ( _BaseColor_snow.rgba );
				float __alpha = ( __albedo.a * __mainColor.a );
				float __ambientIntensity = ( 1.0 );
				float __rampThreshold = ( _RampThreshold );
				float __rampSmoothing = ( _RampSmoothing );
				float __rampSmoothing_snow = ( _RampSmoothing_snow );
				float3 __shadowColor = ( _SColor.rgb );
				float3 __shadowColor_snow = ( _SColor_snow.rgb );
				float3 __highlightColor = ( _HColor.rgb );
				float __reflectionSmoothness = ( _ReflectionSmoothness );
				float __reflectionSmoothness_rain = ( _ReflectionSmoothness_rain );
				float __reflectionCubemapRoughness = ( _ReflectionCubemapRoughness );
				float __reflectionCubemapRoughness_rain = ( _ReflectionCubemapRoughness_rain );
				float3 __reflectionCubemapColor = ( _ReflectionCubemapColor.rgb );
				float3 __reflectionCubemapColor_rain = ( _ReflectionCubemapColor_rain.rgb );
				float __planarNormalsInfluence = ( _PlanarNormalsInfluence );
				float __planarNormalsInfluence_rain = ( _PlanarNormalsInfluence_rain );
				float __fresnelMin = ( _FresnelMin );
				float __fresnelMin_rain = ( _FresnelMin_rain );
				float __fresnelMax = ( _FresnelMax );
				float __fresnelMax_rain = ( _FresnelMax_rain );
				float3 __reflectionColor = ( _ReflectionColor.rgb );
				float3 __reflectionColor_rain = ( _ReflectionColor_rain.rgb );
				float __layer_snow = saturate(  normalWS.y + _NormalSnowThreshold );
				float __contrast_snow = ( _NormalThresholdBlending );
				float __noise_snow = (  saturate( TCP2_TEX2D_SAMPLE(_SnowNoiseTexture, _SnowNoiseTexture, input.pack3.xy * _SnowNoiseTexture_ST.xy + _SnowNoiseTexture_ST.zw).r * imp_101 ) - imp_101 / 2.0 );
				float __layer_rain = saturate( TCP2_TEX2D_SAMPLE(_WetnessMap, _WetnessMap, input.pack3.xy).r * _NormalThreshold_rain );
				float __contrast_rain = ( _contrast_rain );

				// Material Layers Blending
				 __bumpScale = lerp(__bumpScale, __bumpScale_snow, saturate(((__layer_snow + __noise_snow) + (__contrast_snow * 0.5 - 0.5)) / __contrast_snow));
				 __albedo = lerp(__albedo, __albedo_snow, saturate(((__layer_snow + __noise_snow) + (__contrast_snow * 0.5 - 0.5)) / __contrast_snow));
				 __mainColor = lerp(__mainColor, __mainColor_snow, saturate(((__layer_snow + __noise_snow) + (__contrast_snow * 0.5 - 0.5)) / __contrast_snow));
				 __rampSmoothing = lerp(__rampSmoothing, __rampSmoothing_snow, saturate(((__layer_snow + __noise_snow) + (__contrast_snow * 0.5 - 0.5)) / __contrast_snow));
				 __shadowColor = lerp(__shadowColor, __shadowColor_snow, saturate(((__layer_snow + __noise_snow) + (__contrast_snow * 0.5 - 0.5)) / __contrast_snow));
				 __reflectionSmoothness = __reflectionSmoothness + __reflectionSmoothness_rain * saturate((__layer_rain + (__contrast_rain * 0.5 - 0.5)) / __contrast_rain);
				 __reflectionCubemapRoughness = __reflectionCubemapRoughness + __reflectionCubemapRoughness_rain * saturate((__layer_rain + (__contrast_rain * 0.5 - 0.5)) / __contrast_rain);
				 __reflectionCubemapColor = __reflectionCubemapColor + __reflectionCubemapColor_rain * saturate((__layer_rain + (__contrast_rain * 0.5 - 0.5)) / __contrast_rain);
				 __planarNormalsInfluence = __planarNormalsInfluence + __planarNormalsInfluence_rain * saturate((__layer_rain + (__contrast_rain * 0.5 - 0.5)) / __contrast_rain);
				 __fresnelMin = __fresnelMin + __fresnelMin_rain * saturate((__layer_rain + (__contrast_rain * 0.5 - 0.5)) / __contrast_rain);
				 __fresnelMax = __fresnelMax + __fresnelMax_rain * saturate((__layer_rain + (__contrast_rain * 0.5 - 0.5)) / __contrast_rain);
				 __reflectionColor = __reflectionColor + __reflectionColor_rain * saturate((__layer_rain + (__contrast_rain * 0.5 - 0.5)) / __contrast_rain);

				#if defined(_NORMALMAP)
				half4 normalMap = __normalMap;
				half3 normalTS = UnpackNormalScale(normalMap, __bumpScale);
					#if defined(_NORMALMAP)
				normalWS = normalize( mul(normalTS, tangentToWorldMatrix) );
					#endif
				#endif

				half ndv = abs(dot(viewDirWS, normalWS));
				half ndvRaw = ndv;

				// main texture
				half3 albedo = __albedo.rgb;
				half alpha = __alpha;

				half3 emission = half3(0,0,0);
				
				albedo *= __mainColor.rgb;

				// main light: direction, color, distanceAttenuation, shadowAttenuation
			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord = input.shadowCoord;
			#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
				float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
			#else
				float4 shadowCoord = float4(0, 0, 0, 0);
			#endif

			#if defined(URP_10_OR_NEWER)
				#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
					half4 shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
				#elif !defined (LIGHTMAP_ON)
					half4 shadowMask = unity_ProbesOcclusion;
				#else
					half4 shadowMask = half4(1, 1, 1, 1);
				#endif

				Light mainLight = GetMainLight(shadowCoord, positionWS, shadowMask);
			#else
				Light mainLight = GetMainLight(shadowCoord);
			#endif

				// ambient or lightmap
				// Samples SH fully per-pixel. SampleSHVertex and SampleSHPixel functions
				// are also defined in case you want to sample some terms per-vertex.
				half3 bakedGI = SampleSH(normalWS);
				half occlusion = 1;

				half3 indirectDiffuse = bakedGI;
				indirectDiffuse *= occlusion * albedo * __ambientIntensity;

				half3 lightDir = mainLight.direction;
				half3 lightColor = mainLight.color.rgb;

				half atten = mainLight.shadowAttenuation * mainLight.distanceAttenuation;

				half ndl = dot(normalWS, lightDir);
				half3 ramp;
				
				half rampThreshold = __rampThreshold;
				half rampSmooth = __rampSmoothing * 0.5;
				ndl = saturate(ndl);
				ramp = smoothstep(rampThreshold - rampSmooth, rampThreshold + rampSmooth, ndl);

				// apply attenuation
				ramp *= atten;

				half3 color = half3(0,0,0);
				half3 accumulatedRamp = ramp * max(lightColor.r, max(lightColor.g, lightColor.b));
				half3 accumulatedColors = ramp * lightColor.rgb;

				// Additional lights loop
			#ifdef _ADDITIONAL_LIGHTS
				uint pixelLightCount = GetAdditionalLightsCount();

				LIGHT_LOOP_BEGIN(pixelLightCount)
				{
					#if defined(URP_10_OR_NEWER)
						Light light = GetAdditionalLight(lightIndex, positionWS, shadowMask);
					#else
						Light light = GetAdditionalLight(lightIndex, positionWS);
					#endif
					half atten = light.shadowAttenuation * light.distanceAttenuation;

					#if defined(_LIGHT_LAYERS)
						half3 lightDir = half3(0, 1, 0);
						half3 lightColor = half3(0, 0, 0);
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						{
							lightColor = light.color.rgb;
							lightDir = light.direction;
						}
					#else
						half3 lightColor = light.color.rgb;
						half3 lightDir = light.direction;
					#endif

					half ndl = dot(normalWS, lightDir);
					half3 ramp;
					
					ndl = saturate(ndl);
					ramp = smoothstep(rampThreshold - rampSmooth, rampThreshold + rampSmooth, ndl);

					// apply attenuation (shadowmaps & point/spot lights attenuation)
					ramp *= atten;

					accumulatedRamp += ramp * max(lightColor.r, max(lightColor.g, lightColor.b));
					accumulatedColors += ramp * lightColor.rgb;

				}
				LIGHT_LOOP_END
			#endif
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				color += input.vertexLights * albedo;
			#endif

				accumulatedRamp = saturate(accumulatedRamp);
				half3 shadowColor = (1 - accumulatedRamp.rgb) * __shadowColor;
				accumulatedRamp = accumulatedColors.rgb * __highlightColor + shadowColor;
				color += albedo * accumulatedRamp;

				// apply ambient
				color += indirectDiffuse;

				#if defined(TCP2_REFLECTIONS)
				half3 reflections = half3(0, 0, 0);

				// World reflection
				half reflectionRoughness = 1 - __reflectionSmoothness;
				half3 reflectVector = reflect(-viewDirWS, normalWS);
				
				half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, reflectionRoughness, occlusion);
				half reflectionRoughness4 = max(pow(reflectionRoughness, 4), 6.103515625e-5);
				float surfaceReductionRefl = 1.0 / (reflectionRoughness4 + 1.0);
				reflections += indirectSpecular * surfaceReductionRefl;

				#if defined(TCP2_REFLECTIONS)
				// Reflection cubemap
				reflections.rgb += texCUBElod(_Cube, half4(reflectVector.xyz, __reflectionCubemapRoughness * 10.0)).rgb;
				reflections.rgb *= __reflectionCubemapColor;
				#if defined(_NORMALMAP)
				float2 planarReflectionCoords = (input.screenPosition.xyzw.xy + (normalTS.xy * __planarNormalsInfluence)) / input.screenPosition.xyzw.w;
				reflections.rgb += tex2D(_ReflectionTex, planarReflectionCoords).rgb;
				#else
				reflections.rgb += tex2D(_ReflectionTex, input.screenPosition.xyzw.xy / input.screenPosition.xyzw.w).rgb;
				#endif
				#endif
				half fresnelMin = __fresnelMin;
				half fresnelMax = __fresnelMax;
				half fresnelTerm = smoothstep(fresnelMin, fresnelMax, 1 - ndvRaw);
				reflections *= fresnelTerm;

				reflections *= __reflectionColor;
				color.rgb += reflections;
				#endif

				color += emission;

				return half4(color, alpha);
			}
			ENDHLSL
		}

		// Depth & Shadow Caster Passes
		HLSLINCLUDE

		#if defined(SHADOW_CASTER_PASS) || defined(DEPTH_ONLY_PASS)

			#define fixed half
			#define fixed2 half2
			#define fixed3 half3
			#define fixed4 half4

			float3 _LightDirection;
			float3 _LightPosition;

			struct Attributes
			{
				float4 vertex   : POSITION;
				float3 normal   : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS     : SV_POSITION;
				float3 normal         : NORMAL;
				float4 screenPosition : TEXCOORD1;
				float3 pack1 : TEXCOORD2; /* pack1.xyz = positionWS */
				float2 pack2 : TEXCOORD3; /* pack2.xy = texcoord0 */
			#if defined(DEPTH_ONLY_PASS)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			#endif
			};

			float4 GetShadowPositionHClip(Attributes input)
			{
				float3 positionWS = TransformObjectToWorld(input.vertex.xyz);
				float3 normalWS = TransformObjectToWorldNormal(input.normal);

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif
				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

				#if UNITY_REVERSED_Z
					positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#else
					positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#endif

				return positionCS;
			}

			Varyings ShadowDepthPassVertex(Attributes input)
			{
				Varyings output = (Varyings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				#if defined(DEPTH_ONLY_PASS)
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				#endif

				float3 worldNormalUv = mul(unity_ObjectToWorld, float4(input.normal, 1.0)).xyz;

				// Texture Coordinates
				output.pack2.xy = input.texcoord0.xy;
				// Sampled in Custom Code
				float4 imp_102 = _SnowNoiseStrength;
				// Shader Properties Sampling
				float3 __vertexDisplacement = ( input.normal.xyz * TCP2_TEX2D_SAMPLE_LOD(_DisplacementTex, _DisplacementTex, output.pack2.xy * _DisplacementTex_ST.xy + _DisplacementTex_ST.zw, 0).rgb * _DisplacementStrength );
				float3 __vertexDisplacement_snow = ( input.normal.xyz * TCP2_TEX2D_SAMPLE_LOD(_DisplacementTex_snow, _DisplacementTex_snow, output.pack2.xy * _DisplacementTex_snow_ST.xy + _DisplacementTex_snow_ST.zw, 0).rgb * _DisplacementStrength_snow );
				float __layer_snow = saturate(  worldNormalUv.y + _NormalSnowThreshold );
				float __contrast_snow = ( _NormalThresholdBlending );
				float __noise_snow = (  saturate( TCP2_TEX2D_SAMPLE_LOD(_SnowNoiseTexture, _SnowNoiseTexture, output.pack2.xy * _SnowNoiseTexture_ST.xy + _SnowNoiseTexture_ST.zw, -1).r * imp_102 ) - imp_102 / 2.0 );

				// Material Layers Blending
				 __vertexDisplacement = __vertexDisplacement + __vertexDisplacement_snow * saturate(((__layer_snow + __noise_snow) + (__contrast_snow * 0.5 - 0.5)) / __contrast_snow);

				input.vertex.xyz += __vertexDisplacement;
				float3 worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);

				// Screen Space UV
				float4 screenPos = ComputeScreenPos(vertexInput.positionCS);
				output.screenPosition.xyzw = screenPos;
				output.normal = normalize(worldNormalUv);
				output.pack1.xyz = vertexInput.positionWS;

				#if defined(DEPTH_ONLY_PASS)
					output.positionCS = TransformObjectToHClip(input.vertex.xyz);
				#elif defined(SHADOW_CASTER_PASS)
					output.positionCS = GetShadowPositionHClip(input);
				#else
					output.positionCS = float4(0,0,0,0);
				#endif

				return output;
			}

			half4 ShadowDepthPassFragment(
				Varyings input
			) : SV_TARGET
			{
				#if defined(DEPTH_ONLY_PASS)
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				#endif

				float3 positionWS = input.pack1.xyz;
				float3 normalWS = normalize(input.normal);

				// Sampled in Custom Code
				float4 imp_103 = _SnowNoiseStrength;
				// Shader Properties Sampling
				float4 __albedo = ( TCP2_TEX2D_SAMPLE(_BaseMap, _BaseMap, input.pack2.xy * _BaseMap_ST.xy + _BaseMap_ST.zw).rgba );
				float4 __albedo_snow = ( _Albedo_snow.rgba );
				float4 __mainColor = ( _BaseColor.rgba );
				float4 __mainColor_snow = ( _BaseColor_snow.rgba );
				float __alpha = ( __albedo.a * __mainColor.a );
				float __layer_snow = saturate(  normalWS.y + _NormalSnowThreshold );
				float __contrast_snow = ( _NormalThresholdBlending );
				float __noise_snow = (  saturate( TCP2_TEX2D_SAMPLE(_SnowNoiseTexture, _SnowNoiseTexture, input.pack2.xy * _SnowNoiseTexture_ST.xy + _SnowNoiseTexture_ST.zw).r * imp_103 ) - imp_103 / 2.0 );

				// Material Layers Blending
				 __albedo = lerp(__albedo, __albedo_snow, saturate(((__layer_snow + __noise_snow) + (__contrast_snow * 0.5 - 0.5)) / __contrast_snow));
				 __mainColor = lerp(__mainColor, __mainColor_snow, saturate(((__layer_snow + __noise_snow) + (__contrast_snow * 0.5 - 0.5)) / __contrast_snow));

				half3 viewDirWS = SafeNormalize(GetCameraPositionWS() - positionWS);
				half ndv = abs(dot(viewDirWS, normalWS));
				half ndvRaw = ndv;

				half3 albedo = half3(1,1,1);
				half alpha = __alpha;
				half3 emission = half3(0,0,0);

				return 0;
			}

		#endif
		ENDHLSL

		Pass
		{
			Name "DepthOnly"
			Tags
			{
				"LightMode" = "DepthOnly"
			}

			ZWrite On
			ColorMask 0

			HLSLPROGRAM

			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			// using simple #define doesn't work, we have to use this instead
			#pragma multi_compile DEPTH_ONLY_PASS

			#pragma vertex ShadowDepthPassVertex
			#pragma fragment ShadowDepthPassFragment

			ENDHLSL
		}

	}

	FallBack "Hidden/InternalErrorShader"
	CustomEditor "ToonyColorsPro.ShaderGenerator.MaterialInspector_SG2"
}

/* TCP_DATA u config(ver:"2.9.10";unity:"2022.3.22f1";tmplt:"SG2_Template_URP";features:list["UNITY_5_4","UNITY_5_5","UNITY_5_6","UNITY_2017_1","UNITY_2018_1","UNITY_2018_2","UNITY_2018_3","BUMP","BUMP_SCALE","UNITY_2019_1","UNITY_2019_2","UNITY_2019_3","UNITY_2019_4","UNITY_2020_1","UNITY_2021_1","UNITY_2021_2","UNITY_2022_2","DISABLE_SHADOW_RECEIVING","DISABLE_SHADOW_CASTING","BUMP_SHADER_FEATURE","GLOSSY_REFLECTIONS","PLANAR_REFLECTION","REFLECTION_CUBEMAP","REFL_ROUGH","REFLECTION_FRESNEL","REFLECTION_SHADER_FEATURE","VERTEX_DISPLACEMENT","TEMPLATE_LWRP"];flags:list[];flags_extra:dict[];keywords:dict[RENDER_TYPE="Opaque",RampTextureDrawer="[TCP2Gradient]",RampTextureLabel="Ramp Texture",SHADER_TARGET="3.0"];shaderProperties:list[sp(name:"Albedo";imps:list[imp_mp_texture(uto:True;tov:"";tov_lbl:"";gto:False;sbt:False;scr:False;scv:"";scv_lbl:"";gsc:False;roff:False;goff:False;sin_anm:False;sin_anmv:"";sin_anmv_lbl:"";gsin:False;notile:False;triplanar_local:False;def:"white";locked_uv:False;uv:0;cc:4;chan:"RGBA";mip:-1;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";tpln_scale:1;uv_shaderproperty:__NULL__;uv_cmp:__NULL__;sep_sampler:__NULL__;prop:"_BaseMap";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"c5c51c70-cbf2-433d-a60b-68ad34702603";op:Multiply;lbl:"Albedo";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list["1f64b1"];unlocked:list["1f64b1"];layer_blend:dict[1f64b1=LinearInterpolation];custom_blend:dict[1f64b1="lerp(a, b, s)"];clones:dict[1f64b1=sp(name:"Albedo_1f64b1";imps:list[imp_mp_color(def:RGBA(1, 1, 1, 1);hdr:False;cc:4;chan:"RGBA";prop:"_Albedo_1f64b1";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"1a5d445c-f75c-4e7c-a739-3f8b4353f1c9";op:Multiply;lbl:"Albedo";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:True)];isClone:False),sp(name:"Main Color";imps:list[imp_mp_color(def:RGBA(1, 1, 1, 1);hdr:False;cc:4;chan:"RGBA";prop:"_BaseColor";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"f2703281-d9c3-4e4a-9a2e-eed698306612";op:Multiply;lbl:"Color";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list["1f64b1"];unlocked:list[];layer_blend:dict[1f64b1=LinearInterpolation];custom_blend:dict[1f64b1="lerp(a, b, s)"];clones:dict[];isClone:False),,,,sp(name:"Ramp Smoothing";imps:list[imp_mp_range(def:0.5;min:0.001;max:1;prop:"_RampSmoothing";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"8413f877-519a-4ebd-95c1-7061a38c4160";op:Multiply;lbl:"Smoothing";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list["1f64b1"];unlocked:list[];layer_blend:dict[1f64b1=LinearInterpolation];custom_blend:dict[1f64b1="lerp(a, b, s)"];clones:dict[];isClone:False),,sp(name:"Shadow Color";imps:list[imp_mp_color(def:RGBA(0.2, 0.2, 0.2, 1);hdr:False;cc:3;chan:"RGB";prop:"_SColor";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"7d7a8d7d-38b4-4c3c-b81d-1cf098b230d7";op:Multiply;lbl:"Shadow Color";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list["1f64b1"];unlocked:list[];layer_blend:dict[1f64b1=LinearInterpolation];custom_blend:dict[1f64b1="lerp(a, b, s)"];clones:dict[];isClone:False),sp(name:"Reflection Color";imps:list[imp_mp_color(def:RGBA(1, 1, 1, 1);hdr:False;cc:3;chan:"RGB";prop:"_ReflectionColor";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"b5eed70e-56df-426e-8c5e-e4515ddb0f86";op:Multiply;lbl:"Color";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list["56201e"];unlocked:list[];layer_blend:dict[56201e=Add];custom_blend:dict[56201e="lerp(a, b, s)"];clones:dict[];isClone:False),sp(name:"Reflection Smoothness";imps:list[imp_mp_range(def:0.5;min:0;max:1;prop:"_ReflectionSmoothness";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"c92da682-978b-4d09-8d8c-6d56cd4328ac";op:Multiply;lbl:"Smoothness";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list["56201e"];unlocked:list[];layer_blend:dict[56201e=Add];custom_blend:dict[56201e="lerp(a, b, s)"];clones:dict[];isClone:False),sp(name:"Reflection Cubemap Color";imps:list[imp_mp_color(def:RGBA(1, 1, 1, 1);hdr:False;cc:3;chan:"RGB";prop:"_ReflectionCubemapColor";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"9079ad3a-da81-4a29-aa94-5675e55afbf4";op:Multiply;lbl:"Color";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list["56201e"];unlocked:list[];layer_blend:dict[56201e=Add];custom_blend:dict[56201e="lerp(a, b, s)"];clones:dict[];isClone:False),sp(name:"Reflection Cubemap Roughness";imps:list[imp_mp_range(def:0.5;min:0;max:1;prop:"_ReflectionCubemapRoughness";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"163f0733-7888-49a2-ac05-09e82d5d2cbd";op:Multiply;lbl:"Cubemap Roughness";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list["56201e"];unlocked:list[];layer_blend:dict[56201e=Add];custom_blend:dict[56201e="lerp(a, b, s)"];clones:dict[56201e=sp(name:"Reflection Cubemap Roughness_56201e";imps:list[imp_mp_range(def:0.5;min:0;max:1;prop:"_ReflectionCubemapRoughness_56201e";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"00000000-0000-0000-0000-000000000000";op:Multiply;lbl:"Cubemap Roughness";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:True)];isClone:False),sp(name:"Planar Normals Influence";imps:list[imp_mp_range(def:0.1;min:0;max:1;prop:"_PlanarNormalsInfluence";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"553b7eb6-3596-4215-8c17-888e810bfa17";op:Multiply;lbl:"Reflection Normal Influence";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list["56201e"];unlocked:list[];layer_blend:dict[56201e=Add];custom_blend:dict[56201e="lerp(a, b, s)"];clones:dict[];isClone:False),sp(name:"Fresnel Min";imps:list[imp_mp_range(def:0;min:0;max:2;prop:"_FresnelMin";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"562901b2-3a33-4b3a-8bc4-162452778af0";op:Multiply;lbl:"Fresnel Min";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list["56201e"];unlocked:list[];layer_blend:dict[56201e=Add];custom_blend:dict[56201e="lerp(a, b, s)"];clones:dict[];isClone:False),sp(name:"Fresnel Max";imps:list[imp_mp_range(def:1.5;min:0;max:2;prop:"_FresnelMax";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"bd8fddae-1bc3-433f-b9a7-d23b404ed16a";op:Multiply;lbl:"Fresnel Max";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list["56201e"];unlocked:list[];layer_blend:dict[56201e=Add];custom_blend:dict[56201e="lerp(a, b, s)"];clones:dict[];isClone:False),sp(name:"Vertex Displacement";imps:list[imp_localnorm(cc:3;chan:"XYZ";guid:"aa9573eb-785b-4934-a40e-5af7326187ca";op:Multiply;lbl:"Vertex Displacement";gpu_inst:False;dots_inst:False;locked:False;impl_index:0),imp_mp_texture(uto:True;tov:"";tov_lbl:"";gto:False;sbt:False;scr:False;scv:"";scv_lbl:"";gsc:False;roff:False;goff:False;sin_anm:False;sin_anmv:"";sin_anmv_lbl:"";gsin:False;notile:False;triplanar_local:False;def:"black";locked_uv:False;uv:0;cc:3;chan:"RGB";mip:0;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";tpln_scale:1;uv_shaderproperty:__NULL__;uv_cmp:__NULL__;sep_sampler:__NULL__;prop:"_DisplacementTex";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"087e3ea8-5906-4c80-bcec-dddfa076e60e";op:Multiply;lbl:"Displacement Texture";gpu_inst:False;dots_inst:False;locked:False;impl_index:1),imp_mp_range(def:0.01;min:-1;max:1;prop:"_DisplacementStrength";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"ce0298db-363e-4d20-a5be-b2557fe264d4";op:Multiply;lbl:"Displacement Strength";gpu_inst:False;dots_inst:False;locked:False;impl_index:2)];layers:list["1f64b1"];unlocked:list["1f64b1"];layer_blend:dict[1f64b1=Add];custom_blend:dict[1f64b1="lerp(a, b, s)"];clones:dict[1f64b1=sp(name:"Vertex Displacement_1f64b1";imps:list[imp_localnorm(cc:3;chan:"XYZ";guid:"00000000-0000-0000-0000-000000000000";op:Multiply;lbl:"Vertex Displacement";gpu_inst:False;dots_inst:False;locked:False;impl_index:0),imp_mp_texture(uto:True;tov:"";tov_lbl:"";gto:False;sbt:False;scr:False;scv:"";scv_lbl:"";gsc:False;roff:False;goff:False;sin_anm:False;sin_anmv:"";sin_anmv_lbl:"";gsin:False;notile:False;triplanar_local:False;def:"black";locked_uv:False;uv:0;cc:3;chan:"RGB";mip:0;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";tpln_scale:1;uv_shaderproperty:__NULL__;uv_cmp:__NULL__;sep_sampler:__NULL__;prop:"_DisplacementTex_1f64b1";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"00000000-0000-0000-0000-000000000000";op:Multiply;lbl:"Displacement Texture";gpu_inst:False;dots_inst:False;locked:False;impl_index:1),imp_mp_range(def:0.01;min:-1;max:1;prop:"_DisplacementStrength_1f64b1";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"00000000-0000-0000-0000-000000000000";op:Multiply;lbl:"Displacement Strength";gpu_inst:False;dots_inst:False;locked:False;impl_index:2)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:True)];isClone:False),sp(name:"Normal Map";imps:list[imp_mp_texture(uto:True;tov:"";tov_lbl:"";gto:False;sbt:False;scr:False;scv:"";scv_lbl:"";gsc:False;roff:False;goff:False;sin_anm:False;sin_anmv:"";sin_anmv_lbl:"";gsin:False;notile:False;triplanar_local:False;def:"bump";locked_uv:False;uv:0;cc:4;chan:"RGBA";mip:-1;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";tpln_scale:1;uv_shaderproperty:__NULL__;uv_cmp:__NULL__;sep_sampler:__NULL__;prop:"_BumpMap";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"8d536404-7c92-48b7-943b-fa9991736ca1";op:Multiply;lbl:"Normal Map";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list["1f64b1"];layer_blend:dict[1f64b1=LinearInterpolation];custom_blend:dict[1f64b1="lerp(a, b, s)"];clones:dict[1f64b1=sp(name:"Normal Map_1f64b1";imps:list[imp_mp_texture(uto:True;tov:"";tov_lbl:"";gto:False;sbt:False;scr:False;scv:"";scv_lbl:"";gsc:False;roff:False;goff:False;sin_anm:False;sin_anmv:"";sin_anmv_lbl:"";gsin:False;notile:False;triplanar_local:False;def:"bump";locked_uv:False;uv:0;cc:4;chan:"RGBA";mip:-1;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";tpln_scale:1;uv_shaderproperty:__NULL__;uv_cmp:__NULL__;sep_sampler:__NULL__;prop:"_BumpMap_1f64b1";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"00000000-0000-0000-0000-000000000000";op:Multiply;lbl:"Normal Map";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:True)];isClone:False),sp(name:"Bump Scale";imps:list[imp_mp_float(def:1;prop:"_BumpScale";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"2706f148-0266-4544-b546-1865322c1b58";op:Multiply;lbl:"Scale";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list["1f64b1"];unlocked:list[];layer_blend:dict[1f64b1=LinearInterpolation];custom_blend:dict[1f64b1="lerp(a, b, s)"];clones:dict[];isClone:False)];customTextures:list[];codeInjection:codeInjection(injectedFiles:list[];mark:False);matLayers:list[ml(uid:"1f64b1";name:"Snow";src:sp(name:"layer_1f64b1";imps:list[imp_customcode(prepend_type:Disabled;prepend_code:"";prepend_file:"";prepend_file_block:"";preprend_params:dict[];code:"{2}.y + {3}";guid:"1a37bc39-800b-45c6-b75b-15c938c9f9d3";op:Multiply;lbl:"layer_1f64b1";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1),imp_worldnorm(cc:1;chan:"Y";guid:"a45f37dd-40af-477f-8444-222b9e0d0e91";op:Multiply;lbl:"layer_1f64b1";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1),imp_mp_float(def:1;prop:"_NormalSnowThreshold";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"d1ebb6fd-b75e-451a-b23a-e1b2b878c2d1";op:Multiply;lbl:"Normal Snow Threshold";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False);use_contrast:True;ctrst:sp(name:"contrast_1f64b1";imps:list[imp_mp_range(def:0.5;min:0;max:1;prop:"_NormalThresholdBlending";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"7fcf0258-a67a-48e4-a122-4257f315d053";op:Multiply;lbl:"Normal Threshold Blending";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False);use_noise:True;noise:sp(name:"noise_1f64b1";imps:list[imp_customcode(prepend_type:Disabled;prepend_code:"";prepend_file:"";prepend_file_block:"";preprend_params:dict[];code:"saturate( {2}.r * {3} ) - {3} / 2.0";guid:"e89689f2-04cb-4558-98cc-0e62a187da51";op:Multiply;lbl:"noise_1f64b1";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1),imp_mp_texture(uto:True;tov:"";tov_lbl:"";gto:False;sbt:False;scr:False;scv:"";scv_lbl:"";gsc:False;roff:False;goff:False;sin_anm:False;sin_anmv:"";sin_anmv_lbl:"";gsin:False;notile:False;triplanar_local:False;def:"gray";locked_uv:False;uv:0;cc:1;chan:"R";mip:-1;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";tpln_scale:1;uv_shaderproperty:__NULL__;uv_cmp:__NULL__;sep_sampler:__NULL__;prop:"_SnowNoiseTexture";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"c71df622-aa37-4383-8511-0207eee05e55";op:Multiply;lbl:"Snow Noise Texture";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1),imp_mp_range(def:0.1;min:0;max:1;prop:"_SnowNoiseStrength";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"853af456-8954-4e6d-86bb-772349424796";op:Multiply;lbl:"Snow Noise Strength";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False)),ml(uid:"56201e";name:"Rain";src:sp(name:"layer_56201e";imps:list[imp_mp_texture(uto:False;tov:"";tov_lbl:"";gto:False;sbt:False;scr:False;scv:"";scv_lbl:"";gsc:False;roff:False;goff:False;sin_anm:False;sin_anmv:"";sin_anmv_lbl:"";gsin:False;notile:False;triplanar_local:False;def:"black";locked_uv:False;uv:0;cc:1;chan:"R";mip:-1;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";tpln_scale:1;uv_shaderproperty:__NULL__;uv_cmp:__NULL__;sep_sampler:__NULL__;prop:"_WetnessMap";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"d19b17fb-1556-48f2-9eab-4ff9c3b088e4";op:Multiply;lbl:"Wetness Map";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1),imp_mp_float(def:1;prop:"_NormalThreshold_56201e";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"fbaef8d7-d71d-40ea-8f70-738eabbbc3af";op:Multiply;lbl:"Normal Threshold";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False);use_contrast:True;ctrst:sp(name:"contrast_56201e";imps:list[imp_mp_range(def:0.5;min:0;max:1;prop:"_contrast_56201e";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"333d22f3-caf5-4261-9ef1-c6e96021a347";op:Multiply;lbl:"Contrast";gpu_inst:False;dots_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False);use_noise:False;noise:__NULL__)]) */
/* TCP_HASH 9d4867dc9bc32bb63391433ad92b8c49 */
