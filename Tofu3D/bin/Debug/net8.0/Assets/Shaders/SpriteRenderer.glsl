﻿[BUFFERTYPE: Sprite][VERTEX]#version 410 corelayout (location = 0) in vec2 a_pos;layout (location = 1) in vec2 a_uv;out vec2 uv;uniform mat4 u_mvp = mat4(1.0);void main(void){uv = a_uv;gl_Position = u_mvp * vec4(a_pos.xy*0.5, 1.0, 1.0);}[FRAGMENT]#version 410 core in vec2 uv;uniform sampler2D textureObject;uniform vec4 u_rendererColor;uniform vec2 u_tiling;uniform vec2 u_offset;out vec4 color;void main(void){vec4 texColor = texture(textureObject, vec2((uv.x + u_offset.x) /2 * u_tiling.x + 0.5, -(uv.y + u_offset.y) /2* u_tiling.y + 0.5)) * u_rendererColor;//vec4 texColor = u_rendererColor;//gl_FragDepth = gl_FragCoord.z;if (texColor.a < 0.01){discard;}else{color = texColor;}}