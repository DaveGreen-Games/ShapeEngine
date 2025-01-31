using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Lib;
using ShapeEngine.Text;
using ShapeEngine.UI;
using ShapeEngine.Random;

namespace Examples.Scenes.ExampleScenes;

public class OutlineDrawingExample : ExampleScene
{
    private class ValueSlider : ControlNodeSlider
    {
        private readonly TextFont font;
        private readonly string title;
        public bool Percentage = true;

        public ValueSlider(string title, float startValue, float minValue, float maxValue, bool horizontal = true) :
            base(startValue, minValue, maxValue, horizontal)
        {
            this.font = new(GAMELOOP.GetFont(FontIDs.JetBrains), 1f, ColorRgba.White);
            this.title = title;
        }

        protected override bool GetPressedState()
        {
            return ShapeKeyboardButton.SPACE.GetInputState().Down;
        }

        protected override bool GetMousePressedState()
        {
            return ShapeMouseButton.LEFT.GetInputState().Down;
        }

        protected override bool GetDecreaseValuePressed()
        {
            return ShapeKeyboardButton.LEFT.GetInputState().Pressed;
        }

        protected override bool GetIncreaseValuePressed()
        {
            return ShapeKeyboardButton.RIGHT.GetInputState().Pressed;
        }

        protected override void OnDraw()
        {
            
            var bgColor = Colors.Dark;
            var fillColor = MouseInside ? Colors.Special : Colors.Medium;
            var textColor = Colors.Highlight;
            var margin = Rect.Size.Min() * 0.1f;
            var fillRect = Fill.ApplyMarginsAbsolute(margin);
            
            var curGappedOutlineInfo = new GappedOutlineDrawingInfo(4, CurF, 0.5f);
            var lineInfo = new LineDrawingInfo(margin / 4, fillColor, LineCapType.Capped, 6);
        
            Rect.Draw(bgColor);
            fillRect.Draw(fillColor);

            if (Selected)
            {
                Rect.DrawGappedOutline(0f, lineInfo, curGappedOutlineInfo);
            }
            else
            {
                Rect.DrawLines(margin / 4, fillColor);
            }

            font.ColorRgba = textColor;
            if (Percentage)
            {
                int textValue =(int)(CurValue * 100);
                font.DrawTextWrapNone($"{title} {textValue}%", Rect, new AnchorPoint(0.5f, 0.5f));
            }
            else
            {
                int textValue = (int)CurValue;
                font.DrawTextWrapNone($"{title} {textValue}", Rect, new AnchorPoint(0.5f, 0.5f));
            }
            
            
        }
    }

    private InputAction nextShape;
    private InputAction changeDrawingMode;
    private bool gappedMode = true;
    private int shapeIndex = 0;
    private const int MaxShapes = 7;

    private Segment segment;
    private Circle circle;
    private Triangle triangle;
    private Rect rect;
    private Quad quad;
    private Polygon poly;
    private Polyline polyline;
    
    private LineDrawingInfo lineInfo;
    private LineDrawingInfo lineInfoOutline;

    private int curCircleSides = 36;
    
    private float curSideScalingFactor = 0.5f;
    private float curSideScalingOriginFactor = 0.5f;
    
    private float curStartOffset = 0f;
    private const int MaxGaps = 30;
    private int curGaps = 4;
    private float curGapPerimeterPercentage = 0.5f;
    
    private readonly ValueSlider sideScalingFactorSlider; // 0 - 1
    private readonly ValueSlider sideScalingOriginFactorSlider; // 0 - 1
    private readonly ValueSlider startOffsetSlider; // 0 - 1
    private readonly ValueSlider gapsSlider; // 1 - 100
    private readonly ValueSlider gapPerimeterPercentageSlider; // 0 - 1
    
    private readonly ValueSlider circleSideSlider;
    
