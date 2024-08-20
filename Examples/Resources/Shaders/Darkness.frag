#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;

// Input uniform values
uniform sampler2D texture0;

// Output fragment color
out vec4 finalColor;

// NOTE: Add here your custom variables
uniform vec2 origin = vec2(0, 0);
uniform vec4 color = vec4(0.0, 0.0, 0.0, 1.0);
uniform float maxDis = 1;

void main()
{
    // shift to -1 to 1 range
    
    vec2 uv = (fragTexCoord * 2) - 1;
    float dis = length(uv - (origin * vec2(1, -1)));
    float f = dis / maxDis;
    
    // Texel color fetching from texture sampler
    vec4 source = texture(texture0, fragTexCoord);
    
    finalColor = mix(source, color, clamp(f, 0.0, 1.0));
}