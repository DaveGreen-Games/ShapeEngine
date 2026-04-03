#version 330

in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;

// Match your existing pattern for screen-space shaders
uniform float renderWidth = 800.0;
uniform float renderHeight = 450.0;

// Outline controls
uniform vec4 outlineColor = vec4(0.0, 0.0, 0.0, 1.0);
uniform float outlineThickness = 1.0;   // in pixels
uniform float edgeThreshold = 0.25;     // higher = fewer edges
uniform float edgeSoftness = 0.10;      // smooth threshold band
uniform float outlineStrength = 1.0;    // 0..1 blend amount

float luminance(vec3 c)
{
    return dot(c, vec3(0.299, 0.587, 0.114));
}

void main()
{
    vec2 texel = vec2(1.0 / renderWidth, 1.0 / renderHeight) * outlineThickness;

    // 3x3 neighborhood luminance samples
    float tl = luminance(texture(texture0, fragTexCoord + vec2(-texel.x, -texel.y)).rgb);
    float tc = luminance(texture(texture0, fragTexCoord + vec2( 0.0,     -texel.y)).rgb);
    float tr = luminance(texture(texture0, fragTexCoord + vec2( texel.x, -texel.y)).rgb);

    float ml = luminance(texture(texture0, fragTexCoord + vec2(-texel.x,  0.0)).rgb);
    float mc = luminance(texture(texture0, fragTexCoord).rgb);
    float mr = luminance(texture(texture0, fragTexCoord + vec2( texel.x,  0.0)).rgb);

    float bl = luminance(texture(texture0, fragTexCoord + vec2(-texel.x,  texel.y)).rgb);
    float bc = luminance(texture(texture0, fragTexCoord + vec2( 0.0,      texel.y)).rgb);
    float br = luminance(texture(texture0, fragTexCoord + vec2( texel.x,  texel.y)).rgb);

    // Sobel kernels
    float gx = -tl + tr
             - 2.0 * ml + 2.0 * mr
             - bl + br;

    float gy = -tl - 2.0 * tc - tr
             + bl + 2.0 * bc + br;

    float edge = length(vec2(gx, gy));

    // Convert to mask
    float mask = smoothstep(edgeThreshold - edgeSoftness,
                            edgeThreshold + edgeSoftness,
                            edge);

    vec4 src = texture(texture0, fragTexCoord);

    // Blend original scene toward outline color on edges
    finalColor = mix(src, outlineColor, mask * outlineStrength);
}
