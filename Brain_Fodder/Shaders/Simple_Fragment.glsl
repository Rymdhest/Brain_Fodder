#version 330

layout (location = 0) out vec4 out_Colour;
uniform vec3 color;
uniform vec2 center;
uniform vec2 size;

void main(void){
	out_Colour = vec4(color, 1.0f);
}