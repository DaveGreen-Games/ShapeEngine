#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
//in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
//uniform vec4 colDiffuse;


// Output fragment color
out vec4 finalColor;

// NOTE: Add here your custom variables
uniform vec2 curvatureAmount = vec2(6.0, 4.0);//smaller values = bigger curvature
uniform float renderWidth = 960;
uniform float renderHeight = 600;
uniform vec4 cornerColor = vec4(0.0, 0.0, 0.0, 1.0);
uniform float vignetteOpacity = 0.2;
//const float PI = 3.14159265359;

vec2 uvCurve(vec2 uv) {
    uv = uv * 2.0 - 1.0;
    vec2 offset = abs(uv.yx) / curvatureAmount;
    uv = uv + uv * offset * offset;
    uv = uv * 0.5 + 0.5;
    return uv;
}

void main()
{
    vec2 uv = uvCurve(fragTexCoord);
    vec4 texelColor = texture(texture0, uv);//*colDiffuse*fragColor;
    
    float vignette = uv.x * uv.y * (1.0 - uv.x) * (1.0 - uv.y);
    vignette = clamp(pow((renderWidth / 4.0) * vignette, vignetteOpacity), 0.0, 1.0);
    texelColor *= vignette;
    
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) {
        texelColor = cornerColor;
    }
    
    finalColor = texelColor;
}