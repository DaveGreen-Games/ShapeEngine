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
    private const float UniverseSize = 5000;
    private const int CollisionRows = 10;
    private const int CollisionCols = 10;
    
    private readonly float cellSize;
    
    private List<TextureSurface> starSurfaces = new();

    private PhysicsExampleSource.Ship ship;
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
            var sizeMin = ShapeMath.LerpFloat(1f, 1.5f, f);
            var sizeMax = ShapeMath.LerpFloat(1.5f, 3f, f);
            var sizeRange = new ValueRange(sizeMin, sizeMax);
            var color = ColorRgba.Lerp(ColorRgba.White, ColorRgba.White.SetAlpha(220), f);
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

        ship = new(50, Colors.PcWarm);
    }
    
    protected override void OnActivate(Scene oldScene)
    {
        Colors.OnColorPaletteChanged += OnColorPaletteChanged;
        
        GAMELOOP.Camera = camera;
        camera.SetZoom(1f);
        ship.Spawn(universe.Center, Rng.Instance.RandAngleDeg());
        follower.SetTarget(ship);
        follower.Speed = 1000;
        follower.BoundaryDis = new ValueRange(150, 300);
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
        
        ship.Update(time, game, gameUi, ui);
    }


    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
    {
        
    }

    

    protected override void OnDrawGameExample(ScreenInfo game)
    {
        var target = camera.BasePosition;
        var count = starSurfaces.Count;
        for (int i = 0; i < count; i++)
        {
            var surface = starSurfaces[i];
            var f = i / (float)(count - 1);
            var scaleFactor = i == 0 ? 0f : i == 1 ? 0.01f : 0.75f;
            var pos = target * scaleFactor;

            var sourceRect = new Rect(pos, surface.Rect.Size, AnchorPoint.Center);
            var targetRect = new Rect(target, surface.Rect.Size, AnchorPoint.Center);
            surface.Draw(sourceRect, targetRect, ColorRgba.White);
        }
        
        ship.DrawGame(game);
    }

    protected override void OnDrawGameUIExample(ScreenInfo gameUi)
    {
        ship.DrawGameUI(gameUi);
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