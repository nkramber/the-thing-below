// The light of a mote of a weather (D-183, D-825, D-893). A mote holds one gray with no light
// and another gray in full light, and it takes the strength of each light and never its color.
// A mote that took the color of the light went yellow in torchlight, and the energy of a torch
// then pushed it to white.
//
// The shader never writes NORMAL_MAP. Godot corrects the normal of a flipped draw before the
// shader code runs, and a write of NORMAL_MAP would replace that correction (D-183, section
// 7.6 of docs/roadmaps/area-effects.md).
shader_type canvas_item;

// The color of a mote with no light, which the ambient light never dims.
uniform vec4 dark_color : source_color = vec4(0.35, 0.35, 0.35, 1.0);

// The color of a mote in full light.
uniform vec4 light_color : source_color = vec4(0.5, 0.5, 0.5, 1.0);

void fragment() {
	// The mote holds its own dark gray, so the ambient light of the map never dims it (D-183).
	COLOR.rgb = dark_color.rgb;
}

void light() {
	// The strength of the light is its luma of Rec. 601, which drops its color (D-886). The
	// light adds the step from the dark gray to the light gray, so two lights never reach white.
	float strength = clamp(dot(LIGHT_COLOR.rgb, vec3(0.299, 0.587, 0.114)) * LIGHT_COLOR.a * LIGHT_ENERGY, 0.0, 1.0);
	LIGHT = vec4((light_color.rgb - dark_color.rgb) * strength, COLOR.a);
}
