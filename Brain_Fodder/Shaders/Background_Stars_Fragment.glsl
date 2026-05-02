// https://www.shadertoy.com/view/msfGDX

#version 330
layout (location = 0) out vec4 fragColor;
in vec2 v_LocalPos;

uniform vec2 iResolution;
uniform float iTime;


//#define USE_SIMPLE_NOISE



vec2 grad( ivec2 z )  // replace this anything that returns a random vector
{
    // 2D to 1D  (feel free to replace by some other)
    int n = z.x+z.y*11111;

    // Hugo Elias hash (feel free to replace by another one)
    n = (n<<13)^n;
    n = (n*(n*n*15731+789221)+1376312589)>>16;

#if defined(USE_SIMPLE_NOISE)

    // simple random vectors
    return vec2(cos(float(n)),sin(float(n)));
    
#else

    // Perlin style vectors
    n &= 7;
    vec2 gr = vec2(n&1,n>>1)*2.0-1.0;
    return ( n>=6 ) ? vec2(0.0,gr.x) : 
           ( n>=4 ) ? vec2(gr.x,0.0) :
                              gr;
#endif                              
}

float noise( in vec2 p )
{
    ivec2 i = ivec2(floor( p ));
     vec2 f =       fract( p );
	
	vec2 u = f*f*(3.0-2.0*f); // feel free to replace by a quintic smoothstep instead

    return mix( mix( dot( grad( i+ivec2(0,0) ), f-vec2(0.0,0.0) ), 
                     dot( grad( i+ivec2(1,0) ), f-vec2(1.0,0.0) ), u.x),
                mix( dot( grad( i+ivec2(0,1) ), f-vec2(0.0,1.0) ), 
                     dot( grad( i+ivec2(1,1) ), f-vec2(1.0,1.0) ), u.x), u.y);
}
/* ------------------------------------------------------------------------ */

vec3 saturate(vec3 c)
{
    return clamp(c, 0.0, 1.0);
}

float saturate(float f)
{
    return clamp(f, 0.0, 1.0);
}

/* ------------------------------------------------------------------------ */

const float cycle_len_s = 20.0;
float fade_time = 7.0;
float max_scale_pow = 3.0;
float max_scale = 3.0;

float speed_adjust = 1.0;
float global_time = 0.0;

vec2 screen_uv = vec2(0.0);

/* ------------------------------------------------------------------------ */

vec3 stars(vec2 uv, float seed)
{
    const float threshold = 0.6;
    float stars = noise(uv + vec2(seed, seed));
    
#if defined(USE_SIMPLE_NOISE)
    stars *= 1.25;
#endif
        
    vec3 col = vec3(stars) - threshold;
    col *= (1.0/(1.0 - threshold));
    return saturate(col);
}

vec2 stars_uv(vec2 fragCoord, float scale)
{
    vec2 uv = screen_uv - vec2(0.5);
    uv *= iResolution.xy * 0.25;
    uv /= scale;
    return uv;
}

float star_fade(float age)
{
    float fade_out = 1.0 - smoothstep(cycle_len_s - fade_time, cycle_len_s, age);
    float fade_in = smoothstep(0.0, fade_time, age);
    return fade_in * fade_out;
}

vec3 star_field_octave(float start_age, vec2 fragCoord, float seed)
{
    // Draws a plane of stars that fades in, increases in size giving
    // the impression of movement towards the viewer, and fades out at
    // the end. Each plane cycles from back-to-front in a loop
#if 0
    float fade, scale;
    if (start_age >= 0.0)
    {
        float cycle_time = mod(global_time + start_age, cycle_len_s);
        float t = (cycle_time / cycle_len_s);
        fade = star_fade(cycle_time);
        scale = 1.0 + pow(t, max_scale_pow) * max_scale;
    }
    else
    {
        fade = 1.0;
        scale = 1.0;
    }
#else
    
    float cycle_time = mod(global_time + start_age, cycle_len_s);
    float t = (cycle_time / cycle_len_s);
    
    float fade = star_fade(cycle_time);
    float scale = 1.0 + pow(t, max_scale_pow) * max_scale;
    
    fade = (start_age < 0.0)?(1.0):(fade);
    scale = (start_age < 0.0)?(1.0):(scale);
#endif

    vec2 uv = stars_uv(fragCoord, scale);
    vec3 col = fade * stars(uv, seed);
    
    return col;
}

