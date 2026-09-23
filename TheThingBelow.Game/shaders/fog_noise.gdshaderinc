// The procedural fog of a weather (D-897 to D-900). The fog shaders `fog.gdshader` and
// `fog_lit.gdshader` include this file, and they differ in the scene light alone (D-183).
//
// One rectangle covers the view, and the shader draws every layer of the fog in one
// full-screen pass (D-898). Each layer reads a fractal noise at each world pixel. It is clear
// below its start level, full above its full level, and it fades smoothly between the two, in
// one palette key (D-900). The world viewport draws art pixels, so each pixel of the fog is one
// art pixel (D-901). Where layers overlap, the strongest layer wins (D-899).
//
// The shader never reads TIME. Game gives each layer the world pixel of the north-west corner
// of the view, with the drift of that layer at the tick, so one tick gives one picture in
// every capture (F-100, D-172, T-7). The shader never writes NORMAL_MAP (D-183).

// The most layers: FogLayer.MostLayers.
const int MOST_LAYERS = 3;

// The count of octaves of the noise. Each octave has half the size and half the weight of the
// octave before it, so the large shapes of the fog take soft, cloudy edges.
const int OCTAVES = 5;

// The count of layers of the fog, from 1 to MOST_LAYERS.
uniform int layer_count = 0;

// The palette color of each layer (D-181).
uniform vec4 colors[MOST_LAYERS] : source_color;

// The world pixel of the north-west corner of the view for each layer, with its drift.
uniform int origin_x[MOST_LAYERS];
uniform int origin_y[MOST_LAYERS];

// The size of the largest shapes of the noise of each layer, in art pixels.
uniform int scales[MOST_LAYERS];

// The seed of the noise of each layer.
uniform int seeds[MOST_LAYERS];

// The noise level where each layer starts to show, and where it is full, in basis points.
uniform int fade_from[MOST_LAYERS];
uniform int fade_to[MOST_LAYERS];

// The full strength of each layer, in basis points.
uniform int strengths[MOST_LAYERS];

// The pixel of the rectangle, from its north-west corner, in art pixels.
varying vec2 local;

void vertex() {
	local = VERTEX;
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

// A value noise, from 0 to 1: the four points of the grid around the place, mixed with a
// smooth curve. The place is in units of the grid.
float value_noise(vec2 place, int seed) {
	vec2 corner = floor(place);
	ivec2 point = ivec2(corner);
	vec2 part = place - corner;
	part = part * part * part * (part * (part * 6.0 - 15.0) + 10.0);
	float north = mix(point_value(point, seed), point_value(point + ivec2(1, 0), seed), part.x);
	float south = mix(point_value(point + ivec2(0, 1), seed), point_value(point + ivec2(1, 1), seed), part.x);
	return mix(north, south, part.y);
}

// The fractal noise of one layer, from 0 to 1. Each octave takes its own seed, so the octaves
// never line up.
float fog_noise(ivec2 pixel, int scale, int seed) {
	vec2 place = vec2(pixel) / float(scale);
	float total = 0.0;
	float weight = 1.0;
	float weights = 0.0;
	for (int octave = 0; octave < OCTAVES; octave++) {
		total += value_noise(place, seed + octave) * weight;
		weights += weight;
		place *= 2.0;
		weight *= 0.5;
	}

	return total / weights;
}

void fragment() {
	ivec2 pixel = ivec2(floor(local));
	vec3 color = vec3(0.0);
	float strongest = 0.0;
	for (int layer = 0; layer < layer_count; layer++) {
		ivec2 world = ivec2(origin_x[layer], origin_y[layer]) + pixel;
		float level = fog_noise(world, scales[layer], seeds[layer]) * 10000.0;
		float fade = smoothstep(float(fade_from[layer]), float(fade_to[layer]), level);
		float strength = fade * float(strengths[layer]) / 10000.0;
		if (strength > strongest) {
			strongest = strength;
			color = colors[layer].rgb;
		}
	}

	COLOR = vec4(color, strongest);
}
