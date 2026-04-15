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

    // Use normalized 0..1 screen-space coordinates.
    // fragTexCoord uses bottom-left texture origin, so flip origin.y from the top-left mouse convention.
    vec2 uv = fragTexCoord;
    vec2 center = vec2(origin.x, 1.0 - origin.y);

    // Compensate for aspect ratio so the visible area stays circular.
    float aspect = renderWidth / max(renderHeight, 1.0);
    vec2 delta = uv - center;
    delta.x *= aspect;

    float radius = max(maxDis, 0.0);
    float visible = step(length(delta), radius);
    
    // Keep the original scene untouched inside the light area and fully hide the outside.
    finalColor = mix(color, source, visible);
}