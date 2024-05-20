#version 330

layout (location = 0) out vec4 out_Colour;
uniform vec2 center;
uniform vec3 color;

uniform float radius;

void main(void){
	if (distance(gl_FragCoord.xy, center) <= radius) {
		out_Colour.rgb = color;
	} else {
		//out_Colour.rgb = vec3(0.0f, 1.0f, 0.0f);
		discard;
	}
}