// The pixel dissolve (D-195, D-934). The map goes out to the cover one block of 2 by 2 art pixels
// at a time, in a fixed scatter, so the same tick gives the same picture in each capture (D-172).
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/transition_common.gdshaderinc"

// The side of one block, in art pixels.
const float BLOCK = 2.0;

void fragment() {
	vec2 art = art_of(UV);
	bool gone = cell_value(floor(art / BLOCK), 1u) < progress;
	COLOR = vec4(gone ? cover.rgb : texture(frame_under, SCREEN_UV).rgb, 1.0);
}
