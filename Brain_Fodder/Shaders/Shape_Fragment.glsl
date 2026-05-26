#version 330
layout (location = 0) out vec4 out_Colour;

in vec2 v_LocalPos;

uniform int u_sides;
uniform vec4 u_color;
uniform vec4 u_border_color;
uniform float u_border_width;
uniform vec2 u_size;

#define PI 3.14159265359

void main(void) {
    float dist;
    
    // Scale local UV coordinates [-0.5, 0.5] exactly into pixel coordinates
    vec2 p = v_LocalPos * u_size; 

    if (u_sides <= 0) {
        vec2 r = u_size * 0.5;
        
        // Use normalized coordinates for the distance field
        vec2 q = p / r;
        float d = length(q) - 1.0;
        
        // The gradient of (length(p/r) - 1) is (p / (r*r)) / length(p/r)
        // We calculate the length of the gradient to normalize the distance
        vec2 grad = p / (r * r);
        float grad_len = length(grad);
        
        // This gives us an exact, non-distorted pixel-space distance
        dist = d / grad_len;
    } 
    else if (u_sides == 4) {
        // --- 2. PERFECT RECTANGLE (Exact Pixel Math) ---
        vec2 d = abs(p) - (u_size * 0.5);
        dist = min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
    } 
    else {
        // --- 3. EXACT POLYGON (Matches C# Physics Perfectly) ---
        int sides = max(3, u_sides);
        
        float minY = 1.0; float maxY = -1.0; float maxX = 0.0;
        
        for(int i = 0; i < sides; i++) {
            float a = float(i) * (2.0 * PI / float(sides));
            minY = min(minY, cos(a));
            maxY = max(maxY, cos(a));
            maxX = max(maxX, abs(sin(a)));
        }
        
        // Exact scaling mirroring your C# GetShapeVertices
        vec2 wScale = vec2(u_size.x / (maxX * 2.0), u_size.y / (maxY - minY));
        float yOff = -(maxY + minY) / 2.0;
        
        float d = 999999.0;
        float s = 1.0;
        
        // Setup the last vertex to close the loop
        float a_prev = float(sides - 1) * (2.0 * PI / float(sides));
        vec2 v_prev = vec2(sin(a_prev), cos(a_prev) + yOff) * wScale;
        
        // Loop through each edge segment to find exact pixel distance
        for(int i = 0; i < sides; i++) {
            float a = float(i) * (2.0 * PI / float(sides));
            vec2 v_curr = vec2(sin(a), cos(a) + yOff) * wScale;
            
            vec2 e = v_prev - v_curr;
            vec2 w = p - v_curr;
            
            // Shortest distance to the line segment
            vec2 b = w - e * clamp(dot(w, e) / dot(e, e), 0.0, 1.0);
            d = min(d, dot(b, b));
            
            // Winding number check (Are we inside or outside the polygon?)
            bvec3 cond = bvec3(p.y >= v_curr.y, p.y < v_prev.y, e.x * w.y > e.y * w.x);
            if(all(cond) || all(not(cond))) {
                s *= -1.0;
            }
            
            v_prev = v_curr;
        }
        
        // s makes it negative if inside, sqrt(d) is the exact pixel distance
        dist = s * sqrt(d); 
    }

    // --- High-Quality Screen Space Anti-Aliasing & Borders ---
    // Because 'dist' is perfectly in pixels, no derivatives are needed!
    float aa_width = 1.0;
    
    // Outer mask cuts out the shape
    float outer_mask = smoothstep(aa_width, -aa_width, dist);
    
    if (outer_mask < 0.001) {
        discard;
    }
    
    // Inner mask separates border from fill perfectly
    float inner_mask = smoothstep(aa_width, -aa_width, dist + u_border_width);
    
    vec4 final_color;
    if (u_border_width <= 0.001) {
        final_color = u_color;
    } else {
        final_color = mix(u_border_color, u_color, inner_mask);
    }
    final_color.a *= outer_mask;
    
    out_Colour = final_color;
}