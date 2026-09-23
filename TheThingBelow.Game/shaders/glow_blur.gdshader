// The blur of the glow pass (D-913). The texture is the mask view of 640 by 360 art pixels, which
// holds the glow of each light source alone. This view is a quarter of that size, so each of its
// pixels is a cell of 4 by 4 art pixels.
//
// Each cell reads the exact mean of its 16 art pixels: four linear reads, each at the corner of
// 2 by 2 pixels. A read at one point alone would miss a small glow between two cells, and the
// glow would shimmer as the view moves by one pixel. The cell then takes a Gaussian of the cells
// around it.
//
// The reads stay inside fragment(), because Godot refuses TEXTURE as an argument of a function.
shader_type canvas_item;
render_mode unshaded;

// The cells to each side that the blur reads: a reach of 20 art pixels.
const int REACH = 5;

// The spread of the Gaussian, in cells.
const float SPREAD = 2.25;

// The side of a cell, in art pixels of the mask.
const float CELL = 4.0;

void fragment() {
	vec2 pixel = 1.0 / vec2(textureSize(TEXTURE, 0));
	vec3 sum = vec3(0.0);
	float total = 0.0;
	for (int row = -REACH; row <= REACH; row += 1) {
		for (int column = -REACH; column <= REACH; column += 1) {
			float weight = exp(-float(row * row + column * column) / (2.0 * SPREAD * SPREAD));
			vec2 middle = UV + vec2(float(column), float(row)) * CELL * pixel;
			vec3 cell = texture(TEXTURE, middle + vec2(-1.0, -1.0) * pixel).rgb;
			cell += texture(TEXTURE, middle + vec2(1.0, -1.0) * pixel).rgb;
			cell += texture(TEXTURE, middle + vec2(-1.0, 1.0) * pixel).rgb;
			cell += texture(TEXTURE, middle + vec2(1.0, 1.0) * pixel).rgb;
			sum += cell / 4.0 * weight;
			total += weight;
		}
	}

	COLOR = vec4(sum / total, 1.0);
}
