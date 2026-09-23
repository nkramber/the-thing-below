// The fade of the hand-off (D-938, D-939, D-863). The fight fades in from the cover color of its
// transition, and the map fades back in from the back cover after a win or a flee. At the reduced
// level and at off, the fade to the cover takes the place of the color split (D-863).
//
// The fade covers art pixels in the order of a dither of 4 by 4, so each pixel shows the frame or
// the cover, and no pixel blends the two (G-27). The fade never flashes, so it plays the same at
// each level of the reduction.
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/transition_common.gdshaderinc"

void fragment() {
	vec2 art = art_of(UV);
	COLOR = vec4(dither_cover(art, progress, texture(frame_under, SCREEN_UV).rgb), 1.0);
}
