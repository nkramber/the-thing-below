// The mosaic (D-195, D-934). The blocks of the map grow from 1 art pixel to 32, each block in the
// color of the art pixel at its middle, so each block keeps a palette color (G-27). In the last
// quarter, the cover closes over the blocks in a dither.
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/transition_common.gdshaderinc"

// The count of sizes of a block: 1, 2, 4, 8, 16, and 32 art pixels.
const float SIZES = 6.0;

// The part of the transition where the cover starts to close.
const float CLOSE = 0.75;

void fragment() {
	vec2 art = art_of(UV);
	float size = pow(2.0, min(floor(progress * SIZES), SIZES - 1.0));
	vec2 middle = (floor(art / size) * size) + floor(size * 0.5);
	vec3 color = size < 2.0 ? texture(frame_under, SCREEN_UV).rgb : frame_at(middle);
	COLOR = vec4(dither_cover(art, (progress - CLOSE) / (1.0 - CLOSE), color), 1.0);
}
