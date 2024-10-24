[BUFFERTYPE: RenderTexture]
[VERTEX]
#version 410 core

layout (location = 0) in vec2 position;
layout (location = 1) in vec2 aTexCoord;

uniform mat4 u_mvp = mat4(1.0);

out vec2 texCoord;

void main(void)
{
texCoord = aTexCoord * 0.5 + 0.5;
gl_Position = vec4(position, 0.0, 1.0);
}

[FRAGMENT]
#version 410 core 

in vec2 texCoord;
uniform sampler2D horizontalBlurTexture;
uniform sampler2D verticalBlurTexture;

layout (location = 0) out vec4 color;

void main(void)
{

vec4 horizontalBlur = texture(horizontalBlurTexture, texCoord);
vec4 verticalBlur = texture(verticalBlurTexture, texCoord);

color = (horizontalBlur + verticalBlur) * 0.5;
}