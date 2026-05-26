// https://www.shadertoy.com/view/dljcWw

#version 330
layout (location = 0) out vec4 fragColor;
in vec2 v_LocalPos;

uniform vec2 iResolution;
uniform float iTime;
uniform vec3 main_color;

#define NOISE_PERIOD 16. // needs to be an integer value
#define PI 3.14159265359
#define TAU 6.2831853071

vec4 mod289(vec4 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}
vec4 permute(vec4 x) {
  return mod289(((x*34.0)+10.0)*x);
}
vec4 taylorInvSqrt(vec4 r) {
  return 1.79284291400159 - 0.85373472095314 * r;
}
vec2 fade(vec2 t) {
  return t*t*t*(t*(t*6.0-15.0)+10.0);
}

// Classic Perlin noise, periodic variant
float pnoise(vec2 P, vec2 rep) {
  vec4 Pi = floor(P.xyxy) + vec4(0.0, 0.0, 1.0, 1.0);
  vec4 Pf = fract(P.xyxy) - vec4(0.0, 0.0, 1.0, 1.0);
  Pi = mod(Pi, rep.xyxy); // To create noise with explicit period
  Pi = mod289(Pi);        // To avoid truncation effects in permutation
  vec4 ix = Pi.xzxz;
  vec4 iy = Pi.yyww;
  vec4 fx = Pf.xzxz;
  vec4 fy = Pf.yyww;

  vec4 i = permute(permute(ix) + iy);

  vec4 gx = fract(i * (1.0 / 41.0)) * 2.0 - 1.0 ;
  vec4 gy = abs(gx) - 0.5 ;
  vec4 tx = floor(gx + 0.5);
  gx = gx - tx;

  vec2 g00 = vec2(gx.x,gy.x);
  vec2 g10 = vec2(gx.y,gy.y);
  vec2 g01 = vec2(gx.z,gy.z);
  vec2 g11 = vec2(gx.w,gy.w);

  vec4 norm = taylorInvSqrt(vec4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
  g00 *= norm.x;  
  g01 *= norm.y;  
  g10 *= norm.z;  
  g11 *= norm.w;  

  float n00 = dot(g00, vec2(fx.x, fy.x));
  float n10 = dot(g10, vec2(fx.y, fy.y));
  float n01 = dot(g01, vec2(fx.z, fy.z));
  float n11 = dot(g11, vec2(fx.w, fy.w));

  vec2 fade_xy = fade(Pf.xy);
  vec2 n_x = mix(vec2(n00, n01), vec2(n10, n11), fade_xy.x);
  float n_xy = mix(n_x.x, n_x.y, fade_xy.y);
  return 2.3 * n_xy;
}

float fbm_periodic(vec2 pos, int octaves, float persistence, vec2 period) {
	float total = 0., frequency = 1., amplitude = 1., maxValue = 0.;
	for(int i = 0; i < octaves; ++i) {
		total += pnoise(pos * frequency, period) * amplitude;
		maxValue += amplitude;
		amplitude *= persistence;
		frequency *= 2.;
	}
	return total / maxValue;
}

vec3 plasma(vec2 uv, float r) {
	float len = length(uv);
	
	float light = 0.1 / abs(len-r) * r; // we multiply by 'r' to scale it correctly when 'r' changes
	
    if (len < r)
        light *= len/r * 0.3; // cut the inner part out (the constant determines how much glow do we allow to bleed over to the inside of the black hole - note that we could just multiply by some constant and omit len/r part altogether)
    
	light = pow(light, 0.7); // add some more power to it
	light *= smoothstep(3.5*r, 1.5*r, len); // limit the light range
    
	return light * vec3(0.9, 0.65, 0.5)/*Color it a bit*/;
}

// ### TONE MAPPING CODE ###
//
// Uchimura 2017, "HDR theory and practice"
// Math: https://www.desmos.com/calculator/gslcdxvipg
// Source: https://www.slideshare.net/nikuque/hdr-theory-and-practicce-jp
// (code found here: https://github.com/dmnsgn/glsl-tone-map)
vec3 uchimura(vec3 x, float P, float a, float m, float l, float c, float b) {
  float l0 = ((P - m) * l) / a;
  float L0 = m - m / a;
  float L1 = m + (1.0 - m) / a;
  float S0 = m + l0;
  float S1 = m + a * l0;
  float C2 = (a * P) / (P - S1);
  float CP = -C2 / P;

  vec3 w0 = vec3(1.0 - smoothstep(0.0, m, x));
  vec3 w2 = vec3(step(m + l0, x));
  vec3 w1 = vec3(1.0 - w0 - w2);

  vec3 T = vec3(m * pow(x / m, vec3(c)) + b);
  vec3 S = vec3(P - (P - S1) * exp(CP * (x - S0)));
  vec3 L = vec3(m + a * (x - m));

  return T * w0 + L * w1 + S * w2;
}
vec3 uchimura(vec3 x) {
  const float P = 1.0;  // max display brightness
  const float a = 1.0;  // contrast
  const float m = 0.22; // linear section start
  const float l = 0.4;  // linear section length
  const float c = 1.33; // black
  const float b = 0.0;  // pedestal

  return uchimura(x, P, a, m, l, c, b);
}

void main() {
    float BlackHoleRadius = 0.15;

    // first do the rays:
    
    vec2 fragCoord = v_LocalPos*iResolution;
    float radius =1.0; // circle radius
    float time = iTime * 0.9;
    vec2 uv = (fragCoord-.5*iResolution.xy)/iResolution.y;
    uv *= 5.2; // scale it a bit
    vec3 col = vec3(1.0, 0.9, 0.7); // resulting color
    
    // polar coordinates:
    float r = length(uv) / radius; // radial coordinate
    float phi = atan(uv.y, uv.x); // angular coordinate
    
   	float a = fbm_periodic(vec2((phi + PI)/TAU * NOISE_PERIOD, time), 1, 0.5, vec2(NOISE_PERIOD, 100.)); // we don't really need periodicy in Y direction, but anyway, we just put some random number
	a = (a + 1.0) * 0.5; // map it to [0..1]
	a *= .7; // scale it a bit
    a = pow(a, 1.6); // "sharpen" it up a bit
    
    col *= smoothstep(a+0.81 * BlackHoleRadius, a * BlackHoleRadius, r); // creates the rays
    
    // add underneath glow:
	col *= pow(1./pow(r / BlackHoleRadius, 1.2) * .8, 3.) * vec3(1.0, 0.8, 0.7)/*glow color*/;
    
    // cut out the inner area to create the black hole:
    col *= smoothstep(BlackHoleRadius, BlackHoleRadius + 0.04/*antialiasing*/, length(uv));

    // finally do the plasma:
    col += plasma(uv, BlackHoleRadius);
    
    // do some tone mapping (optional):
    col = uchimura(col);
    
    // Output to screen
    fragColor = vec4(vec3(col), 1.0);
}