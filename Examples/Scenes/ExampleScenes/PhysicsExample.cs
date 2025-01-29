using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Lib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using ShapeEngine.Text;

namespace Examples.Scenes.ExampleScenes;

public class PhysicsExample : ExampleScene
{
    private Rect universe;
    private readonly ShapeCamera camera;
    private readonly CameraFollowerSingle follower;
    private const float UniverseSize = 15000;
    private const int CollisionRows = 10;
    private const int CollisionCols = 10;
    
    private readonly float cellSize;
    
    private List<TextureSurface> starSurfaces = new();
    
    private TextFont gameFont = new(GAMELOOP.GetFont(FontIDs.JetBrainsLarge), 15f, Colors.Warm);

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

        var insideRect = universe.ApplyMargins(0.02f, 0.02f, 0.02f, 0.02f);
    }
    
    protected override void OnActivate(Scene oldScene)
    {
        FontDimensions.FontSizeRange = new(10, 500);
        
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
        FontDimensions.FontSizeRange = FontDimensions.FontSizeRangeDefault;
        
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
        
        var universeLineInfo = new LineDrawingInfo(24f, Colors.Warm, LineCapType.Capped, 12);
        var stripedLineInfo = new LineDrawingInfo(32f, Colors.Warm.SetAlpha(200));

        var topRect = universe.ApplyMargins(0f, 0f, 0f, 0.98f);
        var bottomRect = universe.ApplyMargins(0f, 0f, 0.98f, 0);
        var leftRect = universe.ApplyMargins(0f, 0.98f, 0f, 0);
        var rightRect = universe.ApplyMargins(0.98f, 0f, 0f, 0);
        var topRectSplit = topRect.SplitH(9);
        var bottomRectSplit = bottomRect.SplitH(9);
        var leftRectSplit = leftRect.SplitV(9);
        var rightRectSplit = rightRect.SplitV(9);

        var rotationRect = topRectSplit[0];

        topRect.DrawStriped(250f, 25f, stripedLineInfo );
        topRect.BottomSegment.Draw(universeLineInfo);
        
        bottomRect.DrawStriped(250f, 25f, stripedLineInfo);
        bottomRect.TopSegment.Draw(universeLineInfo);
        
        leftRect.DrawStriped(250f, 25f + 90f, stripedLineInfo);
        leftRect.RightSegment.Draw(universeLineInfo);
        
        rightRect.DrawStriped(250f, 25f + 90f, stripedLineInfo);
        rightRect.LeftSegment.Draw(universeLineInfo);
        
        for (int i = 1; i < topRectSplit.Count - 1; i++)
        {
            if(i % 2 == 0) continue;
            var rect = topRectSplit[i];
            gameFont.Draw("SECTOR END", rect, 0f, AnchorPoint.Center);
        }
        for (int i = 1; i < bottomRectSplit.Count - 1; i++)
        {
            if(i % 2 == 0) continue;
            var rect = bottomRectSplit[i];
            gameFont.Draw("SECTOR END", rect, 180f, AnchorPoint.Center);
        }
        
        for (int i = 1; i < leftRectSplit.Count - 1; i++)
        {
            if(i % 2 == 0) continue;
            var rect = leftRectSplit[i];
            var movedRotationRect = rotationRect.SetPosition(rect.Center, AnchorPoint.Center);
            gameFont.Draw("SECTOR END", movedRotationRect, -90, AnchorPoint.Center);
        }
        
        for (int i = 1; i < rightRectSplit.Count - 1; i++)
        {
            if(i % 2 == 0) continue;
            var rect = rightRectSplit[i];
            var movedRotationRect = rotationRect.SetPosition(rect.Center, AnchorPoint.Center);
            gameFont.Draw("SECTOR END", movedRotationRect, 90, AnchorPoint.Center);
        }
        
        universe.DrawLines(universeLineInfo);
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