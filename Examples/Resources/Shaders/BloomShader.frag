#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

// Output fragment color
out vec4 finalColor;

// NOTE Add here your custom variables

uniform vec2 size = vec2(800, 450);       // Framebuffer size in pixels
uniform float threshold = 0.75;           // Only pixels brighter than this contribute to bloom
uniform float softThreshold = 0.15;       // Smooth threshold band to avoid harsh cutoffs
uniform float blurSpread = 5.0;           // Sample radius in pixels
uniform float bloomIntensity = 3.0;       // Strength of the bloom contribution


void main()
{
    vec4 source = texture(texture0, fragTexCoord);
    vec2 texelSize = vec2(1.0) / max(size, vec2(1.0));
    vec2 spread = texelSize * max(blurSpread, 0.0);

    vec2 offsets[9] = vec2[](
        vec2( 0.0,  0.0),
        vec2(-1.0,  0.0),
        vec2( 1.0,  0.0),
        vec2( 0.0, -1.0),
        vec2( 0.0,  1.0),
        vec2(-1.0, -1.0),
        vec2( 1.0, -1.0),
        vec2(-1.0,  1.0),
        vec2( 1.0,  1.0)
    );

    float weights[9] = float[](
        0.227027,
        0.121622,
        0.121622,
        0.121622,
        0.121622,
        0.071351,
        0.071351,
        0.071351,
        0.071351
    );

    vec3 bloom = vec3(0.0);

    for (int i = 0; i < 9; i++)
    {
        vec4 sampleTexel = texture(texture0, fragTexCoord + offsets[i] * spread);
        vec3 sampleColor = sampleTexel.rgb * sampleTexel.a;
        float brightness = dot(sampleColor, vec3(0.2126, 0.7152, 0.0722));
        float brightMask = smoothstep(threshold - softThreshold, threshold + softThreshold, brightness);
        bloom += sampleColor * brightMask * weights[i];
    }

    vec3 bloomContribution = bloom * max(bloomIntensity, 0.0);
    vec3 finalRgb = source.rgb + bloomContribution;
    float bloomAlpha = clamp(max(max(bloomContribution.r, bloomContribution.g), bloomContribution.b), 0.0, 1.0);
    float finalAlpha = max(source.a, bloomAlpha);

    finalColor = vec4(finalRgb, finalAlpha) * colDiffuse;
}