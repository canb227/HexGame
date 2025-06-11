shader_type spatial;

uniform bool weighted = true;
uniform bool enabled = false;
uniform sampler2D SCREEN_TEXTURE: hint_screen_texture, filter_linear_mipmap;

void fragment() {
	vec3 hairColor = texture(SCREEN_TEXTURE, SCREEN_UV).rgb;
	ALBEDO.rgb = mix( vec3(hairColor.r, hairColor.g, hairColor.b) , (mix(vec3((hairColor.r + hairColor.g + hairColor.b) / 3.0),  vec3(0.299 * hairColor.r + 0.587 * hairColor.g + 0.114 * hairColor.b), float(weighted))) , float(enabled));
}