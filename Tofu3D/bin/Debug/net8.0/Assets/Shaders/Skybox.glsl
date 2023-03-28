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

gl_Position = vec4(pos.x, pos.y, pos.w, pos.w);
//gl_Position = u_mvp * vec4(a_pos.xyz, 1.0);
}

[FRAGMENT]
out vec4 FragColor;

in vec3 TexCoords;

uniform samplerCube skybox;
void main(void)
{
vec4 col = texture(skybox, TexCoords);

if (col.a < 0.1){
discard;
}
else{
FragColor = col*0.3;
}

//FragColor = vec4(0,1,1,0);
}