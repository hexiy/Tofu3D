[BUFFERTYPE: Sprite]
[VERTEX]
#version 410 core

layout (location = 0) in vec2 a_pos;
layout (location = 1) in vec2 a_uv;

out vec2 uv;
uniform mat4 u_mvp = mat4(1.0);
uniform vec2 zoomAmount = vec2(1);
uniform mat4 u_unitScaleMatrix = mat4(1.0);

void main(void)
{
uv = a_uv / zoomAmount;
gl_Position = u_mvp * vec4(a_pos.xy, 1.0, 1.0);
}

[FRAGMENT]
#version 410 core

in vec2 uv;
uniform sampler2D textureObject;
uniform vec4 u_color = vec4(1.0);
uniform vec2 u_resolution = vec2(100.0);
uniform vec2 offset = vec2(0, 0);

uniform float isGradient = 0;
uniform vec4 u_color_a = vec4(1.0, 0.0, 0.0, 1.0);
uniform vec4 u_color_b = vec4(0.0, 1.0, 1.0, 1.0);

out vec4 color;
void main(void)
{
vec4 texColor = texture(textureObject, vec2(uv.x + offset.x / u_resolution.x, - uv.y - offset.y / u_resolution.y));

//texColor.rgb= texColor.rgb * u_color.a;
texColor.a = (texColor.r * texColor.g * texColor.b) + 0.3;

texColor.a = pow(texColor.a, 50); // make the brights brighter and darks darker
if (texColor.a < 1){
discard;
}
if (texColor.a > 1){
texColor.a = 1;
}
texColor.r = 1;
texColor.g = 1;
texColor.b = 1;
//if (texColor.a > 1){
//texColor.a = 1;
//}

//if (texColor.a < 1){
//discard;
//}
//else{

//texColor.a = pow(texColor.a,14); // make the brights brighter and darks darker

//texColor.a = texColor.a* 50;


if (isGradient == 1)
{
vec2 newUV = (uv.xy * 10)* vec2(u_resolution.x / u_resolution.y, 1.0);
//
color = mix(u_color_b,u_color_a, newUV.y * 10) * texColor;
}
else
{
color = vec4(1, 1, 1, texColor.a) * u_color;
}
}
//}
