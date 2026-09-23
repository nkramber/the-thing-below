// The tilt-shift blur of PR-92, for the sweep of D-922. It is the shader of the game, with two
// changes. It reads the screen, because the rig has no scene view, and the copy of the screen
// costs more than the view of the game, so the measurement is on the safe side. Its band and its
// radius hold the values of the effect file of the game.
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/look_steps.gdshaderinc"

const vec2 SCENE = vec2(640.0, 360.0);
const int REACH = 2;

uniform sampler2D screen : hint_screen_texture, filter_linear;
uniform int band = 72;
uniform int radius = 3;

float depth_of(float row) {
	if (band <= 0) {
		return 0.0;
	}

	float size = float(band);
	float top = (size - row) / size;
	float bottom = (row - (SCENE.y - size)) / size;
	return clamp(max(top, bottom), 0.0, 1.0);
}

void fragment() {
	vec2 art = floor(SCREEN_UV * SCENE);
	vec2 place = place_of(art);
	float spread = fade_of(depth_of(place.y)) * float(radius);
	if (spread <= 0.0) {
		COLOR = texture(screen, (art + 0.5) / SCENE);
	} else {
		vec4 total = vec4(0.0);
		float weights = 0.0;
		for (int x = -REACH; x <= REACH; x++) {
			for (int y = -REACH; y <= REACH; y++) {
				vec2 tap = vec2(float(x), float(y));
				float weight = exp(-0.5 * dot(tap, tap));
				vec2 at = place + (tap * spread / float(REACH));
				total += texture(screen, clamp(at, vec2(0.5), SCENE - 0.5) / SCENE) * weight;
				weights += weight;
			}
		}

		COLOR = total / weights;
	}
}
