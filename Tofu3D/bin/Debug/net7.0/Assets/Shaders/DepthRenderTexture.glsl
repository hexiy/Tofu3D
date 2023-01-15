[BUFFERTYPE:RenderTexture]
[VERTEX]
#version 410 core

layout (location = 0) in vec2 position;
layout (location = 1) in vec2 aTexCoord;

uniform mat4 u_mvp = mat4(1.0);

out vec2 texCoord;

void main(void)
{
texCoord = aTexCoord;
//gl_Position =u_mvp *  vec4(vec3(position.xy,1),1);

vec4 newPosition = vec4(position.x,position.y,0,1);
gl_Position = u_mvp * (newPosition);
//gl_Position = u_mvp * vec4(1, 1, 1, 1);
}

[FRAGMENT]
#version 410 core
in vec2 texCoord;
uniform float time;
uniform sampler2D textureObject;
layout (location = 0) out vec3 color;

void main(void)
{
  //  color = vec4(0,1,1, 1);

   float depthValue = texture(textureObject, texCoord+0.5).r;// why tf does it not render anything when i try to sample texture
    color =vec3(1-depthValue);

  //  color = vec4(0,1,1, 1);
}