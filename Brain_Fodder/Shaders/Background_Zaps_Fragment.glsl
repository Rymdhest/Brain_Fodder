// https://www.shadertoy.com/view/XXyGzh

#version 330
layout (location = 0) out vec4 fragColor;
in vec2 v_LocalPos;

uniform vec2 iResolution;
uniform float iTime;
uniform vec3 main_color;

vec2 stanh(vec2 a) {
    return tanh(clamp(a, -40.,  40.));
}

void main()
{
    vec4 o;
    vec2 u = v_LocalPos*iResolution;

    vec2 v = iResolution.xy;
         u = .2*(u+u-v)/v.y;    
         
    vec4 z = o = vec4(1,2,3,0);
     
    for (float a = .5, t = iTime, i; 
         ++i < 19.; 
         o += (1. + cos(z+t)) 
            / length((1.+i*dot(v,v)) 
                   * sin(1.5*u/(.5-dot(u,u)) - 9.*u.yx + t))
         )  
        v = cos(++t - 7.*u*pow(a += .03, i)) - 5.*u, 
        u += stanh(40. * dot(u *= mat2(cos(i + .02*t - z.wxzw*11.))
                           ,u)
                      * cos(1e2*u.yx + t)) / 2e2
           + .2 * a * u
           + cos(4./exp(dot(o,o)/1e2) + t) / 3e2;
              
     o = 25.6 / (min(o, 13.) + 164. / o) 
       - dot(u, u) / 250.;
       o.rgb *= 0.45;
       fragColor = o;
}
