// The view of the world in the frame (D-910, F-103). The world draws in HDR 2D, so its texture
// holds linear light, and a light source can pass full white to glow. The frame draws with no
// HDR 2D, and Godot converts no texture between the two, so the world drew too dark. This
// shader turns the linear light of each pixel into sRGB, with a clamp at full white.
shader_type canvas_item;

// The sRGB curve of Godot, from linear light in 0 to 1.
vec3 srgb_of(vec3 linear_light) {
	vec3 low = linear_light * 12.92;
	vec3 high = (pow(linear_light, vec3(1.0 / 2.4)) * 1.055) - 0.055;
	return mix(high, low, lessThan(linear_light, vec3(0.0031308)));
}

void fragment() {
	vec4 world = texture(TEXTURE, UV);
	COLOR = vec4(srgb_of(clamp(world.rgb, 0.0, 1.0)), world.a);
}
