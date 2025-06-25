using System.Numerics;
using Examples.Scenes.ExampleScenes.PhysicsExampleSource;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.Rect;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.Input;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using ShapeEngine.Text;

namespace Examples.Scenes.ExampleScenes;

public class CurveDataExample : ExampleScene
{
    
    private TextFont gameFont = new(GAMELOOP.GetFont(FontIDs.JetBrainsLarge), 15f, Colors.Warm);
    private readonly ShapeCamera camera;
    
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
        (1f,     0)
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
    
    public CurveDataExample()
    {
        Title = "Curve Data Example";
        camera = new();
        
    }
    protected override void OnActivate(Scene oldScene)
    {
        GAMELOOP.Camera = camera;
        camera.SetZoom(0.5f);
        FontDimensions.FontSizeRange = new(10, 500);
    }
    protected override void OnDeactivate()
    {
        GAMELOOP.ResetCamera();
        FontDimensions.FontSizeRange = FontDimensions.FontSizeRangeDefault;
    }
    
    
    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        
    }
    
    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
    {
        
    }
    
    protected override void OnDrawGameExample(ScreenInfo game)
    {
        var mousePosX = game.MousePos.X + game.Area.Width * 0.5f;
        float curveTime = (mousePosX - 200) / (game.Area.Width - 400);
        curveTime = ShapeMath.Clamp(curveTime, 0f, 1f);
        DrawCurveFloat(curveTime, -325, Colors.Warm);
        DrawCurveVector2(curveTime, 75,Colors.Cold); 
        DrawCurveColor(curveTime, 625);
    }
    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        
    }

    private void DrawCurveFloat(float curveTime,  float yOffset,  ColorRgba curveColor)
    {
        var controlPoint = new Vector2(-1000 + curveTime * 2000, yOffset);
        controlPoint.Draw(20, Colors.Warm, 32);
        SegmentDrawing.DrawSegment(new Vector2(-1000, yOffset), new Vector2(1000, yOffset), 2f, Colors.Light, LineCapType.Capped, 12);
        if (testCurve.Sample(curveTime, out var value))
        {
            var p = new Vector2(-1000 + curveTime * 2000, yOffset + value);
            p.Draw(20, Colors.Light, 32);
            var resultRect = new Rect(new Vector2(1100, yOffset), new Size(650, 250), AnchorPoint.Left);
            var resultRectSplit = resultRect.SplitV(0.35f);
            gameFont.Draw($"Time: {curveTime:F2}", resultRectSplit.top, 0f, AnchorPoint.Center);
            gameFont.Draw($"Value: {value:F2}", resultRectSplit.bottom, 0f, AnchorPoint.Center);
        }

        

        var curIndex = testCurve.GetIndex(curveTime);
        for (int i = 0; i < testCurve.Count; i++)
        {
            var k = testCurve.Keys[i];
            var v = testCurve.Values[i];
            var kPoint = new Vector2(-1000 + k * 2000, yOffset);
            var kPointStart = new Vector2(kPoint.X, yOffset + 20);
            var kPointEnd = new Vector2(kPoint.X, yOffset - 20);
            SegmentDrawing.DrawSegment(kPointStart, kPointEnd, 2, Colors.Light, LineCapType.Capped, 12);
            
            var point = new Vector2(-1000 + k * 2000, yOffset + v);
            if (i < testCurve.Count - 1)
            {
                var nextK = testCurve.Keys[i + 1];
                var nextV = testCurve.Values[i + 1];
                var next = new Vector2(-1000 + nextK * 2000, yOffset + nextV);
                SegmentDrawing.DrawSegment(point, next, 4, curveColor, LineCapType.CappedExtended, 12);
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
        
        SegmentDrawing.DrawSegment(new Vector2(-1000, yOffset), new Vector2(1000, yOffset), 2f, Colors.Light, LineCapType.Capped, 12);
        if (testCurve2.Sample(curveTime, out var value))
        {
            value += new Vector2(0, yOffset);
            value.Draw(20, Colors.Light, 32);
            var resultRect = new Rect(new Vector2(1100, yOffset), new Size(650, 250), AnchorPoint.Left);
            var resultRectSplit = resultRect.SplitV(0.2f, 0.4f);
            gameFont.Draw($"Time: {curveTime:F2}", resultRectSplit[0], 0f, AnchorPoint.Center);
            gameFont.Draw($"X: {value.X:F2}", resultRectSplit[1], 0f, AnchorPoint.Center);
            gameFont.Draw($"Y: {value.Y:F2}", resultRectSplit[2], 0f, AnchorPoint.Center);
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
            SegmentDrawing.DrawSegment(kPointStart, kPointEnd, 2, Colors.Light, LineCapType.Capped, 12);
            if (i < testCurve2.Count - 1)
            {
                var nextV = testCurve2.Values[i + 1];
                nextV += new Vector2(0, yOffset);
                SegmentDrawing.DrawSegment(v, nextV, 4, curveColor, LineCapType.CappedExtended, 12);
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

        // var mainRect = new Rect(new Vector2(-500, yOffset - rectHeight), new Vector2(500, yOffset - rectHeight * 0.6f));
        var resultRect = new Rect(new Vector2(1100, yOffset), new Size(650, 250), AnchorPoint.Left);
        var resultRectSplit = resultRect.SplitV(0.35f);
        var controlPoint = new Vector2(-1000 + curveTime * 2000, yOffset);
        controlPoint.Draw(20, Colors.Dark, 32);
        
        if (testCurve3.Sample(curveTime, out var value))
        {
            gameFont.Draw($"Time: {curveTime:F2}", resultRectSplit.top, 0f, AnchorPoint.Center);
            resultRectSplit.bottom.Draw(value);
        }
    }
    
        
}