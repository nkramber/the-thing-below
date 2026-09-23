// The light shafts of PR-92, for the sweep of D-922. It is the shader of the game with the most
// shafts of one map, 8, placed over the whole frame. Godot takes no default value for a uniform array, so each array is a constant. The rig draws it at 1280 by 720, four times
// the pixels of the overlay of the game, so the measurement is on the safe side.
shader_type canvas_item;
render_mode unshaded, blend_premul_alpha;

#include "res://shaders/look_steps.gdshaderinc"

const int MOST_SHAFTS = 8;
const float SOFT_TOP = 0.12;

const int shaft_count = 8;
const int origin_x = 0;
const int origin_y = 0;
const int top_x[MOST_SHAFTS] = { 40, 120, 200, 280, 360, 440, 520, 600 };
const int top_y[MOST_SHAFTS] = { 0, 40, 0, 40, 0, 40, 0, 40 };
const int slants[MOST_SHAFTS] = { 40, -30, 40, -30, 40, -30, 40, -30 };
const int lengths[MOST_SHAFTS] = { 300, 280, 300, 280, 300, 280, 300, 280 };
const int widths[MOST_SHAFTS] = { 24, 20, 24, 20, 24, 20, 24, 20 };
const float strengths[MOST_SHAFTS] = { 0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0.4 };

varying vec2 local;

void vertex() {
	local = VERTEX;
}

void fragment() {
	vec2 origin = vec2(float(origin_x), float(origin_y));
	vec2 place = place_of(origin + floor(local * 0.5)) - origin;
	vec3 light = vec3(0.0);
	for (int shaft = 0; shaft < shaft_count; shaft++) {
		float along = (place.y - float(top_y[shaft])) / float(lengths[shaft]);
		float middle = float(top_x[shaft]) + (float(slants[shaft]) * along);
		float half_width = float(widths[shaft]) * 0.5 * (1.0 + (along * 0.5));
		float across = abs(place.x - middle) / half_width;
		if (along >= 0.0 && along <= 1.0 && across < 1.0) {
			float fade = smoothstep(0.0, SOFT_TOP, along) * (1.0 - along) * (1.0 - smoothstep(0.5, 1.0, across));
			light += vec3(1.0, 0.92, 0.7) * strengths[shaft] * fade_of(fade);
		}
	}

	COLOR = vec4(light, 0.0);
}
