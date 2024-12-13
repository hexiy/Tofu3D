//[BUFFERTYPE: Model]
//[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec2 a_uv;
layout (location = 2) in vec3 a_normal;
layout (location = 3) in vec3 a_tangent;
layout (location = 4) in vec3 a_bitangent;
layout (location = 5) in vec3 a_model_1;
layout (location = 6) in vec3 a_model_2;
layout (location = 7) in vec3 a_model_3;
layout (location = 8) in vec3 a_model_4;
layout (location = 9) in int a_id;
layout (location = 10) in vec2 a_uv_offset;

uniform mat4 u_viewProjection;
uniform mat4 u_lightSpaceViewProjection;

out vec3 vertexPositionWorld;
out vec2 uv;
out vec3 normal;
//out vec4 color;
out vec4 fragPosLightSpace;
out mat3 TBN;
#if UV_OFFSET_IS_INSTANCED == 1
out vec2 uvOffset;
#endif
void main(void)
{
	mat4 a_model = mat4(vec4(a_model_1, 0), vec4(a_model_2, 0), vec4(a_model_3, 0), vec4(a_model_4, 1));
	mat4 mvp = u_viewProjection * a_model;
	gl_Position = mvp * vec4(a_pos.xyz, 1.0);
	uv = a_uv * vec2(-1, -1);
	#if UV_OFFSET_IS_INSTANCED == 1
	uvOffset = a_uv_offset;
	#endif
	//color = a_color;

	vertexPositionWorld = vec3(a_model * vec4(a_pos.xyz, 1.0));
	normal = transpose(inverse(mat3(a_model))) * a_normal;

	mat4 lightMvp = u_lightSpaceViewProjection * a_model;
	fragPosLightSpace = lightMvp * vec4(a_pos.xyz, 1.0);

	// TBN for normal texture mapping
	vec3 T = normalize(vec3(a_model * vec4(a_tangent, 0.0)));
	vec3 B = normalize(vec3(a_model * vec4(a_bitangent, 0.0)));
	vec3 N = normalize(vec3(a_model * vec4(a_normal, 0.0)));
	TBN = mat3(T, B, N);
}

//[FRAGMENT]
#version 410 core

uniform vec2 u_tiling;
uniform vec2 u_offset;
uniform vec4 u_ambientLightColor;
uniform vec4 u_albedoTint;
uniform vec3 u_camPos;
uniform vec4 u_directionalLightColor;
uniform vec3 u_directionalLightDirection = vec3(1, 0, 0);
uniform float u_specularSmoothness = 1;
uniform float u_specularHighlightsEnabled = 1;

uniform float u_renderMode = 0;

uniform float u_fogEnabled = 0;
uniform vec4 u_fogColor = vec4(0, 0, 0, 1);
uniform vec4 u_fogColor2 = vec4(0, 0, 0, 1);
uniform float u_fogStartDistance = 0;
uniform float u_fogEndDistance = 1;
uniform float u_fogPositionY = 0;
uniform float u_fogGradientSmoothness = 1;
uniform float u_fogIntensity = 1;

uniform float u_metallic = 1;
uniform float u_smoothness = 1;

uniform vec4 u_emissiveColor = vec4(0, 0, 0, 0); // could be vec3, whatever

uniform int u_hasAlbedoTexture = 0;
uniform sampler2D u_albedoTexture;
uniform int u_hasNormalTexture = 0;
uniform sampler2D u_normalTexture;
uniform int u_hasAmbientOcclusionTexture = 0;

uniform sampler2D u_ambientOcclusionTexture;
uniform int u_hasShadowmapTexture = 0;

uniform sampler2D u_shadowmapTexture;
uniform int u_hasEnvironmentCubemap = 0;

uniform samplerCube u_environmentCubemap;
uniform int u_hasMetallicTexture = 0;

uniform sampler2D u_metallicTexture;
uniform int u_hasRoughnessTexture = 0;

uniform sampler2D u_roughnessTexture;
uniform int u_hasEmissiveTexture = 0;

uniform sampler2D u_emissiveTexture;

in vec3 normal;
in vec2 uv;
in vec3 vertexPositionWorld;
//in vec4 color;
in vec4 fragPosLightSpace;
in mat3 TBN;
#if UV_OFFSET_IS_INSTANCED == 1
in vec2 uvOffset;
#endif
out vec4 fragColor;

