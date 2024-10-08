[BUFFERTYPE: RenderTexture]
[VERTEX]
#version 410 core

layout (location = 0) in vec2 position;
layout (location = 1) in vec2 aTexCoord;

uniform mat4 u_mvp = mat4(1.0);
uniform float u_time = 0;

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
uniform float u_time = 0;

layout (location = 0) out vec4 color;

void main(void)
{
vec4 texColor = texture(textureObject, texCoord);
//if (texCoord.x < 0.5 + sin(texCoord.y * 50 + u_time) * 0.003* cos(u_time)){
//discard;
//}
//if ((texColor.r + texColor.g+ texColor.b) / 3 < 0.9){
//discard;
//}

vec3 contrastColor = texColor.rgb;

float contrastStrength = 0.1;
contrastColor.r = pow(texColor.r, contrastStrength);
contrastColor.g = pow(texColor.g, contrastStrength);
contrastColor.b = pow(texColor.b, contrastStrength);

vec2 relativePosition = texCoord.xy - 0.5;
float len = length(relativePosition);
float vignette = smoothstep(1.1, .3, len);
vec3 vignetteColor = mix(texColor.rgb, texColor.rgb * vignette, .3);


vec3 resultColor= vec3(1, 1, 1);
resultColor *= contrastColor;
resultColor *= vignetteColor;

color = vec4(resultColor, 1);
}