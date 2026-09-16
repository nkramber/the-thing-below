// Fog and mist, one of the four ambient kinds (D-187). A full-screen pass (D-523).
shader_type canvas_item;

uniform float density : hint_range(0.0, 1.0) = 0.35;
uniform vec3 fog_color : source_color = vec3(0.55, 0.60, 0.70);

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(41.3, 289.1))) * 43758.5453);
}

float value_noise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	f = f * f * (3.0 - 2.0 * f);
	float a = hash(i);
	float b = hash(i + vec2(1.0, 0.0));
	float c = hash(i + vec2(0.0, 1.0));
	float d = hash(i + vec2(1.0, 1.0));
	return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

void fragment() {
	vec2 q = UV * 6.0 + vec2(TIME * 0.05, TIME * -0.03);
	float n = 0.0;
	float amp = 0.5;
	for (int i = 0; i < 4; i++) {
		n += amp * value_noise(q);
		q *= 2.03;
		amp *= 0.5;
	}
	COLOR = vec4(fog_color, n * density);
}
