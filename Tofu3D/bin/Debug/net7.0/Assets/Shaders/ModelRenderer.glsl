[BUFFERTYPE: Model]
[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec2 a_uv;
layout (location = 2) in vec3 a_normal;

out vec3 vertexPositionWorld;
out vec3 normal;
out vec2 uv;

uniform mat4 u_mvp = mat4(1.0);
uniform mat4 u_model = mat4(1.0);
uniform mat4 u_lightSpaceMatrix;
out vec4 FragPosLightSpace;

void main(void)
{
        vec3 newPos = a_pos.xyz;
gl_Position = u_mvp * vec4(newPos.xyz, 1.0);
vertexPositionWorld = vec3(u_model * vec4(newPos.xyz, 1.0));
normal = transpose(inverse(mat3(u_model))) * a_normal;
FragPosLightSpace = u_lightSpaceMatrix * vec4(newPos.xyz, 1.0);
uv = a_uv;
}

[FRAGMENT]
#version 410 core 

 uniform vec4 u_rendererColor;
uniform vec2 u_tiling;
uniform vec2 u_offset;

uniform float u_renderMode = 0;

uniform float u_fogEnabled = 0;
uniform vec4 u_fogColor = vec4(0, 0, 0, 1);
uniform vec4 u_fogColor2 = vec4(0, 0, 0, 1);
uniform float u_fogStartDistance = 0;
uniform float u_fogEndDistance = 1;
uniform float u_fogPositionY = 0;
uniform float u_fogGradientSmoothness = 1;
uniform float u_fogIntensity = 1;

uniform vec3 u_camPos;
uniform vec3 u_ambientLightsColor;
uniform float u_ambientLightsIntensity;

uniform float u_time;

uniform vec3 u_directionalLightColor = vec3(1, 0, 0);
uniform float u_directionalLightIntensity = 1;
uniform vec3 u_directionalLightDirection = vec3(1, 0, 0);
uniform float u_smoothShading = 0;
uniform float u_specularSmoothness = 1;
uniform float u_specularHighlightsEnabled = 1;

uniform sampler2D textureObject;
uniform sampler2D shadowMap;

in vec3 normal;
in vec2 uv;
in vec3 vertexPositionWorld;
in vec4 FragPosLightSpace;

out vec4 frag_color;

float ShadowCalculation(vec4 _fragPosLightSpace)
{
// perform perspective divide
vec3 projCoords = _fragPosLightSpace.xyz / _fragPosLightSpace.w;
// transform to [0,1] range
projCoords = projCoords * 0.5 + 0.5;
// get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
float closestDepth = texture(shadowMap, projCoords.xy).r;
// get depth of current fragment from light's perspective
float currentDepth = projCoords.z;
// check whether current frag pos is in shadow
//    float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;

float bias = - 0.003;

float shadow = currentDepth - bias > closestDepth ? 1.0: 0.0;

return shadow;
}

void main(void){
        float directionalLightClampedIntensity = u_directionalLightIntensity /8;
vec3 norm = normalize(- normal);

float directionalLightFactor = max(dot(norm, u_directionalLightDirection), 0.0);
vec3 dirColor = vec3(directionalLightFactor * directionalLightClampedIntensity * u_directionalLightColor);


vec3 ambColor = vec3(u_ambientLightsColor * u_ambientLightsIntensity);

float shadow = 1 - ShadowCalculation(FragPosLightSpace);

vec4 texturePixelColor = texture(textureObject, (uv + u_offset) * u_tiling);
vec4 result = texturePixelColor * u_rendererColor;

result.a = texturePixelColor.a * u_rendererColor.a;


vec3 directionalAndAmbientLighting = dirColor.rgb + ambColor.rgb;
result *= vec4(directionalAndAmbientLighting.rgb, 1);


if(u_specularHighlightsEnabled == 1){
vec3 reflectedLightVectorWorld = reflect(-u_directionalLightDirection, norm);
vec3 viewDir = -normalize(u_camPos - vertexPositionWorld);
//vec3 viewDir = -normalize(u_camPos - vertexPositionWorld) ;//* vec3(- 1, 1, -1);
//vec3 viewDir = normalize(u_camPos - vertexPositionWorld) ;//* vec3(- 1, 1, -1);
//vec3 viewDir = normalize(u_camPos - vertexPositionWorld)* vec3(- 1, 1, -1);

float spec = pow(max(dot(viewDir, reflectedLightVectorWorld), 0.0), 32);
vec3 specular = u_specularSmoothness * spec * u_directionalLightColor.rgb * directionalLightClampedIntensity *10;
//vec4 specular = vec4(u_directionalLightColor.rgb * s, 1);

result.rgb+= specular;
}

if (result.a < 0.05){
discard; // having this fixes transparency sorting but breaks debug depthmap
}

if (u_fogEnabled == 1 && u_renderMode == 0)
{
float distanceToVertex = distance(u_camPos.xz, vertexPositionWorld.xz);
float fogFactor = 0;
if (distanceToVertex > u_fogStartDistance){
fogFactor = (distanceToVertex) - u_fogStartDistance;
}

fogFactor = fogFactor / (u_fogEndDistance - u_fogStartDistance);
fogFactor = clamp(fogFactor, 0, 1);

float gradientStep = (vertexPositionWorld.y - u_fogPositionY) / u_fogGradientSmoothness;

gradientStep = clamp(gradientStep,0, 1);
//        gradientStep = 1/gradientStep;

vec4 finalFogColor = mix(vec4(u_fogColor2.rgb * u_fogColor2.a, u_fogColor2.a), vec4(u_fogColor.rgb* u_fogColor.a, u_fogColor.a), gradientStep);

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
frag_color = vec4(vertexPositionWorld / 100, result.a);

//vec3 roundedPos = round(vertexPositionWorld/5)*5;
//frag_color = vec4(roundedPos/100, result.a);
}
if (u_renderMode == 2) // normals
{
//frag_color = vec4(normalize(- normal) * result.rgb, result.a);
//frag_color = vec4(normalize(- normal), result.a);
frag_color = vec4(normalize(normal), result.a);
}
//gl_FragDepth = gl_FragCoord.z;
}
