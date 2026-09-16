// A wipe transition. A full-screen pass that runs for the length of the wipe (D-160).
shader_type canvas_item;

uniform float progress : hint_range(0.0, 1.0) = 0.0;
uniform float softness : hint_range(0.01, 0.5) = 0.15;

void fragment() {
	float d = UV.x * 0.7 + UV.y * 0.3;
	float a = smoothstep(progress - softness, progress + softness, d);
	COLOR = vec4(0.0, 0.0, 0.0, 1.0 - a);
}
