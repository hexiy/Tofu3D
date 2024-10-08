[BUFFERTYPE: Cubemap]
[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;

out vec3 TexCoords;
uniform mat4 u_projection;
uniform mat4 u_view;

void main(void)
{
TexCoords = a_pos;
vec4 pos = u_projection * u_view * vec4(a_pos, 1.0);
gl_Position = pos.xyww;
}

[FRAGMENT]
#version 410 core 

out vec4 FragColor;
in vec3 TexCoords;
uniform samplerCube skybox;
void main(void)
{
vec4 col = texture(skybox, TexCoords);

FragColor = col;

//if (col.a < 0.1){
//discard;
//}
//else{
//FragColor = col;
//}
}