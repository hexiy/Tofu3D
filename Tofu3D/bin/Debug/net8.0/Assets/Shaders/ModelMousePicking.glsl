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
layout (location = 10) in vec2 a_uv_offset;
uniform mat4 u_viewProjection;

flat out float v_id;

void main(void)
{
	mat4 a_model = mat4(vec4(a_model_1, 0), vec4(a_model_2, 0), vec4(a_model_3, 0), vec4(a_model_4, 1));
	mat4 mvp = u_viewProjection * a_model;

	v_id = a_id;

	gl_Position = mvp * vec4(a_pos.xyz, 1.0);
}

//[FRAGMENT]
#version 410 core
//layout (location = 0) out uint fragColor;
layout (location = 0) out vec4 fragColor;
flat in float v_id;
void main(void)
{
//	fragColor = uint(v_id);  must be integer framebuffer
//	fragColor = vec4(v_id, 0.0, 0.0, 1.0);
float r = float((int(v_id) & 0xFF000000) >> 24) / 255.0;
//float g = float((int(v_id) & 0x00FF0000) >> 16) / 255.0;
//float b = float((int(v_id) & 0x0000FF00) >> 8) / 255.0;
//float a = float(int(v_id) & 0x000000FF) / 255.0;
//
//fragColor = vec4(r, g, b, a);
fragColor = vec4(int(v_id)/255.0,0,0,0);
}
