[BUFFERTYPE:Model]
[VERTEX]
#version 410 core

layout(location = 0) in vec3 a_pos;
layout (location = 1) in vec3 a_normal;

out vec3 fragPos;  
 out vec3 normal;

uniform mat4 u_mvp = mat4(1.0);
uniform mat4 u_model = mat4(1.0);


void main(void)
{
gl_Position = u_mvp * vec4(a_pos.xyz, 1.0);
fragPos = vec3(u_model * vec4(a_pos.xyz, 1.0));

//    fragPos = vec3(u_model * vec4(a_Pos, 1.0));
normal=a_normal;
}

[FRAGMENT]
#version 410 core
uniform vec4 u_rendererColor;
uniform vec3 lightPos;  

out vec4 frag_color;
uniform sampler2D textureObject;
 in vec3 normal;  
in vec3 fragPos;  


void main(void)
{
vec3 norm = normalize(normal);
vec3 lightDir = normalize(lightPos - fragPos); 

float diff = max(dot(norm, lightDir), 0.0);
vec3 diffuse = diff * vec3(1,1,0);

vec4 result = u_rendererColor + vec4(diffuse,0);
frag_color  = result;
//frag_color = vec4(normal.x,normal.y,normal.z,1);
//vec4 texColor = texture(textureObject, texCoord);
//if (texColor.a < 0.1)
//{
//discard;
//}
//else
//{
//frag_color = texColor * u_rendererColor;
//}
}
