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
		
gl_Position = u_mvp * vec4(position.x, position.y, 0.0, 1.0);// * vec4(2,2,1,1);
}

[FRAGMENT]
#version 410 core 
 
in vec2 texCoord;
uniform sampler2D textureObject;
uniform sampler2D bloomThresholdTexture;

layout (location = 0) out vec4 color;

void main(void)
{
//vec4 texColor1 = texture(textureObject, texCoord);
//vec4 texColor2 = texture(bloomThresholdTexture, texCoord);
////		color = texColor1 * texColor2;
//		color = texColor2*5;
////color = vec4(100,0,0.2,255);

// Sample the original scene texture
vec4 sceneColor = texture(textureObject, texCoord);

// Sample the bloom texture (bright areas blurred)
vec4 bloomColor = texture(bloomThresholdTexture, texCoord);

// Combine the two textures by adding them (Additive Bloom)
color = bloomColor * 3.0;  // Adjust bloom intensity with a factor like 5.0
}