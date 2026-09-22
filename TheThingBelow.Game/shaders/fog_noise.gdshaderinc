// The procedural fog of a weather (D-897 to D-899). The fog shaders `fog.gdshader` and
// `fog_lit.gdshader` include this file, and they differ in the scene light alone (D-183).
//
// One rectangle covers the view, and the shader draws every layer of the fog in one
// full-screen pass (D-898). Each layer reads a value noise at each world pixel and cuts it
// into its bands, so each pixel takes one strength of one palette key, with hard edges and no
// gradient (G-27, D-622). Where layers overlap, the strongest band wins (D-899).
//
// The shader never reads TIME. Game gives each layer the world pixel of the north-west corner
// of the view, with the drift of that layer at the tick, so one tick gives one picture in
// every capture (F-100, D-172, T-7). The shader never writes NORMAL_MAP (D-183).

// The most layers and the most bands of each layer: FogLayer.MostLayers and FogLayer.MostBands.
const int MOST_LAYERS = 3;
const int MOST_BANDS = 3;

// The count of layers of the fog, from 1 to MOST_LAYERS.
uniform int layer_count = 0;

// The palette color of each layer (D-181).
uniform vec4 colors[MOST_LAYERS] : source_color;

// The world pixel of the north-west corner of the view for each layer, with its drift.
uniform int origin_x[MOST_LAYERS];
uniform int origin_y[MOST_LAYERS];

// The distance between two points of the noise of each layer, in art pixels. Always even.
uniform int scales[MOST_LAYERS];

// The side of each block of each layer, in art pixels. Each block takes one band.
uniform int cell_sizes[MOST_LAYERS];

// The seed of the noise of each layer.
uniform int seeds[MOST_LAYERS];

// The count of bands of each layer, from 1 to MOST_BANDS.
uniform int band_counts[MOST_LAYERS];

// The noise level where each band starts, and its strength, both in basis points. The bands
// of the layer i hold the indexes from i * MOST_BANDS.
uniform int band_from[MOST_LAYERS * MOST_BANDS];
uniform int band_strength[MOST_LAYERS * MOST_BANDS];

// The pixel of the rectangle, from its north-west corner, in art pixels.
varying vec2 local;

void vertex() {
	local = VERTEX;
}

// A whole-number division that rounds down, also for a negative value. The drift of a layer
// can take the world pixel below zero, and the division of GLSL rounds toward zero.
int floor_div(int value, int divisor) {
	return value >= 0 ? value / divisor : -((divisor - 1 - value) / divisor);
}

// A hash of 32 bits with a good spread, so the points of the noise show no pattern.
uint mix_bits(uint value) {
	value ^= value >> 16u;
	value *= 2146121005u;
	value ^= value >> 15u;
	value *= 2221713035u;
	value ^= value >> 16u;
	return value;
}

// The value of the noise at one point of the grid of the noise, from 0 to 1.
float point_value(ivec2 point, int seed) {
	uint hash = mix_bits(uint(point.x) ^ mix_bits(uint(point.y) ^ mix_bits(uint(seed))));
	return float(hash >> 8u) / 16777215.0;
}

// A value noise, from 0 to 1: the four points around the pixel, mixed with a smooth curve.
float value_noise(ivec2 pixel, int scale, int seed) {
	ivec2 point = ivec2(floor_div(pixel.x, scale), floor_div(pixel.y, scale));
	vec2 part = vec2(pixel - point * scale) / float(scale);
	part = part * part * (3.0 - 2.0 * part);
	float north = mix(point_value(point, seed), point_value(point + ivec2(1, 0), seed), part.x);
	float south = mix(point_value(point + ivec2(0, 1), seed), point_value(point + ivec2(1, 1), seed), part.x);
	return mix(north, south, part.y);
}

// The noise of one layer, from 0 to 1: the shapes at the scale, and rough edges from a second
// octave at half the scale, with half the weight.
float fog_noise(ivec2 pixel, int scale, int seed) {
	return (2.0 * value_noise(pixel, scale, seed) + value_noise(pixel, scale / 2, seed + 1)) / 3.0;
}

void fragment() {
	ivec2 pixel = ivec2(floor(local));
	vec3 color = vec3(0.0);
	int strongest = 0;
	for (int layer = 0; layer < layer_count; layer++) {
		// Each block of the layer reads the noise at its north-west pixel, so the whole block
		// takes one band.
		int size = cell_sizes[layer];
		ivec2 world = ivec2(origin_x[layer], origin_y[layer]) + pixel;
		ivec2 block = ivec2(floor_div(world.x, size), floor_div(world.y, size)) * size;
		int level = int(fog_noise(block, scales[layer], seeds[layer]) * 10000.0);
		for (int band = 0; band < band_counts[layer]; band++) {
			int index = layer * MOST_BANDS + band;
			if (level >= band_from[index] && band_strength[index] > strongest) {
				strongest = band_strength[index];
				color = colors[layer].rgb;
			}
		}
	}

	COLOR = vec4(color, float(strongest) / 10000.0);
}
