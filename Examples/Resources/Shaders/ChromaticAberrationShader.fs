#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
//in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
//uniform vec4 colDiffuse;


// Output fragment color
out vec4 finalColor;

// NOTE: Add here your custom variables
uniform vec2 amount = vec2(1.2, 1.2);

void main()
{
    // Texel color fetching from texture sampler
    vec4 texelColor = texture(texture0, fragTexCoord);//*colDiffuse*fragColor;
    
    vec2 adjustedAmount = amount * texelColor.x / 100.0;
    texelColor.x = texture(texture0, fragTexCoord + adjustedAmount).r;
    texelColor.y = texture(texture0, fragTexCoord).g;
    texelColor.z = texture(texture0, fragTexCoord - adjustedAmount).b;
    
    finalColor = texelColor;
}