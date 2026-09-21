// The hit flash of the battle screen (D-96, D-825). A sprite that takes a hit mixes each of
// its pixels toward one color of the palette for a few ticks (D-829).
//
// The shader never writes NORMAL_MAP. Godot corrects the normal of a flipped draw before the
// shader code runs, and a write of NORMAL_MAP would replace that correction (D-183, section
// 7.6 of docs/roadmaps/area-effects.md).
shader_type canvas_item;

// The color of the flash, which Game reads from the role `flash` of the UI style file.
uniform vec4 flash_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

// 0 shows the sprite as drawn, and 1 shows the flash color. Game sets 0 or 1 alone.
uniform float flash_amount : hint_range(0.0, 1.0) = 0.0;

void fragment() {
	// COLOR holds the texture color times the modulate, so a dimmed sprite stays dim.
	COLOR.rgb = mix(COLOR.rgb, flash_color.rgb, flash_amount);
}
