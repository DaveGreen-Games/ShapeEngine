using System.Numerics;
using Examples.Scenes.ExampleScenes.PhysicsExampleSource;
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
    
    public CurveFloat testCurve = new(10)
    {
        (0f,     0),
        (0.05f,  -300),
        (0.15f,  0),
        (0.3f,   150),
        (0.49f,  50),
        (0.51f,  -50),
        (0.7f,   -250),
        (0.85f,  50),
        (0.95f,  100),
        (1f,     0),
    };
    
    public CurveVector2 testCurve2 = new(10)
    {
        (0f,     new Vector2(-1000, 0) ),
        (0.05f,  new Vector2(-500, -250) ),
        (0.15f,  new Vector2(-300, -150) ),
        (0.3f,   new Vector2(-100, 50) ),
        (0.49f,  new Vector2(50, 100) ),
        (0.51f,  new Vector2(250, 200) ),
        (0.7f,   new Vector2(500, 50) ),
        (0.85f,  new Vector2(800, 150) ),
        (0.95f,  new Vector2(900, 300) ),
        (1f,     new Vector2(1000, 0) ),
    };
    
    public CurveColor testCurve3 = new(10)
    {
        (0f,     Colors.Warm),
        (0.05f,  Colors.Special),
        (0.15f,  Colors.Light),
        (0.3f,   Colors.Warm),
        (0.49f,  Colors.Cold),
        (0.51f,  Colors.Highlight),
        (0.7f,   Colors.Special),
        (0.85f,  Colors.Cold),
        (0.95f,  Colors.Special2),
        (1f,     Colors.Warm),
    };
    
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
                ShapeDrawing.DrawCircle(pos, size, color);
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
        // insideSectorRect.DrawLines(universeLineInfo);

        var mousePosX = game.MousePos.X + game.Area.Width * 0.5f;
        float curveTime = (mousePosX - 200) / (game.Area.Width - 400);
        curveTime = ShapeMath.Clamp(curveTime, 0f, 1f);
        DrawCurveFloat(curveTime, -325, Colors.Warm);
        DrawCurveVector2(curveTime, 75,Colors.Cold); 
        DrawCurveColor(curveTime, 625);
        
    }

    private void DrawCurveFloat(float curveTime,  float yOffset,  ColorRgba curveColor)
    {
        var controlPoint = new Vector2(-1000 + curveTime * 2000, yOffset);
        controlPoint.Draw(20, Colors.Warm, 32);
        ShapeDrawing.DrawSegment(new Vector2(-1000, yOffset), new Vector2(1000, yOffset), 2f, Colors.Light, LineCapType.Capped, 12);
        if (testCurve.Sample(curveTime, out var value))
        {
            var p = new Vector2(-1000 + curveTime * 2000, yOffset + value);
            p.Draw(20, Colors.Light, 32);
        }

        var curIndex = testCurve.GetIndex(curveTime);
        for (int i = 0; i < testCurve.Count; i++)
        {
            var k = testCurve.Keys[i];
            var v = testCurve.Values[i];
            var kPoint = new Vector2(-1000 + k * 2000, yOffset);
            var kPointStart = new Vector2(kPoint.X, yOffset + 20);
            var kPointEnd = new Vector2(kPoint.X, yOffset - 20);
            ShapeDrawing.DrawSegment(kPointStart, kPointEnd, 2, Colors.Light, LineCapType.Capped, 12);
            
            var point = new Vector2(-1000 + k * 2000, yOffset + v);
            if (i < testCurve.Count - 1)
            {
                var nextK = testCurve.Keys[i + 1];
                var nextV = testCurve.Values[i + 1];
                var next = new Vector2(-1000 + nextK * 2000, yOffset + nextV);
                ShapeDrawing.DrawSegment(point, next, 4, curveColor, LineCapType.CappedExtended, 12);
            }

            if (i == curIndex)
            {
                point.Draw(30, curveColor, 32);
            }
            else
            {
                point.Draw(10, curveColor, 32);
            }
            
        }
    }
    private void DrawCurveVector2(float curveTime, float yOffset,  ColorRgba curveColor)
    {
        
        var controlPoint = new Vector2(-1000 + curveTime * 2000, yOffset);
        controlPoint.Draw(20, Colors.Warm, 32);
        
        ShapeDrawing.DrawSegment(new Vector2(-1000, yOffset), new Vector2(1000, yOffset), 2f, Colors.Light, LineCapType.Capped, 12);
        if (testCurve2.Sample(curveTime, out var value))
        {
            value += new Vector2(0, yOffset);
            value.Draw(20, Colors.Light, 32);
        }

        var curIndex = testCurve2.GetIndex(curveTime);
        for (int i = 0; i < testCurve2.Count; i++)
        {
            var v = testCurve2.Values[i];
            v += new Vector2(0, yOffset);
            var k = testCurve2.Keys[i];
            var kPoint = new Vector2(-1000 + k * 2000, yOffset);
            var kPointStart = new Vector2(kPoint.X, yOffset + 20);
            var kPointEnd = new Vector2(kPoint.X, yOffset - 20);
            ShapeDrawing.DrawSegment(kPointStart, kPointEnd, 2, Colors.Light, LineCapType.Capped, 12);
            if (i < testCurve2.Count - 1)
            {
                var nextV = testCurve2.Values[i + 1];
                nextV += new Vector2(0, yOffset);
                ShapeDrawing.DrawSegment(v, nextV, 4, curveColor, LineCapType.CappedExtended, 12);
            }

            if (i == curIndex)
            {
                v.Draw(30, curveColor, 32);
            }
            else
            {
                v.Draw(10, curveColor, 32);
            }
            
        }
    }
    private void DrawCurveColor(float curveTime, float yOffset)
    {
        float rectHeight = 250;
        float startX = -1000;
        float totolWidth = 2000;
        
        var curIndex = testCurve3.GetIndex(curveTime);
        for (int i = 0; i < testCurve3.Count; i++)
        {
            var k = testCurve3.Keys[i];
            var v = testCurve3.Values[i];
            var cur = i == curIndex;
            if (i == testCurve3.Count - 1)
            {
                var w = totolWidth - k * totolWidth;
                var x = startX + k * totolWidth;
                var tl = new Vector2(x, yOffset - rectHeight / 2);
                var br = new Vector2(x + w,  yOffset + rectHeight / 2);
                var r = new Rect(tl, br);
                r.Draw(v);
                if(cur)r.DrawLines(4f, Colors.Dark);
            }
            else
            {
                var k2 = testCurve3.Keys[i + 1];
                var curW = k2 * totolWidth - k * totolWidth;
                var curX = startX + k * totolWidth;
                var curTl = new Vector2(curX, yOffset - rectHeight / 2);
                var curBr = new Vector2(curX + curW,  yOffset +  rectHeight / 2);
                var curR = new Rect(curTl, curBr);
                curR.Draw(v);
                if(cur)curR.DrawLines(4f, Colors.Dark);
            }
            
            
        }

        var mainRect = new Rect(new Vector2(-500, yOffset - rectHeight), new Vector2(500, yOffset - rectHeight * 0.6f));
        
        var controlPoint = new Vector2(-1000 + curveTime * 2000, yOffset);
        controlPoint.Draw(20, Colors.Dark, 32);
        
        if (testCurve3.Sample(curveTime, out var value))
        {
            mainRect.Draw(value);
        }
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