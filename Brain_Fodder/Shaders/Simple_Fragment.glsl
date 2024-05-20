#version 330

layout (location = 0) out vec4 out_Colour;
uniform vec3 color;

void main(void){
	out_Colour.rgb = color;
}