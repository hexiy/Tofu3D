//[BUFFERTYPE: Cubemap]
//[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;

out vec3 TexCoords;
uniform mat4 u_projection;
uniform mat4 u_view;

void main(void)
{
TexCoords = a_pos;
vec4 pos = u_projection * u_view * vec4(a_pos, 1.0);
gl_Position = pos.xyww;
}

//[FRAGMENT]
#version 410 core 

out vec4 FragColor;
in vec3 TexCoords;
uniform samplerCube skybox;
void main(void)
{
vec4 col = texture(skybox, TexCoords);
float desaturationFactor = 0.2;
// Calculate the grayscale intensity (luminance) of the color
float gray = dot(col.rgb, vec3(0.3, 0.59, 0.11));

// Linearly interpolate between the grayscale color and the original color
vec3 desaturatedColor = mix(col.rgb, vec3(gray), desaturationFactor);

// Set the final output color with the modified saturation
FragColor = vec4(desaturatedColor, col.a);
//FragColor = col;

//if (col.a < 0.1){
//discard;
//}
//else{
//FragColor = col;
//}
}