// 1 if in shadow-black, 0 if in light
float ShadowCalculation()
{
	// perform perspective divide
	vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	// transform to [0,1] range
	projCoords = projCoords * 0.5 + 0.5;
	// get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
	float closestDepth = texture(u_shadowmapTexture, projCoords.xy).r;
	// get depth of current fragment from light's perspective
	float currentDepth = projCoords.z;
	// check whether current frag pos is in shadow
	//    float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;

	float bias = 0.0001;

	float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;

	if (projCoords.z > 1.0) // fixes dark border behind the light
	{
		shadow = 0.0;
	}
	return shadow;
}
// Helper function to convert from sRGB to linear space
vec3 sRGBToLinear(vec3 color) {
	return pow(color, vec3(2.2));
}

// Helper function to convert from linear space to sRGB
vec3 LinearToSRGB(vec3 color) {
	return pow(color, vec3(1.0 / 2.2));
}
float metallic = 0;
float roughness = 1;
void main(void)
{
	vec2 uvCoords = (uv) * u_tiling;
	#if UV_OFFSET_IS_INSTANCED == 1
	uvCoords += uvOffset;
	#endif
	if (u_hasMetallicTexture == 1) {
		metallic = texture(u_metallicTexture, uvCoords).r;
	}
	if (u_hasRoughnessTexture == 1) {
		roughness = texture(u_roughnessTexture, uvCoords).r;
	}

	vec3 vertexNormalTBNed = normalize(TBN * normal);

	if (u_hasNormalTexture == 1) {
		vec3 notUsedJustForCompiler = texture(u_normalTexture, uvCoords).rgb; // just so we use u_normalTexture
	}
	//  texNormal = texNormal * 2.0 - 1.0; // Normalizing the normal values from the texture
	//  texNormal = normalize(TBN * -texNormal); // Transforming the normal values from the texture space to the world space
	//  //norm = normalize(TBN * norm);
	//  float blendFactor = 0.8 * u_hasNormalTexture;
	//  blendFactor = 0;
	//  vec3 finalNormal = normalize(mix(vertexNormalTBNed, texNormal, blendFactor));
	vec3 finalNormal = vertexNormalTBNed;

	vec4 albedoColor = u_albedoTint;
	if (u_hasAlbedoTexture == 1) {
		albedoColor = texture(u_albedoTexture, uvCoords) * u_albedoTint; //*color;
	}
	//	albedoColor.rgb = sRGBToLinear(albedoColor.rgb);

	vec3 viewDir = normalize(u_camPos - vertexPositionWorld);
	//	// Compute the Fresnel factor using the Schlick approximation
	float fresnelEdgeWidth = 3.4;
	float fresnelFactor = pow(1.0 - max(dot(viewDir, normalize(normal)), 0.0), 10 / fresnelEdgeWidth) * 0.9 + 0.1;
	float fresnelFactor2 = pow(1.0 - max(dot(viewDir, normalize(normal)), 0.0), 10 / 5) * 0.9 + 0.1;
	fresnelFactor2 = pow(fresnelFactor2, 3);

	vec3 reflectionI = normalize(vertexPositionWorld - u_camPos);
	vec3 reflectionR = reflect(reflectionI, normalize(normal));

	//		float ratio = 1.00 / 1.1;
	float ratio = 1.00 / 1.309; // Water
	//	float ratio = 1.00 / 1.309; // Ice
	//		float ratio = 1.00 / 1.52; // Glass
	//		float ratio = 1.00 / 2.42; // Diamond

	vec3 refractionR = refract(reflectionI, normalize(normal), ratio);

	vec3 environmentReflection = vec3(1, 1, 1);
	vec3 environmentReflectionT = vec3(1, 1, 1);
	vec3 environmentRefraction = vec3(1, 1, 1);

	if (u_hasEnvironmentCubemap == 1) {
		environmentReflection = texture(u_environmentCubemap, reflectionR).rgb;
		environmentRefraction = texture(u_environmentCubemap, refractionR).rgb;
		environmentReflectionT = texture(u_environmentCubemap, vec3(10, 10, 10)).rgb;
	}

	environmentReflectionT.rgb = sRGBToLinear(environmentReflectionT.rgb);
	environmentRefraction.rgb = sRGBToLinear(environmentRefraction.rgb);

	//	float a = fresnelFactor*1.5;

	//	 at the edge fresnel is 1, so basically i just want to add skybox color the more 1 it is, literally should be just + fresnel*skyboxcolor
	//		AFTER the tint, and then just add tint and that together
	//		and we can tame the skybox reflection it isnt gonna be at full blast obviously
	//

	vec3 environmentReflectionTinted = environmentReflection * (mix(vec3(1, 1, 1), u_albedoTint.rgb, 1 - fresnelFactor));
	vec3 environmentReflectionSkyboxFresnel = fresnelFactor2 * environmentReflection;

	environmentReflection.rgb = environmentReflectionTinted * 0.6; // + environmentReflectionSkyboxFresnel;
	environmentReflection.rgb = sRGBToLinear(environmentReflection.rgb);

	vec4 aoColor = vec4(1, 1, 1, 1);
	if (u_hasAmbientOcclusionTexture == 1) {
		aoColor = texture(u_ambientOcclusionTexture, uvCoords);
	}

	vec4 final_ambient = vec4(u_ambientLightColor.rgb * u_ambientLightColor.a, 0);

	vec3 correctedLightDir = u_directionalLightDirection * vec3(1, -1, 1); // what is this where is it flipping so that i need to flip it here? is the tbn incorrect?
	vec3 lightDirTangent = normalize(TBN * -correctedLightDir.rgb);
	float directionalLightFactor = max(dot(finalNormal, lightDirTangent), 0.0);
	float directionalLightClampedIntensity = u_directionalLightColor.a;
	vec4 diffuse = vec4(directionalLightFactor * directionalLightClampedIntensity * u_directionalLightColor.rgb, 1);
	//result *= ambient;

	//	vec4 result = albedoColor * aoColor * max(final_ambient, final_diffuse) + min(final_ambient, final_diffuse);
	//	result.a = albedoColor.a;// * color.a;

//		if (result.a < 0.05)
		if (albedoColor.a < 0.05)
		{
			discard; // having this fixes transparency sorting but breaks debug depthmap
		}

	//	if (u_specularHighlightsEnabled == 1)
	//	{
	//		vec3 reflectedLightVectorWorld = reflect(correctedLightDir, finalNormal);
	//		vec3 viewDir = normalize(u_camPos - vertexPositionWorld);
	//		////////// problem is below
	//		float clampedSpecularSmoothness = max(u_specularSmoothness, 0);
	//		float spec = pow(max(dot(viewDir, reflectedLightVectorWorld), 0.0), 32 * clampedSpecularSmoothness);
	//		spec = max(spec, 0);
	//		vec3 specular = clampedSpecularSmoothness * spec * u_directionalLightColor.rgb * directionalLightClampedIntensity * 2;
	//
	//		//vec4 specular = vec4(u_directionalLightColor.rgb * s, 1);
	//
	//		//if (shadow == 0) {
	//		//specular /= 3;
	//		//}
	//
	//		result.rgb *= max(vec3(1), specular + 1);//*normalize(albedoColor.rgb+vec3(0.3));
	//
	//	}

	//	float shadow = ShadowCalculation(); // 1 if in shadow
	//	//        shadow
	//	if (shadow == 1) {
	//		//result.rgb = result.rgb * 0.1;
	//		//		result.rgb = vec3(1,0,0); // red
	//		result = albedoColor * aoColor * final_ambient;
	//
	//	}
	//	else {
	//		result.rgb = result.rgb;
	//		//result.rgb = vec3(0,1,0); // green
	//
	//	}

	vec4 result = vec4(0, 0, 0, 1);
	//	result.rgb = environmentReflection* u_metallic;
	//	result.rgb = environmentRefraction;

	//	// Combine reflection and refraction using Fresnel blending
	float newSmoothness = (u_smoothness / 2.0 * u_metallic) + (u_smoothness / 2.0);
	float metallicCapped = max(u_metallic, 0.3 * newSmoothness); // u_smoothness 0 everything will be black, 1 the metallic will get clamped to 0.2, we dont have blurry reflections yet so this is just to somewhat match what unity is doing temporarily

	//    vec4 albedoColorLit = albedoColor * diffuse;


	vec4 albedoColorLit = albedoColor * vec4(final_ambient.rgb, 1);
	albedoColorLit.rgb += environmentReflectionT * 0.01;
	float reflectivity = (metallicCapped + (fresnelFactor * (1 - metallicCapped))) * newSmoothness;

//	result.rgb = albedoColorLit.rgb + final_ambient.rgb; //+ environmentReflectionT*0.01;
	result.rgb = albedoColorLit.rgb; //+ environmentReflectionT*0.01;

	//    	vec3 environmentRefractionAndReflectionMix = mix(environmentRefraction, environmentReflection, 1 - fresnelFactor) * metallicCapped;
	// disable refraction for now
	vec3 environmentRefractionAndReflectionMix = environmentReflection;
	vec3 albedoAndMetallicMix = mix(result.rgb, environmentRefractionAndReflectionMix * metallicCapped, reflectivity);

	float aaa = max(u_smoothness - u_metallic, 0);
	//	aaa is strength of the fresnel halo

	result.rgb = albedoAndMetallicMix + (aaa * environmentReflectionSkyboxFresnel * 0.4);
	//	result.rgb = environmentReflectionSkyboxFresnel;
	//	result.rgb = environmentReflectionT*fresnelFactor*2;
	//	result.rgb = vec3(directionalLightFactor,0,0);
	//	result.rgb = vec3(directionalLightFactor*reflectivity,0,0);
	//	result.rgb = vec3(metallicCapped,0,0);
	//	result.rgb = vec3(reflectivity,0,0);
	//	result.rgb = environmentRefractionAndReflectionMix;

	//	result.rgb = mix(environmentRefraction, environmentReflection, 1-fresnelFactor);
	//result.a = 1;

	//		result.rgb = vec3(fresnelFactor,0,0); // debug fresnel
	//if(fresnelFactor>1){
	//	result.rgb = vec3(0,fresnelFactor,0); // debug fresnel
	//
	//}

	if (u_fogEnabled == 1 && u_renderMode == 0)
	{
		float distanceToVertex = distance(u_camPos.xz, vertexPositionWorld.xz);
		float fogFactor = 0;
		if (distanceToVertex > u_fogStartDistance) {
			fogFactor = (distanceToVertex) - u_fogStartDistance;
		}

		fogFactor = fogFactor / (u_fogEndDistance - u_fogStartDistance);
		fogFactor = clamp(fogFactor, 0, 1);

		float gradientStep = (vertexPositionWorld.y - u_fogPositionY) / u_fogGradientSmoothness;

		gradientStep = clamp(gradientStep, 0, 1);
		//        gradientStep = 1/gradientStep;

		vec4 finalFogColor = mix(vec4(u_fogColor2.rgb * u_fogColor2.a, u_fogColor2.a), vec4(u_fogColor.rgb * u_fogColor.a, u_fogColor.a), gradientStep);

		//        float density = 0.000009;
		//fogFactor = 1- exp(-density*density*distanceToVertex*distanceToVertex);
		//fogFactor = clamp(fogFactor, 0,1);
		fogFactor = fogFactor * u_fogIntensity;
		fogFactor = fogFactor * finalFogColor.a;

		result.rgb = mix(result.rgb, finalFogColor.rgb, fogFactor);
	}

	result.rgb = LinearToSRGB(result.rgb);

	// u_emissiveColor.a  == 0 ->
	float emissiveIntensity = u_emissiveColor.a / 10.0 + 1.0;

	vec3 emissiveColor = vec3(0, 0, 0);
	if (u_hasEmissiveTexture == 1) {
		emissiveColor = texture(u_emissiveTexture, uvCoords).rgb * u_emissiveColor.rgb * emissiveIntensity;
	}
	//	float emissiveIntensity= u_emissiveColor.a + (-10.0/255.0) + 1;
	result.rgb += emissiveColor;
	//	result.rgb = u_emissiveColor.rgb;
	if (u_renderMode == 0) // regular
	{
		//		result.rgb = vec3(emissiveIntensity,0,0); dbg

		fragColor = result;
	}
	if (u_renderMode == 1) // positions
	{
		//fragColor = vec4(normalize(- vertexPositionWorld) * result.rgb, result.a);
		fragColor = vec4(vertexPositionWorld, result.a);

		//vec3 roundedPos = round(vertexPositionWorld/5)*5;
		//fragColor = vec4(roundedPos, result.a);
	}
	if (u_renderMode == 2) // normals
	{
		//fragColor = vec4(normalize(- normal) * result.rgb, result.a);
		//fragColor = vec4(normalize(- normal), result.a);
		fragColor = vec4(finalNormal, result.a);
	}
	//	gl_FragDepth = gl_FragCoord.z;
}
