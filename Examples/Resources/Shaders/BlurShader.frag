#version 330

in vec2 fragTexCoord;
out vec4 finalColor;
uniform sampler2D texture0;

// NOTE Render size values must be passed from code
uniform float renderWidth = 800;
uniform float renderHeight = 450;

float offset[3] = float[](0.0, 1.3846153846, 3.2307692308);
float weight[3] = float[](0.2270270270, 0.3162162162, 0.0702702703);


void main()
{
    
    vec4 texelColor = texture(texture0, fragTexCoord)*weight[0];
    for (int i = 1; i < 3; i++)
    {
        texelColor += texture(texture0, fragTexCoord + vec2(offset[i])/renderWidth)*weight[i];
        texelColor += texture(texture0, fragTexCoord - vec2(offset[i])/renderWidth)*weight[i];
                
    }
    finalColor = texelColor;
}

//previous version
//void main()
//{
//    // Texel color fetching from texture sampler
//    vec3 texelColor = texture(texture0, fragTexCoord).rgb*weight[0];
//
//    for (int i = 1; i < 3; i++)
//    {
//        texelColor += texture(texture0, fragTexCoord + vec2(offset[i]*scale)/renderWidth, 0.0).rgb*weight[i];
//        texelColor += texture(texture0, fragTexCoord - vec2(offset[i]*scale)/renderWidth, 0.0).rgb*weight[i];
//    }
//
//    finalColor = vec4(texelColor, 1.0);
//}







//in vec2 fragTexCoord;
//out vec4 finalColor;
//uniform sampler2D texture0;
//
////uniform vec4 vColor = vec4(1, 1, 1, 1);
////uniform vec4 v_time = vec4(1, 1, 1, 1);
//uniform float renderWidth = 800;
//uniform float renderHeight = 450;
//
//
//const float RADIUS = 0.75;
//const float SOFTNESS = 0.6;
//const float blurSize = 1.0/1000.0;
//
//void main() {
//
//    vec4 texColor = vec4(0.0);
//    texColor += texture(texture0, fragTexCoord - 4.0*blurSize) * 0.05;
//    texColor += texture(texture0, fragTexCoord - 3.0*blurSize) * 0.09;
//    texColor += texture(texture0, fragTexCoord - 2.0*blurSize) * 0.12;
//    texColor += texture(texture0, fragTexCoord - blurSize) * 0.15;
//    texColor += texture(texture0, fragTexCoord) * 0.16;
//    texColor += texture(texture0, fragTexCoord + blurSize) * 0.15;
//    texColor += texture(texture0, fragTexCoord + 2.0*blurSize) * 0.12;
//    texColor += texture(texture0, fragTexCoord + 3.0*blurSize) * 0.09;
//    texColor += texture(texture0, fragTexCoord + 4.0*blurSize) * 0.05;
//
////    vec4 timedColor = (vColor + v_time);
//    
//    vec2 position = (fragTexCoord.xy / vec2(renderWidth, renderHeight).xy) - vec2(0.5);
//    float len = length(position);
//
//    float vignette = smoothstep(RADIUS, RADIUS-SOFTNESS, len);
//
//    texColor.rgb = mix(texColor.rgb, texColor.rgb * vignette, 0.5);
//
////    finalColor = vec4(texColor.rgb * timedColor.rgb, texColor.a);
//    finalColor = vec4(texColor.rgb, texColor.a);
//}

//// Fragment
//#version 420 core
//
//in vec2 fragTexCoord;                    // screen position <-1,+1>
//out vec4 finalColor;          // fragment output color
//uniform sampler2D texture0;          // texture to blur
//uniform float renderWidth = 800;
//uniform float renderHeight = 450;
////uniform float xs,ys;            // texture resolution
//float r = 10;                // blur radius
//void main()
//{
//    float x,y,xx,yy,rr=r*r,dx,dy,w,w0;
//    w0=0.3780/pow(r,1.975);
//    vec2 p;
//    vec4 col=vec4(0.0,0.0,0.0,0.0);
//    for (dx=1.0/renderWidth,x=-r,p.x=0.5+(fragTexCoord.x*0.5)+(x*dx);x<=r;x++,p.x+=dx)
//    { 
//        xx=x*x;
//        for (dy=1.0/renderHeight,y=-r,p.y=0.5+(fragTexCoord.y*0.5)+(y*dy);y<=r;y++,p.y+=dy)
//        { 
//            yy=y*y;
//            if (xx+yy<=rr)
//            {
//                w=w0*exp((-xx-yy)/(2.0*rr));
//                col+=texture(texture0,p)*w;
//            }
//        }
//    }
//    finalColor=col;
//
//}

//// Fragment
////#version 330
//in vec2 fragTexCoord;                    // screen position <-1,+1>
//out vec4 finalColor;          // fragment output color
//uniform sampler2D texture0;          // texture to blur
//uniform float renderWidth = 800;
//uniform float renderHeight = 450;
////uniform float xs,ys;            // texture resolution
//float r = 1;                // blur radius
//int axis = 0;
//void main()
//{
//    float x,y,rr=r*r,d,w,w0;
//    vec2 p=0.5*(vec2(1.0,1.0)+fragTexCoord);
//    vec4 col=vec4(0.0,0.0,0.0,0.0);
//    w0=0.5135/pow(r,0.96);
//    if (axis==0) for (d=1.0/renderWidth,x=-r,p.x+=x*d;x<=r;x++)
//    { 
//        p.x += d;
//        w=w0*exp((-x*x)/(2.0*rr)); col+=texture(texture0,p)*w; 
//    }
//    if (axis==1) for (d=1.0/renderHeight,y=-r,p.y+=y*d;y<=r;y++)
//    {
//        p.y += d;
//        w=w0*exp((-y*y)/(2.0*rr)); col+=texture(texture0,p)*w; 
//    }
//    finalColor=col;
//}


