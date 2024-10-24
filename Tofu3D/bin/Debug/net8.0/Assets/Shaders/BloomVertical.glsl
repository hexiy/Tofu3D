[BUFFERTYPE: RenderTexture]
[VERTEX]
#version 410 core

layout (location = 0) in vec2 position;
layout (location = 1) in vec2 aTexCoord;

uniform mat4 u_mvp = mat4(1.0);

out vec2 texCoord;

void main(void)
{
texCoord = (aTexCoord * 0.5 + 0.5)*4;

gl_Position = u_mvp * vec4(position.x, position.y, 0.0, 1.0);// * vec4(2,2,1,1);
}

[FRAGMENT]
#version 410 core 

in vec2 texCoord;
uniform float texelWidth;  // 1 / w
uniform float texelHeight; // 1 / h
uniform sampler2D textureObject;

layout (location = 0) out vec4 color;
const float gaussian_21[21] = float[](
9.536743164e-7,
0.00001907348633,
0.0001811981201,
0.001087188721,
0.004620552063,
0.01478576660,
0.03696441650,
0.07392883301,
0.1201343536,
0.1601791382,
0.1761970520,
0.1601791382,
0.1201343536,
0.07392883301,
0.03696441650,
0.01478576660,
0.004620552063,
0.001087188721,
0.0001811981201,
0.00001907348633,
9.536743164e-7
);
void main(void)
{
vec2 offset = vec2(0.0, texelHeight);
		
vec4 result = texture(textureObject, texCoord) * gaussian_21[10]; // Center pixel
result += texture(textureObject, texCoord - offset * 10.0) * gaussian_21[0];
result += texture(textureObject, texCoord - offset * 9.0) * gaussian_21[1];
result += texture(textureObject, texCoord - offset * 8.0) * gaussian_21[2];
result += texture(textureObject, texCoord - offset * 7.0) * gaussian_21[3];
result += texture(textureObject, texCoord - offset * 6.0) * gaussian_21[4];
result += texture(textureObject, texCoord - offset * 5.0) * gaussian_21[5];
result += texture(textureObject, texCoord - offset * 4.0) * gaussian_21[6];
result += texture(textureObject, texCoord - offset * 3.0) * gaussian_21[7];
result += texture(textureObject, texCoord - offset * 2.0) * gaussian_21[8];
result += texture(textureObject, texCoord - offset * 1.0) * gaussian_21[9];







result += texture(textureObject, texCoord + offset * 1.0) * gaussian_21[10];
result += texture(textureObject, texCoord + offset * 2.0) * gaussian_21[11];
result += texture(textureObject, texCoord + offset * 3.0) * gaussian_21[12];
result += texture(textureObject, texCoord + offset * 4.0) * gaussian_21[13];
result += texture(textureObject, texCoord + offset * 4.0) * gaussian_21[14];
result += texture(textureObject, texCoord + offset * 5.0) * gaussian_21[15];
result += texture(textureObject, texCoord + offset * 6.0) * gaussian_21[16];
result += texture(textureObject, texCoord + offset * 7.0) * gaussian_21[17];
result += texture(textureObject, texCoord + offset * 8.0) * gaussian_21[18];
result += texture(textureObject, texCoord + offset * 9.0) * gaussian_21[19];
result += texture(textureObject, texCoord + offset * 10.0) * gaussian_21[20];

color = result;
}