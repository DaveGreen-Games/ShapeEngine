#version 330

// Fake depth / focus-falloff post-process shader.
// Uses screen-space distance from origin as a radial depth value and applies
// fog, darkness, blur, and desaturation as pixels get farther away.

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;

// Input uniform values
uniform sampler2D texture0;

// Output fragment color
out vec4 finalColor;

uniform float renderWidth = 800.0;
uniform float renderHeight = 450.0;
uniform vec2 origin = vec2(0.0, 0.0);
uniform float minDis = 0.2;
uniform float maxDis = 1.0;
uniform vec4 fogColor = vec4(0.05, 0.08, 0.12, 1.0);
uniform float fogStrength = 0.45;
uniform float darknessStrength = 0.35;
uniform float desaturationStrength = 0.4;
uniform float blurStrength = 2.0;

vec4 sampleBlur9(vec2 uv, vec2 blurOffset)
{
    vec2 dx = vec2(blurOffset.x, 0.0);
    vec2 dy = vec2(0.0, blurOffset.y);

    vec4 samples[9] = vec4[9](
        texture(texture0, uv - dx - dy),
        texture(texture0, uv - dy),
        texture(texture0, uv + dx - dy),
        texture(texture0, uv - dx),
        texture(texture0, uv),
        texture(texture0, uv + dx),
        texture(texture0, uv - dx + dy),
        texture(texture0, uv + dy),
        texture(texture0, uv + dx + dy)
    );

    float weights[9] = float[9](
        1.0 / 16.0, 2.0 / 16.0, 1.0 / 16.0,
        2.0 / 16.0, 4.0 / 16.0, 2.0 / 16.0,
        1.0 / 16.0, 2.0 / 16.0, 1.0 / 16.0
    );

    vec3 premultipliedRgb = vec3(0.0);
    float alpha = 0.0;

    for (int i = 0; i < 9; i++)
    {
        premultipliedRgb += samples[i].rgb * samples[i].a * weights[i];
        alpha += samples[i].a * weights[i];
    }

    vec3 rgb = alpha > 0.0001 ? premultipliedRgb / alpha : vec3(0.0);
    return vec4(rgb, alpha);
}

void main()
{
    vec4 source = texture(texture0, fragTexCoord);

    vec2 uv = (fragTexCoord * 2.0) - 1.0;
    vec2 center = origin * vec2(1.0, -1.0);

    float aspect = renderWidth / max(renderHeight, 1.0);
    vec2 delta = uv - center;
    delta.x *= aspect;

    float innerRadius = min(minDis, maxDis);
    float outerRadius = max(minDis, maxDis);
    float fakeDepth = smoothstep(innerRadius, outerRadius, length(delta));
    float effectAmount = fakeDepth;

    vec2 texelSize = vec2(1.0 / max(renderWidth, 1.0), 1.0 / max(renderHeight, 1.0));
    vec2 blurOffset = texelSize * max(blurStrength, 0.0);

    vec4 blurred = sampleBlur9(fragTexCoord, blurOffset);
    vec3 rgb = mix(source.rgb, blurred.rgb, effectAmount);
    float alpha = mix(source.a, blurred.a, effectAmount);
    float visibleAmount = effectAmount * alpha;

    float luminance = dot(rgb, vec3(0.299, 0.587, 0.114));
    rgb = mix(rgb, vec3(luminance), clamp(desaturationStrength, 0.0, 1.0) * visibleAmount);

    rgb = mix(rgb, fogColor.rgb, clamp(fogStrength, 0.0, 1.0) * visibleAmount);
    rgb *= 1.0 - (clamp(darknessStrength, 0.0, 1.0) * visibleAmount);

    finalColor = vec4(rgb, alpha);
}