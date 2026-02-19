//[BUFFERTYPE: Cubemap]
//[VERTEX]
#version 410 core

layout (location = 0) in vec3 a_pos;

out vec3 TexCoords;
uniform mat4 u_projection;
uniform mat4 u_view;
uniform float u_dayNight;
uniform float u_time;

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
uniform float u_time;
uniform float u_dayNight;

vec3 hash(vec3 p)  // replace this by something better
{
	p = vec3(dot(p, vec3(127.1, 311.7, 74.7)),
	dot(p, vec3(269.5, 183.3, 246.1)),
	dot(p, vec3(113.5, 271.9, 124.6)));

	return -1.0 + 2.0 * fract(sin(p) * 43758.5453123);
}
// Flickering Stars shader from turanszkij
float noise(in vec3 p)
{
	vec3 i = floor(p);
	vec3 f = fract(p);

	vec3 u = f * f * (3.0 - 2.0 * f);

	return mix(mix(mix(dot(hash(i + vec3(0.0, 0.0, 0.0)), f - vec3(0.0, 0.0, 0.0)),
					   dot(hash(i + vec3(1.0, 0.0, 0.0)), f - vec3(1.0, 0.0, 0.0)),
					   u.x),
				   mix(dot(hash(i + vec3(0.0, 1.0, 0.0)), f - vec3(0.0, 1.0, 0.0)),
					   dot(hash(i + vec3(1.0, 1.0, 0.0)), f - vec3(1.0, 1.0, 0.0)),
					   u.x),
				   u.y),
			   mix(mix(dot(hash(i + vec3(0.0, 0.0, 1.0)), f - vec3(0.0, 0.0, 1.0)),
					   dot(hash(i + vec3(1.0, 0.0, 1.0)), f - vec3(1.0, 0.0, 1.0)),
					   u.x),
				   mix(dot(hash(i + vec3(0.0, 1.0, 1.0)), f - vec3(0.0, 1.0, 1.0)),
					   dot(hash(i + vec3(1.0, 1.0, 1.0)), f - vec3(1.0, 1.0, 1.0)),
					   u.x),
				   u.y),
			   u.z);
}


void main(void)
{

	vec2 uv = gl_FragCoord.xy / TexCoords.xy;
	uv = TexCoords.xy;
	vec3 stars_direction = normalize(TexCoords);
	float stars_threshold = 8.0f;    // modifies the number of stars that are visible
	float stars_exposure = 200.0f;    // modifies the overall strength of the stars
	float stars = pow(clamp(noise(stars_direction * 200.0f), 0.0f, 1.0f), stars_threshold) * stars_exposure;
	stars *= mix(0.4, 1.4, noise(stars_direction * 100.0f + vec3(u_time * 0.5)));    // time based flickering


	// skybox cubemap
	vec4 col = texture(skybox, TexCoords);
	float desaturationFactor = 0.2;
	// Calculate the grayscale intensity (luminance) of the color
	float gray = dot(col.rgb, vec3(0.3, 0.59, 0.11));
	// Linearly interpolate between the grayscale color and the original color
	vec3 desaturatedColor = mix(col.rgb, vec3(gray), desaturationFactor);
	// Set the final output color with the modified saturation
	//	FragColor = vec4(desaturatedColor, col.a);
	// animation phase 1: sky fades out in first half
	float skyBrightness = 1.0 - smoothstep(0.0, 0.5, u_dayNight);

	// animation phase 2: stars fade in in second half
	float starsBrightness = smoothstep(0.5, 1.0, u_dayNight);

	vec3 skyColor = desaturatedColor * skyBrightness;
	vec3 starsColor = vec3(stars) * starsBrightness;

	FragColor = vec4(skyColor + starsColor, 1.0);
}