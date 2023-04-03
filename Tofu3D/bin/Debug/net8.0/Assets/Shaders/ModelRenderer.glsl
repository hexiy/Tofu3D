[BUFFERTYPE: Model]
[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec3 a_normal;

out vec3 fragPos;
out vec3 normal;

uniform mat4 u_mvp = mat4(1.0);
uniform mat4 u_model = mat4(1.0);

out vec4 FragPosLightSpace;

uniform mat4 u_lightSpaceMatrix;


void main(void)
{



gl_Position = u_mvp * vec4(a_pos.xyz, 1.0);
fragPos = vec3(u_model * vec4(a_pos.xyz, 1.0));

//    fragPos = vec3(u_model * vec4(a_Pos, 1.0));
//normal=a_normal;
normal = transpose(inverse(mat3(u_model))) * a_normal;
// NEW
FragPosLightSpace = u_lightSpaceMatrix * vec4(fragPos, 1.0);
// NEW
}

[FRAGMENT]
#version 410 core
uniform vec4 u_rendererColor;

uniform vec3 u_ambientLightsColor;
// direction too
uniform float u_ambientLightsIntensity;

uniform vec3 u_directionalLightColor = vec3(1, 0, 0);
uniform float u_directionalLightIntensity = 1;
uniform vec3 u_directionalLightDirection = vec3(1, 0, 0);

//uniform vec3 u_pointLightLocations[100];
//uniform vec3 u_pointLightColors[100];
//uniform float u_pointLightIntensities[100];

out vec4 frag_color;
uniform sampler2D textureObject;
uniform sampler2D shadowMap;

in vec3 normal;
in vec3 fragPos;
in vec4 FragPosLightSpace;

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

float bias = - 0.005;

float shadow = currentDepth - bias > closestDepth ? 1.0: 0.0;

return shadow;
}

//void main()
//{           
//    vec3 color = u_rendererColor.rgb;
//    vec3 norm = normalize(normal);
//    vec3 lightColor = u_ambientLightsColor;
//    // ambient
//    vec3 ambient = 0.15 * lightColor;
//    // calculate shadow
//    float shadow = ShadowCalculation(FragPosLightSpace);       
//    
//    vec3 directionLight =  max(dot(norm, -u_directionalLightDirection), 0.0) * u_directionalLightIntensity * u_directionalLightColor;
//    vec3 lighting = (ambient + (shadow) ) * color;    
//   // lighting += directionLight;
//    
//    // diff += d * u_directionalLightColor;
//    
//    frag_color = vec4(lighting, 1.0);
//}
//
void main(void)
{
// we need to rotate normals....
vec3 norm = normalize(normal);

/**for(int i =0;i<u_pointLightLocations.length();i++){
vec3 lightDir = normalize(u_pointLightLocations[i] - fragPos); 

float d = max(dot(norm, lightDir), 0.0) * u_pointLightIntensities[i];
 diff += d * u_pointLightColors[i];

}*/

//vec3 dirColor = u_directionalLightIntensity * u_directionalLightColor;
vec3 dirColor = vec3(max(dot(norm, u_directionalLightDirection), 0.0) * u_directionalLightIntensity * u_directionalLightColor);


//result += vec4((u_ambientLightsColor * u_rendererColor.rgb* u_ambientLightsIntensity) + (1 - shadow),0);
vec3 ambColor = vec3(u_ambientLightsColor * u_ambientLightsIntensity);

float shadow = 1;
//float shadow = ShadowCalculation(FragPosLightSpace);

vec4 result = vec4(dirColor, u_rendererColor.a);

frag_color = result;
gl_FragDepth = gl_FragCoord.z;

}
