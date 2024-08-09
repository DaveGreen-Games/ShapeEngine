#version 330

//bloom / glow shader

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
//in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
//uniform vec4 colDiffuse;

// Output fragment color
out vec4 finalColor;

// input
uniform vec4 targetColor = vec4(1.0, 1.0, 0.5, 1.0);
uniform vec4 replacementColor = vec4(0.0, 1.0, 1.0, 1.0);
uniform float threshold = 0.35;
uniform float intensity = 1.0;
uniform float blend = 1.0;

void main() {
    
    // Get the pixel color from the texture
    vec4 pixel_color = texture(texture0, fragTexCoord);
    
    // Calculate the distance between the pixel color and the first source color
    float dis = length(pixel_color - targetColor) / 4;
    
    // If the distance to either source color is below the threshold, set the output color to a blend of the pixel color and the modified glow color
    if (dis < threshold) 
    {
        finalColor = mix(pixel_color, replacementColor * intensity, blend);
    }
    // Otherwise, set the output color to the pixel color
    else 
    {
        finalColor = pixel_color;
    }
}