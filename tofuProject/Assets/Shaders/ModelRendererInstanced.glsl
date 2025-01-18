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
layout (location = 9) in float a_id;
layout (location = 10) in vec2 a_uv_offset;

uniform mat4 u_viewProjection;
uniform mat4 u_lightSpaceViewProjection;

out vec3 vertexPositionWorld;
out vec2 uv;
out vec3 normalWorldSpace;
//out vec4 color;
out vec4 fragPosLightSpace;
out mat3 TBN;
#ifdef UV_OFFSET_IS_INSTANCED
out vec2 uvOffset;
#endif
flat out uint v_id;
void main(void)
{
	mat4 a_model = mat4(vec4(a_model_1, 0), vec4(a_model_2, 0), vec4(a_model_3, 0), vec4(a_model_4, 1));
	mat4 mvp = u_viewProjection * a_model;
	gl_Position = mvp * vec4(a_pos.xyz, 1.0);
	uv = a_uv * vec2(1, -1);
	#ifdef UV_OFFSET_IS_INSTANCED
    uvOffset = a_uv_offset;
	#endif
    //color = a_color;
	v_id = uint(a_id);

	vertexPositionWorld = vec3(a_model * vec4(a_pos.xyz, 1.0));
//	normalWorldSpace = transpose(inverse(mat3(a_model))) * a_normal;
	normalWorldSpace = normalize(transpose(inverse(mat3(a_model))) * a_normal);
	
	mat4 lightMvp = u_lightSpaceViewProjection * a_model;
	fragPosLightSpace = lightMvp * vec4(a_pos.xyz, 1.0);

	vec3 T = normalize(vec3(a_model * vec4(a_tangent, 0.0)));
	vec3 B = normalize(vec3(a_model * vec4(a_bitangent, 0.0)));
	vec3 N = normalize(vec3(a_model * vec4(a_normal, 0.0)));
	TBN = mat3(T, B, N); // Keep as a forward TBN matrix
}

//[FRAGMENT]
#version 410 core

in vec3 normalWorldSpace;
in vec3 vertexPositionWorld;
in vec2 uv;
in vec4 fragPosLightSpace;
in mat3 TBN;
flat in uint v_id;
out vec4 fragColor;

// Uniforms
uniform vec2 u_tiling;
uniform vec4 u_ambientLightColor;
uniform vec4 u_albedoTint;
uniform vec3 u_camPosWorldSpace;
uniform vec4 u_directionalLightColor;
uniform vec3 u_directionalLightDirection;
uniform float u_smoothness;
uniform float u_metallic;
uniform float u_renderMode = 0;
uniform float u_cameraFrustumLength = 100;

uniform int u_materialType;
uniform int u_hasAlbedoTexture;
uniform int u_hasAlphaMaskTexture;
uniform int u_hasNormalTexture;
uniform int u_hasShadowmapTexture;
uniform int u_hasAmbientOcclusionTexture;
uniform int u_hasEmissiveTexture;
uniform int u_hasMetallicTexture;
uniform int u_hasRoughnessTexture;
uniform int u_directionalLightEnabled;
uniform int u_smoothShadows;

//////////////////// FOG
uniform float u_fogEnabled = 0;
uniform vec4 u_fogColor = vec4(0, 0, 0, 1);
uniform vec4 u_fogColor2 = vec4(0, 0, 0, 1);
uniform float u_fogStartDistance = 0;
uniform float u_fogEndDistance = 1;
uniform float u_fogPositionY = 0;
uniform float u_fogGradientSmoothness = 1;
uniform float u_fogIntensity = 1;

uniform sampler2D u_albedoTexture;
uniform sampler2D u_alphaMaskTexture;
uniform sampler2D u_normalTexture;
uniform sampler2D u_ambientOcclusionTexture;
uniform samplerCube u_environmentCubemap;
uniform sampler2D u_shadowmapTexture;
uniform sampler2D u_emissiveTexture;
uniform sampler2D u_metallicTexture;
uniform sampler2D u_roughnessTexture;


uniform struct PointLight {
	vec3 position;   // 12 bytes
	float intensity; // 16 bytes (next multiple of 4)
	vec3 color;      // 12 bytes
	float radius;    // 16 bytes (next multiple of 4)
};
layout (std140) uniform LightBuffer { // 16kb size limit
									  PointLight u_pointLights[1];
};
uniform int _pointLightsCount = 0;

