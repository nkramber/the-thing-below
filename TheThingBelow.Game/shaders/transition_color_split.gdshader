// The color split (D-195, D-940), the transition of an ambush. The red of the map pulls to the
// left and the blue to the right, and each art pixel then takes the nearest palette color, so no
// pixel leaves the palette (G-27). The cover closes over it in a dither in the last part. At the
// reduced level and at off, the fade takes its place, and this shader never draws (D-863).
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/transition_common.gdshaderinc"

// The most colors of the palette (D-181).
const int MOST_COLORS = 64;

// The most pull of each color, in art pixels.
const float PULL = 24.0;

// The part of the transition where the cover starts to close.
const float CLOSE = 0.6;

// The colors of the palette, in the order of the palette file, as Game reads them.
uniform vec4 palette[64];

// The count of colors of the palette.
uniform int palette_count = 0;

// Gives the palette color that lies nearest to one color.
vec3 nearest_of(vec3 color) {
	vec3 best = cover.rgb;
	float least = 1000.0;
	for (int index = 0; index < MOST_COLORS; index++) {
		if (index >= palette_count) {
			break;
		}

		vec3 gap = palette[index].rgb - color;
		float size = dot(gap, gap);
		if (size < least) {
			least = size;
			best = palette[index].rgb;
		}
	}

	return best;
}

void fragment() {
	vec2 art = art_of(UV);
	float pull = floor(progress * PULL);
	vec3 split = vec3(frame_at(art + vec2(pull, 0.0)).r, frame_at(art).g, frame_at(art - vec2(pull, 0.0)).b);
	vec3 color = pull < 1.0 ? texture(frame_under, SCREEN_UV).rgb : nearest_of(split);
	COLOR = vec4(dither_cover(art, (progress - CLOSE) / (1.0 - CLOSE), color), 1.0);
}
