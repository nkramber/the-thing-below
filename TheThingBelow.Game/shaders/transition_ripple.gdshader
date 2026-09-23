// The ripple (D-195, D-940), the transition of the wrong things. Rings of waves spread from the
// middle and bend the map further as they go, and the cover closes over it in a dither in the
// second half. Each art pixel reads one art pixel of the map, so each pixel keeps a palette color
// (G-27).
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/transition_common.gdshaderinc"

// The distance from one ring to the next, in art pixels.
const float WAVE = 28.0;

// The most bend of a pixel, in art pixels.
const float BEND = 10.0;

// The count of rings that pass one place over the transition.
const float RINGS = 6.0;

// The part of the transition where the cover starts to close.
const float CLOSE = 0.5;

void fragment() {
	vec2 art = art_of(UV);
	vec2 middle = ART * 0.5;
	vec2 offset = (art + 0.5) - middle;
	float reach = length(offset);
	vec2 away = reach > 0.0 ? offset / reach : vec2(0.0);
	float bend = sin(((reach / WAVE) - (progress * RINGS)) * 6.2831853) * BEND * progress;
	vec3 color = frame_at(floor(middle + offset + (away * bend)));
	COLOR = vec4(dither_cover(art, (progress - CLOSE) / (1.0 - CLOSE), color), 1.0);
}
