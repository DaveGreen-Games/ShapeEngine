#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;

// Input uniform values
uniform sampler2D texture0;

// Output fragment color
out vec4 finalColor;

// NOTE Add here your custom variables
uniform float renderWidth = 800.0;
uniform float renderHeight = 450.0;
uniform vec2 origin = vec2(0, 0);
uniform vec4 color = vec4(0.0, 0.0, 0.0, 1.0);
uniform float maxDis = 1;

void main()
{
    // Texel color fetching from texture sampler
    vec4 source = texture(texture0, fragTexCoord);

    // Shift UVs to -1..1 so origin stays compatible with the existing setup.
    vec2 uv = (fragTexCoord * 2.0) - 1.0;
    vec2 center = origin * vec2(1.0, -1.0);

    // Compensate for aspect ratio so the visible area stays circular.
    float aspect = renderWidth / max(renderHeight, 1.0);
    vec2 delta = uv - center;
    delta.x *= aspect;

    float radius = max(maxDis, 0.0);
    float visible = step(length(delta), radius);
    
    // Keep the original scene untouched inside the light area and fully hide the outside.
    finalColor = mix(color, source, visible);
}