    public OutlineDrawingExample()
    {
        Title = "Outline Drawing Example";

        var nextStaticShapeMb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
        var nextStaticShapeGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
        var nextStaticShapeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
        nextShape = new(nextStaticShapeMb, nextStaticShapeGp, nextStaticShapeKb);
        
        var changeModeMB = new InputTypeMouseButton(ShapeMouseButton.MIDDLE);
        var changeModeGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
        var changeModeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.TAB);
        changeDrawingMode = new(changeModeMB, changeModeGp, changeModeKb);
        
        textFont.FontSpacing = 1f;
        textFont.ColorRgba = Colors.Light;

        lineInfo = new(3f, Colors.Highlight, LineCapType.CappedExtended, 4);
        lineInfoOutline = new(1.5f, Colors.Dark, LineCapType.CappedExtended, 4);

        float size = 400;
        float radius = size / 2;
        var center = new Vector2();

        segment = new(center, size, Rng.Instance.RandAngleRad(), 0.5f, false);
        circle = new(center, radius);
        triangle = Triangle.Generate(center, size / 2, size);
        rect = new Rect(center, new Size(size, size), new AnchorPoint(0.5f, 0.5f));
        quad = new Quad(center, new Size(size, size), 45 * ShapeMath.DEGTORAD, new AnchorPoint(0.5f, 0.5f));
        poly = Polygon.Generate(center, 16, radius / 2, radius);
        polyline = Polygon.Generate(center, 16, radius / 2, radius).ToPolyline();

        var font = GAMELOOP.GetFont(FontIDs.JetBrains);
        sideScalingFactorSlider = new("Scaling", 0.5f, 0f, 1f, true); // new(0.5f, "Scaling", font);
        sideScalingOriginFactorSlider = new("Origin", 0.5f, 0f, 1f, true);
        
