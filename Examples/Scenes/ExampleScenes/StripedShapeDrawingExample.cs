using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.StaticLib;
using ShapeEngine.StaticLib.Drawing;
using ShapeEngine.Text;
using ShapeEngine.UI;
using ShapeEngine.Random;

namespace Examples.Scenes.ExampleScenes;

public class StripedShapeDrawingExample : ExampleScene
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
    private bool insideShapeMode = false;
    private int shapeIndex = 0;
    private const int MaxShapes = 5;
    private const int outsideShapeSize = 450;

    private Circle circle;
    private Triangle triangle;
    private Rect rect;
    private Quad quad;
    private Polygon poly;
    
    private LineDrawingInfo lineStripedInfo;
    private LineDrawingInfo lineInfoOutline;

    private int curCircleSides = 64;
    
    private float curInsideShapeRotDeg = 0f;
    private float curInsideShapeSize = 150f;
    private Vector2 curInsideShapePos = Vector2.Zero;
    private readonly ValueSlider insideShapeRotDegSlider; // 0 - 360
    private readonly ValueSlider insideShapeSizeSlider; // 100 - 350
    
    private float curSpacing = 4;
    private float curSpacingOffset = 0f;
    private float curRotationDeg = 45;
    private float curLineThickness = 2f;
    
    private readonly ValueSlider spacingSlider; // 4 - 128
    private readonly ValueSlider spacingOffsetSlider; // 0 - 1
    private readonly ValueSlider rotationDegSlider; // 0 - 360
    private readonly ValueSlider lineThicknessSlider; // 1 - 32

    private readonly PaletteColor outlineColor = Colors.PcLight;
    private readonly PaletteColor stripedColor = Colors.PcSpecial;
    
    public StripedShapeDrawingExample()
    {
        Title = "Shape Striped Drawing";

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

        lineStripedInfo = new(4f, stripedColor.ColorRgba, LineCapType.Capped, 4);
        lineInfoOutline = new(4f, outlineColor.ColorRgba, LineCapType.CappedExtended, 4);

        float size = outsideShapeSize;
        float radius = size / 2;
        var center = new Vector2();

        circle = new(center, radius);
        triangle = Triangle.Generate(center, size / 2, size);
        rect = new Rect(center, new Size(size, size), new AnchorPoint(0.5f, 0.5f));
        quad = new Quad(center, new Size(size, size), 45 * ShapeMath.DEGTORAD, new AnchorPoint(0.5f, 0.5f));
        poly = Polygon.Generate(center, 16, size / 4, size);

        var font = GAMELOOP.GetFont(FontIDs.JetBrains);
        insideShapeRotDegSlider = new("Inside Rotation", 0, 0f, 360, true);
        insideShapeRotDegSlider.Percentage = false;
        
        insideShapeSizeSlider = new("Inside Size", 150, 100f, 350f, true);
        insideShapeSizeSlider.Percentage = false;
        
        spacingOffsetSlider = new("Offset", 0.5f, 0f, 1f, true);
        
        rotationDegSlider = new("Rotation", 45, 0f, 360f, true);
        rotationDegSlider.Percentage = false;
        
        lineThicknessSlider = new("Thickness", 2f, 1f, 32f, true);
        lineThicknessSlider.Percentage = false;
        
        spacingSlider = new("Spacing", 12, 4f, 128f, true);
        spacingSlider.Percentage = false;
        spacingSlider.MinValue = lineThicknessSlider.CurValue * 2 + 4;

    }
    private void ActualizeSliderValues()
    {
        curSpacing = spacingSlider.CurValue;
        curSpacingOffset = spacingOffsetSlider.CurValue;
        curRotationDeg = rotationDegSlider.CurValue;
    
        float t = lineThicknessSlider.CurValue;
        lineStripedInfo = lineStripedInfo.ChangeThickness(t);
        lineInfoOutline = lineInfoOutline.ChangeThickness(t);
        
        curInsideShapeRotDeg = insideShapeRotDegSlider.CurValue;
        curInsideShapeSize = insideShapeSizeSlider.CurValue;
    }
    public override void Reset()
    {
        shapeIndex = 0;
        insideShapeMode = false;

        curSpacingOffset = 0f;
        spacingOffsetSlider.SetCurValue(curSpacingOffset);

        curRotationDeg = 0f;
        rotationDegSlider.SetCurValue(curRotationDeg);
        
        curLineThickness = 2f;
        lineThicknessSlider.SetCurValue(curLineThickness);

        curSpacing = curLineThickness * 2 + 4;
        spacingSlider.SetCurValue(curSpacing);
        spacingSlider.MinValue = curSpacing;
        
        curInsideShapeRotDeg = 0f;
        insideShapeRotDegSlider.SetCurValue(curInsideShapeRotDeg);

        curInsideShapeSize = 150f;
        insideShapeSizeSlider.SetCurValue(curInsideShapeSize);
        
    }

    private void RegenerateOutsideShape()
    {
        float size = outsideShapeSize;
        float radius = size / 2;
        var center = new Vector2();
        
        if (shapeIndex == 1)
        {
            triangle = Triangle.Generate(center, size / 2, size);
        }
        else if (shapeIndex == 4)
        {
            poly = Polygon.Generate(center, 16, radius / 2, radius);
        }
    }
    private void RegenerateInsideShape()
    {
        // float size = outsideShapeSize;
        // float radius = size / 2;
        // var center = new Vector2();
        //
        // if (shapeIndex == 1)
        // {
        //     triangle = Triangle.Generate(center, size / 2, size);
        // }
        // else if (shapeIndex == 4)
        // {
        //     poly = Polygon.Generate(center, 16, radius / 2, radius);
        // }
    }
    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        var sliderBox = ui.Area.ApplyMargins(0.01f, 0.01f, 0.75f, 0.12f);
        var vSplit = sliderBox.SplitV(0.45f);
        var sliderRectsBottom = vSplit.bottom.SplitH(4);
        var sliderRectsTop = vSplit.top.ApplyMargins(0f, 0f, 0f, 0.1f).SplitH(0.5f);
        
        if (insideShapeMode)
        {
            insideShapeSizeSlider.SetRect(sliderRectsTop.left.ApplyMargins(0.1f, 0.05f, 0f, 0f));
            insideShapeSizeSlider.Update(time.Delta, ui.MousePos);
                
            insideShapeRotDegSlider.SetRect(sliderRectsTop.right.ApplyMargins(0.05f, 0.1f, 0f, 0f));
            insideShapeRotDegSlider.Update(time.Delta, ui.MousePos);
        }
            
        lineThicknessSlider.SetRect(sliderRectsBottom[0].ApplyMargins(0f, 0.05f, 0f, 0f));
        lineThicknessSlider.Update(time.Delta, ui.MousePos);
        
        spacingSlider.MinValue = lineThicknessSlider.CurValue * 2 + 4;
        spacingSlider.SetRect(sliderRectsBottom[1].ApplyMargins(0.025f, 0.025f, 0f, 0f));
        spacingSlider.Update(time.Delta, ui.MousePos);

        spacingOffsetSlider.SetRect(sliderRectsBottom[2].ApplyMargins(0.025f, 0.025f, 0f, 0f));
        spacingOffsetSlider.Update(time.Delta, ui.MousePos);
            
        rotationDegSlider.SetRect(sliderRectsBottom[3].ApplyMargins(0.05f, 0f, 0f, 0f));
        rotationDegSlider.Update(time.Delta, ui.MousePos);
        
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
            insideShapeMode = !insideShapeMode;
        }
        
    }
    protected override void OnDrawGameExample(ScreenInfo game)
    {
        lineStripedInfo = lineStripedInfo.ChangeColor(stripedColor.ColorRgba);
        lineInfoOutline = lineInfoOutline.ChangeColor(outlineColor.ColorRgba);
        
        

        
        if (shapeIndex == 0) // Circle
        {
            
            if (insideShapeMode)
            {
                
            }
            else
            {
                circle.DrawStriped(curSpacing, curRotationDeg, lineStripedInfo, curSpacingOffset);
            }
            circle.DrawLines(lineInfoOutline, curCircleSides);
        }
        else if (shapeIndex == 1) // Triangle
        {
            
            if (insideShapeMode)
            {
                
            }
            else
            {
                triangle.DrawStriped(curSpacing, curRotationDeg, lineStripedInfo, curSpacingOffset);
            }
            triangle.DrawLines(lineInfoOutline);
        }
        else if (shapeIndex == 2) // Rect
        {
            
            if (insideShapeMode)
            {
                
            }
            else
            {
                rect.DrawStriped(curSpacing, curRotationDeg, lineStripedInfo, curSpacingOffset);
            }
            rect.DrawLines(lineInfoOutline);
        }
        else if (shapeIndex == 3) // Quad
        {
            
            if (insideShapeMode)
            {
                
            }
            else
            {
                quad.DrawStriped(curSpacing, curRotationDeg, lineStripedInfo, curSpacingOffset);
            }
            quad.DrawLines(lineInfoOutline);
        }
        else if (shapeIndex == 4) // Polygon
        {
            
            if (insideShapeMode)
            {
                
            }
            else
            {
                poly.DrawStriped(curSpacing, curRotationDeg, lineStripedInfo, curSpacingOffset);
            }
            poly.DrawLines(lineInfoOutline);
        }
        
        
    }
    
    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        if (insideShapeMode)
        {
            rotationDegSlider.Draw();
            spacingSlider.Draw();
            spacingOffsetSlider.Draw();
            lineThicknessSlider.Draw();
            insideShapeSizeSlider.Draw();
            insideShapeRotDegSlider.Draw();
        }
        else
        {
            rotationDegSlider.Draw();
            spacingSlider.Draw();
            spacingOffsetSlider.Draw();
            lineThicknessSlider.Draw();
        }
        var curDevice = ShapeInput.CurrentInputDeviceType;
        var nextShapeText = nextShape. GetInputTypeDescription( curDevice, true, 1, false); 
        var changeDrawingModeText = changeDrawingMode. GetInputTypeDescription( curDevice, true, 1, false); 

        var topCenter = GAMELOOP.UIRects.GetRect("center").ApplyMargins(0,0,0.05f,0.9f);
        textFont.ColorRgba = Colors.Light;
        var mode = insideShapeMode ? "Outside & Inside Shape Mode" : "Outside Shape Only Mode";
        
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
        if (shapeIndex == 1) return "Triangle";
        if (shapeIndex == 2) return "Rect";
        if (shapeIndex == 3) return "Quad";
        if (shapeIndex == 4) return "Polygon";
        return "Circle";
    }

}


