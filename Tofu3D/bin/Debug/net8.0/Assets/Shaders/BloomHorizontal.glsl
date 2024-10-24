[BUFFERTYPE: RenderTexture]
[VERTEX]
#version 410 core

layout (location = 0) in vec2 position;
layout (location = 1) in vec2 aTexCoord;

uniform mat4 u_mvp = mat4(1.0);

out vec2 texCoord;

void main(void)
{
texCoord = (aTexCoord * 0.5 + 0.5) *4;

gl_Position = u_mvp * vec4(position.x, position.y, 0.0, 1.0);// * vec4(2,2,1,1);
}

[FRAGMENT]
#version 410 core 

in vec2 texCoord;
uniform float texelWidth; // 1 / w
uniform float texelHeight; // 1 / h
uniform sampler2D textureObject;

layout (location = 0) out vec4 color;

void main(void)
{
vec2 offset = vec2(texelWidth, 0.0);


float kernel[9] = float[](0.02414, 0.10798, 0.15, 0.20778, 0.32021, 0.20778, 0.15, 0.10798, 0.02414);

vec4 result = texture(textureObject, texCoord) * kernel[4]; // Center pixel
result += texture(textureObject, texCoord - offset * 4.0) * kernel[0]; // Left 4
result += texture(textureObject, texCoord - offset * 3.0) * kernel[1]; // Left 3
result += texture(textureObject, texCoord - offset * 2.0) * kernel[2]; // Left 2
result += texture(textureObject, texCoord - offset * 1.0) * kernel[3]; // Left 1
result += texture(textureObject, texCoord + offset * 1.0) * kernel[5]; // Right 1
result += texture(textureObject, texCoord + offset * 2.0) * kernel[6]; // Right 2
result += texture(textureObject, texCoord + offset * 3.0) * kernel[7]; // Right 3
result += texture(textureObject, texCoord + offset * 4.0) * kernel[8]; // Right 4

color = result;
}