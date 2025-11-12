
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.StripedDrawingDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Input;
using ShapeEngine.StaticLib;
using ShapeEngine.Text;
using ShapeEngine.UI;
using Size = ShapeEngine.Core.Structs.Size;

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
            this.font = new(GameloopExamples.Instance.GetFont(FontIDs.JetBrains), 1f, ColorRgba.White);
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

    private readonly InputAction nextShape;
    private readonly InputAction changeDrawingMode;
    private readonly InputAction regenerateOutsideShape;
    private readonly InputAction regenerateInsideShape;
    private readonly InputAction toggleCrissCrossPattern;
    private readonly InputActionTree inputActionTree;
    
    private bool insideShapeMode = false;
    private int outsideShapeIndex = 0;
    private int insideShapeIndex = 0;
    private const int MaxShapes = 5;
    private const int outsideShapeSize = 450;

    private readonly Circle outsideCircle;
    private Triangle outsideTriangle;
    private readonly Rect outsideRect;
    private readonly Quad outsideQuad;
    private Polygon outsidePoly;
    
    private Circle insideCircle;
    private Triangle insideTriangle;
    private Rect insideRect;
    private Quad insideQuad;
    private Polygon insidePoly;
    
    private LineDrawingInfo lineInfoStriped;
    private LineDrawingInfo lineInfoOutline;

    private int curCircleSides = 64;
    
    private float curInsideShapeRotDeg = 0f;
    private float curInsideShapeSize = 150f;
    private float curInsidePolygonSize;
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

    private bool crissCrossPatternActive = false;
    
    public StripedShapeDrawingExample()
    {
        Title = "Shape Striped Drawing";

        InputActionSettings defaultSettings = new();
        
        var nextStaticShapeMb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
        var nextStaticShapeGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
        var nextStaticShapeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
        nextShape = new(defaultSettings,nextStaticShapeMb, nextStaticShapeGp, nextStaticShapeKb);
        
        var changeModeMB = new InputTypeMouseButton(ShapeMouseButton.MIDDLE);
        var changeModeGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
        var changeModeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.TAB);
        changeDrawingMode = new(defaultSettings,changeModeMB, changeModeGp, changeModeKb);
        
        var regenOutsideShapeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.ONE);
        regenerateOutsideShape = new(defaultSettings,regenOutsideShapeKb);
        
        var regenInsideShapeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.TWO);
        regenerateInsideShape = new(defaultSettings,regenInsideShapeKb);
        
        var toggleCrissCrossKb = new InputTypeKeyboardButton(ShapeKeyboardButton.THREE);
        toggleCrissCrossPattern = new(defaultSettings,toggleCrissCrossKb);

        inputActionTree =
        [
            nextShape,
            changeDrawingMode,
            regenerateOutsideShape,
            regenerateInsideShape,
            toggleCrissCrossPattern
        ];
        
        textFont.FontSpacing = 1f;
        textFont.ColorRgba = Colors.Light;

        lineInfoStriped = new(4f, stripedColor.ColorRgba, LineCapType.Capped, 4);
        lineInfoOutline = new(4f, outlineColor.ColorRgba, LineCapType.CappedExtended, 4);

        float size = outsideShapeSize;
        float radius = size / 2;
        var center = new Vector2();
        center += new Vector2(0, -6);
        outsideCircle = new(center, radius);
        outsideTriangle = Triangle.Generate(center, size / 2, size);
        outsideRect = new Rect(center, new Size(size, size), new AnchorPoint(0.5f, 0.5f));
        outsideQuad = new Quad(center, new Size(size, size), 45 * ShapeMath.DEGTORAD, new AnchorPoint(0.5f, 0.5f));
        var generatedPoly = Polygon.Generate(center, 16, size / 4, size);
        outsidePoly = generatedPoly ?? [];
        
        size = 100;
        radius = size / 2;
        insideCircle = new(center, radius);
        insideTriangle = Triangle.Generate(center, size / 2, size);
        insideRect = new Rect(center, new Size(size, size), new AnchorPoint(0.5f, 0.5f));
        insideQuad = new Quad(center, new Size(size, size), 45 * ShapeMath.DEGTORAD, new AnchorPoint(0.5f, 0.5f));
        generatedPoly = Polygon.Generate(center, 16, size / 4, size);
        insidePoly = generatedPoly ?? [];
        curInsidePolygonSize = size;

        insideShapeRotDegSlider = new("Inside Rotation", 0, 0f, 360, true)
        {
            Percentage = false
        };

        insideShapeSizeSlider = new("Inside Size", 150, 100f, 350f * 2f, true)
        {
            Percentage = false
        };

        spacingOffsetSlider = new("Offset", 0f, -2f, 2f, true);
        
        rotationDegSlider = new("Rotation", 45, 0f, 360f, true)
        {
            Percentage = false
        };

        lineThicknessSlider = new("Thickness", 2f, 1f, 20f, true)
        {
            Percentage = false
        };

        var min = lineThicknessSlider.CurValue * 2 + 4;
        spacingSlider = new("Spacing", min, min, 128f, true)
        {
            Percentage = false
        };
    }
    private void ActualizeSliderValues()
    {
        curSpacing = spacingSlider.CurValue;
        curSpacingOffset = spacingOffsetSlider.CurValue;
        curRotationDeg = rotationDegSlider.CurValue;
    
        float t = lineThicknessSlider.CurValue;
        lineInfoStriped = lineInfoStriped.ChangeThickness(t);
        lineInfoOutline = lineInfoOutline.ChangeThickness(t * 1.25f);
        
        curInsideShapeRotDeg = insideShapeRotDegSlider.CurValue;
        curInsideShapeSize = insideShapeSizeSlider.CurValue;
    }
    public override void Reset()
    {
        outsideShapeIndex = 0;
        insideShapeMode = false;

        curSpacingOffset = 0f;
        spacingOffsetSlider.SetCurValue(curSpacingOffset);

        curRotationDeg = 45f;
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
        var center = new Vector2();
        
        if (outsideShapeIndex == 1)
        {
            outsideTriangle = Triangle.Generate(center, size / 2, size);
        }
        else if (outsideShapeIndex == 4)
        {
            var shape = Polygon.Generate(center, 16, size / 4, size);
            if (shape != null) outsidePoly = shape;
        }
    }
    private void RegenerateInsideShape()
    {
        float size = curInsideShapeSize;
        var center = curInsideShapePos;
        
        if (insideShapeIndex == 1)
        {
            insideTriangle = Triangle.Generate(center, size / 2, size);
        }
        else if (insideShapeIndex == 4)
        {
            var shape = Polygon.Generate(center, 16, size / 2, size);
            if(shape != null) insidePoly = shape;
        }
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
            
        lineThicknessSlider.SetRect(sliderRectsBottom[0].ApplyMargins(0f, 0.1f, 0f, 0f));
        lineThicknessSlider.Update(time.Delta, ui.MousePos);
        
        spacingSlider.SetRect(sliderRectsBottom[1].ApplyMargins(0.05f, 0.05f, 0f, 0f));
        spacingSlider.Update(time.Delta, ui.MousePos);
        var v = spacingSlider.CurValue;
        spacingSlider.MinValue = lineThicknessSlider.CurValue * 2 + 4;
        if(v < spacingSlider.MinValue) spacingSlider.SetCurValue(spacingSlider.MinValue);
        else spacingSlider.SetCurValue(v);

        spacingOffsetSlider.SetRect(sliderRectsBottom[2].ApplyMargins(0.05f, 0.05f, 0f, 0f));
        spacingOffsetSlider.Update(time.Delta, ui.MousePos);
            
        rotationDegSlider.SetRect(sliderRectsBottom[3].ApplyMargins(0.1f, 0f, 0f, 0f));
        rotationDegSlider.Update(time.Delta, ui.MousePos);
        
        ActualizeSliderValues();
        
        if (insideShapeMode)
        {
            curInsideShapePos = game.MousePos;
            var size = curInsideShapeSize;
            var pos = curInsideShapePos;
            var rot = curInsideShapeRotDeg * ShapeMath.DEGTORAD;
            
            if (insideShapeIndex == 0) // Circle
            {
               insideCircle = insideCircle.SetPosition(pos);
               insideCircle = insideCircle.SetRadius(size / 2);
            }
            else if (insideShapeIndex == 1) // Triangle
            {
               insideTriangle = insideTriangle.SetPosition(pos);
               insideTriangle = insideTriangle.SetSize(size, pos);
               insideTriangle = insideTriangle.SetRotation(rot, pos);
            }
            else if (insideShapeIndex == 2) // Rect
            {
                
                insideRect = insideRect.SetPosition(pos, AnchorPoint.Center);
                insideRect = insideRect.SetSize(new Size(size, size), AnchorPoint.Center);
            }
            else if (insideShapeIndex == 3) // Quad
            {
                insideQuad = insideQuad.SetPosition(pos, AnchorPoint.Center);
                insideQuad = insideQuad.SetSize(size, AnchorPoint.Center);
                insideQuad = insideQuad.SetRotation(rot, AnchorPoint.Center);
            }
            else if (insideShapeIndex == 4) // Polygon
            {
                insidePoly.SetPosition(pos);
                var scale = size / curInsidePolygonSize;
                insidePoly.ScaleSize(scale, pos);
                
                curInsidePolygonSize = size;
                
                insidePoly.SetRotation(rot, pos);
            }
                
        }
    }

    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
    {
        base.HandleInput(dt, mousePosGame, mousePosGameUi, mousePosUI);
        var gamepad = Input.GamepadManager.LastUsedGamepad;
        
        inputActionTree.CurrentGamepad = gamepad;
        inputActionTree.Update(dt);
        
        
        if (regenerateOutsideShape.State.Pressed)
        {
            RegenerateOutsideShape();
        }
        if (regenerateInsideShape.State.Pressed)
        {
            RegenerateInsideShape();
        }
        if (toggleCrissCrossPattern.State.Pressed)
        {
            crissCrossPatternActive = !crissCrossPatternActive;
        }
        
        if (nextShape.State.Pressed)
        {
            if (insideShapeMode)
            {
                insideShapeIndex++;
                if(insideShapeIndex >= MaxShapes) insideShapeIndex = 0;
            }
            else
            {
                outsideShapeIndex++;
                if (outsideShapeIndex >= MaxShapes) outsideShapeIndex = 0;
            }
            
        }

        if (changeDrawingMode.State.Pressed)
        {
            insideShapeMode = !insideShapeMode;
        }
        
    }
    protected override void OnDrawGameExample(ScreenInfo game)
    {
        lineInfoStriped = lineInfoStriped.ChangeColor(stripedColor.ColorRgba);
        lineInfoOutline = lineInfoOutline.ChangeColor(outlineColor.ColorRgba);
        var lineInfoOutlineMasked = new LineDrawingInfo(lineInfoOutline.Thickness / 3, outlineColor.ColorRgba.ChangeBrightness(-0.5f), LineCapType.CappedExtended, 4);
        const bool reverseMask = false;
        const bool doubleMask = false; //if true draws the outsideShape outline different inside and outside the insideShape
        
        if (outsideShapeIndex == 0) // Circle
        {
            if (insideShapeMode)
            {
                if (insideShapeIndex == 0) // Circle
                {
                    outsideCircle.DrawStriped(insideCircle, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideCircle.DrawStriped(insideCircle, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideCircle.DrawLinesMasked(insideCircle, lineInfoOutline, 0f, curCircleSides, reverseMask);
                    else
                    {
                        outsideCircle.DrawLinesMasked(insideCircle, lineInfoOutlineMasked, 0f, curCircleSides, true);
                        outsideCircle.DrawLinesMasked(insideCircle, lineInfoOutline, 0f, curCircleSides, false);
                    }
                }
                else if (insideShapeIndex == 1) // Triangle
                {
                    outsideCircle.DrawStriped(insideTriangle, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideCircle.DrawStriped(insideTriangle, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideCircle.DrawLinesMasked(insideTriangle, lineInfoOutline, 0f, curCircleSides, reverseMask);
                    else
                    {
                        outsideCircle.DrawLinesMasked(insideTriangle, lineInfoOutlineMasked, 0f, curCircleSides, true);
                        outsideCircle.DrawLinesMasked(insideTriangle, lineInfoOutline, 0f, curCircleSides, false);
                    }
                }
                else if (insideShapeIndex == 2) // Rect
                {
                    outsideCircle.DrawStriped(insideRect, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideCircle.DrawStriped(insideRect, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideCircle.DrawLinesMasked(insideRect, lineInfoOutline, 0f, curCircleSides, reverseMask);
                    else
                    {
                        outsideCircle.DrawLinesMasked(insideRect, lineInfoOutlineMasked, 0f, curCircleSides, true);
                        outsideCircle.DrawLinesMasked(insideRect, lineInfoOutline, 0f, curCircleSides, false);
                    }
                }
                else if (insideShapeIndex == 3) // Quad
                {
                    outsideCircle.DrawStriped(insideQuad, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideCircle.DrawStriped(insideQuad, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideCircle.DrawLinesMasked(insideQuad, lineInfoOutline, 0f, curCircleSides, reverseMask);
                    else
                    {
                        outsideCircle.DrawLinesMasked(insideQuad, lineInfoOutlineMasked, 0f, curCircleSides, true);
                        outsideCircle.DrawLinesMasked(insideQuad, lineInfoOutline, 0f, curCircleSides, false);
                    }
                }
                else if (insideShapeIndex == 4) // Polygon
                {
                    outsideCircle.DrawStriped(insidePoly, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideCircle.DrawStriped(insidePoly, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideCircle.DrawLinesMasked(insidePoly, lineInfoOutline, 0f, curCircleSides, reverseMask);
                    else
                    {
                        outsideCircle.DrawLinesMasked(insidePoly, lineInfoOutlineMasked, 0f, curCircleSides, true);
                        outsideCircle.DrawLinesMasked(insidePoly, lineInfoOutline, 0f, curCircleSides, false);
                    }
                }
            }
            else
            {
                outsideCircle.DrawStriped(curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                if (crissCrossPatternActive)
                {
                    outsideCircle.DrawStriped(curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                }
                outsideCircle.DrawLines(lineInfoOutline, curCircleSides);
            }
            
        }
        else if (outsideShapeIndex == 1) // Triangle
        {
            if (insideShapeMode)
            {
                if (insideShapeIndex == 0) // Circle
                {
                    outsideTriangle.DrawStriped(insideCircle, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideTriangle.DrawStriped(insideCircle, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideTriangle.DrawLinesMasked(insideCircle, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideTriangle.DrawLinesMasked(insideCircle, lineInfoOutlineMasked, true);
                        outsideTriangle.DrawLinesMasked(insideCircle, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 1) // Triangle
                {
                    outsideTriangle.DrawStriped(insideTriangle, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideTriangle.DrawStriped(insideTriangle, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideTriangle.DrawLinesMasked(insideTriangle, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideTriangle.DrawLinesMasked(insideTriangle, lineInfoOutlineMasked, true);
                        outsideTriangle.DrawLinesMasked(insideTriangle, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 2) // Rect
                {
                    outsideTriangle.DrawStriped(insideRect, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideTriangle.DrawStriped(insideRect, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideTriangle.DrawLinesMasked(insideRect, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideTriangle.DrawLinesMasked(insideRect, lineInfoOutlineMasked, true);
                        outsideTriangle.DrawLinesMasked(insideRect, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 3) // Quad
                {
                    outsideTriangle.DrawStriped(insideQuad, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideTriangle.DrawStriped(insideQuad, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideTriangle.DrawLinesMasked(insideQuad, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideTriangle.DrawLinesMasked(insideQuad, lineInfoOutlineMasked, true);
                        outsideTriangle.DrawLinesMasked(insideQuad, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 4) // Polygon
                {
                    outsideTriangle.DrawStriped(insidePoly, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideTriangle.DrawStriped(insidePoly, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideTriangle.DrawLinesMasked(insidePoly, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideTriangle.DrawLinesMasked(insidePoly, lineInfoOutlineMasked, true);
                        outsideTriangle.DrawLinesMasked(insidePoly, lineInfoOutline, false);
                    }
                }
            }
            else
            {
                outsideTriangle.DrawStriped(curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                if (crissCrossPatternActive)
                {
                    outsideTriangle.DrawStriped(curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                }
                outsideTriangle.DrawLines(lineInfoOutline);
            }
        }
        else if (outsideShapeIndex == 2) // Rect
        {
            if (insideShapeMode)
            {
                if (insideShapeIndex == 0) // Circle
                {
                    outsideRect.DrawStriped(insideCircle, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideRect.DrawStriped(insideCircle, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideRect.DrawLinesMasked(insideCircle, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideRect.DrawLinesMasked(insideCircle, lineInfoOutlineMasked, true);
                        outsideRect.DrawLinesMasked(insideCircle, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 1) // Triangle
                {
                    outsideRect.DrawStriped(insideTriangle, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideRect.DrawStriped(insideTriangle, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideRect.DrawLinesMasked(insideTriangle, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideRect.DrawLinesMasked(insideTriangle, lineInfoOutlineMasked, true);
                        outsideRect.DrawLinesMasked(insideTriangle, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 2) // Rect
                {
                    outsideRect.DrawStriped(insideRect, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideRect.DrawStriped(insideRect, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideRect.DrawLinesMasked(insideRect, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideRect.DrawLinesMasked(insideRect, lineInfoOutlineMasked, true);
                        outsideRect.DrawLinesMasked(insideRect, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 3) // Quad
                {
                    outsideRect.DrawStriped(insideQuad, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideRect.DrawStriped(insideQuad, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideRect.DrawLinesMasked(insideQuad, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideRect.DrawLinesMasked(insideQuad, lineInfoOutlineMasked, true);
                        outsideRect.DrawLinesMasked(insideQuad, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 4) // Polygon
                {
                    outsideRect.DrawStriped(insidePoly, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideRect.DrawStriped(insidePoly, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideRect.DrawLinesMasked(insidePoly, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideRect.DrawLinesMasked(insidePoly, lineInfoOutlineMasked, true);
                        outsideRect.DrawLinesMasked(insidePoly, lineInfoOutline, false);
                    }
                }
            }
            else
            {
                outsideRect.DrawStriped(curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                if (crissCrossPatternActive)
                {
                    outsideRect.DrawStriped(curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                }
                outsideRect.DrawLines(lineInfoOutline);
            }
        }
        else if (outsideShapeIndex == 3) // Quad
        {
            if (insideShapeMode)
            {
                if (insideShapeIndex == 0) // Circle
                {
                    outsideQuad.DrawStriped(insideCircle, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideQuad.DrawStriped(insideCircle, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideQuad.DrawLinesMasked(insideCircle, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideQuad.DrawLinesMasked(insideCircle, lineInfoOutlineMasked, true);
                        outsideQuad.DrawLinesMasked(insideCircle, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 1) // Triangle
                {
                    outsideQuad.DrawStriped(insideTriangle, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideQuad.DrawStriped(insideTriangle, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideQuad.DrawLinesMasked(insideTriangle, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideQuad.DrawLinesMasked(insideTriangle, lineInfoOutlineMasked, true);
                        outsideQuad.DrawLinesMasked(insideTriangle, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 2) // Rect
                {
                    outsideQuad.DrawStriped(insideRect, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideQuad.DrawStriped(insideRect, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideQuad.DrawLinesMasked(insideRect, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideQuad.DrawLinesMasked(insideRect, lineInfoOutlineMasked, true);
                        outsideQuad.DrawLinesMasked(insideRect, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 3) // Quad
                {
                    outsideQuad.DrawStriped(insideQuad, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideQuad.DrawStriped(insideQuad, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideQuad.DrawLinesMasked(insideQuad, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideQuad.DrawLinesMasked(insideQuad, lineInfoOutlineMasked, true);
                        outsideQuad.DrawLinesMasked(insideQuad, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 4) // Polygon
                {
                    outsideQuad.DrawStriped(insidePoly, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsideQuad.DrawStriped(insidePoly, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsideQuad.DrawLinesMasked(insidePoly, lineInfoOutline, reverseMask);
                    else
                    {
                        outsideQuad.DrawLinesMasked(insidePoly, lineInfoOutlineMasked, true);
                        outsideQuad.DrawLinesMasked(insidePoly, lineInfoOutline, false);
                    }
                }
            }
            else
            {
                outsideQuad.DrawStriped(curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                if (crissCrossPatternActive)
                {
                    outsideQuad.DrawStriped(curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                }
                outsideQuad.DrawLines(lineInfoOutline);
            }
        }
        else if (outsideShapeIndex == 4) // Polygon
        {
            if (insideShapeMode)
            {
                if (insideShapeIndex == 0) // Circle
                {
                    outsidePoly.DrawStriped(insideCircle, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsidePoly.DrawStriped(insideCircle, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsidePoly.DrawLinesMasked(insideCircle, lineInfoOutline, reverseMask);
                    else
                    {
                        outsidePoly.DrawLinesMasked(insideCircle, lineInfoOutlineMasked, true);
                        outsidePoly.DrawLinesMasked(insideCircle, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 1) // Triangle
                {
                    outsidePoly.DrawStriped(insideTriangle, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsidePoly.DrawStriped(insideTriangle, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsidePoly.DrawLinesMasked(insideTriangle, lineInfoOutline, reverseMask);
                    else
                    {
                        outsidePoly.DrawLinesMasked(insideTriangle, lineInfoOutlineMasked, true);
                        outsidePoly.DrawLinesMasked(insideTriangle, lineInfoOutline, false); 
                    }
                    
                }
                else if (insideShapeIndex == 2) // Rect
                {
                    outsidePoly.DrawStriped(insideRect, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsidePoly.DrawStriped(insideRect, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsidePoly.DrawLinesMasked(insideRect, lineInfoOutline, reverseMask);
                    else
                    {
                        outsidePoly.DrawLinesMasked(insideRect, lineInfoOutlineMasked, true);
                        outsidePoly.DrawLinesMasked(insideRect, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 3) // Quad
                {
                    outsidePoly.DrawStriped(insideQuad, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsidePoly.DrawStriped(insideQuad, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsidePoly.DrawLinesMasked(insideQuad, lineInfoOutline, reverseMask);
                    else
                    {
                        outsidePoly.DrawLinesMasked(insideQuad, lineInfoOutlineMasked, true);
                        outsidePoly.DrawLinesMasked(insideQuad, lineInfoOutline, false);
                    }
                }
                else if (insideShapeIndex == 4) // Polygon
                {
                    outsidePoly.DrawStriped(insidePoly, curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                    if (crissCrossPatternActive)
                    {
                        outsidePoly.DrawStriped(insidePoly, curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                    }
                    if(!doubleMask) outsidePoly.DrawLinesMasked(insidePoly, lineInfoOutline, reverseMask);
                    else
                    {
                        outsidePoly.DrawLinesMasked(insidePoly, lineInfoOutlineMasked, true);
                        outsidePoly.DrawLinesMasked(insidePoly, lineInfoOutline, false);
                    }
                }
            }
            else
            {
                outsidePoly.DrawStriped(curSpacing, curRotationDeg, lineInfoStriped, curSpacingOffset);
                if (crissCrossPatternActive)
                {
                    outsidePoly.DrawStriped(curSpacing, curRotationDeg + 90, lineInfoStriped, curSpacingOffset);
                }
                outsidePoly.DrawLines(lineInfoOutline);
            }
        }

        if (insideShapeMode)
        {
            if (insideShapeIndex == 0) // Circle
            {
                insideCircle.DrawLines(lineInfoOutline, curCircleSides);
            }
            else if (insideShapeIndex == 1) // Triangle
            {
                insideTriangle.DrawLines(lineInfoOutline);
            }
            else if (insideShapeIndex == 2) // Rect
            {
                insideRect.DrawLines(lineInfoOutline);
            }
            else if (insideShapeIndex == 3) // Quad
            {
                insideQuad.DrawLines(lineInfoOutline);
            }
            else if (insideShapeIndex == 4) // Polygon
            {
                insidePoly.DrawLines(lineInfoOutline);
            }
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
        var curDevice = Input.CurrentInputDeviceType;
        var nextShapeText = nextShape.GetInputTypeDescription( curDevice, true, 1, false); 
        var changeDrawingModeText = changeDrawingMode.GetInputTypeDescription( curDevice, true, 1, false); 
        var regenOutsideShapeText = regenerateOutsideShape.GetInputTypeDescription(InputDeviceType.Keyboard, true, 1, false); 
        var regenInsideShapeText = regenerateInsideShape.GetInputTypeDescription(InputDeviceType.Keyboard, true, 1, false); 
        var toggleCrissCrossPatternText = toggleCrissCrossPattern.GetInputTypeDescription(InputDeviceType.Keyboard, true, 1, false); 

        var topCenter = GameloopExamples.Instance.UIRects.GetRect("center").ApplyMargins(0,0,0.01f,0.94f);
        var topCenterSplit = topCenter.SplitH(4);
        textFont.ColorRgba = Colors.Light;
        var mode = insideShapeMode ? "Outside & Inside" : "Outside Only";
        var anchorPoint = new AnchorPoint(0.5f, 0f);
        textFont.DrawTextWrapNone($"{changeDrawingModeText} {mode}", topCenterSplit[0], anchorPoint);
        textFont.DrawTextWrapNone($"{regenOutsideShapeText} Regen Outside", topCenterSplit[1], anchorPoint);
        textFont.DrawTextWrapNone($"{regenInsideShapeText} Regen Inside", topCenterSplit[2], anchorPoint);
        textFont.DrawTextWrapNone($"{toggleCrissCrossPatternText} Criss Cross", topCenterSplit[3], anchorPoint);
        
        var bottomCenter = GameloopExamples.Instance.UIRects.GetRect("bottom center").ApplyMargins(0.1f, 0.1f, 0.15f, 0.15f);
        var margin = bottomCenter.Height * 0.05f;
        bottomCenter.DrawLines(2f, Colors.Highlight);
        var textStatic = $"{nextShapeText} {GetCurShapeName()}";
        
        textFont.ColorRgba = Colors.Highlight;
        textFont.DrawTextWrapNone(textStatic, bottomCenter.ApplyMarginsAbsolute(margin, margin, margin, margin), new(0.5f, 0.5f));
        
        
    }
    private string GetCurShapeName()
    {
        if (outsideShapeIndex == 0) return "Circle";
        if (outsideShapeIndex == 1) return "Triangle";
        if (outsideShapeIndex == 2) return "Rect";
        if (outsideShapeIndex == 3) return "Quad";
        if (outsideShapeIndex == 4) return "Polygon";
        return "Circle";
    }

}


