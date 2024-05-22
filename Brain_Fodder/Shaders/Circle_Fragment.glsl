#version 330

layout (location = 0) out vec4 out_Colour;
uniform vec2 center;
uniform vec3 color;

uniform float radius;

void main(void){
	float dist = distance(gl_FragCoord.xy, center);
	if (dist <= radius) {
		out_Colour.rgb = color;
		out_Colour.a = 1.0f-dist/radius;
	} else {
		//out_Colour.rgb = vec3(0.0f, 1.0f, 0.0f);
		discard;
	}
}