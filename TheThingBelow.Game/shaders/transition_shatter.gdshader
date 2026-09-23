// The shatter (D-195, D-940), the transition of a boss fight. Cracks cut the map into shards of
// two triangles in each cell, and the shards then fall away, each after its own wait, and leave
// the cover. Each art pixel reads one art pixel of the map, so each pixel keeps a palette color
// (G-27).
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/transition_common.gdshaderinc"

// The size of the cell of two shards, in art pixels: 10 columns and 6 rows.
const vec2 CELL = vec2(64.0, 60.0);

// The part of the transition that shows the cracks before the first shard falls.
const float CRACK = 0.15;

// The most wait of a shard after the cracks, as a part of the transition.
const float WAIT = 0.4;

// The fall of a shard is this many art pixels times the square of the part of the transition since
// its wait. The last shard falls for 0.45 of the transition, about 445 art pixels, so it leaves the
// view of 360 rows before the end.
const float DROP = 2200.0;

// Tells whether a place of a cell lies in its first triangle. The diagonal of each cell goes one
// of two ways, in a fixed scatter.
bool first_half(vec2 cell, vec2 inside) {
	vec2 part = inside / CELL;
	return cell_value(cell, 5u) < 0.5 ? part.x > part.y : (part.x + part.y) < 1.0;
}

// Gives the fall of one shard so far, in whole art pixels.
float fall_of(vec2 cell, bool first) {
	float wait = CRACK + (cell_value(cell, first ? 6u : 7u) * WAIT);
	float time = max(0.0, progress - wait);
	return floor(DROP * time * time);
}

// Tells whether a place of a cell lies on a crack: the edge of the cell or its diagonal.
bool on_crack(vec2 cell, vec2 inside) {
	if (inside.x < 1.0 || inside.y < 1.0) {
		return true;
	}

	vec2 part = inside / CELL;
	float gap = cell_value(cell, 5u) < 0.5 ? abs(part.x - part.y) : abs((part.x + part.y) - 1.0);
	return gap * CELL.y < 0.75;
}

void fragment() {
	vec2 art = art_of(UV);
	vec2 column = vec2(floor(art.x / CELL.x), 0.0);
	vec3 color = cover.rgb;

	// A shard falls straight down, so an art pixel can show a shard of its own column alone. The
	// loop reads the rows from the bottom, so a lower shard draws over a higher one.
	for (int row = int(ART.y / CELL.y) - 1; row >= 0; row--) {
		vec2 cell = vec2(column.x, float(row));
		bool drawn = false;
		for (int side = 0; side < 2; side++) {
			bool first = side == 0;
			vec2 source = vec2(art.x, art.y - fall_of(cell, first));
			vec2 inside = source - (cell * CELL);
			if (inside.y < 0.0 || inside.y >= CELL.y || first_half(cell, inside) != first) {
				continue;
			}

			color = progress > 0.0 && on_crack(cell, inside) ? cover.rgb : frame_at(source);
			drawn = true;
			break;
		}

		if (drawn) {
			break;
		}
	}

	COLOR = vec4(color, 1.0);
}
