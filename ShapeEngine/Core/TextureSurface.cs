using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core;

/// <summary>
/// Represents a renderable texture surface that allows drawing operations onto an off-screen texture.
/// </summary>
/// <remarks>
/// The texture is created with a fixed size and cannot be resized after creation. 
/// You must manually unload the texture using <see cref="Unload"/> to free resources.
/// Use <see cref="BeginDraw()"/> and <see cref="EndDraw"/> to perform drawing operations on the texture.
/// </remarks>
public class TextureSurface(int width, int height)
{
    /// <summary>
    /// Gets the rectangle representing the bounds and size of the texture surface.
    /// </summary>
    public Rect Rect { get; private set; } = new(0, 0, width, height);
    private readonly RenderTexture2D renderTexture = Raylib.LoadRenderTexture(width, height);

    #region Drawing functions

    /// <summary>
    /// Draws the texture at the specified integer coordinates.
    /// </summary>
    /// <param name="x">The X coordinate on the screen.</param>
    /// <param name="y">The Y coordinate on the screen.</param>
    /// <param name="tint">The color tint to apply to the texture.</param>
    public void Draw(int x, int y, ColorRgba tint)
    {
        Raylib.DrawTexture(renderTexture.Texture, x, y, tint.ToRayColor());
    }

    /// <summary>
    /// Draws the texture at the specified position.
    /// </summary>
    /// <param name="position">The position on the screen.</param>
    /// <param name="tint">The color tint to apply to the texture.</param>
    public void Draw(Vector2 position, ColorRgba tint)
    {
        Raylib.DrawTextureV(renderTexture.Texture, position, tint.ToRayColor());
    }

    /// <summary>
    /// Draws the texture at the specified position using an anchor point.
    /// </summary>
    /// <param name="position">The position on the screen.</param>
    /// <param name="anchorPoint">The anchor point for positioning the texture.</param>
    /// <param name="tint">The color tint to apply to the texture.</param>
    public void Draw(Vector2 position, AnchorPoint anchorPoint, ColorRgba tint)
    {
        var targetRect = new Rect(position, Rect.Size, anchorPoint);
        Raylib.DrawTextureV(renderTexture.Texture, targetRect.TopLeft, tint.ToRayColor());
    }

    /// <summary>
    /// Draws the texture at the specified position with scaling.
    /// </summary>
    /// <param name="position">The position on the screen.</param>
    /// <param name="scale">The scale factor to apply to the texture.</param>
    /// <param name="tint">The color tint to apply to the texture.</param>
    public void Draw(Vector2 position, float scale, ColorRgba tint)
    {
        Raylib.DrawTextureEx(renderTexture.Texture, position, 0f, scale, tint.ToRayColor());
    }

    /// <summary>
    /// Draws the texture at the specified position using an anchor point and scaling.
    /// </summary>
    /// <param name="position">The position on the screen.</param>
    /// <param name="anchorPoint">The anchor point for positioning the texture.</param>
    /// <param name="scale">The scale factor to apply to the texture.</param>
    /// <param name="tint">The color tint to apply to the texture.</param>
    public void Draw(Vector2 position, AnchorPoint anchorPoint, float scale, ColorRgba tint)
    {
        var targetRect = new Rect(position, Rect.Size * scale, anchorPoint);
        Raylib.DrawTextureEx(renderTexture.Texture, targetRect.TopLeft, 0f, scale, tint.ToRayColor());
    }

    /// <summary>
    /// Draws the texture at the specified position with scaling, rotation, and origin.
    /// </summary>
    /// <param name="position">The position on the screen.</param>
    /// <param name="scale">The scale factor to apply to the texture.</param>
    /// <param name="rotationDeg">The rotation in degrees.</param>
    /// <param name="origin">The origin point for rotation and scaling.</param>
    /// <param name="tint">The color tint to apply to the texture.</param>
    public void Draw(Vector2 position, float scale, float rotationDeg, AnchorPoint origin, ColorRgba tint)
    {
        var size = Rect.Size * scale;
        var pivot = origin.ToVector2() * size;
        var targetRect = new Rect(position + pivot, size , origin);
        Raylib.DrawTexturePro(renderTexture.Texture, Rect.Rectangle, targetRect.Rectangle, pivot,  rotationDeg, tint.ToRayColor());
    }

