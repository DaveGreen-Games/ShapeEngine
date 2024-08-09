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
uniform vec4 color1 = vec4(1.0, 0.0, 0.0, 1.0);
uniform vec4 color2 = vec4(1.0, 0.0, 0.0, 1.0);
uniform vec4 glow_color = vec4(1.0, 0.0, 1.0, 1.0);
uniform float threshold = 1.0;
uniform float intensity = 0.75;
uniform float opacity = 1.0;

void main() {
    
    // Get the pixel color from the texture
    vec4 pixel_color = texture(texture0, fragTexCoord);
    
    // Calculate the distance between the pixel color and the first source color
    float distance_first = length(pixel_color - color1);
    
    // Calculate the distance between the pixel color and the second source color
    float distance_second = length(pixel_color - color2);
    
    // Create a new variable to store the modified glow color
    vec4 modified_glow_color = glow_color;
    
    // Set the alpha value of the modified glow color to the specified opacity
    modified_glow_color.a = opacity;
    
    // If the distance to either source color is below the threshold, set the output color to a blend of the pixel color and the modified glow color
    if (distance_first < threshold || distance_second < threshold) 
    {
        finalColor = mix(pixel_color, modified_glow_color * intensity, modified_glow_color.a);
    }
    // Otherwise, set the output color to the pixel color
    else 
    {
        finalColor = pixel_color;
    }
}