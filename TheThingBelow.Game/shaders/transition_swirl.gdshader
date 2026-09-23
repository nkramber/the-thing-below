// The swirl (D-195, D-940), the transition of an elite fight. The map turns around the middle of
// the view, faster near the middle, and the cover closes over it in a dither in the second half.
// Each art pixel reads one art pixel of the map, so each pixel keeps a palette color (G-27).
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/transition_common.gdshaderinc"

// The most turn at the middle, in radians.
const float TURN = 9.0;

// The part of the transition where the cover starts to close.
const float CLOSE = 0.5;

void fragment() {
	vec2 art = art_of(UV);
	vec2 middle = ART * 0.5;
	vec2 offset = (art + 0.5) - middle;
	float reach = clamp(length(offset) / length(middle), 0.0, 1.0);
	float angle = TURN * progress * progress * (1.0 - reach) * (1.0 - reach);
	vec2 turned = vec2((offset.x * cos(angle)) - (offset.y * sin(angle)), (offset.x * sin(angle)) + (offset.y * cos(angle)));
	vec3 color = frame_at(floor(middle + turned));
	COLOR = vec4(dither_cover(art, (progress - CLOSE) / (1.0 - CLOSE), color), 1.0);
}
