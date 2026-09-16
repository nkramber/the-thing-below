// The CRT pass: curvature, colour bleed, and the scanlines of D-240.
// It runs on the frame of 1280 by 720, before the fit (D-568).
shader_type canvas_item;

uniform sampler2D screen_tex : hint_screen_texture, repeat_disable, filter_nearest;
uniform float curvature = 0.08;
uniform float scanline_strength = 0.12;
uniform float bleed = 0.6;

void fragment() {
	vec2 centred = SCREEN_UV * 2.0 - 1.0;
	centred *= 1.0 + curvature * dot(centred, centred);
	vec2 warped = centred * 0.5 + 0.5;

	if (warped.x < 0.0 || warped.x > 1.0 || warped.y < 0.0 || warped.y > 1.0) {
		COLOR = vec4(0.0, 0.0, 0.0, 1.0);
	} else {
		vec2 px = SCREEN_PIXEL_SIZE;
		vec3 col;
		col.r = texture(screen_tex, warped + vec2(px.x * bleed, 0.0)).r;
		col.g = texture(screen_tex, warped).g;
		col.b = texture(screen_tex, warped - vec2(px.x * bleed, 0.0)).b;

		float line = mod(floor(warped.y / px.y), 2.0);
		col *= 1.0 - scanline_strength * line;
		COLOR = vec4(col, 1.0);
	}
}
