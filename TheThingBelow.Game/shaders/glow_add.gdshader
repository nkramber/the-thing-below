// The glow pass over the world (D-188, D-913, D-914). The texture is the blur of the mask of the
// light sources, and this shader adds it over the world, under the UI (D-210). No sprite or tile
// draws into the mask, so a sprite or a tile never glows (F-47).
//
// A smooth glow reads the blur between its cells (D-911). A stepped glow reads it once for each
// block of art pixels, and it cuts the strength into a few steps, as the fog does (D-907, D-914).
shader_type canvas_item;
render_mode blend_add, unshaded;

// The size of the world view, in art pixels (D-634).
const vec2 WORLD = vec2(640.0, 360.0);

// The intensity of the glow, where 1.0 adds the color of the source.
uniform float intensity = 1.0;

// The count of steps of the fade, or 0 for a smooth glow.
uniform int steps = 0;

// The side of a block of a stepped glow, in art pixels.
uniform int cell_size = 1;

void fragment() {
	vec2 place = UV;
	if (steps > 0) {
		vec2 blocks = WORLD / float(cell_size);
		place = (floor(UV * blocks) + 0.5) / blocks;
	}

	vec3 glow = texture(TEXTURE, place).rgb * intensity;
	if (steps > 0) {
		// The strongest channel sets the step, and the color keeps its hue.
		float level = min(max(glow.r, max(glow.g, glow.b)), 1.0);
		float stepped = floor(level * float(steps)) / float(steps);
		glow = level > 0.0 ? glow * (stepped / level) : vec3(0.0);
	}

	COLOR = vec4(glow, 1.0);
}
