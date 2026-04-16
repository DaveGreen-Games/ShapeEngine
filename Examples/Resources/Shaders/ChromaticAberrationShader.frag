#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
//in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
//uniform vec4 colDiffuse;


// Output fragment color
out vec4 finalColor;

// NOTE amount is in pixels and origin uses the same normalized 0..1 convention as ScreenInfo.RelativeMousePosition.
// The shader flips origin.y because fragTexCoord uses a bottom-left texture origin.
uniform float renderWidth = 800.0;
uniform float renderHeight = 450.0;
uniform vec2 origin = vec2(0.0, 0.0);
uniform vec2 amount = vec2(2, 2);

void main()
{
    vec4 source = texture(texture0, fragTexCoord);

    vec2 uv = fragTexCoord;
    vec2 center = vec2(origin.x, 1.0 - origin.y);

    float aspect = renderWidth / max(renderHeight, 1.0);
    vec2 delta = uv - center;
    vec2 correctedDelta = delta;
    correctedDelta.x *= aspect;

    float distanceFactor = clamp(length(correctedDelta), 0.0, 1.0);
    vec2 uvDirection = vec2(0.0);
    if (distanceFactor > 0.0)
    {
        uvDirection = normalize(vec2(correctedDelta.x / max(aspect, 0.0001), correctedDelta.y));
    }

    vec2 texelSize = vec2(1.0 / max(renderWidth, 1.0), 1.0 / max(renderHeight, 1.0));
    vec2 offset = uvDirection * amount * distanceFactor * texelSize;

    finalColor = vec4(
        texture(texture0, fragTexCoord + offset).r,
        source.g,
        texture(texture0, fragTexCoord - offset).b,
        source.a
    );
}