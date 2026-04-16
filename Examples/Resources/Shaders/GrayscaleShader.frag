#version 330

in vec2 fragTexCoord;
out vec4 finalColor;
uniform sampler2D texture0;

void main()
{
    vec4 source = texture(texture0, fragTexCoord);
    float luminance = dot(source.rgb, vec3(0.299, 0.587, 0.114));
    finalColor = vec4(vec3(luminance), source.a);
}

