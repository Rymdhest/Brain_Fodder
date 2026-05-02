// https://www.shadertoy.com/view/dlXyDf

#version 330
layout (location = 0) out vec4 fragColor;
in vec2 v_LocalPos;

uniform vec2 iResolution;
uniform float iTime;


// a bunch of random parameters
#define AmbientColor vec3(0.05, 0.125, 0.15)
#define Itterations 20.

// bubble parameters
#define BubbleColor vec3(0.7, 0.85, 1.)*0.1
#define CellSize vec2(18., 18.)
#define BubbleOffsetStrength 8.
#define BubbleSeed 5.

// fish parameters
#define FishOffsetStrength vec2(10., 15)
#define CellSizeFish vec2(25., 35.)

// sunray parameters
#define MaxSunRayDepth 10.
#define SunRaySeed 60.

// the matrix for the hash function (weights from the origonal Hash function)
#define HashMat  mat3( vec3(127.1,311.7, 74.7), vec3(269.5,183.3,246.1),vec3(113.5,271.9,124.6))
#define HashMat2 mat2( vec2(127.1, 311.7), vec2(269.5, 183.3))

// the color scheme (blue, dark, green)
#define dark

// this hash is from someone elses shader (with some modifactions)
vec3 Hash(vec3 p) {  return fract(sin(p * HashMat ) * 43758.5453123) * 2. - 1.;  }
vec2 Hash(vec2 p) {  return fract(sin(p * HashMat2) * 43758.5453123) * 2. - 1.;  }

// from another persons shader
float Perlin(vec3 x)
{
    // grid
    vec3 i = floor(x);
    vec3 w = fract(x);
    
    // cubic interpolant
    vec3 u = w*w*(3.0-2.0*w);
    vec3 du = 6.0*w*(1.0-w);
    
    // gradients
    vec3 ga = Hash( i+vec3(0.0,0.0,0.0) );
    vec3 gb = Hash( i+vec3(1.0,0.0,0.0) );
    vec3 gc = Hash( i+vec3(0.0,1.0,0.0) );
    vec3 gd = Hash( i+vec3(1.0,1.0,0.0) );
    vec3 ge = Hash( i+vec3(0.0,0.0,1.0) );
	vec3 gf = Hash( i+vec3(1.0,0.0,1.0) );
    vec3 gg = Hash( i+vec3(0.0,1.0,1.0) );
    vec3 gh = Hash( i+vec3(1.0,1.0,1.0) );
    
    // projections
    float va = dot( ga, w-vec3(0.0,0.0,0.0) );
    float vb = dot( gb, w-vec3(1.0,0.0,0.0) );
    float vc = dot( gc, w-vec3(0.0,1.0,0.0) );
    float vd = dot( gd, w-vec3(1.0,1.0,0.0) );
    float ve = dot( ge, w-vec3(0.0,0.0,1.0) );
    float vf = dot( gf, w-vec3(1.0,0.0,1.0) );
    float vg = dot( gg, w-vec3(0.0,1.0,1.0) );
    float vh = dot( gh, w-vec3(1.0,1.0,1.0) );
	
    // interpolations
    return va + u.x*(vb-va) + u.y*(vc-va) + u.z*(ve-va) + u.x*u.y*(va-vb-vc+vd) + u.y*u.z*(va-vc-ve+vg) + u.z*u.x*(va-vb-ve+vf) + (-va+vb+vc-vd+ve-vf-vg+vh)*u.x*u.y*u.z;
}

// multiple layers of perlin noise (to provide more detail to it)
float Fractal(vec3 p)
{
    float perlin  = Perlin(p      ) * 0.4 ;
    perlin       += Perlin(p * 2. ) * 0.27;
    perlin       += Perlin(p * 4. ) * 0.17;
    perlin       += Perlin(p * 8. ) * 0.1 ;
    perlin       += Perlin(p * 16.) * 0.06;
    return pow(max(perlin * 2. - 0.05, 0.), 0.5);
}

