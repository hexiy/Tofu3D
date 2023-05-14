[BUFFERTYPE: Model]
[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 3) in vec3 a_model_1;
layout (location = 4) in vec3 a_model_2;
layout (location = 5) in vec3 a_model_3;
layout (location = 6) in vec3 a_model_4;

uniform mat4 u_viewProjection;

void main(void)
{
mat4 a_model = mat4(vec4(a_model_1, 0), vec4(a_model_2, 0), vec4(a_model_3, 0), vec4(a_model_4,1));
mat4 mvp = u_viewProjection * a_model;
gl_Position = mvp * vec4(a_pos.xyz, 1.0);
}

[FRAGMENT]
#version 410 core 

void main(void)
{
gl_FragDepth = gl_FragCoord.z;
}
