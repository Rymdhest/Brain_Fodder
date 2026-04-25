#version 330
layout (location = 0) out vec4 out_Colour;
in vec2 v_LocalPos;

uniform vec3 color;
uniform float width;

void main(void) {
    float softness = 4.0;
    float dist = length(v_LocalPos);
    float R = 0.5; // Outer radius matches quad size
    
    // 1. Convert pixel thickness to coordinate space
    float px = fwidth(dist);
    float W = width * px;
    
    // 2. SDF Math: max(outside_outer_edge, inside_inner_edge)
    float d = max(dist - R, (R - W) - dist);
    
    // 3. Apply softness to the transition
    // By multiplying px by softness, we define how many pixels wide the edge gradient is
    float alpha = 1.0 - smoothstep(-0.5 * px * softness, 0.5 * px * softness, d);

    if (alpha <= 0.0) discard;

    out_Colour = vec4(color, alpha);
}