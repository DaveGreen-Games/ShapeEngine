using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Screen;

internal sealed class ScreenTexture
{
    public bool Loaded { get; private set; } = false;
    public RenderTexture2D RenderTexture { get; private set; } = new();
    public int Width { get; private set; } = 0;
    public int Height { get; private set; } = 0;

    public ScreenTexture(){}

    public void Load(Dimensions dimensions)
    {
        if (Loaded) return;
        Loaded = true;
        SetTexture(dimensions);
    }
    public void Unload()
    {
        if (!Loaded) return;
        Loaded = false;
        Raylib.UnloadRenderTexture(RenderTexture);
    }
    public void UpdateDimensions(Dimensions dimensions)
    {
        if (!Loaded) return;

        if (Width == dimensions.Width && Height == dimensions.Height) return;
        
        Raylib.UnloadRenderTexture(RenderTexture);
        SetTexture(dimensions);
    }
    public void Draw()
    {
        var destRec = new Rectangle
        {
            X = Width * 0.5f,
            Y = Height * 0.5f,
            Width = Width,
            Height = Height
        };
        Vector2 origin = new()
        {
            X = Width * 0.5f,
            Y = Height * 0.5f
        };
        
        var sourceRec = new Rectangle(0, 0, Width, -Height);
        
        Raylib.DrawTexturePro(RenderTexture.Texture, sourceRec, destRec, origin, 0f, new ColorRgba(System.Drawing.Color.White).ToRayColor());
    }
    
    private void SetTexture(Dimensions dimensions)
    {
        Width = dimensions.Width;
        Height = dimensions.Height;
        RenderTexture = Raylib.LoadRenderTexture(Width, Height);
    }
    
    //public void DrawTexture(int targetWidth, int targetHeight)
    //{
    //var destRec = new Rectangle
    //{
    //    x = targetWidth * 0.5f,
    //    y = targetHeight * 0.5f,
    //    width = targetWidth,
    //    height = targetHeight
    //};
    //Vector2 origin = new()
    //{
    //    X = targetWidth * 0.5f,
    //    Y = targetHeight * 0.5f
    //};
    //
    //
    //
    //var sourceRec = new Rectangle(0, 0, Width, -Height);
    //
    //DrawTexturePro(RenderTexture.texture, sourceRec, destRec, origin, 0f, WHITE);
    //
}