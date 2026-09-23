// The vignette of the HD-2D look (D-849, D-917, D-919). One rectangle covers the view of the
// scene in the frame, above the tilt-shift blur and under the marks and the UI, so no mark and no
// menu darkens (D-208, D-210).
//
// The dark starts at a part of the way from the middle of the view to a corner, and it grows to
// its full strength at the corners, in one palette color (D-181). The shader never reads TIME
// (F-100).
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/look_steps.gdshaderinc"

// The size of the scene, in art pixels (D-634).
const vec2 SCENE = vec2(640.0, 360.0);

// The palette color of the dark (D-181).
uniform vec4 color : source_color;

// The strength of the dark at the corners, in basis points. A strength of 0 draws nothing.
uniform int strength = 0;

// The part of the way from the middle of the view to a corner where the dark starts, in basis
// points.
uniform int start = 0;

void fragment() {
	vec2 place = place_of(floor(UV * SCENE));

	// The reach is 0 in the middle of the view and 1 at each corner. It follows the shape of the
	// frame, so the dark reaches each edge at the same part of the way.
	vec2 offset = ((place / SCENE) * 2.0) - 1.0;
	float reach = length(offset) / sqrt(2.0);
	float fade = fade_of(smoothstep(float(start) / 10000.0, 1.0, reach));
	COLOR = vec4(color.rgb, fade * float(strength) / 10000.0);
}
