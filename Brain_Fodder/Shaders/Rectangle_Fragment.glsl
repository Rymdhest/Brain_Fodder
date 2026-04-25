#version 330

layout (location = 0) out vec4 out_Colour;

in vec2 v_LocalPos;

uniform vec3 color;

void main(void)
{
    float softness = 4.0;
vec2 d = abs(v_LocalPos) - vec2(0.5);
    float dist = max(d.x, d.y);

    float px = fwidth(dist);

    float alpha = 1.0 - smoothstep(-0.5 * px * softness, 0.5 * px * softness, dist);

    if (alpha <= 0.0) discard;

    out_Colour = vec4(color, alpha);
}