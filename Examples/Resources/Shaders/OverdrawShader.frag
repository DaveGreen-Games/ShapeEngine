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
uniform vec4 overdrawColor = vec4(1.0, 1.0, 1.0, 1.0);
uniform float blend = 0.5;

void main() {
    
    // Get the pixel color from the texture
    vec4 pixel_color = texture(texture0, fragTexCoord);
//    float x = fract(fragTexCoord.s);
//    float final = smoothstep(blend - 0.1, blend + 0.1, x);
    finalColor = mix(pixel_color, overdrawColor, blend);
    //finalColor = mix(pixel_color, overdrawColor, blend);
}