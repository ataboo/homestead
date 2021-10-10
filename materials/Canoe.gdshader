shader_type spatial;
render_mode unshaded;

void fragment() {
	ALBEDO = texture(SCREEN_TEXTURE, SCREEN_UV).rgb;
}