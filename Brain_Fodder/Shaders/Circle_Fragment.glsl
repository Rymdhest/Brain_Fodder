#version 330

layout (location = 0) out vec4 out_Colour;

in vec2 v_LocalPos;
uniform vec3 color;

void main(void) {
    float softness = 4.0;

    float dist = length(v_LocalPos);
    float d = dist - 0.5;
    float px = fwidth(dist);
    float alpha = 1.0 - smoothstep(-0.5 * px * softness, 0.5 * px * softness, d);

    if (alpha <= 0.0) discard;

    out_Colour = vec4(color, alpha);
}