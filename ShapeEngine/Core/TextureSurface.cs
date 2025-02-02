using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core;

/// <summary>
/// Allows you to create a render texture and draw to its surface.
/// The texture can not be resized after creation and has to be manually unloaded.
/// Multiple functions allow you to draw to the texture (or part of it) to the screen.
/// UseBegin/EndDraw to draw to the texture with any drawing functions.
/// </summary>
/// <param name="width">The width of the texture. Must be bigger than 0.</param>
/// <param name="height">The height of the texture. Must be bigger than 0.</param>
public class TextureSurface(int width, int height)
{
    public Rect Rect { get; private set; } = new(0, 0, width, height);
    private readonly RenderTexture2D renderTexture = Raylib.LoadRenderTexture(width, height);

    #region Drawing functions
    public void Draw(int x, int y, ColorRgba tint)
    {
        Raylib.DrawTexture(renderTexture.Texture, x, y, tint.ToRayColor());
    }
    public void Draw(Vector2 position, ColorRgba tint)
    {
        Raylib.DrawTextureV(renderTexture.Texture, position, tint.ToRayColor());
    }
    public void Draw(Vector2 position, AnchorPoint anchorPoint, ColorRgba tint)
    {
        var targetRect = new Rect(position, Rect.Size, anchorPoint);
        Raylib.DrawTextureV(renderTexture.Texture, targetRect.TopLeft, tint.ToRayColor());
    }
    public void Draw(Vector2 position, float scale, ColorRgba tint)
    {
        Raylib.DrawTextureEx(renderTexture.Texture, position, 0f, scale, tint.ToRayColor());
    }
    public void Draw(Vector2 position, AnchorPoint anchorPoint, float scale, ColorRgba tint)
    {
        var targetRect = new Rect(position, Rect.Size * scale, anchorPoint);
        Raylib.DrawTextureEx(renderTexture.Texture, targetRect.TopLeft, 0f, scale, tint.ToRayColor());
    }
    public void Draw(Vector2 position, float scale, float rotationDeg, AnchorPoint origin, ColorRgba tint)
    {
        var size = Rect.Size * scale;
        var pivot = origin.ToVector2() * size;
        var targetRect = new Rect(position + pivot, size , origin);
        // pivot.Draw(5f, new ColorRgba(System.Drawing.Color.Red));
        // position.Draw(5f, new ColorRgba(System.Drawing.Color.Aquamarine));
        // targetRect.DrawLines(5f, new ColorRgba(System.Drawing.Color.Crimson));
        // targetRect = targetRect.ChangePosition(pivot);
        Raylib.DrawTexturePro(renderTexture.Texture, Rect.Rectangle, targetRect.Rectangle, pivot,  rotationDeg, tint.ToRayColor());
    }
    public void Draw(Rect source, Vector2 position, AnchorPoint anchorPoint, ColorRgba tint)
    {
        var targetRect = new Rect(position, Rect.Size, anchorPoint);
        Raylib.DrawTextureRec(renderTexture.Texture, source.Rectangle, targetRect.TopLeft, tint.ToRayColor());
    }
    public void Draw(Rect destination, ColorRgba tint)
    {
        Raylib.DrawTexturePro(renderTexture.Texture, Rect.Rectangle, destination.Rectangle, Vector2.Zero, 0f, tint.ToRayColor());
    }
    public void Draw(Rect source, Rect destination, ColorRgba tint)
    {
        Raylib.DrawTexturePro(renderTexture.Texture, source.Rectangle, destination.Rectangle, Vector2.Zero, 0f, tint.ToRayColor());
    }
    public void Draw(Rect source, Vector2 position, float scale, float rotationDeg, AnchorPoint origin, ColorRgba tint)
    {
        var size = Rect.Size * scale;
        var pivot = origin.ToVector2() * size;
        var targetRect = new Rect(position + pivot, size , origin);
        Raylib.DrawTexturePro(renderTexture.Texture, source.Rectangle, targetRect.Rectangle, pivot,  rotationDeg, tint.ToRayColor());
    }
    // public void Draw(Rect source, Rect destination, float rotationDeg, AnchorPoint origin, ColorRgba tint)
    // {
    //     Draw(source, destination.GetPoint(origin), 1f, rotationDeg, origin, tint);
    // }
    #endregion
    
    #region Public Functions
    
    public void BeginDraw() => Raylib.BeginTextureMode(renderTexture);
    public void BeginDraw(ColorRgba clearColor)
    {
        Raylib.BeginTextureMode(renderTexture);
        Raylib.ClearBackground(clearColor.ToRayColor());
    }
    public void SetTextureFilter(TextureFilter filter) => Raylib.SetTextureFilter(renderTexture.Texture, filter);
    public void EndDraw() => Raylib.EndTextureMode();
    public void Unload() => Raylib.UnloadRenderTexture(renderTexture);
    
    #endregion
}

//
// public void Draw(Vector2 position,  float rotation, float scale, ColorRgba tint)
// {
//     Raylib.DrawTextureEx(renderTexture.Texture, position, rotation, scale, tint.ToRayColor());
// }
// public void Draw(Vector2 position, AnchorPoint anchorPoint, float rotationDeg, float scale, ColorRgba tint)
// {
//     var targetRect = new Rect(position, Rect.Size * scale, anchorPoint);
//     Raylib.DrawTextureEx(renderTexture.Texture, targetRect.TopLeft, rotationDeg, scale, tint.ToRayColor());
// }
//
//
// public void Draw(Rect destination, float rotationDeg, ColorRgba tint)
// {
//     Raylib.DrawTexturePro(renderTexture.Texture, Rect.Rectangle, destination.Rectangle, Vector2.Zero, rotationDeg, tint.ToRayColor());
// }
//
// public void Draw(Rect source, Rect destination, float rotationDeg, ColorRgba tint)
// {
//     Raylib.DrawTexturePro(renderTexture.Texture, source.Rectangle, destination.Rectangle, Vector2.Zero, rotationDeg, tint.ToRayColor());
// }
