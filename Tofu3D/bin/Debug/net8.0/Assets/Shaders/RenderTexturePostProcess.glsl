﻿[BUFFERTYPE: RenderTexture]
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
layout (location = 0) out vec4 color;

void main(void)
{
vec4 texColor = texture(textureObject, texCoord);

vec2 relativePosition = texCoord.xy - 0.5;
float len = length(relativePosition);
float vignette = smoothstep(.9, .3, len);
texColor.rgb = mix(texColor.rgb, texColor.rgb * vignette, .7);
texColor.rgb = texColor.rgb * 1;


//        grayscale
//        float grayscale = (texColor.r+texColor.g+texColor.b)/3;
//        texColor.rgb = vec3(grayscale,grayscale,grayscale);


texColor.r = pow(texColor.r + 0.1, 2);
texColor.g = pow(texColor.g +0.1, 2);
texColor.b = pow(texColor.b + 0.1, 2);

texColor.a = 1;
color = texColor;
}