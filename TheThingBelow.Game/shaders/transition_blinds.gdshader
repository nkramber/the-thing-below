// The blinds (D-195, D-934). The cover closes down over each band of 24 art pixels, as blinds do,
// and each band starts a little after the one above it.
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/transition_common.gdshaderinc"

// The height of one band, in art pixels. The frame holds 15 bands.
const float BAND = 24.0;

// The part of the transition that the start of the last band waits.
const float LAG = 0.4;

void fragment() {
	vec2 art = art_of(UV);
	float band = floor(art.y / BAND);
	float start = (band / ((ART.y / BAND) - 1.0)) * LAG;
	float closed = clamp((progress - start) / (1.0 - LAG), 0.0, 1.0) * BAND;
	bool covered = mod(art.y, BAND) < ceil(closed);
	COLOR = vec4(covered ? cover.rgb : texture(frame_under, SCREEN_UV).rgb, 1.0);
}
