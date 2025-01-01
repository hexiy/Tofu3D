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
	uv = a_uv * vec2(1, -1);
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

in vec3 normal;
in vec3 vertexPositionWorld;
in vec2 uv;
in vec4 fragPosLightSpace;
in mat3 TBN;

out vec4 fragColor;

// Uniforms
uniform vec2 u_tiling;
uniform vec4 u_ambientLightColor;
uniform vec4 u_albedoTint;
uniform vec3 u_camPos;
uniform vec4 u_directionalLightColor;
uniform vec3 u_directionalLightDirection;
uniform float u_smoothness;
uniform float u_metallic;
uniform float u_renderMode = 0;

uniform int u_hasAlbedoTexture;
uniform int u_hasNormalTexture;
uniform int u_hasShadowmapTexture;
uniform int u_hasAmbientOcclusionTexture;
uniform int u_hasEmissiveTexture;
uniform int u_hasMetallicTexture;
uniform int u_hasRoughnessTexture;
uniform int u_directionalLightEnabled;

uniform sampler2D u_albedoTexture;
uniform sampler2D u_normalTexture;
uniform sampler2D u_ambientOcclusionTexture;
uniform samplerCube u_environmentCubemap;
uniform sampler2D u_shadowmapTexture;
uniform sampler2D u_emissiveTexture;
uniform sampler2D u_metallicTexture;
uniform sampler2D u_roughnessTexture;
float OldShadowCalculation(){
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
float ShadowCalculationPCFNotSmooth() {
	vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	projCoords = projCoords * 0.5 + 0.5;

	// Early return for fragments outside the light's frustum
	if (projCoords.z > 1.0 || projCoords.z < 0.0) return 0.0;
	
	// Calculate distance from the fragment to the camera
	float distanceToCamera = length(vertexPositionWorld - u_camPos);

	
	float shadow = 0.0;
	float bias = 0.0001;
	vec2 texelSize = 1.0 / textureSize(u_shadowmapTexture, 0); // Shadowmap size
	// Dynamic PCF sampling: Choose sample count and kernel size based on distance
	int samples = int(mix(50.0, 0.0, clamp(distanceToCamera / 35.0, 0.0, 1.0))); // Adjust range [3..5] based on distance

	for (int x = -samples / 2; x <= samples / 2; ++x) {
		for (int y = -samples / 2; y <= samples / 2; ++y) {
			float closestDepth = texture(u_shadowmapTexture, projCoords.xy + vec2(x, y) * texelSize).r;
			shadow += (projCoords.z - bias > closestDepth) ? 1.0 : 0.0;
		}
	}

	shadow /= float((samples + 1) * (samples + 1)); // Total samples in the kernel

	return shadow;
}
float ShadowCalculationPCFSmooth() {
	vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	projCoords = projCoords * 0.5 + 0.5;

	// Early return for fragments outside the light's frustum
	if (projCoords.z > 1.0 || projCoords.z < 0.0) return 0.0;

	// Calculate distance from the fragment to the camera
	float distanceToCamera = length(vertexPositionWorld - u_camPos);


	// Calculate smooth sample interpolation factor
	float smoothFactor = 1-clamp(distanceToCamera / 30.0, 0.0, 1.0); // Normalize to [0.0, 1.0]

	// Compute the two kernel sizes to blend between
	float minSamples = 1.0; // Minimum kernel size (3x3)
	float maxSamples = 5.0; // Maximum kernel size (5x5)
	float sampleSize = mix(minSamples, maxSamples, smoothFactor); // Smoothly blend between kernels

	// Separate kernel sizes into integer components for lower and upper bound
	int lowSamples = int(floor(sampleSize));  // Lower grid size
	int highSamples = int(ceil(sampleSize)); // Upper grid size
	float interpFactor = fract(sampleSize);  // Fractional amount between the two sizes

	// Initialize shadow intensity
	float shadowLow = 0.0;
	float shadowHigh = 0.0;
	float bias = 0.0001;
	vec2 texelSize = 1.0 / textureSize(u_shadowmapTexture, 0); // Size of one texel in shadow map

	// Low kernel sampling (lower bound)
	for (int x = -lowSamples / 2; x <= lowSamples / 2; ++x) {
		for (int y = -lowSamples / 2; y <= lowSamples / 2; ++y) {
			vec2 offset = vec2(x, y) * texelSize;
			float closestDepth = texture(u_shadowmapTexture, projCoords.xy + offset).r;
			shadowLow += (projCoords.z - bias > closestDepth) ? 1.0 : 0.0;
		}
	}
	shadowLow /= float((lowSamples + 1) * (lowSamples + 1)); // Normalize low kernel contribution

	// High kernel sampling (upper bound)
	for (int x = -highSamples / 2; x <= highSamples / 2; ++x) {
		for (int y = -highSamples / 2; y <= highSamples / 2; ++y) {
			vec2 offset = vec2(x, y) * texelSize;
			float closestDepth = texture(u_shadowmapTexture, projCoords.xy + offset).r;
			shadowHigh += (projCoords.z - bias > closestDepth) ? 1.0 : 0.0;
		}
	}
	shadowHigh /= float((highSamples + 1) * (highSamples + 1)); // Normalize high kernel contribution

	// Blend between low and high kernel results based on fractional sampling factor
	float shadow = mix(shadowLow, shadowHigh, interpFactor);

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
float D_GGX(float NdotH, float roughness) {
	float a = roughness * roughness;
	float a2 = a * a;
	float NdotH2 = NdotH * NdotH;

	float denominator = NdotH2 * (a2 - 1.0) + 1.0;
	return a2 / (3.14159 * denominator * denominator);
}
void main() {
	// UV Coordinates with tiling
	vec2 uvCoords = uv * u_tiling;

	// Albedo Color
	vec4 albedo = u_albedoTint;
	if (u_hasAlbedoTexture == 1) {
		albedo *= texture(u_albedoTexture, uvCoords);
	}
	vec3 baseColor = albedo.rgb; // Separate out RGB only

	// Normal Mapping
	vec3 finalNormal = normalize(TBN * normal);
	if (u_hasNormalTexture == 1) {
		vec3 texNormal = texture(u_normalTexture, uvCoords).rgb * 2.0 - 1.0; // Map [0,1] to [-1,1]
		//		finalNormal = normalize(TBN * texNormal);
		finalNormal = normalize(TBN * -texNormal);







		//			vec3 vertexNormalTBNed = normalize(TBN * normal);

		//  texNormal = normalize(TBN * -texNormal); // Transforming the normal values from the texture space to the world space
		//  //norm = normalize(TBN * norm);
		//  float blendFactor = 0.8 * u_hasNormalTexture;
		//  blendFactor = 0;
		//  vec3 finalNormal = normalize(mix(vertexNormalTBNed, texNormal, blendFactor));
		//			vec3 finalNormal = vertexNormalTBNed;
	}

	// View and Light Directions
	vec3 viewDir = normalize(u_camPos - vertexPositionWorld);
	vec3 lightDir = normalize(-u_directionalLightDirection);
	vec3 correctedLightDir = u_directionalLightDirection * vec3(1, -1, 1); // what is this where is it flipping so that i need to flip it here? is the tbn incorrect?
	lightDir = correctedLightDir;

	// Metallic and Roughness Maps
	float metallicValue = u_metallic; // Default metallic value (uniform)
	if (u_hasMetallicTexture == 1) {
		metallicValue = texture(u_metallicTexture, uvCoords).r * u_metallic; // Metallic texture (red channel)
	}

	float roughnessValue = 1.0 - u_smoothness; // Default roughness from smoothness
	if (u_hasRoughnessTexture == 1) {
		float textureRoughness = texture(u_roughnessTexture, uvCoords).r; // Roughness texture (red channel)
		roughnessValue = mix(roughnessValue, textureRoughness, 0.5); // Blend uniform and texture roughness
	}

	// Ambient Occlusion
	float ao = 1.0;
	if (u_hasAmbientOcclusionTexture == 1) {
		ao = texture(u_ambientOcclusionTexture, uvCoords).r; // AO texture affects ambient light
		ao = clamp(ao, 0.0, 1.0); // Ensure AO is within valid range
	}

	// Ambient Lighting
	vec3 ambient = u_ambientLightColor.rgb *
	u_ambientLightColor.a *
	ao *
	baseColor;

	// Diffuse Lighting
	float diffuseFactor = max(dot(finalNormal, lightDir), 0.0);
	vec3 diffuse = diffuseFactor *
	u_directionalLightColor.rgb *
	u_directionalLightColor.a *
	baseColor;

	// Specular Highlights
	vec3 reflectedLight = reflect(lightDir, finalNormal);
	//	float specExponent = mix(32.0, 1.0, roughnessValue); // 32 for low roughness, 1 for high roughness
	//	float specFactor = pow(max(dot(reflectedLight, viewDir), 0.0), specExponent);
	//	float specIntensity = mix(1.0, 0.0, roughnessValue); // Full specular for low roughness, none for high roughness

	float specFactor = D_GGX(max(dot(reflectedLight, viewDir), 0.0), roughnessValue) * metallicValue;
	vec3 specular = mix(vec3(0.04), u_directionalLightColor.rgb, metallicValue) * specFactor;

	// Shadows
	float shadow = (u_hasShadowmapTexture == 1) ? ShadowCalculationPCFSmooth() : 0.0;

	// Subtract shadow influence for direct lighting
	vec3 lighting = ambient + (diffuse + specular) *
	(1.0 - shadow);

	// Environmental Reflections
	vec3 reflection = vec3(0.0);
	//	if (metallicValue > 0.0) {

	vec3 reflectionI = normalize(vertexPositionWorld - u_camPos);
	vec3 reflectionDir = reflect(reflectionI, normalize(normal));

	float MAX_LOD = 7.0; // Maximum level-of-detail for the cubemap mipmaps
	vec3 environmentReflection = textureLod(u_environmentCubemap, reflectionDir, roughnessValue * MAX_LOD).rgb;
	//		reflection = texture(u_environmentCubemap, reflectionDir).rgb;

	// Adjust reflection intensity (optional for non-metallic surfaces)
	reflection *= mix(0.04, 1.0, metallicValue); // Base reflectivity: Dielectric vs Metal

	// Reflection scaling based on metallic and roughness
	vec3 surfaceReflectivity = mix(vec3(0.04), albedo.rgb, metallicValue); // Non-metallic uses F0 ~ 0.04
	reflection = environmentReflection * surfaceReflectivity;
	// Roughness reduces reflection intensity
	// Roughness impact on sharpness, not intensity
	reflection = mix(reflection, vec3(0.0), roughnessValue); // Soften reflections without killing intensity
	reflection = sRGBToLinear(reflection);
	//	}

	// Combine Lighting and Reflections
	vec3 color = lighting + reflection;

	// Emissive Lighting (if available)
	if (u_hasEmissiveTexture == 1) {
		color += texture(u_emissiveTexture, uvCoords).rgb * baseColor;
	}

	// Final Conversion to SRGB
	color = LinearToSRGB(color);

	// Final Output
	if (u_renderMode == 0) // regular
	{
		fragColor = vec4(color, 1); // Preserve the original albedo alpha
	}
	else if (u_renderMode == 1) // albedo
	{
		fragColor = vec4(albedo.rgb, 1);
	}
	else if (u_renderMode == 2) // positions
	{
		fragColor = vec4(vertexPositionWorld, 1);
	}
	else if (u_renderMode == 3) // normals
	{
		fragColor = vec4(finalNormal, 1);
	}
	else if (u_renderMode == 4) // directional light diffuse visualisation
	{
		float light = (diffuseFactor) * u_directionalLightColor.a;
		fragColor = vec4(vec3(light), 1);
	}
	else if (u_renderMode == 5) // directional light specular
	{
		float light = (specFactor) * u_directionalLightColor.a;
		fragColor = vec4(vec3(light), 1);
	}else if (u_renderMode == 6) // shadows
	{

//		shadow = OldShadowCalculation();
		shadow = ShadowCalculationPCFSmooth();
		fragColor = vec4(vec3(1-shadow), 1);
//		fragColor = vec4(projCoords, 1.0);
	}
}