// rotates a 2d point based on an angle
vec2 rot2D(vec2 v, float a)
{
    float sa = sin(a);
    float ca = cos(a);
    return v * mat2(ca, -sa, sa, ca);
}

// renders a bubble (just a circle subtracted from a larger cirlce)
float Bubble(vec2 uv, float size, float rot, float seed)
{
    // getting the new position
    vec3 cell = vec3(floor(uv * size / CellSize), seed);
    vec3 hash = Hash(cell);
    float offset = Perlin(cell + vec3(0., 0., iTime * 0.2));
    vec2 np = rot2D(mod(uv * size, CellSize) - CellSize*0.5, rot) + hash.xy*BubbleOffsetStrength*vec2(offset, 1.);
    
    // rendering the bubble
    float len = length(np);
    return max(smoothstep(1., 0.8, len) - smoothstep(0.8, 0.5, len), 0.);
}

// renders a fish (elipse plus weird function I made)
float Fish(vec2 uv, float size, float displacement)
{
    // getting the new position
    vec3 cell = vec3(floor(uv * size / CellSizeFish), 0.);
    vec3 hash = Hash(cell);
    vec2 np = mod(uv * size, CellSizeFish) - CellSizeFish*0.5 + hash.xy*FishOffsetStrength;
    np += vec2(0., sin(np.x*2. + displacement)*0.125);
    
    // rendering the bubble
    float body = smoothstep(1., 0.9, length(np*vec2(0.5, 1.)));
    float tail = np.x - 2. + pow(max(abs(np.y) - 0.2, 0.) / (np.x - 1.5), 21.);
    
    float fish = body;
    if (np.x > 1.5) fish = max(fish, smoothstep(1., 0.9, tail));
    
    return fish * (hash.z * 0.5 + 1.);
}

// the main shader
void main()
{
    // getting the uv coord
    vec2 fragCoord = v_LocalPos*iResolution;
    float zoom = 24.;
    vec2 mouseOffset = vec2(0, 0);
    vec2 uv = (fragCoord - iResolution.xy * 0.5) / iResolution.y * zoom;
    
    // the pixel size for antialiasing
    float pixelSize = zoom / iResolution.y * 1.5;
    
    // drawing the background water + fish
    float depthDarken = smoothstep(-0.5, 0.6, fragCoord.y/iResolution.y);
    vec3 col = AmbientColor * depthDarken;
    float fish = Fish( uv + vec2(iTime,                  0.), 2., iTime*2.);
    fish      += Fish(-uv + vec2(iTime, -CellSizeFish.x*0.5), 2., iTime*2.);
    col += depthDarken*0.1*min(fish, 1.);
    
    // looping through the layers of bubbles
    float stepSize = 1./Itterations;
    vec2 bubbleOffset = vec2(0., iTime);
    for (float i = stepSize; i <= 1.+stepSize; i+=stepSize)
    {
        // the itteration number
        float itteration = i*Itterations;
        
        // sun rays
        if (itteration < MaxSunRayDepth)
        {
            float sunRayLength = Fractal(vec3((uv.x - mouseOffset.x*i + uv.y*0.3) * 0.3, iTime*0.1, itteration + SunRaySeed));
            if (uv.y > (1.-sunRayLength) * 35. - 18.) col *= i*0.3+1.;
        }
        
        // rendering the bubble layer
        vec3 hash = Hash(vec3(itteration, itteration, BubbleSeed));
        col += depthDarken * BubbleColor * i *
            Bubble(uv - (bubbleOffset+mouseOffset)*i + itteration*CellSize*hash.x, (1.-i)+1., 0., itteration);
    }
    
    // the final color
    #ifdef blue
        fragColor = vec4(pow(col, vec3(0.75, 0.85, 0.6)),1.0);
    #endif
    #ifdef green
        fragColor = vec4(pow(col, vec3(0.85, 0.6, 0.6)),1.0);
    #endif
    #ifdef dark
        fragColor = vec4(col,1.0);
    #endif
}
