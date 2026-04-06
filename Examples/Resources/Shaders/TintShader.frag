#version 330

// Full-screen tint / overlay shader.
// Default mode tints visible pixels toward overdrawColor while preserving source alpha.
// Optional mode can affect transparency too, restoring the original full RGBA overlay behavior.

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
//in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
//uniform vec4 colDiffuse;

// Output fragment color
out vec4 finalColor;

// input
uniform vec4 tintColor = vec4(1.0, 1.0, 1.0, 1.0);
uniform float blend = 0.5;
uniform float affectTransparency = 0.0; // 0 = preserve source alpha, 1 = blend RGBA uniformly

void main() {
    vec4 pixelColor = texture(texture0, fragTexCoord);
    float overlayAmount = clamp(blend, 0.0, 1.0);
    float transparencyMode = step(0.5, affectTransparency);

    float rgbAmount = mix(overlayAmount * pixelColor.a, overlayAmount, transparencyMode);
    vec3 finalRgb = mix(pixelColor.rgb, tintColor.rgb, rgbAmount);

    float overlayAlpha = mix(pixelColor.a, tintColor.a, overlayAmount);
    float finalAlpha = mix(pixelColor.a, overlayAlpha, transparencyMode);

    finalColor = vec4(finalRgb, finalAlpha);
}