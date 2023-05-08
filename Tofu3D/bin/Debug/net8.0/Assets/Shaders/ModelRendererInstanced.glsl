[BUFFERTYPE: Model]
[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec2 a_uv;
layout (location = 2) in vec3 a_normal;
layout (location = 3) in vec3 a_translation;

        out float instanceId;
out vec3 vertexPositionWorld;
out vec3 normal;
out vec2 uv;

uniform mat4 u_mvp = mat4(1.0);

void main(void)
{
        vec3 newPos = a_pos.xyz + a_translation.xyz;
//        newPos = a_pos.xyz + vec3(3,3,3)* gl_InstanceID;
gl_Position = u_mvp * vec4(newPos.xyz, 1.0);
uv = a_uv;
        instanceId =  gl_InstanceID;
}

[FRAGMENT]
#version 410 core 

 uniform vec4 u_rendererColor;
uniform vec2 u_tiling;
uniform vec2 u_offset;

uniform sampler2D textureObject;
uniform sampler2D shadowMap;
        in float instanceId;
in vec3 normal;
in vec2 uv;
in vec3 vertexPositionWorld;

out vec4 frag_color;
void main(void){
vec4 texturePixelColor = texture(textureObject, (uv + u_offset) * u_tiling);
vec4 result = texturePixelColor * u_rendererColor;

result.a = texturePixelColor.a * u_rendererColor.a;

if (result.a < 0.05){
discard; // having this fixes transparency sorting but breaks debug depthmap
}
result.r = instanceId/50;
result.g = -instanceId/50;
frag_color = result;

gl_FragDepth = gl_FragCoord.z;
}
