[BUFFERTYPE: Model]
[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec2 a_uv;
layout (location = 2) in vec3 a_normal;

out vec3 fragPos;
out vec3 normal;
out vec2 uv;

uniform mat4 u_mvp = mat4(1.0);
uniform mat4 u_model = mat4(1.0);
uniform mat4 u_lightSpaceMatrix;
out vec4 FragPosLightSpace;

void main(void)
{
gl_Position = u_mvp * vec4(a_pos.xyz, 1.0);
fragPos = vec3(u_model * vec4(a_pos.xyz, 1.0));
normal = transpose(inverse(mat3(u_model))) * a_normal;
uv = a_uv;
}

[FRAGMENT]
#version 410 core 

uniform vec4 u_rendererColor;
uniform vec2 u_tiling;
uniform vec2 u_offset;

uniform vec3 u_ambientLightsColor;
uniform float u_ambientLightsIntensity;

uniform vec3 u_directionalLightColor = vec3(1, 0, 0);
uniform float u_directionalLightIntensity = 1;
uniform vec3 u_directionalLightDirection = vec3(1, 0, 0);
uniform float u_smoothShading = 0;

uniform sampler2D textureObject;
uniform sampler2D shadowMap;


in vec3 normal;
in vec2 uv;
in vec3 fragPos;
in vec4 FragPosLightSpace;

out vec4 frag_color;

void main(void)
{
vec4 texturePixelColor = texture(textureObject, (uv + u_offset) * u_tiling);
vec4 result = vec4(1, 1, 1, 1);
result *= texturePixelColor.rgba;
result *=  u_rendererColor.rgba;

result.a = (texturePixelColor.rgba * u_rendererColor.rgba).a;
if (result.a == 0){
discard;
}
frag_color = result;
//gl_FragDepth = gl_FragCoord.z;
}
