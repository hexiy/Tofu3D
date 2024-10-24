﻿[BUFFERTYPE: Model]
[VERTEX]
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
layout (location = 9) in vec4 a_color;

uniform mat4 u_viewProjection;
uniform mat4 u_lightSpaceViewProjection;

out vec3 vertexPositionWorld;
out vec2 uv;
out vec3 normal;
out vec4 color;
out vec4 fragPosLightSpace;
out mat3 TBN;

void main(void)
{
mat4 a_model = mat4(vec4(a_model_1, 0), vec4(a_model_2, 0), vec4(a_model_3, 0), vec4(a_model_4, 1));
mat4 mvp = u_viewProjection * a_model;
gl_Position = mvp * vec4(a_pos.xyz, 1.0);
uv = a_uv * vec2(1,-1);
color = a_color;


vertexPositionWorld = vec3(a_model * vec4(a_pos.xyz, 1.0));
normal = transpose(inverse(mat3(a_model))) * a_normal;

mat4 lightMvp = u_lightSpaceViewProjection * a_model;
fragPosLightSpace = lightMvp * vec4(a_pos.xyz, 1.0);

// TBN for normal texture mapping
vec3 T = normalize(vec3(a_model * vec4(a_tangent,   0.0)));
vec3 B = normalize(vec3(a_model * vec4(a_bitangent, 0.0)));
vec3 N = normalize(vec3(a_model * vec4(a_normal,    0.0)));
TBN = mat3(T, B, N);
}

[FRAGMENT]
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

uniform float u_hasNormalTexture = 0;
uniform float u_hasAOTexture = 0;

uniform sampler2D textureAlbedo;
uniform sampler2D textureNormal;
uniform sampler2D textureAo;
uniform sampler2D shadowMap;

in vec3 normal;
in vec2 uv;
in vec3 vertexPositionWorld;
in vec4 color;
in vec4 fragPosLightSpace;
in mat3 TBN;

out vec4 frag_color;

		
		// 1 if in shadow-black, 0 if in light
float ShadowCalculation()
{
// perform perspective divide
vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
// transform to [0,1] range
projCoords = projCoords * 0.5 + 0.5;
// get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
float closestDepth = texture(shadowMap, projCoords.xy).r;
// get depth of current fragment from light's perspective
float currentDepth = projCoords.z;
// check whether current frag pos is in shadow
//    float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;

float bias = 0.0001;

float shadow = currentDepth - bias > closestDepth ? 1.0: 0.0;

if (projCoords.z > 1.0) // fixes dark border behind the light
{
shadow = 0.0;
}
return shadow;
}

void main(void)
{
vec2 uvCoords = (uv + u_offset) * u_tiling;


vec3 variableJustSoTextureNormalIsUsedByCompiler = texture(textureNormal, uvCoords).rgb;
		
		
vec3 vertexNormalTBN = normalize(TBN * normal);
        


vec3 texNormal = texture(textureNormal, uvCoords).rgb;
texNormal = texNormal * 2.0 - 1.0; // Normalizing the normal values from the texture
texNormal = normalize(TBN * -texNormal); // Transforming the normal values from the texture space to the world space
//norm = normalize(TBN * norm);
 float blendFactor = 0.8 * u_hasNormalTexture;
        blendFactor = 0;
 vec3 finalNormal = normalize(mix(vertexNormalTBN, texNormal, blendFactor));
        

vec4 albedoColor = texture(textureAlbedo, uvCoords) * u_albedoTint*color;

vec4 aoColor = texture(textureAo, uvCoords);
aoColor = mix(vec4(1,1,1,1),aoColor, u_hasAOTexture);


vec4 final_ambient = vec4(u_ambientLightColor.rgb * u_ambientLightColor.a, 1);
		
vec3 correctedLightDir = u_directionalLightDirection * vec3(1,-1,1); // what is this where is it flipping so that i need to flip it here? is the tbn incorrect?
vec3 lightDirTangent = normalize(TBN * -correctedLightDir.rgb);
float directionalLightFactor = max(dot(finalNormal, lightDirTangent), 0.0);
float directionalLightClampedIntensity = u_directionalLightColor.a / 8;
vec4 final_diffuse = vec4(directionalLightFactor * directionalLightClampedIntensity * u_directionalLightColor.rgb, 1);

//result *= ambient;

vec4 result = albedoColor * aoColor * max(final_ambient, final_diffuse) + min(final_ambient, final_diffuse);
result.a = albedoColor.a * color.a;


if (result.a < 0.05) {
discard; // having this fixes transparency sorting but breaks debug depthmap
}


if (u_specularHighlightsEnabled == 1) {
vec3 reflectedLightVectorWorld = reflect(correctedLightDir, finalNormal);
vec3 viewDir = normalize(u_camPos - vertexPositionWorld);
////////// problem is below
float clampedSpecularSmoothness= max(u_specularSmoothness,0);
float spec = pow(max(dot(viewDir, reflectedLightVectorWorld), 0.0), 32 * clampedSpecularSmoothness);
spec = max(spec,0);
vec3 specular = clampedSpecularSmoothness * spec * u_directionalLightColor.rgb * directionalLightClampedIntensity * 2;

//vec4 specular = vec4(u_directionalLightColor.rgb * s, 1);

//if (shadow == 0) {
//specular /= 3;
//}

result.rgb *= max(vec3(1), specular+1);//*normalize(albedoColor.rgb+vec3(0.3));

}

float shadow = ShadowCalculation(); // 1 if in shadow
//        shadow
if (shadow == 1) {
//result.rgb = result.rgb * 0.1;
//		result.rgb = vec3(1,0,0); // red
 result = albedoColor * aoColor *  final_ambient;

}
else{
result.rgb= result.rgb;
//result.rgb = vec3(0,1,0); // green

}
		




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



if (u_renderMode == 0) // regular
{
frag_color = result;
}
if (u_renderMode == 1) // positions
{
//frag_color = vec4(normalize(- vertexPositionWorld) * result.rgb, result.a);
frag_color = vec4(vertexPositionWorld, result.a);

//vec3 roundedPos = round(vertexPositionWorld/5)*5;
//frag_color = vec4(roundedPos, result.a);
}
if (u_renderMode == 2) // normals
{
//frag_color = vec4(normalize(- normal) * result.rgb, result.a);
//frag_color = vec4(normalize(- normal), result.a);
frag_color = vec4(finalNormal, result.a);
}
//	gl_FragDepth = gl_FragCoord.z;
}
