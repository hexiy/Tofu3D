[BUFFERTYPE: Model]
[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec2 a_uv;
layout (location = 2) in vec3 a_normal;
layout (location = 3) in mat4 a_model;
layout (location = 7) in vec4 a_color;

uniform mat4 u_viewProjection;
uniform mat4 u_lightSpaceMatrix;

out vec3 vertexPositionWorld;
out vec3 normal;
out vec2 uv;
out vec4 color;
out vec4 fragPosLightSpace;

void main(void)
{
mat4 mvp = u_viewProjection * a_model;
gl_Position = mvp * vec4(a_pos.xyz, 1.0);
uv = a_uv;
color = a_color;


vertexPositionWorld = vec3(a_model * vec4(a_pos.xyz, 1.0));
normal = transpose(inverse(mat3(a_model))) * a_normal;
fragPosLightSpace = u_lightSpaceMatrix * vec4(a_pos.xyz, 1.0);
}

[FRAGMENT]
#version 410 core 

uniform vec2 u_tiling;
uniform vec2 u_offset;
uniform vec4 u_ambientLightColor;

uniform vec3 u_camPos;
uniform vec4 u_directionalLightColor;
uniform vec3 u_directionalLightDirection = vec3(1, 0, 0);
uniform float u_specularSmoothness = 1;
uniform float u_specularHighlightsEnabled = 1;   
        
uniform sampler2D textureObject;
uniform sampler2D shadowMap;
        
in vec3 normal;
in vec2 uv;
in vec3 vertexPositionWorld;
in vec4 color;
in vec4 fragPosLightSpace;

out vec4 frag_color;
        
void main(void)
{

float directionalLightClampedIntensity = u_directionalLightColor.a / 8;
vec3 norm = normalize(- normal);

float directionalLightFactor = max(dot(norm, u_directionalLightDirection), 0.0);
vec4 dirColor = vec4(directionalLightFactor * directionalLightClampedIntensity * u_directionalLightColor.rgb,1);

        
        
vec4 texturePixelColor = texture(textureObject, (uv + u_offset) * u_tiling);
vec4 result = texturePixelColor * color;

vec4 ambientLighting = vec4(u_ambientLightColor.rgb * u_ambientLightColor.a, 1);
result *= ambientLighting + dirColor;

result.a = texturePixelColor.a * color.a;

if (u_specularHighlightsEnabled == 1){
vec3 reflectedLightVectorWorld = reflect(- u_directionalLightDirection, norm);
vec3 viewDir = - normalize(u_camPos - vertexPositionWorld);

float spec = pow(max(dot(viewDir, reflectedLightVectorWorld), 0.0), 32);
vec3 specular = u_specularSmoothness * spec * u_directionalLightColor.rgb * directionalLightClampedIntensity * 10;
//vec4 specular = vec4(u_directionalLightColor.rgb * s, 1);

result.rgb+= specular;
}
        
if (result.a < 0.05){
discard; // having this fixes transparency sorting but breaks debug depthmap
}
frag_color = result;

gl_FragDepth = gl_FragCoord.z;
}
