// The two modes of the three passes of the HD-2D look: the tilt-shift blur, the vignette, and
// the light shafts (D-849, D-917). Each shader of the three includes this file.
//
// The smooth mode reads each art pixel and fades with no steps, and its colors can leave the
// palette. The stepped mode reads one value for each block of art pixels, and each fade rounds
// down to one of a few steps, as the fog does (D-907). The effect file holds the mode, and the
// owner picks one mode after reading both.

// True for the stepped mode, and false for the smooth mode.
uniform bool stepped = false;

// The count of steps of a fade in the stepped mode, from 2 to 8.
uniform int steps = 2;

// The side of each block of the stepped mode, in art pixels, from 1 to 8.
uniform int cell_size = 1;

// Gives the fade of the mode: the fade as it is in the smooth mode, and the fade rounded down
// to its step in the stepped mode, so a fade below the first step stays clear.
float fade_of(float fade) {
	if (!stepped) {
		return fade;
	}

	float count = float(steps);
	return floor(fade * count) / count;
}

// Gives the place that one art pixel reads: the middle of the pixel in the smooth mode, and the
// middle of the block that holds the pixel in the stepped mode. The place is in art pixels.
vec2 place_of(vec2 art) {
	if (!stepped) {
		return art + 0.5;
	}

	float size = float(cell_size);
	return (floor(art / size) * size) + (size * 0.5);
}
