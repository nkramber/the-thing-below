// The parts that every transition and the fade share (D-195, D-825, D-938, D-939). Each shader of
// a transition includes this file.
//
// One rectangle covers the whole frame, above the UI, so a transition covers the menus too
// (D-195, D-210). The shader reads the frame under it and draws each effect pixel at the size of
// an art pixel, 2 by 2 pixels of the frame (D-230, D-622). The effect moves pixels of the frame or
// covers them with the one palette color of the cover, so each pixel keeps a palette color and a
// hard edge (G-27). The shader never reads TIME: Game gives the progress of each tick (F-100).

// The frame under the transition, read with no filter.
uniform sampler2D frame_under : hint_screen_texture, filter_nearest;

// The part of the way to a full cover, from 0 to 1. Game gives it from the ticks of the phase.
uniform float progress = 0.0;

// The palette color that covers the view at the end of the transition (D-181, D-939).
uniform vec4 cover : source_color = vec4(0.0, 0.0, 0.0, 1.0);

// The size of the frame, in art pixels (D-634).
const vec2 ART = vec2(640.0, 360.0);

// Gives the art pixel that one place of the rectangle falls in.
vec2 art_of(vec2 uv) {
	return floor(uv * ART);
}

// Gives the color of the frame at the middle of one art pixel, or the cover outside the frame.
vec3 frame_at(vec2 art) {
	if (art.x < 0.0 || art.y < 0.0 || art.x >= ART.x || art.y >= ART.y) {
		return cover.rgb;
	}

	return texture(frame_under, (art + 0.5) / ART).rgb;
}

// Gives a fixed value from 0 to 1 for one cell, the same on each frame and on each machine. The
// hash uses whole numbers alone, so no rounding of the GPU moves it.
float cell_value(vec2 cell, uint salt) {
	uint value = uint(int(cell.x)) * 73856093u ^ uint(int(cell.y)) * 19349663u ^ salt * 83492791u;
	value ^= value >> 16u;
	value *= 2246822519u;
	value ^= value >> 13u;
	value *= 3266489917u;
	value ^= value >> 16u;
	return float(value & 65535u) / 65536.0;
}

// Gives the threshold of the ordered dither of 4 by 4 art pixels, from 1/32 to 31/32. A cover
// part of 1 covers each art pixel, and a part of 0 covers none.
float dither_of(vec2 art) {
	int x = int(mod(art.x, 4.0));
	int y = int(mod(art.y, 4.0));
	int order[16] = int[](0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5);
	return (float(order[(y * 4) + x]) + 0.5) / 16.0;
}

// Gives the cover color where the dither covers the art pixel, and the color of the effect where
// it does not.
vec3 dither_cover(vec2 art, float part, vec3 color) {
	return dither_of(art) < part ? cover.rgb : color;
}
