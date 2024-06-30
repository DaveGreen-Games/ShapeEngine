using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Lib;
using ShapeEngine.Text;
using ShapeEngine.UI;


namespace Examples.Scenes.ExampleScenes;

public class ShapeDrawingExample : ExampleScene
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
        
            Rect.Draw(bgColor);
            fillRect.Draw(fillColor);
            Rect.DrawLines(margin / 4, fillColor);

            font.ColorRgba = textColor;
            if (Percentage)
            {
                int textValue =(int)(CurValue * 100);
                font.DrawTextWrapNone($"{title} {textValue}%", Rect, new Vector2(0.5f, 0.5f));
            }
            else
            {
                int textValue = (int)CurValue;
                font.DrawTextWrapNone($"{title} {textValue}", Rect, new Vector2(0.5f, 0.5f));
            }
            
            
        }
    }

    private InputAction nextShape;
    private InputAction changeDrawingMode;
    private bool gappedMode = true;
    private int shapeIndex = 0;
    private const int MaxShapes = 6;

    private Circle circle;
    private Triangle triangle;
    private Rect rect;
    private Quad quad;
    private Polygon poly;
    private Polyline polyline;
    
    private LineDrawingInfo lineInfo;
    private LineDrawingInfo lineInfoOutline;

    private int curCircleSides = 12;
    
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
    
    public ShapeDrawingExample()
    {
        Title = "Shape Drawing Example";

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

        circle = new(center, radius);
        triangle = Triangle.Generate(center, size / 2, size);
        rect = new Rect(center, new Size(size, size), new Vector2(0.5f, 0.5f));
        quad = new Quad(center, new Size(size, size), 45 * ShapeMath.DEGTORAD, new Vector2(0.5f, 0.5f));
        poly = Polygon.Generate(center, 16, radius / 2, radius);
        polyline = Polygon.Generate(center, 16, radius / 2, radius).ToPolyline();

        var font = GAMELOOP.GetFont(FontIDs.JetBrains);
        sideScalingFactorSlider = new("Scaling", 0.5f, 0f, 1f, true); // new(0.5f, "Scaling", font);
        sideScalingOriginFactorSlider = new("Origin", 0.5f, 0f, 1f, true);
        
        startOffsetSlider = new("Offset", 0f, 0f, 1f, true);
        gapsSlider = new( "Gaps", 4, 1, MaxGaps, true);
        gapsSlider.Percentage = false;
        gapPerimeterPercentageSlider = new("Perimeter", 0.5f, 0f, 1f, true);

    }
    private void ActualizeSliderValues()
    {
        curSideScalingFactor = sideScalingFactorSlider.CurValue;
        curSideScalingOriginFactor = sideScalingOriginFactorSlider.CurValue;
    
        curStartOffset = startOffsetSlider.CurValue;
        curGaps = (int)(gapsSlider.CurValue);
        curGapPerimeterPercentage = gapPerimeterPercentageSlider.CurValue;
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
        
        curGapPerimeterPercentage = 0.5f;
        gapPerimeterPercentageSlider.SetCurValue(0.5f);
        
    }

    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo ui)
    {
        var sliderBox = ui.Area.ApplyMargins(0.01f, 0.01f, 0.82f, 0.12f);
        var sliderRects = sliderBox.SplitH(3);
        if (!gappedMode)
        {
            // sideScalingFactorSlider.Update(time.Delta, sliderRects[0].ApplyMargins(0f, 0.05f, 0f, 0f), ui.MousePos);
            sideScalingFactorSlider.SetRect(sliderRects[0].ApplyMargins(0f, 0.05f, 0f, 0f));
            sideScalingFactorSlider.Update(time.Delta, ui.MousePos);
            
            sideScalingOriginFactorSlider.SetRect(sliderRects[1].ApplyMargins(0.05f, 0f, 0f, 0f));
            sideScalingOriginFactorSlider.Update(time.Delta, ui.MousePos);
        }
        else
        {
            
            startOffsetSlider.SetRect(sliderRects[0].ApplyMargins(0f, 0.05f, 0f, 0f));
            startOffsetSlider.Update(time.Delta, ui.MousePos);
            
            gapsSlider.SetRect(sliderRects[1].ApplyMargins(0.025f, 0.025f, 0f, 0f));
            gapsSlider.Update(time.Delta, ui.MousePos);
            
            gapPerimeterPercentageSlider.SetRect(sliderRects[2].ApplyMargins(0.05f, 0f, 0f, 0f));
            gapPerimeterPercentageSlider.Update(time.Delta, ui.MousePos);
        }
        
        ActualizeSliderValues();
    }

    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
    {
        base.HandleInput(dt, mousePosGame, mousePosUI);
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

        if (shapeIndex == 0) // Circle
        {
            circle.DrawLines(lineInfoOutline, curCircleSides);
            if (gappedMode)
            {
                
            }
            else
            {
                circle.DrawLinesScaled(lineInfo, 0f, curCircleSides, curSideScalingFactor, curSideScalingOriginFactor);
            }
        }
        else if (shapeIndex == 1) // Triangle
        {
            triangle.DrawLines(lineInfoOutline);
            if (gappedMode)
            {
                
            }
            else
            {
                triangle.DrawLinesScaled(lineInfo, 0f, new(), curSideScalingFactor, curSideScalingOriginFactor);
            }
        }
        else if (shapeIndex == 2) // Rect
        {
            rect.DrawLines(lineInfoOutline);
            if (gappedMode)
            {
                
            }
            else
            {
                rect.DrawLinesScaled(lineInfo, 0f, new(), curSideScalingFactor, curSideScalingOriginFactor);
            }
        }
        else if (shapeIndex == 3) // Quad
        {
            quad.DrawLines(lineInfoOutline);
            if (gappedMode)
            {
                
            }
            else
            {
                quad.DrawLinesScaled(lineInfo, 0f, new(), curSideScalingFactor, curSideScalingOriginFactor);
            }
        }
        else if (shapeIndex == 4) // Polygon
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
                
            }
            else
            {
                polyline.DrawLinesScaled(lineInfo, curSideScalingFactor, curSideScalingOriginFactor);
            }
        }
        
        
    }
    protected override void OnDrawGameUIExample(ScreenInfo ui)
    {
        if (!gappedMode)
        {
            sideScalingFactorSlider.Draw();
            sideScalingOriginFactorSlider.Draw();
        }
        else
        {
            startOffsetSlider.Draw();
            gapsSlider.Draw();
            gapPerimeterPercentageSlider.Draw();
        }
    }
    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        
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
        if (shapeIndex == 0) return "Circle";
        else if (shapeIndex == 1) return "Triangle";
        else if (shapeIndex == 2) return "Rect";
        else if (shapeIndex == 3) return "Quad";
        else if (shapeIndex == 4) return "Polygon";
        else return "Polyline";
    }
   
}


