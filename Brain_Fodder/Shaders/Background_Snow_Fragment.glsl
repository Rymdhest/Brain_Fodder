// https://www.shadertoy.com/view/XtBfzw

#version 330
layout (location = 0) out vec4 fragColor;
in vec2 v_LocalPos;

uniform vec2 iResolution;
uniform float iTime;

float snow(vec2 uv, float scale)
{
    float timeOffset = 10.0;
    float iTimeOffsetetd = iTime + timeOffset;
    // 1. Perspective/Fade effect: Scales with resolution properly now
    float w = smoothstep(1., 0., -uv.y * (scale / 10.));
    if (w < .1) return 0.;

    // 2. Animation: Using scale to differentiate speed (parallax)
    uv += iTimeOffsetetd / scale;
    uv.y += iTimeOffsetetd * 2. / scale;
    uv.x += sin(uv.y + iTimeOffsetetd * .5) / scale;
    
    // 3. Grid Tiling
    uv *= scale;
    vec2 s = floor(uv), f = fract(uv), p;
    float k = 3., d;
    
    // 4. Randomness: Using a high-frequency sine hash for flake positioning
    // Added a small offset to 's' to prevent zero-index artifacts
    vec2 hash = fract(sin((s + 5.0) * mat2(7, 3, 6, 5)) * 5.);
    p = .5 + .35 * sin(11. * hash) - f; 
    
    d = length(p);
    k = min(d, k);
    
    // 5. Smoothing the flake edges
    k = smoothstep(0.05, 0.0, k); // Adjusted for consistent flake size
    return k * w;
}

void main()
{
    vec2 fragCoord = v_LocalPos*iResolution;
    
    vec2 uv = (2.0 * fragCoord - iResolution.xy) / min(iResolution.x, iResolution.y);

    vec3 finalColor = vec3(0);
    
    // Background gradient (adjusted for better vertical centering)
    float c = smoothstep(1.0, 0.1, clamp(uv.y * 0.5 + 0.5, 0.0, 1.0))*0.3;
    
    // Snow layers (Parallax)
    // The larger the scale, the "further" and smaller the flakes appear
    c += snow(uv, 30.) * .3;
    c += snow(uv, 20.) * .5;
    c += snow(uv, 15.) * .8;
    c += snow(uv, 10.);
    c += snow(uv, 8.);
    c += snow(uv, 6.);
    c += snow(uv, 5.);

    fragColor = vec4(vec3(c), 1.0);
}