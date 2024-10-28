//[BUFFERTYPE: RenderTexture]
//[VERTEX]
#version 410 core

layout (location = 0) in vec2 position;
layout (location = 1) in vec2 aTexCoord;

uniform mat4 u_mvp = mat4(1.0);

out vec2 texCoord;

void main(void)
{
texCoord = (aTexCoord * 0.5 + 0.5);

gl_Position = u_mvp * vec4(position.x, position.y, 0.0, 1.0);// * vec4(2,2,1,1);
}

//[FRAGMENT]
#version 410 core 

in vec2 texCoord;
uniform float texelWidth; // 1 / w
uniform float texelHeight; // 1 / h
uniform sampler2D textureObject;

layout (location = 0) out vec4 color;
uniform float weights[5] = float[](0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

void main(void)
{
	vec2 offset = vec2(texelWidth, 0.0);
	vec4 result = texture(textureObject, texCoord) * weights[0];

	for (int i = 1; i < 5; ++i) {
		result += texture(textureObject, texCoord + offset * float(i)) * weights[i];
		result += texture(textureObject, texCoord - offset * float(i)) * weights[i];
	}

	color = result;
}