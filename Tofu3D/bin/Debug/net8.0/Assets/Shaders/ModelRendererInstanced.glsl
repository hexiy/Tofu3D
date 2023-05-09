[BUFFERTYPE: Model]
[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec2 a_uv;
layout (location = 2) in vec3 a_normal;
layout (location = 3) in mat4 a_model;
layout (location = 7) in vec4 a_color;

out vec3 vertexPositionWorld;
out vec3 normal;
out vec2 uv;
out vec4 color;

        uniform mat4 u_viewProjection;
void main(void)
{
        mat4 mvp = u_viewProjection * a_model;
//        newPos = a_pos.xyz + vec3(3,3,3)* gl_InstanceID;
//gl_Position = a_mvp * vec4(newPos.xyz, 1.0);
gl_Position = mvp * vec4(a_pos.xyz, 1.0);
uv = a_uv;
color = a_color;
//        color = a_color;
//        instanceId =  gl_InstanceID;
}

[FRAGMENT]
#version 410 core 

 uniform vec2 u_tiling;
uniform vec2 u_offset;
uniform vec4 u_ambientLightColor;

uniform sampler2D textureObject;
uniform sampler2D shadowMap;
in vec3 normal;
in vec2 uv;
in vec3 vertexPositionWorld;
in vec4 color;

out vec4 frag_color;
void main(void){
vec4 texturePixelColor = texture(textureObject, (uv + u_offset) * u_tiling);
vec4 result = texturePixelColor * color;
result *= vec4(u_ambientLightColor.rgb* u_ambientLightColor.a, 1);

result.a = texturePixelColor.a * color.a;

if (result.a < 0.05){
discard; // having this fixes transparency sorting but breaks debug depthmap
}
//result.r = instanceId/50;
//result.g = -instanceId/50;
frag_color = result;

gl_FragDepth = gl_FragCoord.z;
}
