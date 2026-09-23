// The tilt-shift blur of the HD-2D look (D-849, D-917, D-919). The view of the scene in the
// frame takes this shader. The scene holds the world with its glow, and the fog, the hit bursts,
// and the light shafts of the overlay, so the blur reads all of them. The marks and the UI draw
// above the frame of this view, so they stay sharp (D-208, D-210).
//
// A band at the top and a band at the bottom of the view blur the scene, and the blur grows
// from no blur at the inner edge of a band to the full radius at the edge of the view. The
// middle of the view stays sharp. The view reads the scene with a linear filter, and each sharp
// pixel reads the middle of its art pixel, so the filter never softens it. The shader never
// reads TIME (F-100).
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/look_steps.gdshaderinc"

// The size of the scene, in art pixels (D-634).
const vec2 SCENE = vec2(640.0, 360.0);

// The count of taps on each side of the middle tap, on each axis. The blur reads a grid of
// 5 by 5 taps.
const int REACH = 2;

// The height of the band at the top and at the bottom of the view, in art pixels. A band of 0
// draws no blur.
uniform int band = 0;

// The radius of the blur at the edge of the view, in art pixels. A radius of 0 draws no blur.
uniform int radius = 0;

// Gives the part of the full blur at one row of the scene: 0 in the middle of the view, and 1
// at the top edge and at the bottom edge.
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
	vec2 art = floor(UV * SCENE);
	vec2 place = place_of(art);
	float spread = fade_of(depth_of(place.y)) * float(radius);
	if (spread <= 0.0) {
		// A sharp pixel reads the middle of its own art pixel, also in the stepped mode, so the
		// middle of the view keeps each pixel of the art.
		COLOR = texture(TEXTURE, (art + 0.5) / SCENE);
	} else {
		// A grid of taps over the radius, with a weight of a Gaussian curve on each tap.
		vec4 total = vec4(0.0);
		float weights = 0.0;
		for (int x = -REACH; x <= REACH; x++) {
			for (int y = -REACH; y <= REACH; y++) {
				vec2 tap = vec2(float(x), float(y));
				float weight = exp(-0.5 * dot(tap, tap));
				vec2 at = place + (tap * spread / float(REACH));
				total += texture(TEXTURE, clamp(at, vec2(0.5), SCENE - 0.5) / SCENE) * weight;
				weights += weight;
			}
		}

		COLOR = total / weights;
	}
}
