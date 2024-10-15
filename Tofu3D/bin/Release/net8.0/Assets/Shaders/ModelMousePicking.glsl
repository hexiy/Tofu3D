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
out vec4 frag_color;

in vec3 normal;
in vec3 fragPos;
in vec4 FragPosLightSpace;

void main(void)
{
frag_color = u_rendererColor;
}
