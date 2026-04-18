#version 330

layout (location = 0) out vec4 out_Colour;

in vec2 v_LocalPos;
uniform vec3 color;
uniform float radius;

void main(void) {
    float softness = 2.0;
// 1. Calculate distance from the center (0,0)
    float dist = length(v_LocalPos);
    
    // 2. Calculate distance from the edge of the circle (SDF)
    float d = dist - 0.5;

    // 3. Calculate the pixel-space derivative (for resolution-independent AA)
    float px = fwidth(dist);

    // 4. Smoothstep the alpha
    // We create a gradient that spans exactly 1-2 pixels wide at the edge
    float alpha = 1.0 - smoothstep(-0.5 * px * softness, 0.5 * px * softness, d);

    // 5. Discard invisible pixels to optimize the GPU
    if (alpha <= 0.0) discard;

    out_Colour = vec4(color, alpha);
}