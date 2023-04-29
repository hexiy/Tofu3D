[BUFFERTYPE: Model]
[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec2 a_uv;
layout (location = 2) in vec3 a_normal;

out vec3 vertexPositionWorld;
out vec3 normal;
out vec2 uv;

uniform mat4 u_mvp = mat4(1.0);
uniform mat4 u_model = mat4(1.0);
uniform mat4 u_lightSpaceMatrix;
out vec4 FragPosLightSpace;

void main(void)
{
gl_Position = u_mvp * vec4(a_pos.xyz, 1.0);
vertexPositionWorld = vec3(u_model * vec4(a_pos.xyz, 1.0));
normal = transpose(inverse(mat3(u_model))) * a_normal;
FragPosLightSpace = u_lightSpaceMatrix * vec4(a_pos.xyz, 1.0);
uv = a_uv;
}

[FRAGMENT]
#version 410 core 

 uniform vec4 u_rendererColor;
uniform vec2 u_tiling;
uniform vec2 u_offset;

uniform vec3 u_camPos;
uniform vec3 u_ambientLightsColor;
uniform float u_ambientLightsIntensity;

uniform vec3 u_directionalLightColor = vec3(1, 0, 0);
uniform float u_directionalLightIntensity = 1;
uniform vec3 u_directionalLightDirection = vec3(1, 0, 0);
uniform float u_smoothShading = 0;

uniform sampler2D textureObject;
uniform sampler2D shadowMap;


in vec3 normal;
in vec2 uv;
in vec3 vertexPositionWorld;
in vec4 FragPosLightSpace;

out vec4 frag_color;

float ShadowCalculation(vec4 _fragPosLightSpace)
{
// perform perspective divide
vec3 projCoords = _fragPosLightSpace.xyz / _fragPosLightSpace.w;
// transform to [0,1] range
projCoords = projCoords * 0.5 + 0.5;
// get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
float closestDepth = texture(shadowMap, projCoords.xy).r;
// get depth of current fragment from light's perspective
float currentDepth = projCoords.z;
// check whether current frag pos is in shadow
//    float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;

float bias = - 0.003;

float shadow = currentDepth - bias > closestDepth ? 1.0: 0.0;

return shadow;
}

void main(void)
{
vec3 norm = normalize(- normal);

float directionalLightStrength = max(dot(norm, u_directionalLightDirection), 0.0) * (1 - u_smoothShading);
vec3 dirColor = vec3(directionalLightStrength * u_directionalLightIntensity * u_directionalLightColor);


vec3 ambColor = vec3(u_ambientLightsColor * u_ambientLightsIntensity);

float shadow = 1 - ShadowCalculation(FragPosLightSpace);
//if (shadow < 0.8 || (dirColor.r + dirColor.g + dirColor.b) / 3 < 0.3)
//{
//shadow += (ambColor.r + ambColor.g+ ambColor.b) / 3;
//}
//
//if (shadow == 0.0)
//{
//shadow = 1;
//dirColor = dirColor * ambColor;
//}
//
//if (directionalLightStrength < 1)
//{
//float x = 1 - directionalLightStrength;
//dirColor += ambColor * x;
//}


vec4 texturePixelColor = texture(textureObject, (uv + u_offset) * u_tiling);
vec4 result = texturePixelColor * u_rendererColor;

result.a = texturePixelColor.a * u_rendererColor.a;

//
//float specularStrength = 0.5f;
//vec3 viewDir = normalize(vertexPositionWorld- u_camPos);
//vec3 reflectDir = reflect(u_directionalLightDirection, normalize(normal));
//float spec = pow(max(dot(viewDir, reflectDir), 0.0), 50);
//vec3 specular = specularStrength * spec * u_directionalLightColor;
        
        vec3 reflectedLightVectorWorld = reflect(-u_directionalLightDirection, normal);
        vec3 eyeVectorWorld = normalize(u_camPos - vertexPositionWorld);
        float s = clamp(dot(reflectedLightVectorWorld, eyeVectorWorld),0,1);
        s = pow(s,50);
        vec4 specular = vec4(s,s,s,1);
vec3 lighting = dirColor.rgb + ambColor.rgb + specular.rgb;
//vec3 lighting = specular.rgb;

result *= vec4(lighting.rgb, 1);



        
//if (result.b > 1){
//result = result * (1 / result.b);
//}
//if (result.g > 1){
//result = result * (1 / result.g);
//}
//if (result.r > 1){
//result = result * (1 / result.r);
//}
//        result = clamp(result,0,1);



//result.a = (texturePixelColor.rgba * u_rendererColor.rgba).a;
if(result.a < 0.05){
discard; // having this fixes transparency sorting but breaks debug depthmap
}
//vec4 result = vec4(((dirColor.rgb * shadow)) * ccc.rgb * ambColor.rgb, ccc.a);

frag_color = result;
gl_FragDepth = gl_FragCoord.z;
}
