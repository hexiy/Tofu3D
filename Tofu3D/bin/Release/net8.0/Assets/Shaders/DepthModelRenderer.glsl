[BUFFERTYPE: Model]
[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec3 a_normal;

uniform mat4 u_mvp = mat4(1.0);


void main(void)
{
gl_Position = u_mvp * vec4(a_pos.xyz, 1.0);

//	gl_Position = u_lightSpaceMatrix * u_model * vec4(a_pos, 1.0);

}

[FRAGMENT]
#version 410 core

void main(void)
{
//    gl_FragDepth = gl_FragCoord.z;
}
