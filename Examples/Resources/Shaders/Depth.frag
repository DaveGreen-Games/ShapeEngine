#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;

// Input uniform values
uniform sampler2D texture0;

// Output fragment color
out vec4 finalColor;

uniform float cameraNear = 0.01;
uniform float cameraFar = 10.0;

void main()
{
    float z = texture(texture0, fragTexCoord).x;

    // Linearize depth value
    float depth = (2.0*cameraNear)/(cameraFar + cameraNear - z*(cameraFar - cameraNear));

    // Calculate final fragment color
    finalColor = vec4(depth, depth, depth, 1.0f);
    
}