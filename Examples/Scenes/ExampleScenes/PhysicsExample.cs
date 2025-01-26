using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Lib;
using ShapeEngine.Random;
using ShapeEngine.Screen;

namespace Examples.Scenes.ExampleScenes;

public class PhysicsExample : ExampleScene
{
    private Rect universe;
    private readonly ShapeCamera camera;
    private readonly CameraFollowerSingle follower;
    private const float UniverseSize = 15000;
    private const int CollisionRows = 25;
    private const int CollisionCols = 25;
    
    private readonly float cellSize;
    
    private List<TextureSurface> starSurfaces = new();
    
    public PhysicsExample()
    {
        Title = "Physics!";

        universe = new(new Vector2(0f), new Size(UniverseSize, UniverseSize) , new AnchorPoint(0.5f));

        InitCollisionHandler(universe, CollisionRows, CollisionCols);
        cellSize = UniverseSize / CollisionRows;
        camera = new();
        follower = new(0, 300, 500);
        camera.Follower = follower;

        const int textureCount = 3;
        const int starCount = 1000;
        for (int i = 0; i < textureCount; i++)
        {
            var f = i / (float)(textureCount - 1);
            var t = new TextureSurface(2048, 2048);
            t.SetTextureFilter(TextureFilter.Bilinear);
            var sizeMin = ShapeMath.LerpFloat(1, 1.5f, f);
            var sizeMax = ShapeMath.LerpFloat(1.5f, 2, f);
            var sizeRange = new ValueRange(sizeMin, sizeMax);
            var color = ColorRgba.Lerp(Colors.Dark, Colors.Medium, f);
            t.BeginDraw(ColorRgba.Clear);
            for (int j = 0; j < starCount; j++)
            {
                var x = Rng.Instance.RandF(0f, 2048);
                var y = Rng.Instance.RandF(0f, 2048);
                var pos = new Vector2(x, y);
                var size = sizeRange.Rand();
                ShapeDrawing.DrawCircle(pos, size, color);
            }
            t.EndDraw();
            
            starSurfaces.Add(t);
        }
    }
    
    protected override void OnActivate(Scene oldScene)
    {
        Colors.OnColorPaletteChanged += OnColorPaletteChanged;
        
        GAMELOOP.Camera = camera;
        camera.SetZoom(1f);
        // follower.SetTarget(ship);
    }
    protected override void OnDeactivate()
    {
        Colors.OnColorPaletteChanged -= OnColorPaletteChanged;
        
        GAMELOOP.ResetCamera();
    }
    private void OnColorPaletteChanged()
    {
        
    }

    public override void Reset()
    {
        
    }
    

    
    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        
        
    }


    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
    {
        
    }

    

    protected override void OnDrawGameExample(ScreenInfo game)
    {
        foreach (var surface in starSurfaces)
        {
            surface.Draw(surface.Rect, new Rect(Vector2.Zero, surface.Rect.Size * 2f, AnchorPoint.Center), ColorRgba.White);
        }

    }

    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        // DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));
        //
        // var objectCountText = $"Object Count: {SpawnArea?.Count ?? 0}";
        //
        // textFont.FontSpacing = 1f;
        // textFont.ColorRgba = Colors.Warm;
        // textFont.DrawTextWrapNone(objectCountText, GAMELOOP.UIRects.GetRect("bottom right"), new AnchorPoint(0.98f, 0.98f));
    }

    private void DrawInputDescription(Rect rect)
    {
        // var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
        //
        // string addText = iaAdd.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
        // string toggleConvexHullText = iaToggleConvexHull.GetInputTypeDescription(curInputDeviceAll, true, 1, false, false);
        //
        // var text = $"Add {addText} | Convex Hull [{showConvexHull}] {toggleConvexHullText}";
        //
        // textFont.FontSpacing = 1f;
        // textFont.ColorRgba = Colors.Light;
        // textFont.DrawTextWrapNone(text, rect, new(0.5f));
    }
        
}