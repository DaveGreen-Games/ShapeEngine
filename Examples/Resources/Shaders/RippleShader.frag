#version 330

// Radial ripple / shockwave-style post-process shader.
// Distorts the screen texture outward/inward from origin using a sinusoidal ripple.
// animation is intended to be driven from 0.0 to 1.0 and wraps seamlessly.

in vec2 fragTexCoord;
out vec4 finalColor;
uniform sampler2D texture0;

uniform float renderWidth = 800.0;
uniform float renderHeight = 450.0;
uniform vec2 origin = vec2(0.0, 0.0);
uniform float magnitude = 0.03;
uniform float frequency = 24.0;
uniform float animation = 0.0;   // expected range: 0.0 .. 1.0
uniform float radius = 1.0;
uniform float falloff = 2.0;

const float TWO_PI = 6.28318530718;

void main()
{
    vec2 uvCentered = fragTexCoord;
    vec2 center = vec2(origin.x, 1.0 - origin.y);

    float aspect = renderWidth / max(renderHeight, 1.0);
    vec2 delta = uvCentered - center;
    vec2 deltaAspect = vec2(delta.x * aspect, delta.y);

    float distance = length(deltaAspect);
    vec2 directionAspect = distance > 0.0001 ? deltaAspect / distance : vec2(0.0, 0.0);
    vec2 directionCentered = vec2(directionAspect.x / max(aspect, 0.0001), directionAspect.y);

    float safeRadius = max(radius, 0.0001);
    float normalizedDistance = clamp(distance / safeRadius, 0.0, 1.0);
    float attenuation = pow(max(1.0 - normalizedDistance, 0.0), max(falloff, 0.0001));

    float phase = fract(animation) * TWO_PI;
    float wave = sin((distance * frequency) - phase);
    float displacement = wave * magnitude * attenuation;

    vec2 rippleUv = fragTexCoord + (directionCentered * displacement * 0.5);
    rippleUv = clamp(rippleUv, vec2(0.0), vec2(1.0));

    finalColor = texture(texture0, rippleUv);
}

