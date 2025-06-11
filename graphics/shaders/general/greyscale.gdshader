shader_type canvas_item;

uniform bool weighted = true;
uniform bool enabled = false;

void fragment() {
	COLOR.rgb = mix( vec3(COLOR.r, COLOR.g, COLOR.b) , (mix(vec3((COLOR.r + COLOR.g + COLOR.b) / 3.0),  vec3(0.299 * COLOR.r + 0.587 * COLOR.g + 0.114 * COLOR.b), float(weighted))) , float(enabled));
}