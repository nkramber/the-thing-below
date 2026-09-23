// The light shafts of the HD-2D look (D-849, D-917 to D-921). One rectangle covers the view in the
// overlay, above the fog and the hit bursts, so a beam lights the fog and never glows (D-916,
// D-919). The tilt-shift blur and the vignette draw above the overlay.
//
// Each shaft is a beam from a place on a wall into the room to its south. The beam slants by a
// count of art pixels over its length, and it grows a little wider as it falls. Its light is
// full at the top and fades to nothing at its foot. Where beams overlap, their light adds.
//
// The shader adds the light of each beam to the picture under it. It writes the light with an
// alpha of 0 and the blend of premultiplied alpha, so the overlay adds that light over the world
// and dims nothing. The shader never reads TIME. Game gives it the strength of each beam at the
// tick, with the shimmer of its kind, so one tick gives one picture (F-100, D-921). The shader
// never writes NORMAL_MAP (D-183).
shader_type canvas_item;
render_mode unshaded, blend_premul_alpha;

#include "res://shaders/look_steps.gdshaderinc"

// The most shafts of one map: ShaftKind.MostShaftsOnMap.
const int MOST_SHAFTS = 8;

// The part of the length of a beam where the light grows from nothing at the wall to full.
const float SOFT_TOP = 0.12;

// The count of shafts, from 0 to MOST_SHAFTS.
uniform int shaft_count = 0;

// The world pixel of the north-west corner of the view, so each block of the stepped mode
// stays on the world when the view moves.
uniform int origin_x = 0;
uniform int origin_y = 0;

// The top of each beam, in art pixels of the view.
uniform int top_x[MOST_SHAFTS];
uniform int top_y[MOST_SHAFTS];

// The art pixels that each beam moves east over its length. A value below 0 moves it west.
uniform int slants[MOST_SHAFTS];

// The length of each beam, and its width at the top, in art pixels.
uniform int lengths[MOST_SHAFTS];
uniform int widths[MOST_SHAFTS];

// The palette color of each beam (D-181).
uniform vec4 colors[MOST_SHAFTS] : source_color;

// The strength of each beam at the tick, from 0 to 1, with its shimmer (D-921).
uniform float strengths[MOST_SHAFTS];

// The pixel of the rectangle, from its north-west corner, in art pixels.
varying vec2 local;

void vertex() {
	local = VERTEX;
}

void fragment() {
	vec2 origin = vec2(float(origin_x), float(origin_y));
	vec2 place = place_of(origin + floor(local)) - origin;
	vec3 light = vec3(0.0);
	for (int shaft = 0; shaft < shaft_count; shaft++) {
		float along = (place.y - float(top_y[shaft])) / float(lengths[shaft]);
		float middle = float(top_x[shaft]) + (float(slants[shaft]) * along);
		float half_width = float(widths[shaft]) * 0.5 * (1.0 + (along * 0.5));
		float across = abs(place.x - middle) / half_width;
		if (along >= 0.0 && along <= 1.0 && across < 1.0) {
			float fade = smoothstep(0.0, SOFT_TOP, along) * (1.0 - along) * (1.0 - smoothstep(0.5, 1.0, across));
			light += colors[shaft].rgb * strengths[shaft] * fade_of(fade);
		}
	}

	COLOR = vec4(light, 0.0);
}
