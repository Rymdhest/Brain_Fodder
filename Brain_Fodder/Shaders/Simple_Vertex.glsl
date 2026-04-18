#version 330

in vec2 position;
out vec2 v_LocalPos;
uniform mat4 uProjection;
uniform mat4 modelMatrix;

void main(void){
	v_LocalPos = position;
	gl_Position = vec4(position, 0.0, 1.0)*modelMatrix*uProjection;
}