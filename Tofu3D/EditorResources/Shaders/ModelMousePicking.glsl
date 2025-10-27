//[BUFFERTYPE: Model]
//[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec2 a_uv;
layout (location = 2) in vec3 a_normal;
layout (location = 3) in vec3 a_tangent;
layout (location = 4) in vec3 a_bitangent;
layout (location = 5) in vec3 a_model_1;
layout (location = 6) in vec3 a_model_2;
layout (location = 7) in vec3 a_model_3;
layout (location = 8) in vec3 a_model_4;
layout (location = 9) in float a_id;
layout (location = 10) in vec4 a_albedoBoundingBoxAndIndexInAtlas;
layout (location = 11) in vec2 a_uv_offset;

uniform mat4 u_viewProjection;

flat out uint v_id;
uniform float u_depthOverrideEnabled=0;
uniform float u_depthOverride;

void main(void)
{
	mat4 a_model = mat4(vec4(a_model_1, 0), vec4(a_model_2, 0), vec4(a_model_3, 0), vec4(a_model_4, 1));
	mat4 mvp = u_viewProjection * a_model;

	v_id = uint(a_id);

	gl_Position = mvp * vec4(a_pos.xyz, 1.0);
//
//	if(u_depthOverrideEnabled==1) {
//		gl_Position.z = (u_depthOverride * 2.0 - 1.0) * gl_Position.w;
//	}

}

//[FRAGMENT]
#version 410 core
layout (location = 0) out vec4 fragColor;
flat in uint v_id;
void main(void)
{
	float a = float((v_id >> 24) & 0xFFu) / 255.0; // highest byte
	float r = float((v_id >> 16) & 0xFFu) / 255.0; // red
	float g = float((v_id >> 8) & 0xFFu) / 255.0;  // green
	float b = float(v_id & 0xFFu) / 255.0;         // blue
	fragColor = vec4(r, g, b, a);
}




