#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

// Output fragment color
out vec4 finalColor;

// NOTE: Add here your custom variables

uniform vec2 size = vec2(800, 450);   // Framebuffer size
uniform float samples = 5.0;          // Pixels per axis; higher = bigger glow, worse performance
uniform float quality = 2.5;          // Defines size factor: Lower = smaller glow, better quality
//uniform vec4 targetColor = vec4(1.0, 0.498, 0.314, 1.0);
//uniform float targetThreshold = 0.25;


void main()
{
    vec4 sum = vec4(0);
    vec2 sizeFactor = vec2(1)/size*quality;

    // Texel color fetching from texture sampler
    vec4 source = texture(texture0, fragTexCoord);

    const int range = 2;            // should be = (samples - 1)/2;

    for (int x = -range; x <= range; x++)
    {
        for (int y = -range; y <= range; y++)
        {
            sum += texture(texture0, fragTexCoord + vec2(x, y)*sizeFactor);
        }
    }

    // Calculate final fragment color
    finalColor = ((sum/(samples*samples)) + source)*colDiffuse;

    
}