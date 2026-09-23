// The vignette of PR-92, for the sweep of D-922. It is the shader of the game, with the values
// of the effect file of the game as defaults.
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/look_steps.gdshaderinc"

const vec2 SCENE = vec2(640.0, 360.0);

uniform vec4 color : source_color = vec4(0.05, 0.04, 0.08, 1.0);
uniform int strength = 5000;
uniform int start = 4000;

void fragment() {
	vec2 place = place_of(floor(UV * SCENE));
	vec2 offset = ((place / SCENE) * 2.0) - 1.0;
	float reach = length(offset) / sqrt(2.0);
	float fade = fade_of(smoothstep(float(start) / 10000.0, 1.0, reach));
	COLOR = vec4(color.rgb, fade * float(strength) / 10000.0);
}
