// https://www.shadertoy.com/view/4dKXDK
#version 330
layout (location = 0) out vec4 fragColor;
in vec2 v_LocalPos;

uniform vec2 iResolution;
uniform float iTime;

#define LEVEL_NUMBER 10.

#define SF 15.

#define bsin(x) (sin(x) * .5 + .5)

#define NB_COLORS 4
vec3 mColors[NB_COLORS];
void init()
{
    mColors[0] = vec3(0.3,.00,.50)*.9;
    mColors[1] = vec3(0.3,.01,.50)*.8;
    mColors[2] = vec3(0.3,.00,.49)*.7;
    mColors[3] = vec3(0.3,.00,.49)*.6;
}

float fbm(float x)
{
    float r  = bsin(x * 0001.9 ) * 0.25000;
 	r 		+= bsin(x * 002.54 ) * 0.25000;
 	r 		+= bsin(x * 0006.5 ) * 0.17000;
    
    r *= .7;
    r += .7;
    
    return floor(r * SF) / SF;
}

float mountain(float x)
{
    float d = SF;
    float f1 = fbm(floor(x * SF) / SF);
    float f2 = fbm((floor(x * SF) + 1.) / SF);
    return f1 + (f2 - f1) * fract(x*d);
}

void applyMountain(inout vec3 col,float height,float xCoord, vec3 mColor)
{
    float m = mountain(xCoord);
 	col = mix(col,mColor,step(height,m));
}

void main()
{
    init();
    vec2 fragCoord = v_LocalPos*iResolution;
	vec2 uv = fragCoord.xy / 400.;
    
    uv.x += floor(uv.y/1.2) * 3.;
    uv.y = mod(uv.y,1.2);
    
    
    uv.x +=+ iTime *.15;
    
	
    vec3 col = mColors[NB_COLORS-1];
    applyMountain(col,uv.y         , uv.x          , mColors[0]);
    applyMountain(col,uv.y + .25   , uv.x + 7.6    , mColors[1]);
    applyMountain(col,uv.y + .5    , uv.x + 62.2   , mColors[2]);
    applyMountain(col,uv.y + .75   , uv.x + 1413.7 , mColors[3]);
    
    fragColor = vec4(col*0.7,1.);
    
}