float star_shimmer()
{
    // Intersecting noise to modulate the stars but be random enough
    // that your eye doesn't quite see the scrolling of the pattern
    vec2 uv = screen_uv;
    float shimmer_speed = iTime * 2.0;
    float octave_a = abs(noise(vec2(23.0, 19.0) * uv + vec2(shimmer_speed)));
    float octave_b = abs(noise(vec2(27.0, -29.0) * uv + vec2(shimmer_speed)));
    return 0.5 + 0.5 * saturate(octave_a + octave_b);
}

vec2 rotate2d(vec2 v, float r)
{
    float cos_r = cos(r);
    float sin_r = sin(r);
    mat2 m = mat2(cos_r, -sin_r, sin_r, cos_r);
    return v * m;
}

vec4 nebula()
{
    vec2 uv = screen_uv;
    
    float debug_rot = 0.2f;
    vec2 uv_r = rotate2d(uv, global_time * 0.0125 * debug_rot);
    vec2 uv_g = rotate2d(uv, global_time * -0.00625 * debug_rot);
    vec2 uv_b = uv;
    
    uv_r += vec2(-1.0, 1.0) * vec2(global_time * 0.05 * debug_rot);
    uv_g += vec2(1.0, 1.0) * vec2(global_time * 0.03 * debug_rot);
    uv_b += vec2(1.0, -1.0) * (global_time * 0.01 * debug_rot);
    
    vec4 neb = vec4(0.0);
    
    // Red and green are two independent combinations of multi-octave noise 
    float r = 1.3 * (noise(vec2(192.0) + uv_r * 3.2))
            + 0.5 * (noise(vec2(1123.0) + uv_r * 5.7))
            //+ 0.1 * (noise(vec2(173.0) + uv_b * 35.7))
            + 0.1 * (noise(vec2(173.0) + uv_r * 125.7));
    
    float g = 1.1 * (noise(vec2(17.0) + uv_g * 2.3))
            + 0.6 * (noise(vec2(41.0) + uv_r * 5.2))
            //+ 0.2 * (noise(vec2(97.0) + uv_g * 19.4))
            + 0.1 * (noise(vec2(137.0) + uv_g * 65.7));
    
    neb.r = saturate(r * 0.5);
    neb.g = saturate(g * 0.5);
    
    // Blue occurs as a noise plane within the green and red areas of nebula
    neb.b = 1.0 * noise(vec2(179.0) + uv_b * 3.2)
          //+ 0.4 * noise(vec2(242.0) + uv_b * 17.0)
          + 0.1 * noise(vec2(342.0) + uv_b * 45.0);
    neb.b = saturate(2.0 * (neb.r + 0.8*neb.g) * neb.b);
    
    // Add a bit of b in r/g
    neb.g += 0.1 * neb.b;
    neb.r += 0.2 * neb.b;
    
    neb.r += 0.2 * neb.g;
    
    // Interstellar dust
    neb.a = 0.1 * neb.r + 0.2 * neb.g + 0.1 * neb.b;
    neb.a += saturate(0.25 + 0.75 * noise(vec2(457.0) + uv * 3.12));
    
    return neb;
}

void main()
{
    vec2 fragCoord = v_LocalPos*iResolution;
    // Calculate some initial globals
    screen_uv = fragCoord / iResolution.xy;
    global_time = iTime * speed_adjust;
    
    // Offset the 4 planes of stars by 1/4 cycle length
    float age_offset = cycle_len_s * 0.25;
    
    float shimmer = star_shimmer();
    vec4 neb = nebula();
    
    vec3 col = vec3(0.0)
        + 0.2 * neb.rgb
        + neb.a * star_field_octave(-1.0, fragCoord, 265.0) * shimmer;
        
    vec3 stars = vec3(0.0) 
        + star_field_octave(0.0 * age_offset, fragCoord, 42.0)
        + star_field_octave(1.0 * age_offset, fragCoord, 93.0)
        + star_field_octave(2.0 * age_offset, fragCoord, 137.0)
        + star_field_octave(3.0 * age_offset, fragCoord, 17.0)
        ;
    stars *= shimmer;   
    col += stars;
    
    // Output to screen
    fragColor = vec4(col, 1.0);
}