//void main()
//{
//    vec4 texelColor = texture(texture0, fragTexCoord / renderWidth)*weight[0];
//
//    for (int i = 1; i < 3; i++)
//    {
//        vec2 offset = vec2(0.0, offset[i]*scale);
//        texelColor += texture(texture0, (fragTexCoord + offset)/renderHeight)*weight[i];
//        texelColor += texture(texture0, (fragTexCoord - offset)/renderHeight)*weight[i];
//    }
//
//    finalColor = texelColor; //vec4(texelColor, 1.0);
//}

//Did not work!
//gaussian blur
//uniforms
//vec4 v_vColour;
//vec3 size;//width,height,radius
//
//const int Quality = 8;
//const int Directions = 16;
//const float Pi = 6.28318530718;//pi * 2
//
//void main()
//{
//    size = vec3(renderWidth, renderHeight, 500);
//    v_vColour = vec4(1, 0, 0, 1);
//    
//    vec2 radius = size.z/size.xy;
//    vec4 Color = texture(texture0, fragTexCoord);
//    for( float d=0.0;d<Pi;d+=Pi/float(Directions) )
//    {
//        for( float i=1.0/float(Quality);i<=1.0;i+=1.0/float(Quality) )
//        {
//            Color += texture(texture0, fragTexCoord+vec2(cos(d),sin(d))*radius*i);
//        }
//    }
//    Color /= float(Quality)*float(Directions)+1.0;
//    finalColor =  Color;// *  v_vColour;
//    
//}

//Did NOT Work
//simple radial/directional blur
//const int SampleCount = 64; // use a multiple of 2 here
//float Intensity = 0.1;
//vec2 mousePos = vec2(0, 0);
//
//vec4 directionalBlur(in vec2 uv, in vec2 direction, in float intensity)
//{
//    vec4 color = vec4(0.0);
//
//    for (int i=1; i<=SampleCount; i++)
//    {
//        color += texture(texture0,uv+float(i)*intensity/float(SampleCount)*direction);
//    }
//
//    return color/float(SampleCount);
//}
//
//void main()
//{
//    vec2 resolution = vec2(renderWidth, renderHeight);
//    vec2 uv = fragTexCoord.xy / resolution.xy;
//    vec2 middle = resolution.xy * 0.5;
//
//    mousePos = middle;
//    
//    vec2 direction = mousePos - fragTexCoord.xy;    // Take blur direction from mouse coordinate
//    float dist = length(direction) / length(middle);
//    vec4 color = directionalBlur(uv, normalize(direction), dist * Intensity);
//
//    finalColor = color;
//}





//gaussian blur
//varying vec2 v_vTexcoord;
//varying vec4 v_vColour;
//uniform vec3 size;//width,height,radius
//
//const int Quality = 8;
//const int Directions = 16;
//const float Pi = 6.28318530718;//pi * 2
//
//void main()
//{
//    vec2 radius = size.z/size.xy;
//    vec4 Color = texture2D( gm_BaseTexture, v_vTexcoord);
//    for( float d=0.0;d<Pi;d+=Pi/float(Directions) )
//    {
//        for( float i=1.0/float(Quality);i<=1.0;i+=1.0/float(Quality) )
//        {
//            Color += texture2D( gm_BaseTexture, v_vTexcoord+vec2(cos(d),sin(d))*radius*i);
//        }
//    }
//    Color /= float(Quality)*float(Directions)+1.0;
//    gl_FragColor =  Color *  v_vColour;
//}

//Simple Gaussian blur
//uniform sampler2D image;
//
//out vec4 FragmentColor;
//
//uniform float offset[3] = float[](0.0, 1.3846153846, 3.2307692308);
//uniform float weight[3] = float[](0.2270270270, 0.3162162162, 0.0702702703);
//
//void main(void) {
//    FragmentColor = texture2D(image, vec2(gl_FragCoord) / 1024.0) * weight[0];
//    for (int i=1; i<3; i++) {
//        FragmentColor +=
//        texture2D(image, (vec2(gl_FragCoord) + vec2(0.0, offset[i])) / 1024.0)
//        * weight[i];
//        FragmentColor +=
//        texture2D(image, (vec2(gl_FragCoord) - vec2(0.0, offset[i])) / 1024.0)
//        * weight[i];
//    }
//}

//simple radial/directional blur
//const int SampleCount = 64; // use a multiple of 2 here
//float Intensity = 0.1;
//
//vec4 directionalBlur(in vec2 uv, in vec2 direction, in float intensity)
//{
//    vec4 color = vec4(0.0);
//
//    for (int i=1; i<=SampleCount; i++)
//    {
//        color += texture(iChannel0,uv+float(i)*intensity/float(SampleCount)*direction);
//    }
//
//    return color/float(SampleCount);
//}
//
//void mainImage( out vec4 fragColor, in vec2 fragCoord )
//{
//    vec2 uv = fragCoord.xy / iResolution.xy;
//    vec2 middle = iResolution.xy * 0.5;
//
//    vec2 direction = iMouse.xy - fragCoord.xy;    // Take blur direction from mouse coordinate
//    float dist = length(direction) / length(middle);
//    vec4 color = directionalBlur(uv, normalize(direction), dist * Intensity);
//
//    fragColor = color;
//}