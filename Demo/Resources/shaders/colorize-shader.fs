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
const vec3 scale = vec3(1, 1, 1);

void main()
{
    vec4 texelColor = texture(texture0, fragTexCoord);//*colDiffuse*fragColor;
    texelColor.x *= scale.x;
    texelColor.y *= scale.y;
    texelColor.z *= scale.z;
    finalColor = texelColor;
}