using System.Numerics;
using Examples.Scenes.ExampleScenes.PhysicsExampleSource;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using ShapeEngine.Text;
using ShapeEngine.Lib.Drawing;

namespace Examples.Scenes.ExampleScenes;

public class PhysicsExample : ExampleScene
{
    private Rect sectorRect;
    private Rect insideSectorRect;
    private readonly ShapeCamera camera;
    private readonly CameraFollowerSingle follower;
    private const float SectorSize = 15000;
    private const int CollisionRows = 10;
    private const int CollisionCols = 10;
    
    private readonly float cellSize;
    
    private List<TextureSurface> starSurfaces = new();
    
    private TextFont gameFont = new(GAMELOOP.GetFont(FontIDs.JetBrainsLarge), 15f, Colors.Warm);

    private PhysicsExampleSource.Ship ship;
    private Sector sector;
    
    
    public PhysicsExample()
    {
        Title = "Physics!";

        sectorRect = new(new Vector2(0f), new Size(SectorSize, SectorSize) , new AnchorPoint(0.5f));
        insideSectorRect= sectorRect.ApplyMargins(0.02f, 0.02f, 0.02f, 0.02f);

        InitCollisionHandler(sectorRect, CollisionRows, CollisionCols);
        cellSize = SectorSize / CollisionRows;
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
                CircleDrawing.DrawCircle(pos, size, color);
            }
            t.EndDraw();
            
            starSurfaces.Add(t);
        }

        ship = new(50, Colors.PcWarm);

        
        sector = new Sector(insideSectorRect);
        if (CollisionHandler != null)
        {
            CollisionHandler.Add(sector);
        }
        
    }
    
    protected override void OnActivate(Scene oldScene)
    {
        FontDimensions.FontSizeRange = new(10, 500);
        
        Colors.OnColorPaletteChanged += OnColorPaletteChanged;
        
        GAMELOOP.Camera = camera;
        camera.SetZoom(0.65f);
        ship.Spawn(sectorRect.Center, Rng.Instance.RandAngleDeg());
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
        //TODO: make camera zoom dynamic based on ship speed (would also disable zoom from input!)
        //TODO: make camera follow the ship faster the faster the ship gets ?
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
            var targetRect = new Rect(target, surface.Rect.Size * 2, AnchorPoint.Center);
            surface.Draw(sourceRect, targetRect, ColorRgba.White);
        }
        
        ship.DrawGame(game);
        
        var universeLineInfo = new LineDrawingInfo(24f, Colors.Warm, LineCapType.Extended, 0);
        var stripedLineInfo = new LineDrawingInfo(32f, Colors.Warm.SetAlpha(200), LineCapType.None, 0);

        var topRect = sectorRect.ApplyMargins(0f, 0f, 0f, 0.98f);
        var bottomRect = sectorRect.ApplyMargins(0f, 0f, 0.98f, 0);
        var leftRect = sectorRect.ApplyMargins(0f, 0.98f, 0.02f, 0.02f);
        var rightRect = sectorRect.ApplyMargins(0.98f, 0f, 0.02f, 0.02f);
        
        int splits = (int)(sectorRect.Size.Max() / 2000);
        if(splits % 2 == 0) splits--; //make sure splits are always odd
        var topRectSplit = topRect.SplitH(splits);
        var bottomRectSplit = bottomRect.SplitH(splits);
        var leftRectSplit = leftRect.SplitV(splits);
        var rightRectSplit = rightRect.SplitV(splits);

        var rotationRect = topRectSplit[0];

        leftRect.DrawStriped(250f, 25f + 90f, stripedLineInfo);
        leftRect.RightSegment.Draw(universeLineInfo);
        
        rightRect.DrawStriped(250f, 25f + 90f, stripedLineInfo);
        rightRect.LeftSegment.Draw(universeLineInfo);
        
        topRect.DrawStriped(250f, 25f, stripedLineInfo );
        topRect.BottomSegment.Draw(universeLineInfo);
        
        bottomRect.DrawStriped(250f, 25f, stripedLineInfo);
        bottomRect.TopSegment.Draw(universeLineInfo);
        
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
        
        sectorRect.DrawLines(universeLineInfo);
        
        
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