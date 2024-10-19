#version 330

in vec2 fragTexCoord;
out vec4 finalColor;
uniform sampler2D texture0;

uniform vec2 origin = vec2(0, 0);
uniform float minDis = 0.25;
uniform float maxDis = 1;
//uniform float threshold = 0.5;

void main()
{
    vec2 uv = (fragTexCoord * 2) - 1;
    float dis = length(uv - (origin * vec2(1, -1)));
    float f = 0;
    
    if(dis > minDis) f = dis - minDis / maxDis - minDis;
    f = 1 - f;
    
    vec4 c = texture(texture0, fragTexCoord);
    if(c.a > 0.0) finalColor = vec4(c.rgb, c.a * f);
    else finalColor = c;

}

