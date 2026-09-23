// The fog of a weather that gives its own light, so the dark of the ambient light never dims
// it (D-183, D-897). `fog_noise.gdshaderinc` holds the fog.
shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/fog_noise.gdshaderinc"
