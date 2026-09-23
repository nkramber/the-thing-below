// The snow whiteout (D-194, D-934), the snow of region one. Flakes of 2 by 2 art pixels fall over
// the map in more and more cells. In the second half, the snow settles in blocks of 2 by 2 art
// pixels in a fixed scatter, until the view is the cover color, the snow of the palette (D-185,
// D-939).
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/transition_common.gdshaderinc"

// The side of the cell of one flake, in art pixels.
const float CELL = 6.0;

// The side of one flake, and of one block of settled snow, in art pixels.
const float FLAKE = 2.0;

// The art pixels that a flake falls over the transition.
const float FALL = 48.0;

// The part of the transition where a flake falls in each cell.
const float FULL = 0.7;

// The part of the transition where the snow starts to settle.
const float SETTLE = 0.5;

void fragment() {
	vec2 art = art_of(UV);

	// The flakes fall, so each art pixel reads the cell above it by the fall so far.
	vec2 fallen = vec2(art.x, art.y - floor(progress * FALL));
	vec2 cell = floor(fallen / CELL);
	vec2 place = floor(vec2(cell_value(cell, 2u), cell_value(cell, 3u)) * (CELL - FLAKE + 1.0));
	vec2 inside = fallen - (cell * CELL) - place;
	bool shown = cell_value(cell, 4u) < progress / FULL;
	bool flake = shown && inside.x >= 0.0 && inside.y >= 0.0 && inside.x < FLAKE && inside.y < FLAKE;
	bool settled = cell_value(floor(art / FLAKE), 8u) < (progress - SETTLE) / (1.0 - SETTLE);

	COLOR = vec4(flake || settled ? cover.rgb : texture(frame_under, SCREEN_UV).rgb, 1.0);
}
