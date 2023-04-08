[BUFFERTYPE: Model]
[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec3 a_normal;

out vec3 fragPos;
out vec3 normal;

uniform mat4 u_mvp = mat4(1.0);
uniform mat4 u_model = mat4(1.0);

out vec4 FragPosLightSpace;

uniform mat4 u_lightSpaceMatrix;


void main(void)
{



gl_Position = u_mvp * vec4(a_pos.xyz, 1.0);
fragPos = vec3(u_model * vec4(a_pos.xyz, 1.0));

//    fragPos = vec3(u_model * vec4(a_Pos, 1.0));
//normal=a_normal;
normal = transpose(inverse(mat3(u_model))) * a_normal;
// NEW
FragPosLightSpace = u_lightSpaceMatrix * vec4(fragPos, 1.0);
// NEW
}

[FRAGMENT]
#version 410 core
uniform vec4 u_rendererColor;

uniform vec3 u_ambientLightsColor;
// direction too
uniform float u_ambientLightsIntensity;

uniform vec3 u_directionalLightColor = vec3(1, 0, 0);
uniform float u_directionalLightIntensity = 1;
uniform vec3 u_directionalLightDirection = vec3(1, 0, 0);

//uniform vec3 u_pointLightLocations[100];
//uniform vec3 u_pointLightColors[100];
//uniform float u_pointLightIntensities[100];

out vec4 frag_color;
uniform sampler2D textureObject;
uniform sampler2D shadowMap;

in vec3 normal;
in vec3 fragPos;
in vec4 FragPosLightSpace;

void main(void)
{
// we need to rotate normals....
vec3 norm = normalize(normal);
		
vec3 dirColor = vec3(max(dot(norm, u_directionalLightDirection), 0.0));

vec3 ambColor = u_rendererColor.rgb * 0.85;

vec4 result = vec4(ambColor.rgb + (dirColor.rgb * (u_rendererColor.rgb)), u_rendererColor.a);

frag_color = result;
gl_FragDepth = gl_FragCoord.z;
}
