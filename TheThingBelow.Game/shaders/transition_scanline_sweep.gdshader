// The scanline sweep (D-195, D-619, D-934). In the first half, a line sweeps down the view and
// covers every other row of art pixels. In the second half, a line sweeps up and covers the rest.
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/transition_common.gdshaderinc"

void fragment() {
	vec2 art = art_of(UV);
	float down = clamp(progress * 2.0, 0.0, 1.0) * ART.y;
	float up = ART.y - (clamp((progress * 2.0) - 1.0, 0.0, 1.0) * ART.y);
	bool even = mod(art.y, 2.0) < 1.0;
	bool covered = even ? art.y < ceil(down) : art.y >= floor(up);
	COLOR = vec4(covered ? cover.rgb : texture(frame_under, SCREEN_UV).rgb, 1.0);
}
