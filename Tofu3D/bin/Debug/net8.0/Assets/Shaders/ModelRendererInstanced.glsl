[BUFFERTYPE: Model]
[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec2 a_uv;
layout (location = 2) in vec3 a_normal;
layout (location = 3) in mat4 a_model;
layout (location = 7) in vec4 a_color;

uniform mat4 u_viewProjection;
uniform mat4 u_lightSpaceMatrix;

out vec3 vertexPositionWorld;
out vec3 normal;
out vec2 uv;
out vec4 color;
out vec4 fragPosLightSpace;

void main(void)
{
mat4 mvp = u_viewProjection * a_model;
gl_Position = mvp * vec4(a_pos.xyz, 1.0);
uv = a_uv;
color = a_color;


vertexPositionWorld = vec3(a_model * vec4(a_pos.xyz, 1.0));
normal = transpose(inverse(mat3(a_model))) * a_normal;
fragPosLightSpace = u_lightSpaceMatrix * vec4(a_pos.xyz, 1.0);
}

[FRAGMENT]
#version 410 core 

 uniform vec2 u_tiling;
uniform vec2 u_offset;
uniform vec4 u_ambientLightColor;

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
        
uniform sampler2D textureAlbedo;
//uniform sampler2D textureNormal;
uniform sampler2D textureAo;

in vec3 normal;
in vec2 uv;
in vec3 vertexPositionWorld;
in vec4 color;
in vec4 fragPosLightSpace;

out vec4 frag_color;

void main(void)
{

vec2 uvCoords = (uv+u_offset)*u_tiling;
float directionalLightClampedIntensity = u_directionalLightColor.a / 8;
vec3 norm = normalize(- normal);
        
//if(u_normalStrength!=0){
//}

float directionalLightFactor = max(dot(norm, u_directionalLightDirection), 0.0);
vec4 dirColor = vec4(directionalLightFactor * directionalLightClampedIntensity * u_directionalLightColor.rgb, 1);


vec4 texturePixelColor = texture(textureAlbedo, uvCoords);
vec4 result = texturePixelColor * color;

vec4 ambientLighting = vec4(u_ambientLightColor.rgb * u_ambientLightColor.a, 1);
result *= ambientLighting + dirColor;

result.a = texturePixelColor.a * color.a;

if (u_specularHighlightsEnabled == 1){
vec3 reflectedLightVectorWorld = reflect(- u_directionalLightDirection, norm);
vec3 viewDir = - normalize(u_camPos - vertexPositionWorld);

float spec = pow(max(dot(viewDir, reflectedLightVectorWorld), 0.0), 32);
vec3 specular = u_specularSmoothness * spec * u_directionalLightColor.rgb * directionalLightClampedIntensity * 10;
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

vec4 aoTexture = texture(textureAo, uvCoords);
result.rgb *= aoTexture.rgb;

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

gl_FragDepth = gl_FragCoord.z;
}
