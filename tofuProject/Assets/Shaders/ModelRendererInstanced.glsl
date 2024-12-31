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

uniform sampler2D u_albedoTexture;
uniform sampler2D u_normalTexture;
uniform sampler2D u_ambientOcclusionTexture;
uniform samplerCube u_environmentCubemap;
uniform sampler2D u_shadowmapTexture;
uniform sampler2D u_emissiveTexture;
uniform sampler2D u_metallicTexture;
uniform sampler2D u_roughnessTexture;

float ShadowCalculation() {
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

void main() {
	// UV Coordinates with tiling
	vec2 uvCoords = uv * u_tiling;

	// Albedo Color
	vec4 albedo = u_albedoTint;
	if (u_hasAlbedoTexture == 1) {
		albedo *= texture(u_albedoTexture, uvCoords);
	}

	// Normal Mapping
	vec3 finalNormal = normalize(TBN * normal);
	if (u_hasNormalTexture == 1) {
		vec3 texNormal = texture(u_normalTexture, uvCoords).rgb * 2.0 - 1.0; // Map [0,1] to [-1,1]
		finalNormal = normalize(TBN * texNormal);
	}

	// View and Light Directions
	vec3 viewDir = normalize(u_camPos - vertexPositionWorld);
	vec3 lightDir = normalize(-u_directionalLightDirection);
	vec3 correctedLightDir = u_directionalLightDirection * vec3(1, -1, 1); // what is this where is it flipping so that i need to flip it here? is the tbn incorrect?
	lightDir = correctedLightDir;

	// Metallic and Roughness Maps
	float metallicValue = u_metallic; // Default metallic value (uniform)
	if (u_hasMetallicTexture == 1) {
		metallicValue = texture(u_metallicTexture, uvCoords).r; // Metallic texture (red channel)
	}

	float roughnessValue = 1.0 - u_smoothness; // Default roughness from smoothness
	if (u_hasRoughnessTexture == 1) {
		roughnessValue = texture(u_roughnessTexture, uvCoords).r; // Roughness texture (red channel)
	}

	// Ambient Occlusion
	float ao = 1.0;
	if (u_hasAmbientOcclusionTexture == 1) {
		ao = texture(u_ambientOcclusionTexture, uvCoords).r; // AO texture affects ambient light
		ao = clamp(ao, 0.0, 1.0); // Ensure AO is within valid range
	}

	// Ambient Lighting
	vec3 ambient = u_ambientLightColor.rgb * u_ambientLightColor.a * ao;

	// Diffuse Lighting
	float diffuseFactor = max(dot(finalNormal, lightDir), 0.0);
	vec3 diffuse = diffuseFactor * u_directionalLightColor.rgb * u_directionalLightColor.a;

	// Specular Highlights
	vec3 reflectedLight = reflect(correctedLightDir, finalNormal);
	float specFactor = pow(max(dot(reflectedLight, viewDir), 0.0), 32.0 * (1.0 - roughnessValue)); // Roughness decreases intensity/sharpness
	vec3 specular = mix(vec3(0.04), u_directionalLightColor.rgb, metallicValue) * specFactor;

	// Shadows
	float shadow = (u_hasShadowmapTexture == 1) ? ShadowCalculation() : 0.0;

	// Subtract shadow influence for direct lighting
	vec3 lighting = ambient + (diffuse + specular) * (1.0 - shadow);

	// Environmental Reflections
	vec3 reflection = vec3(0.0);
	if (metallicValue > 0.0) {

		vec3 reflectionI = normalize(vertexPositionWorld - u_camPos);
		vec3 reflectionDir = reflect(reflectionI, normalize(normal));
		
		
//		vec3 reflectionDir = reflect(-viewDir, finalNormal);
		reflection = texture(u_environmentCubemap, reflectionDir).rgb;

		// Roughness reduces reflection intensity
		reflection *= mix(1.0, roughnessValue, roughnessValue);
		reflection = sRGBToLinear(reflection);
	}

	// Combine Lighting and Reflections
	vec3 color = lighting + reflection;

	// Emissive Lighting (if available)
	if (u_hasEmissiveTexture == 1) {
		color += texture(u_emissiveTexture, uvCoords).rgb * u_albedoTint.rgb;
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
}