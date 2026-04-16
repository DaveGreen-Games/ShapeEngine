#version 330

in vec2 fragTexCoord;
out vec4 finalColor;
uniform sampler2D texture0;

// NOTE Render size values must be passed from code
uniform float renderWidth = 800;
uniform float renderHeight = 450;
uniform float blurStrength = 15.0;

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
    vec2 texelSize = vec2(1.0 / max(renderWidth, 1.0), 1.0 / max(renderHeight, 1.0));
    vec2 blurOffset = texelSize * max(blurStrength, 0.0);

    finalColor = sampleBlur9(fragTexCoord, blurOffset);
}