    /// <summary>
    /// Draws a portion of the texture (source rectangle) at the specified position using an anchor point.
    /// </summary>
    /// <param name="source">The source rectangle within the texture to draw.</param>
    /// <param name="position">The position on the screen.</param>
    /// <param name="anchorPoint">The anchor point for positioning the texture.</param>
    /// <param name="tint">The color tint to apply to the texture.</param>
    public void Draw(Rect source, Vector2 position, AnchorPoint anchorPoint, ColorRgba tint)
    {
        var targetRect = new Rect(position, Rect.Size, anchorPoint);
        Raylib.DrawTextureRec(renderTexture.Texture, source.Rectangle, targetRect.TopLeft, tint.ToRayColor());
    }

    /// <summary>
    /// Draws the texture stretched or shrunk to fit the specified destination rectangle.
    /// </summary>
    /// <param name="destination">The destination rectangle on the screen.</param>
    /// <param name="tint">The color tint to apply to the texture.</param>
    public void Draw(Rect destination, ColorRgba tint)
    {
        Raylib.DrawTexturePro(renderTexture.Texture, Rect.Rectangle, destination.Rectangle, Vector2.Zero, 0f, tint.ToRayColor());
    }

    /// <summary>
    /// Draws a portion of the texture (source rectangle) to a destination rectangle.
    /// </summary>
    /// <param name="source">The source rectangle within the texture to draw.</param>
    /// <param name="destination">The destination rectangle on the screen.</param>
    /// <param name="tint">The color tint to apply to the texture.</param>
    public void Draw(Rect source, Rect destination, ColorRgba tint)
    {
        Raylib.DrawTexturePro(renderTexture.Texture, source.Rectangle, destination.Rectangle, Vector2.Zero, 0f, tint.ToRayColor());
    }

    /// <summary>
    /// Draws a portion of the texture (source rectangle) at the specified position with scaling, rotation, and origin.
    /// </summary>
    /// <param name="source">The source rectangle within the texture to draw.</param>
    /// <param name="position">The position on the screen.</param>
    /// <param name="scale">The scale factor to apply to the texture.</param>
    /// <param name="rotationDeg">The rotation in degrees.</param>
    /// <param name="origin">The origin point for rotation and scaling.</param>
    /// <param name="tint">The color tint to apply to the texture.</param>
    public void Draw(Rect source, Vector2 position, float scale, float rotationDeg, AnchorPoint origin, ColorRgba tint)
    {
        var size = Rect.Size * scale;
        var pivot = origin.ToVector2() * size;
        var targetRect = new Rect(position + pivot, size , origin);
        Raylib.DrawTexturePro(renderTexture.Texture, source.Rectangle, targetRect.Rectangle, pivot,  rotationDeg, tint.ToRayColor());
    }
    #endregion
    
    #region Public Functions

    /// <summary>
    /// Begins drawing to the texture surface.
    /// </summary>
    /// <remarks>
    /// All subsequent drawing operations will be rendered to this texture until <see cref="EndDraw"/> is called.
    /// </remarks>
    public void BeginDraw() => Raylib.BeginTextureMode(renderTexture);

    /// <summary>
    /// Begins drawing to the texture surface and clears it with the specified color.
    /// </summary>
    /// <param name="clearColor">The color to clear the texture with before drawing.</param>
    /// <remarks>
    /// All subsequent drawing operations will be rendered to this texture until <see cref="EndDraw"/> is called.
    /// </remarks>
    public void BeginDraw(ColorRgba clearColor)
    {
        Raylib.BeginTextureMode(renderTexture);
        Raylib.ClearBackground(clearColor.ToRayColor());
    }

    /// <summary>
    /// Sets the filtering mode for the texture.
    /// </summary>
    /// <param name="filter">The texture filtering mode to use.</param>
    /// <remarks>
    /// Filtering affects how the texture is sampled when scaled or transformed.
    /// </remarks>
    public void SetTextureFilter(TextureFilter filter) => Raylib.SetTextureFilter(renderTexture.Texture, filter);

    /// <summary>
    /// Ends drawing to the texture surface.
    /// </summary>
    /// <remarks>
    /// Call this after <see cref="BeginDraw()"/> to finalize drawing operations to the texture.
    /// </remarks>
    public void EndDraw() => Raylib.EndTextureMode();

    /// <summary>
    /// Unloads the render texture and frees associated resources.
    /// </summary>
    /// <remarks>
    /// Call this when the texture surface is no longer needed to avoid memory leaks.
    /// </remarks>
    public void Unload() => Raylib.UnloadRenderTexture(renderTexture);
    
    #endregion
}