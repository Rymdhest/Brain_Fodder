#version 330

layout (location = 0) out vec4 out_Colour;

in vec2 v_LocalPos;

uniform vec3 color;

void main(void)
{
    float softness = 2.0;
vec2 d = abs(v_LocalPos) - vec2(0.5);
    float dist = max(d.x, d.y);

    // Calculate the size of a single pixel in distance units
    float px = fwidth(dist);

    // Multiply the range by your softness factor
    // We straddle the edge (0.0) by -0.5 * softness and +0.5 * softness
    float alpha = 1.0 - smoothstep(-0.5 * px * softness, 0.5 * px * softness, dist);

    if (alpha <= 0.0) discard;

    out_Colour = vec4(color, alpha);
}