        startOffsetSlider = new("Offset", 0f, 0f, 1f, true);
        gapsSlider = new( "Gaps", 4, 1, MaxGaps, true);
        gapsSlider.Percentage = false;
        circleSideSlider = new( "Sides", 18, 3, 120, true);
        circleSideSlider.Percentage = false;
        gapPerimeterPercentageSlider = new("Perimeter", 0.5f, 0f, 1f, true);

    }
    private void ActualizeSliderValues()
    {
        curSideScalingFactor = sideScalingFactorSlider.CurValue;
        curSideScalingOriginFactor = sideScalingOriginFactorSlider.CurValue;
    
        curStartOffset = startOffsetSlider.CurValue;
        curGaps = (int)(gapsSlider.CurValue);
        curGapPerimeterPercentage = gapPerimeterPercentageSlider.CurValue;

        curCircleSides = (int)circleSideSlider.CurValue;
    }
    public override void Reset()
    {
        shapeIndex = 0;
        gappedMode = true;
        
        curSideScalingFactor = 0.5f;
        sideScalingFactorSlider.SetCurValue(0.5f);
        
        curSideScalingOriginFactor = 0.5f;
        sideScalingOriginFactorSlider.SetCurValue(0.5f);
    
        curStartOffset = 0f;
        startOffsetSlider.SetCurValue(0f);
        
        curGaps = 4;
        gapsSlider.SetCurValue(4);
        
        curCircleSides = 18;
        circleSideSlider.SetCurValue(18);
        
        curGapPerimeterPercentage = 0.5f;
        gapPerimeterPercentageSlider.SetCurValue(0.5f);
        
    }

    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        var sliderBox = ui.Area.ApplyMargins(0.01f, 0.01f, 0.82f, 0.12f);
        
        if (!gappedMode)
        {
            var sliderRects = sliderBox.SplitH(2);
            // sideScalingFactorSlider.Update(time.Delta, sliderRects[0].ApplyMargins(0f, 0.05f, 0f, 0f), ui.MousePos);
            sideScalingFactorSlider.SetRect(sliderRects[0].ApplyMargins(0f, 0.05f, 0f, 0f));
            sideScalingFactorSlider.Update(time.Delta, ui.MousePos);
            
            sideScalingOriginFactorSlider.SetRect(sliderRects[1].ApplyMargins(0.05f, 0f, 0f, 0f));
            sideScalingOriginFactorSlider.Update(time.Delta, ui.MousePos);
        }
        else
        {
            if (shapeIndex == 1)//circle
            {
                var sliderRects = sliderBox.SplitH(4);
                
                circleSideSlider.SetRect(sliderRects[0].ApplyMargins(0f, 0.05f, 0f, 0f));
                circleSideSlider.Update(time.Delta, ui.MousePos);
                
                startOffsetSlider.SetRect(sliderRects[1].ApplyMargins(0.025f, 0.025f, 0f, 0f));
                startOffsetSlider.Update(time.Delta, ui.MousePos);
            
                gapsSlider.SetRect(sliderRects[2].ApplyMargins(0.025f, 0.025f, 0f, 0f));
                gapsSlider.Update(time.Delta, ui.MousePos);
            
                gapPerimeterPercentageSlider.SetRect(sliderRects[3].ApplyMargins(0.05f, 0f, 0f, 0f));
                gapPerimeterPercentageSlider.Update(time.Delta, ui.MousePos);
            }
            else
            {
                var sliderRects = sliderBox.SplitH(3);
                startOffsetSlider.SetRect(sliderRects[0].ApplyMargins(0f, 0.05f, 0f, 0f));
                startOffsetSlider.Update(time.Delta, ui.MousePos);
            
                gapsSlider.SetRect(sliderRects[1].ApplyMargins(0.025f, 0.025f, 0f, 0f));
                gapsSlider.Update(time.Delta, ui.MousePos);
            
                gapPerimeterPercentageSlider.SetRect(sliderRects[2].ApplyMargins(0.05f, 0f, 0f, 0f));
                gapPerimeterPercentageSlider.Update(time.Delta, ui.MousePos);
            }
            
        }
        
        ActualizeSliderValues();
    }

    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
    {
        base.HandleInput(dt, mousePosGame, mousePosGameUi, mousePosUI);
        var gamepad = GAMELOOP.CurGamepad;
        
        nextShape.Gamepad = gamepad;
        nextShape.Update(dt);
        
        changeDrawingMode.Gamepad = gamepad;
        changeDrawingMode.Update(dt);
        
        
        if (nextShape.State.Pressed)
        {
            shapeIndex++;
            if (shapeIndex >= MaxShapes) shapeIndex = 0;
        }

        if (changeDrawingMode.State.Pressed)
        {
            gappedMode = !gappedMode;
        }
        
    }
    protected override void OnDrawGameExample(ScreenInfo game)
    {
        lineInfo = lineInfo.ChangeColor(Colors.Highlight);
        lineInfoOutline = lineInfoOutline.ChangeColor(Colors.Dark);
        
        var curGappedOutlineInfo = new GappedOutlineDrawingInfo(curGaps, curStartOffset, curGapPerimeterPercentage);

        
        if (shapeIndex == 0) // Segment
        {
            
            segment.Draw(lineInfoOutline);
            if (gappedMode)
            {
                // ShapeDrawing.DrawGappedLine(segment.Start, segment.End, -1f, lineInfo, curGappedOutlineInfo);
                segment.DrawGapped(-1, lineInfo, curGappedOutlineInfo);
            }
            else
            {
                segment.DrawScaled(lineInfo, curSideScalingFactor, curSideScalingOriginFactor);
            }
        }
        else if (shapeIndex == 1) // Circle
        {
            circle.DrawLines(lineInfoOutline, curCircleSides);
            if (gappedMode)
            {
                circle.DrawGappedOutline(lineInfo, curGappedOutlineInfo, 0f, curCircleSides);
            }
            else
            {
                circle.DrawLinesScaled(lineInfo, 0f, curCircleSides, curSideScalingFactor, curSideScalingOriginFactor);
            }
        }
        else if (shapeIndex == 2) // Triangle
        {
            triangle.DrawLines(lineInfoOutline);
            if (gappedMode)
            {
                triangle.DrawGappedOutline(0f, lineInfo, curGappedOutlineInfo);
            }
            else
            {
                triangle.DrawLinesScaled(lineInfo, 0f, new(), curSideScalingFactor, curSideScalingOriginFactor);
            }
        }
        else if (shapeIndex == 3) // Rect
        {
            rect.DrawLines(lineInfoOutline);
            if (gappedMode)
            {
                rect.DrawGappedOutline(0f, lineInfo, curGappedOutlineInfo);
            }
            else
            {
                rect.DrawLinesScaled(lineInfo, 0f, new(), curSideScalingFactor, curSideScalingOriginFactor);
            }
        }
        else if (shapeIndex == 4) // Quad
        {
            quad.DrawLines(lineInfoOutline);
            if (gappedMode)
            {
                quad.DrawGappedOutline(0f, lineInfo, curGappedOutlineInfo);
            }
            else
            {
                quad.DrawLinesScaled(lineInfo, 0f, new(), curSideScalingFactor, curSideScalingOriginFactor);
            }
        }
        else if (shapeIndex == 5) // Polygon
        {
            poly.DrawLines(lineInfoOutline);
            if (gappedMode)
            {
                poly.DrawGappedOutline(0f, lineInfo, curGappedOutlineInfo);
            }
            else
            {
                poly.DrawLinesScaled(lineInfo, curSideScalingFactor, curSideScalingOriginFactor);
            }
        }
        else // Polyline
        {
            polyline.Draw(lineInfoOutline);
            if (gappedMode)
            {
                polyline.DrawGappedOutline(0f, lineInfo, curGappedOutlineInfo);
            }
            else
            {
                polyline.DrawLinesScaled(lineInfo, curSideScalingFactor, curSideScalingOriginFactor);
            }
        }
        
        
    }
    
    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        if (!gappedMode)
        {
            sideScalingFactorSlider.Draw();
            sideScalingOriginFactorSlider.Draw();
        }
        else
        {
            if (shapeIndex == 1)
            {
                circleSideSlider.Draw();
            }
            startOffsetSlider.Draw();
            gapsSlider.Draw();
            gapPerimeterPercentageSlider.Draw();
        }
        var curDevice = ShapeInput.CurrentInputDeviceType;
        var nextShapeText = nextShape. GetInputTypeDescription( curDevice, true, 1, false); 
        var changeDrawingModeText = changeDrawingMode. GetInputTypeDescription( curDevice, true, 1, false); 

        var topCenter = GAMELOOP.UIRects.GetRect("center").ApplyMargins(0,0,0.05f,0.9f);
        textFont.ColorRgba = Colors.Light;
        var mode = gappedMode ? "Gapped Outline" : "Scaled Lines";
        
        textFont.DrawTextWrapNone($"{changeDrawingModeText} Mode: {mode}", topCenter, new(0.5f, 0.5f));
        
        var bottomCenter = GAMELOOP.UIRects.GetRect("bottom center").ApplyMargins(0.1f, 0.1f, 0.15f, 0.15f);
        var margin = bottomCenter.Height * 0.05f;
        bottomCenter.DrawLines(2f, Colors.Highlight);
        var textStatic = $"{nextShapeText} {GetCurShapeName()}";
        
        textFont.ColorRgba = Colors.Highlight;
        textFont.DrawTextWrapNone(textStatic, bottomCenter.ApplyMarginsAbsolute(margin, margin, margin, margin), new(0.5f, 0.5f));
        
        
    }
    private string GetCurShapeName()
    {
        if (shapeIndex == 0) return "Segment";
        if (shapeIndex == 1) return "Circle";
        if (shapeIndex == 2) return "Triangle";
        if (shapeIndex == 3) return "Rect";
        if (shapeIndex == 4) return "Quad";
        if (shapeIndex == 5) return "Polygon";
        return "Polyline";
    }

}


