// https://www.shadertoy.com/view/wtdGWl

#version 330
layout (location = 0) out vec4 fragColor;
in vec2 v_LocalPos;

uniform vec2 iResolution;
uniform float iTime;

#define S(a, b, t) smoothstep(a,b,t)
#define LAYER_COUNT 8.
#define MOON_SIZE .1
#define TREE_COL vec3(.8, .8, 1.)
#define ORBIT_SPEED .025
#define SCROLL_SPEED .3
#define ROT -0.785398
#define ZOOM .4
#define STAR_SPEED .25

float N11(float p)
{
    p = fract(p * .1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}

float N21(vec2 p)
{
	vec3 p3  = fract(vec3(p.xyx) * .1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float DistLine(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p-a;
    vec2 ba = b-a;
    float t = clamp(dot(pa, ba)/ dot(ba, ba), 0., 1.);
    return length(pa - ba*t);
}


float DrawLine(vec2 p, vec2 a, vec2 b) {
    float d = DistLine(p, a, b);
    float m = S(.00125, .000001, d);
    float d2 = length(a-b);
    m *= S(1., .5, d2) + S(.04, .03, abs(d2-.75));
    return m;
}

float ShootingStar(vec2 uv) {    
    vec2 gv = fract(uv)-.5;
    vec2 id = floor(uv);
    
    float h = N21(id);
    
    float line = DrawLine(gv, vec2(0., h), vec2(.1, h));
    float trail = S(.12, .0, gv.x);
	
    return line * trail;
}

float LayerShootingStars(vec2 uv) {
    float t = iTime * STAR_SPEED;
    vec2 rv1 = vec2(uv.x - t, uv.y + t);
    rv1.x *= 1.05;
    
    float r = 3. * ROT;
    float s = sin(r);
    float c = cos(r);
    mat2 rot = mat2(c, -s, s, c);
    rv1 *= rot;
    rv1 *= ZOOM * .9;
    
    vec2 rv2 = uv + t * 1.2;
    rv2.x *= 1.05;
    
    r = ROT;
    s = sin(r);
    c = cos(r);
    rot = mat2(c, -s, s, c);
    rv2 *= rot;
    rv2 *= ZOOM * 1.1;
    
    float star1 = ShootingStar(rv1);
    float star2 = ShootingStar(rv2);
    return clamp(0., 1., star1 + star2);
}

float GetHeight(float x) {
    return sin(x*.412213)+sin(x)*.512347;
}

float TaperBox(vec2 p, float wb, float wt, float yb, float yt, float blur) {
    //bottom edge
    float m = S(-blur, blur, p.y - yb);
    //top edge
    m *= S(blur, -blur, p.y - yt);
    //mirror x to get both edges
    p.x = abs(p.x);
    //side edges
    // 0 p.y = yb 1 p.y = yt
    float w = mix(wb, wt, (p.y - yb) / (yt - yb));
    m *= S(blur, -blur, p.x - w);
    return m;
}

vec4 Tree(vec2 uv, vec3 col, float blur) {
    float m = TaperBox(uv, .03, .03, -.05, .25, blur); //trunk
    m += TaperBox(uv, .2, .1, .25, .5, blur); //canopy 1
    m += TaperBox(uv, .15, .05, .5, .75, blur); //canopy 2
    m += TaperBox(uv, .1, .0, .75, 1., blur); //top
    
    blur *= 3.;
    float shadow = TaperBox(uv-vec2(.2,0.), .1, .5, .15, .253, blur);
    shadow += TaperBox(uv+vec2(.25,0.), .1, .5, .45, .503, blur);
    shadow += TaperBox(uv-vec2(.25,0.), .1, .5, .7, .753, blur);
    col -= shadow*.8;
    
    return vec4(col, m);
}

vec4 Layer(vec2 uv, float blur) {
    vec4 col = vec4(0);
    float id = floor(uv.x);
    
    //random [-1, 1]
    float n = N11(id)*2.-1.;
    float x = n*.3;
    float y = GetHeight(uv.x);
    
    //ground
    float ground = S(blur, -blur, uv.y-y);
    col += ground;
    
    y = GetHeight(id + .5 - x);
    
    //vertical grid
    uv.x = fract(uv.x)-.5;
    //					 offset		scale tree size		color
    vec4 tree = Tree((uv+vec2(x,-y))*vec2(1, 1.+n*.2), vec3(1), blur);
    
    col = mix(col, tree, tree.a);
    col.a = max(ground, tree.a);
    return col;
}

vec2 MoonPos() {
    float t = iTime * ORBIT_SPEED;
    return vec2(sin(t), cos(t))/2.;
}

vec4 MoonGlow(vec2 uv) {
    vec2 moonPos = MoonPos();
    float md = length(uv-(moonPos - vec2(.07, 0.01)));
    float moon = S(.1, -.01, md-MOON_SIZE*8.);
    moon = mix(0., moon, clamp((moonPos.y + .2)*3., 0., 1.));
    
    vec4 col = vec4(moon);
    md = clamp(1.-md, 0., 1.);
    md *= md;
    col.rgb *= md;
    return col;
}

void main()
{
    vec2 fragCoord = v_LocalPos*iResolution;
    vec2 uv = (fragCoord - .5*iResolution.xy)/iResolution.y;
    float t = iTime * SCROLL_SPEED;
    float blur = .005;
    
    // Background Stars
    float twinkle = dot(length(sin(uv+t)), length(cos(uv*vec2(22., 6.7)-t*3.)));
    twinkle = sin(twinkle*5.)*.5+.5;
    float stars = pow(N21(uv*1000.), 1024.)*twinkle;
    vec4 col = vec4(stars);
    
    // Moon
    vec2 moonPos = MoonPos();
    float moon = S(.01, -.01, length(uv-moonPos)-MOON_SIZE);
    col *= 1.-moon;
    moon *= S(-.01, .05, length(uv-(moonPos+vec2(.1,.05)))-MOON_SIZE);
    col += moon;
    
    col += LayerShootingStars(uv);
    
    vec4 layer;
    // CHANGE: Start 'i' at 0.3 instead of 0.0 to skip the closest layers
    for (float i=0.3; i < 1.0; i += 1.0/LAYER_COUNT) {
        float scale = mix(30., 1., i);
        blur = mix(.05, .005, i);
        layer = Layer(uv*scale+vec2(t+i*100.,i), blur);
        layer.rgb *= (1.-i) * TREE_COL;
        col = mix(col, layer, layer.a);
    }
    
    col += MoonGlow(uv);
    
    // REMOVED: The extra foreground layer that was rendered here
    
    fragColor = col*0.3;
}