/**
//[BUFFERTYPE: Box]
//[VERTEX]
#version 410 core

layout (location = 0) in vec4 position;
uniform mat4 u_mvp = mat4(1.0);
out vec4 fragColor;
uniform vec4 u_color;
uniform vec4 u_tint = vec4(1.0); // tint is for shader editing

void main(void)
{
gl_Position = u_mvp * position;
fragColor = vec4( u_color.r * u_tint.r, u_color.g * u_tint.g, u_color.b *u_tint.b, u_color.a * u_tint.a);
}

//[FRAGMENT]
#version 410 core
 in vec4 fragColor;
out vec4 color;

void main(void)
{
color = fragColor;
}*/
