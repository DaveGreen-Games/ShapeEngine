#version 330

// Multi-source explosion shockwave post-process shader.
// Each shockwave is an expanding ring that distorts the screen outward from an origin.
// This version supports up to 8 simultaneous shockwaves in a single pass.
//
// shockwaveN      = vec4(origin.x, origin.y, progress, magnitude)
// shockwaveParamsN = vec4(maxRadius, bandWidth, lifeFalloff, enabled)
//
// progress:    0.0 -> just started, 1.0 -> finished
// magnitude:   distortion strength
// maxRadius:   maximum ring radius in centered/aspect-corrected space
// bandWidth:   ring thickness / softness
// lifeFalloff: how quickly the wave fades over its lifetime
// enabled:     > 0.5 = active, <= 0.5 = ignored

in vec2 fragTexCoord;
out vec4 finalColor;
uniform sampler2D texture0;

uniform float renderWidth = 800.0;
uniform float renderHeight = 450.0;
uniform float globalStrength = 1.0;
uniform float maxCombinedOffset = 0.08;

uniform vec4 shockwave0 = vec4(0.0, 0.0, 0.0, 0.0);
uniform vec4 shockwave1 = vec4(0.0, 0.0, 0.0, 0.0);
uniform vec4 shockwave2 = vec4(0.0, 0.0, 0.0, 0.0);
uniform vec4 shockwave3 = vec4(0.0, 0.0, 0.0, 0.0);
uniform vec4 shockwave4 = vec4(0.0, 0.0, 0.0, 0.0);
uniform vec4 shockwave5 = vec4(0.0, 0.0, 0.0, 0.0);
uniform vec4 shockwave6 = vec4(0.0, 0.0, 0.0, 0.0);
uniform vec4 shockwave7 = vec4(0.0, 0.0, 0.0, 0.0);

uniform vec4 shockwaveParams0 = vec4(1.0, 0.08, 1.5, 0.0);
uniform vec4 shockwaveParams1 = vec4(1.0, 0.08, 1.5, 0.0);
uniform vec4 shockwaveParams2 = vec4(1.0, 0.08, 1.5, 0.0);
uniform vec4 shockwaveParams3 = vec4(1.0, 0.08, 1.5, 0.0);
uniform vec4 shockwaveParams4 = vec4(1.0, 0.08, 1.5, 0.0);
uniform vec4 shockwaveParams5 = vec4(1.0, 0.08, 1.5, 0.0);
uniform vec4 shockwaveParams6 = vec4(1.0, 0.08, 1.5, 0.0);
uniform vec4 shockwaveParams7 = vec4(1.0, 0.08, 1.5, 0.0);

vec2 accumulateShockwave(vec2 uvCentered, float aspect, vec4 shockwave, vec4 params)
{
    if (params.w <= 0.5) return vec2(0.0);

    float progress = clamp(shockwave.z, 0.0, 1.0);
    float magnitude = shockwave.w;
    float maxRadius = max(params.x, 0.0001);
    float bandWidth = max(params.y, 0.0001);
    float lifeFalloff = max(params.z, 0.0001);

    vec2 center = shockwave.xy * vec2(1.0, -1.0);
    vec2 delta = uvCentered - center;
    vec2 deltaAspect = vec2(delta.x * aspect, delta.y);

    float distance = length(deltaAspect);
    if (distance <= 0.0001) return vec2(0.0);

    vec2 directionAspect = deltaAspect / distance;
    float currentRadius = progress * maxRadius;
    float ringDistance = abs(distance - currentRadius);

    float ring = 1.0 - smoothstep(bandWidth, bandWidth * 2.0, ringDistance);
    float life = pow(max(1.0 - progress, 0.0), lifeFalloff);
    float strength = ring * life * magnitude;

    return directionAspect * strength;
}

void main()
{
    vec2 uvCentered = (fragTexCoord * 2.0) - 1.0;
    float aspect = renderWidth / max(renderHeight, 1.0);

    vec2 totalOffsetAspect = vec2(0.0);
    totalOffsetAspect += accumulateShockwave(uvCentered, aspect, shockwave0, shockwaveParams0);
    totalOffsetAspect += accumulateShockwave(uvCentered, aspect, shockwave1, shockwaveParams1);
    totalOffsetAspect += accumulateShockwave(uvCentered, aspect, shockwave2, shockwaveParams2);
    totalOffsetAspect += accumulateShockwave(uvCentered, aspect, shockwave3, shockwaveParams3);
    totalOffsetAspect += accumulateShockwave(uvCentered, aspect, shockwave4, shockwaveParams4);
    totalOffsetAspect += accumulateShockwave(uvCentered, aspect, shockwave5, shockwaveParams5);
    totalOffsetAspect += accumulateShockwave(uvCentered, aspect, shockwave6, shockwaveParams6);
    totalOffsetAspect += accumulateShockwave(uvCentered, aspect, shockwave7, shockwaveParams7);

    totalOffsetAspect *= max(globalStrength, 0.0);

    float offsetLength = length(totalOffsetAspect);
    float safeMaxOffset = max(maxCombinedOffset, 0.0001);
    if (offsetLength > safeMaxOffset)
    {
        totalOffsetAspect = (totalOffsetAspect / offsetLength) * safeMaxOffset;
    }

    vec2 totalOffset = vec2(totalOffsetAspect.x / max(aspect, 0.0001), totalOffsetAspect.y) * 0.5;
    vec2 distortedUv = clamp(fragTexCoord + totalOffset, vec2(0.0), vec2(1.0));

    finalColor = texture(texture0, distortedUv);
}

