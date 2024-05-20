#version 330

in vec2 position;
uniform mat4 uProjection;
uniform mat4 modelMatrix;

void main(void){
	gl_Position = vec4(position, 0.0, 1.0)*modelMatrix*uProjection;
}