// The CRT power-off (D-195, D-619, D-934). In the first half, the map shrinks to the one row at the
// middle of the view. Then that row shrinks to a dot at the middle, and the dot goes out. Each art
// pixel reads one art pixel of the map, so each pixel keeps a palette color (G-27).
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/transition_common.gdshaderinc"

// The part of the transition where the map is one row.
const float LINE = 0.5;

// The part of the transition where the dot goes out.
const float DOT = 0.92;

void fragment() {
	vec2 art = art_of(UV);
	vec2 middle = floor(ART * 0.5);
	vec3 color = cover.rgb;
	if (progress < LINE) {
		// The height of the map shrinks from the whole view to one row.
		float height = max(1.0, ART.y * (1.0 - (progress / LINE)));
		float offset = art.y - middle.y;
		if (abs(offset + 0.5) < height * 0.5) {
			color = frame_at(vec2(art.x, middle.y + floor(offset * (ART.y / height))));
		}
	} else if (progress < DOT) {
		// The row shrinks to a dot at the middle.
		float half_width = max(1.0, (ART.x * 0.5) * (1.0 - ((progress - LINE) / (DOT - LINE))));
		if (art.y == middle.y && abs(art.x - middle.x + 0.5) < half_width) {
			color = frame_at(vec2(middle.x + floor((art.x - middle.x) * ((ART.x * 0.5) / half_width)), middle.y));
		}
	}

	COLOR = vec4(color, 1.0);
}
