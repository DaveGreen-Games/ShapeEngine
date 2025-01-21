using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.UI;

//TODO: class contains image (constructor only creates image and saves it) -> ImageCreator
// then there a multiple different drawing functions that draw to the image
// there is also a function to clear the image
// and there is a function that returns a new class that contains the texture2d -> TextureBar
// this new class has the functions to draw the texture with f as a bar -> gets a rect for cutting out of the texture to make scaling to different screen sizes possible
// this way more complex things / multiple things can be drawn to the image
public class StripedTextureBar
{
    private readonly Texture2D texture;

    public StripedTextureBar(Dimensions textureSize, float lineSpacing, float lineAngleDeg, LineDrawingInfo stripedInfo, ColorRgba backgroundColor)
    {
        var image = Raylib.GenImageColor(textureSize.Width, textureSize.Height, backgroundColor.ToRayColor());
        
        var rect = new Rect(0, 0, textureSize.Width, textureSize.Height);
        if (lineSpacing <= 0) return;
        float maxDimension = (rect.TopLeft - rect.BottomRight).Length();

        if (lineSpacing > maxDimension) return;
        
        var center = rect.Center;
        var dir = ShapeVec.VecFromAngleDeg(lineAngleDeg);
        var rayDir = dir.GetPerpendicularRight();
        var start = center - dir * maxDimension * 0.5f;
        int steps = (int)(maxDimension / lineSpacing);
        
        var cur = start + dir * lineSpacing;
        cur -= rayDir * maxDimension;
        for (int i = 0; i < steps; i++)
        {
            unsafe
            {
                var end = cur + rayDir * maxDimension * 2;
                Raylib.ImageDrawLineEx(&image, cur, end, (int)stripedInfo.Thickness, stripedInfo.Color.ToRayColor());
                cur += dir * lineSpacing;
            }
        }
        
        texture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);
    }

    public void Unload()
    {
        Raylib.UnloadTexture(texture);
    }
}