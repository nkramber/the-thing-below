// The light of a mote of a weather (D-183, D-825, D-893). A mote takes the strength of each
// light and never its color, so it reads as light gray inside a pool of torchlight and as dark
// gray outside one. A mote that took the color of the light went yellow in torchlight.
//
// The shader never writes NORMAL_MAP. Godot corrects the normal of a flipped draw before the
// shader code runs, and a write of NORMAL_MAP would replace that correction (D-183, section
// 7.6 of docs/roadmaps/area-effects.md).
shader_type canvas_item;

void light() {
	// The strength of the light is its luma of Rec. 601, which drops its color (D-886).
	float strength = dot(LIGHT_COLOR.rgb, vec3(0.299, 0.587, 0.114)) * LIGHT_COLOR.a * LIGHT_ENERGY;
	LIGHT = vec4(COLOR.rgb * strength, COLOR.a);
}
