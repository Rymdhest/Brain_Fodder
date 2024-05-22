#version 330

in vec2 uv;
layout (location = 0) out vec4 out_Colour;
uniform vec2 center;
uniform vec3 color;
uniform float radius;
uniform float width;

void main(void){
	float dist = distance(gl_FragCoord.xy, center);
	if (dist <= radius && dist > radius-width) {
		out_Colour = vec4(color, 1.0f);
	} else {
		discard;
	}
}