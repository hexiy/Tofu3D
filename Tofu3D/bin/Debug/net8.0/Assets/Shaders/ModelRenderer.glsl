[BUFFERTYPE: Model]
[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec2 a_uv;
layout (location = 2) in vec3 a_normal;

out vec3 vertexPositionWorld;
out vec3 vertexPositionView;
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
vertexPositionView = vec3(u_mvp * vec4(a_pos.xyz, 1.0));
normal = transpose(inverse(mat3(u_model))) * a_normal;
FragPosLightSpace = u_lightSpaceMatrix * vec4(a_pos.xyz, 1.0);
uv = a_uv;
}

[FRAGMENT]
#version 410 core 

 uniform vec4 u_rendererColor;
uniform vec2 u_tiling;
uniform vec2 u_offset;

uniform float u_renderMode = 0;

uniform float u_fogEnabled = 0;
uniform vec4 u_fogColor = vec4(0, 0, 0, 1);
uniform vec4 u_fogColor2 = vec4(0, 0, 0, 1);
uniform float u_fogStartDistance = 0;
uniform float u_fogEndDistance = 1;
uniform float u_fogPositionY = 0;
uniform float u_fogGradientSmoothness = 1;

uniform vec3 u_camPos;
uniform vec3 u_ambientLightsColor;
uniform float u_ambientLightsIntensity;

uniform float u_time;

uniform vec3 u_directionalLightColor = vec3(1, 0, 0);
uniform float u_directionalLightIntensity = 1;
uniform vec3 u_directionalLightDirection = vec3(1, 0, 0);
uniform float u_smoothShading = 0;

uniform sampler2D textureObject;
uniform sampler2D shadowMap;

in vec3 normal;
in vec2 uv;
in vec3 vertexPositionWorld;
in vec3 vertexPositionView;
in vec4 FragPosLightSpace;

out vec4 frag_color;


// A single iteration of Bob Jenkins' One-At-A-Time hashing algorithm.
uint hash(uint x) {
x += (x << 10u);
x ^= (x >> 6u);
x += (x << 3u);
x ^= (x >> 11u );
x += (x << 15u);
return x;
}



// Compound versions of the hashing algorithm I whipped together.
uint hash(uvec2 v) { return hash(v.x ^ hash(v.y)); }
uint hash(uvec3 v) { return hash(v.x ^ hash(v.y) ^ hash(v.z)             ); }
uint hash(uvec4 v) { return hash(v.x ^ hash(v.y) ^ hash(v.z) ^ hash(v.w)); }



// Construct a float with half-open range [0:1] using low 23 bits.
// All zeroes yields 0.0, all ones yields the next smallest representable value below 1.0.
float floatConstruct(uint m) {
const uint ieeeMantissa = 0x007FFFFFu; // binary32 mantissa bitmask
const uint ieeeOne = 0x3F800000u; // 1.0 in IEEE binary32

m &= ieeeMantissa; // Keep only mantissa bits (fractional part)
m |= ieeeOne;                          // Add fractional part to 1.0

float  f = uintBitsToFloat(m); // Range [1:2]
return f - 1.0;                        // Range [0:1]
}



// Pseudo-random value in half-open range [0:1].
float random(float x) { return floatConstruct(hash(floatBitsToUint(x))); }
float random(vec2  v) { return floatConstruct(hash(floatBitsToUint(v))); }
float random(vec3  v) { return floatConstruct(hash(floatBitsToUint(v))); }
float random(vec4  v) { return floatConstruct(hash(floatBitsToUint(v))); }




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

void main(void){
vec3 norm = normalize(- normal);

float directionalLightStrength = max(dot(norm, u_directionalLightDirection), 0.0) * (1 - u_smoothShading);
vec3 dirColor = vec3(directionalLightStrength * u_directionalLightIntensity * u_directionalLightColor);


vec3 ambColor = vec3(u_ambientLightsColor * u_ambientLightsIntensity);

float shadow = 1 - ShadowCalculation(FragPosLightSpace);

vec4 texturePixelColor = texture(textureObject, (uv + u_offset) * u_tiling);
vec4 result = texturePixelColor * u_rendererColor;

result.a = texturePixelColor.a * u_rendererColor.a;

vec3 reflectedLightVectorWorld = reflect(- u_directionalLightDirection, normal);
vec3 eyeVectorWorld = normalize(u_camPos - vertexPositionWorld) * vec3(- 1, 1, 1);
float s = clamp(dot(reflectedLightVectorWorld, eyeVectorWorld),0, 1);
s = pow(s, 100);
vec4 specular = vec4(u_directionalLightColor.rgb * s, 1);
vec3 lighting = dirColor.rgb + ambColor.rgb + specular.rgb;

result *= vec4(lighting.rgb, 1);

if (result.a < 0.05){
discard; // having this fixes transparency sorting but breaks debug depthmap
}

if (u_fogEnabled == 1 && u_renderMode == 0)
{
float fogIntensity = u_fogColor.a;
float distanceToVertex = distance(u_camPos, vertexPositionWorld);
float fog = 0;
if (distanceToVertex / 100 > u_fogStartDistance){
fog = (distanceToVertex / 100) - u_fogStartDistance;
}


fog = fog / (u_fogEndDistance - u_fogStartDistance);

fog = clamp(fog, 0, 1);

float x = (vertexPositionWorld.y + u_fogPositionY) / u_fogGradientSmoothness;


if (distanceToVertex / 100 > u_fogEndDistance){
        float vertexFromFogCenter = vertexPositionWorld.y - u_fogPositionY;
        float cameraFromFogCenter = u_camPos.y- u_fogPositionY;
fog = 1;
    //        x = 0.1;
//        x=0;
}
fog = clamp(fog, 0, 1);

vec3 finalFogColor = mix(u_fogColor.rgb, u_fogColor2.rgb, x);

result.rgb = mix(result.rgb, finalFogColor.rgb, fog * fogIntensity);
}

if (u_renderMode == 0) // regular
{
frag_color = result;
}
if (u_renderMode == 1) // positions
{
frag_color = vec4(normalize(- vertexPositionWorld) * result.rgb, result.a);
}
if (u_renderMode == 2) // normals
{
frag_color = vec4(normalize(- normal) * result.rgb, result.a);
}
gl_FragDepth = gl_FragCoord.z;
}
