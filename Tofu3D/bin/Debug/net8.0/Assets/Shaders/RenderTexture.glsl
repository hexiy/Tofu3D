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

//vec4 newPosition = vec4(position.x,position.y,0,1);
//gl_Position = u_mvp * (position-vec4(0.5,0.5,0,0));
//gl_Position = u_mvp * position;// * vec4(2,2,1,1);
gl_Position = u_mvp * vec4(position.x, position.y, 0.0, 1.0);// * vec4(2,2,1,1);
}

[FRAGMENT]
#version 410 core
in vec2 texCoord;
uniform sampler2D textureObject;
layout (location = 0) out vec4 color;

void main(void)
{

vec4 c = texture(textureObject, texCoord);
//		if(c.a <0.3){
//		discard;
//}
//		else{
color = vec4(c.r, c.g, c.b, c.a*0.7);
//}
		
//   FragColor = texture(textureObject, texCoord).rgb;
//color += vec3(0,texCoord.x,texCoord.y);
//color = vec3(0,posssss.x,posssss.y);
}