vec3 calculatePointLightsLighting(vec3 normal, vec3 viewDir, vec3 fragPos) {
	vec3 result = vec3(0.0);

	for (int i = 0; i < _pointLightsCount; i++) {
		PointLight light = u_pointLights[i];

		vec3 lightDir = normalize(light.position - fragPos);
		float distance = length(light.position - fragPos);

		if (distance > light.radius) {
			continue;
		}

		float attenuation = 1.0 - clamp(distance / light.radius, 0.0, 1.0);
		attenuation = attenuation * attenuation; // Falloff curve for smoother transition

		normal = normalize(normal);
		lightDir = normalize(lightDir);

		float diffuseFactor = max(dot(normal, lightDir), 0.0);
		vec3 diffuse = diffuseFactor * light.color * light.intensity;

//		vec3 halfwayDir = normalize(viewDir + lightDir);
//		float specFactor = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
//		vec3 specular = specFactor * light.color * light.intensity;

				result += attenuation * (diffuse);
//		result += attenuation * (diffuse + specular);
	}

	return result;
}
float OldShadowCalculation() {
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
float ShadowCalculationPCFConstantQuality() {
	vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	projCoords = projCoords * 0.5 + 0.5;

	// Early return for fragments outside the light's frustum
	if (projCoords.z > 1.0 || projCoords.z < 0.0) return 0.0;

	float shadow = 0.0;
	float bias = 0.0001;//
	vec2 texelSize = 1.0 / textureSize(u_shadowmapTexture, 0); // Shadowmap size
	// Dynamic PCF sampling: Choose sample count and kernel size based on distance
	int samples = 3; // must be odd

	for (float x = -samples / 2; x <= samples / 2; ++x) {
		for (float y = -samples / 2; y <= samples / 2; ++y) {
			float closestDepth = texture(u_shadowmapTexture, projCoords.xy + vec2(x, y) * texelSize * 0.1).r;
			shadow += (projCoords.z - bias > closestDepth) ? 1.0 : 0.0;
		}
	}

	shadow /= float((samples) * (samples)); // Total samples in the kernel

	return shadow;
}
float ShadowCalculationPCFSmoothDistanceToCamera() {
	vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	projCoords = projCoords * 0.5 + 0.5;

	// Early return for fragments outside the light's frustum
	if (projCoords.z > 1.0 || projCoords.z < 0.0) return 0.0;

	// Calculate distance from the fragment to the camera
	float distanceToCamera = length(vertexPositionWorld - u_camPosWorldSpace);


	// Calculate smooth sample interpolation factor
	float smoothFactor = 1 - clamp(distanceToCamera / 30.0, 0.0, 1.0); // Normalize to [0.0, 1.0]

	// Compute the two kernel sizes to blend between
	float minSamples = 5.0; // Minimum kernel size (3x3)
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
// Fresnel-Schlick approximation for fresnel reflectance (F)
vec3 F_Schlick(float VdotH, vec3 F0) {
	return F0 + (1.0 - F0) * pow(1.0 - VdotH, 5.0);
}

// Geometry (G) term for GGX
float G_Smith(float NdotV, float NdotL, float roughness) {
	float r = (roughness + 1.0);
	float k = (r * r) / 8.0;

	float G_V = NdotV / (NdotV * (1.0 - k) + k);
	float G_L = NdotL / (NdotL * (1.0 - k) + k);

	return G_V * G_L;
}
float D_GGX(float NdotH, float roughness) {
	float a = roughness * roughness;
	float a2 = a * a;
	float NdotH2 = NdotH * NdotH;

	float denominator = NdotH2 * (a2 - 1.0) + 1.0;
	return a2 / (3.14159 * denominator * denominator);
}
// Correct specular term
vec3 SpecularReflectionGGX(vec3 N, vec3 V, vec3 L, vec3 H, vec3 F0, float roughness) {
	float D = D_GGX(max(dot(N, H), 0.0), roughness);
	vec3 F = F_Schlick(max(dot(V, H), 0.0), F0);
	float G = G_Smith(max(dot(N, V), 0.0), max(dot(N, L), 0.0), roughness);

	return (D * F * G) / (4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.001);
}
void main() {
	// UV Coordinates with tiling
	vec2 uvCoords = uv * u_tiling;

	// Albedo Color
	vec4 albedo = u_albedoTint;
	if (u_hasAlbedoTexture == 1) {
		albedo *= texture(u_albedoTexture, uvCoords);
	}
	if (u_materialType == 1 && u_renderMode == 0) {
		if (albedo.a < 0.9) {
			discard;
		}
		fragColor = albedo;

		return;
	}

	vec3 baseColor = albedo.rgb; // Separate out RGB only

	// Normal Mapping
	vec3 finalNormalWorldSpace = normalWorldSpace;
	if (u_hasNormalTexture == 1) {
		vec3 texNormal = texture(u_normalTexture, uvCoords).rgb * 2.0 - 1.0; // Map [0,1] to [-1,1]
		//		normalWorldSpace = normalize(TBN * texNormal);
		finalNormalWorldSpace = normalize(TBN * normalize(texNormal)); // from tangent space to world space

		//			vec3 vertexNormalTBNed = normalize(TBN * normal);

		//  texNormal = normalize(TBN * -texNormal); // Transforming the normal values from the texture space to the world space
		//  //norm = normalize(TBN * norm);
		//  float blendFactor = 0.8 * u_hasNormalTexture;
		//  blendFactor = 0;
		//  vec3 normalWorldSpace = normalize(mix(vertexNormalTBNed, texNormal, blendFactor));
		//			vec3 normalWorldSpace = vertexNormalTBNed;
	}

	// View and Light Directions
	vec3 viewDir = normalize(u_camPosWorldSpace - vertexPositionWorld);
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
	vec3 lightDirTangentSpace = normalize(TBN * correctedLightDir.rgb);
//	float diffuseFactor = max(dot(-normalWorldSpace, TBN*-lightDirTangentSpace), 0.0);

		float diffuseFactor = max(dot(-normalWorldSpace, lightDir), 0.0);
//		float diffuseFactor = max(dot(lightDirTangentSpace, lightDir), 0.0);


	vec3 diffuse = diffuseFactor *
	u_directionalLightColor.rgb *
	u_directionalLightColor.a *
	baseColor;

	// Specular Highlights
	vec3 H = normalize(viewDir + correctedLightDir); // Halfway vector
	vec3 F0 = mix(vec3(0.04), baseColor, metallicValue * 5); // Base reflectance (metallic or dielectric)
	vec3 specular = SpecularReflectionGGX(normalWorldSpace, viewDir, correctedLightDir, H, F0, 5);
	specular = specular * u_directionalLightColor.rgb * u_directionalLightColor.a * 5;


	// Shadows
	float shadow = 0.0;

	if (u_hasShadowmapTexture == 1) {
//		if (u_smoothShadows == 1) {
			shadow = ShadowCalculationPCFConstantQuality();
//		}
//		else {
//			shadow = OldShadowCalculation();
//		}
	}		
	shadow = ShadowCalculationPCFConstantQuality();


	// Subtract shadow influence for direct lighting
	//	vec3 lighting = ambient + (diffuse + specular) *
	vec3 lighting = ambient + (diffuse) *
	(1.0 - shadow);

	vec3 pointLight = calculatePointLightsLighting(normalWorldSpace, viewDir, vertexPositionWorld);
	lighting += pointLight;

	// Environmental Reflections
	vec3 reflection = vec3(0.0);
	//	if (metallicValue > 0.0) {

	vec3 reflectionI = normalize(vertexPositionWorld - u_camPosWorldSpace);
	vec3 reflectionDir = reflect(reflectionI, normalize(normalWorldSpace));

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

	if (u_fogEnabled == 1)
	{
		float distanceToVertex = distance(u_camPosWorldSpace.xyz, vertexPositionWorld.xyz);
		//		float distanceToVertex = distance(u_camPosWorldSpace.xz, vertexPositionWorld.xz); // no y axis fog
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
		fogFactor = fogFactor * u_fogIntensity * finalFogColor.a;

		fogFactor = pow(fogFactor, 5);

		color.rgb = mix(color.rgb, finalFogColor.rgb, fogFactor);
	}

	// Final Conversion to SRGB
	color = LinearToSRGB(color);

	float alpha = albedo.a;

	if (u_hasAlphaMaskTexture == 1) {
		vec4 alphaMask = texture(u_alphaMaskTexture, uvCoords);
		alphaMask.rgb *= alphaMask.a;
		alpha = (alphaMask.r + alphaMask.g + alphaMask.b) / 3;
		//		
		//		alpha=alphaMask.a;
		//		
		//		color = alphaMask.rgb;
	}


	if (alpha < 0.9) {
		discard;
	}
	// Final Output
	if (u_renderMode == 0) // regular
	{
		fragColor = vec4(color, 1);

		//		fragColor = vec4(color, alpha);
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
		fragColor = vec4(normalWorldSpace, 1);
	}
	else if (u_renderMode == 4) // directional light diffuse visualisation
	{
		float light = (diffuseFactor) * u_directionalLightColor.a;
		fragColor = vec4(vec3(light), 1);
	}
	else if (u_renderMode == 5) // directional light specular
	{
		fragColor = vec4(specular, 1);
	}
	else if (u_renderMode == 6) // shadows
	{

		//		shadow = OldShadowCalculation();
		//		shadow = ShadowCalculationPCFDistanceToCamera();


		if (u_hasShadowmapTexture == 1) {
//			if (u_smoothShadows == 1) {
				shadow = ShadowCalculationPCFConstantQuality();
//			}
//			else {
//				shadow = OldShadowCalculation();
//			}
		}
		fragColor = vec4(vec3(1 - shadow), 1);
	}
	else if (u_renderMode == 7) // ambient+albedo
	{
		fragColor = vec4(ambient, 1);
	}
	else if (u_renderMode == 8) // depth
	{
		float z = gl_FragCoord.z / gl_FragCoord.w;
		fragColor = vec4(vec3(z / u_cameraFrustumLength), 1);
		//		float z = gl_FragCoord.w ;
		//		fragColor = vec4(vec3(z),1);
	}
	else if (u_renderMode == 9)  // mouse picking
	{
		float a = float((v_id >> 24) & 0xFFu) / 255.0; // Extract alpha (highest byte)
		float r = float((v_id >> 16) & 0xFFu) / 255.0; // Extract red (highest byte)
		float g = float((v_id >> 8) & 0xFFu) / 255.0;  // Extract green (middle byte)
		float b = float(v_id & 0xFFu) / 255.0;         // Extract blue (lowest byte)

		fragColor = vec4(r, g, b, a); // RGB color with alpha = 1.0
	}
}