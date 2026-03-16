using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Input;
using ShapeEngine.StaticLib;
using ShapeEngine.Text;
using ShapeEngine.UI;

namespace Examples.Scenes.ExampleScenes;

public class ShapeDrawingTestExample : ExampleScene
{
    private enum ShapeType
    {
        Segment,
        Circle,
        Triangle,
        Rect,
        Quad,
        Polygon,
        Polyline
    }

    private enum ParamId
    {
        Thickness,
        Smoothness,
        Percentage,
        Scale,
        Origin,
        StartIndex,
        CornerLength,
        CornerFactor,
        RotationDeg,
        Roundness,
        Segments,
        RingThickness,
        StartAngleDeg,
        EndAngleDeg,
        DrawType,
        Width,
        EndWidth,
        Steps,
        VertexRadius,
        MiterLimit
    }

    private sealed class ValueSlider : ControlNodeSlider
    {
        private readonly TextFont font;
        public string Label;
        public bool Percentage = false;
        public bool Integer = false;
        public bool ShowValue = true;

        public ValueSlider(string label, float value, float min, float max, bool horizontal = true)
            : base(value, min, max, horizontal)
        {
            Label = label;
            font = new(GameloopExamples.Instance.GetFont(FontIDs.JetBrains), 1f, ColorRgba.White);
        }

        protected override bool GetPressedState() => ShapeKeyboardButton.SPACE.GetInputState().Down;
        protected override bool GetMousePressedState() => ShapeMouseButton.LEFT.GetInputState().Down;
        protected override bool GetDecreaseValuePressed() => ShapeKeyboardButton.LEFT.GetInputState().Pressed;
        protected override bool GetIncreaseValuePressed() => ShapeKeyboardButton.RIGHT.GetInputState().Pressed;

        protected override void OnDraw()
        {
            var bgColor = Colors.Dark;
            var fillColor = MouseInside ? Colors.Special : Colors.Medium;
            var textColor = Colors.Highlight;
            var margin = Rect.Size.Min() * 0.1f;
            var fillRect = Fill.ApplyMarginsAbsolute(margin);

            Rect.Draw(bgColor);
            fillRect.Draw(fillColor);

            if (Selected) Rect.DrawLines(2f, Colors.Special);
            else Rect.DrawLines(2f, Colors.Medium);

            font.ColorRgba = textColor;

            if (!ShowValue)
            {
                font.DrawTextWrapNone(Label, Rect, new AnchorPoint(0.5f, 0.5f));
                return;
            }

            string valueText;
            if (Percentage) valueText = $"{(int)(CurValue * 100f)}%";
            else if (Integer) valueText = $"{(int)CurValue}";
            else valueText = $"{CurValue:0.##}";

            font.DrawTextWrapNone($"{Label} {valueText}", Rect, new AnchorPoint(0.5f, 0.5f));
        }
    }

    private sealed record DrawingCase(string Name, Action Draw, params ParamId[] Params);

    private static readonly ColorRgba DrawColor = ColorRgba.White.SetAlpha(200);

    private readonly ValueSlider shapeSlider;
    private readonly ValueSlider methodSlider;
    private readonly Dictionary<ParamId, ValueSlider> paramSliders = new();
    private readonly Dictionary<ShapeType, List<DrawingCase>> cases = new();

    private Segment segment;
    private Circle circle;
    private Triangle triangle;
    private Rect rect;
    private Quad quad;
    private Triangle triangleTemplate;
    private Polygon polygonTemplate = [];
    private Polygon polygon = [];
    private Polyline polyline = [];

    public ShapeDrawingTestExample()
    {
        Title = "Shape Drawing Test";

        textFont.FontSpacing = 1f;
        textFont.ColorRgba = Colors.Light;

        BuildParamSliders();
        InitializeStableShapes();
        BuildShapes();
        BuildCases();

        shapeSlider = new("Shape", 0, 0, Enum.GetValues<ShapeType>().Length - 1, false)
        {
            Integer = true
        };

        methodSlider = new("Method", 0, 0, 0, false)
        {
            Integer = true
        };

        SyncMethodSlider();
    }

    public override void Reset()
    {
        shapeSlider.SetCurValue(0);
        methodSlider.SetCurValue(0);

        foreach (var slider in paramSliders.Values)
        {
            slider.SetCurValue(slider.MinValue);
        }

        paramSliders[ParamId.Thickness].SetCurValue(6f);
        paramSliders[ParamId.Smoothness].SetCurValue(0.5f);
        paramSliders[ParamId.Percentage].SetCurValue(0.6f);
        paramSliders[ParamId.Scale].SetCurValue(0.6f);
        paramSliders[ParamId.Origin].SetCurValue(0.5f);
        paramSliders[ParamId.StartIndex].SetCurValue(0);
        paramSliders[ParamId.CornerLength].SetCurValue(50f);
        paramSliders[ParamId.CornerFactor].SetCurValue(0.2f);
        paramSliders[ParamId.RotationDeg].SetCurValue(45f);
        paramSliders[ParamId.Roundness].SetCurValue(0.25f);
        paramSliders[ParamId.Segments].SetCurValue(8);
        paramSliders[ParamId.RingThickness].SetCurValue(40f);
        paramSliders[ParamId.StartAngleDeg].SetCurValue(0f);
        paramSliders[ParamId.EndAngleDeg].SetCurValue(220f);
        paramSliders[ParamId.DrawType].SetCurValue(0);
        paramSliders[ParamId.Width].SetCurValue(20f);
        paramSliders[ParamId.EndWidth].SetCurValue(4f);
        paramSliders[ParamId.Steps].SetCurValue(8);
        paramSliders[ParamId.VertexRadius].SetCurValue(12f);
        paramSliders[ParamId.MiterLimit].SetCurValue(4f);

        InitializeStableShapes();
        BuildShapes();
        SyncMethodSlider();
    }

    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        var leftRect = ui.Area.ApplyMargins(0.01f, 0.92f, 0.14f, 0.18f);
        var rightRect = ui.Area.ApplyMargins(0.92f, 0.01f, 0.14f, 0.18f);
        var bottomRect = ui.Area.ApplyMargins(0.12f, 0.12f, 0.82f, 0.02f);

        shapeSlider.SetRect(leftRect);
        shapeSlider.Update(time.Delta, ui.MousePos);

        SyncMethodSlider();

        methodSlider.SetRect(rightRect);
        methodSlider.Update(time.Delta, ui.MousePos);

        var active = GetCurrentCase().Params.ToList();
        if (UsesRotationSlider()) active.Add(ParamId.RotationDeg);

        if (active.Count > 0)
        {
            var rects = bottomRect.SplitV(active.Count);
            for (int i = 0; i < active.Count; i++)
            {
                var slider = paramSliders[active[i]];
                slider.SetRect(rects[i].ApplyMargins(0.02f, 0.02f, 0f, 0f));
                slider.Update(time.Delta, ui.MousePos);
            }
        }

        BuildShapes();

        Title = $"{GetCurrentShape()} :: {GetCurrentCase().Name}";
    }

    protected override void OnDrawGameExample(ScreenInfo game)
    {
        GetCurrentCase().Draw();
    }

    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        var shapeLabel = GetCurrentShape().ToString();
        var methodLabel = GetCurrentCase().Name;

        shapeSlider.Label = string.Empty;
        methodSlider.Label = string.Empty;

        shapeSlider.ShowValue = false;
        methodSlider.ShowValue = false;

        shapeSlider.Draw();
        methodSlider.Draw();

        var leftRect = ui.Area.ApplyMargins(0.01f, 0.92f, 0.14f, 0.18f);
        var rightRect = ui.Area.ApplyMargins(0.92f, 0.01f, 0.14f, 0.18f);
        var leftLabelRect = leftRect;
        var rightLabelRect = rightRect;

        textFont.ColorRgba = Colors.Highlight;
        textFont.Draw(shapeLabel, leftLabelRect, 90f, new AnchorPoint(0.5f, 0.5f));
        textFont.Draw(methodLabel, rightLabelRect, -90f, new AnchorPoint(0.5f, 0.5f));
        

        var active = GetCurrentCase().Params.ToList();
        if (UsesRotationSlider()) active.Add(ParamId.RotationDeg);
        foreach (var id in active) paramSliders[id].Draw();
    }

    private void InitializeStableShapes()
    {
        float size = 420f;
        float radius = size * 0.5f;
        var center = Vector2.Zero;

        triangleTemplate = Triangle.Generate(center, size * 0.5f, size);
        var generatedPolygon = Polygon.Generate(center, 10, radius * 0.5f, radius);
        polygonTemplate = generatedPolygon ?? [];
    }

    private void BuildShapes()
    {
        float size = 420f;
        float radius = size * 0.5f;
        float rotRad = RotationDeg * ShapeMath.DEGTORAD;
        var center = Vector2.Zero;

        segment = new(center, size, rotRad);
        circle = new(center, radius);
        triangle = triangleTemplate;
        rect = new Rect(center, new Size(size, size), new AnchorPoint(0.5f, 0.5f));
        quad = new Quad(center, new Size(size, size), rotRad, new AnchorPoint(0.5f, 0.5f));

        polygon = new Polygon(polygonTemplate);
        polygon.SetRotation(rotRad, center);

        polyline = polygon.ToPolyline();
    }

    private void BuildParamSliders()
    {
        paramSliders[ParamId.Thickness] = new("Thickness", 6f, 1f, 32f) { Integer = false };
        paramSliders[ParamId.Smoothness] = new("Smoothness", 0.5f, 0f, 1f) { Percentage = true };
        paramSliders[ParamId.Percentage] = new("Percentage", 0.6f, -1f, 1f) { Percentage = true };
        paramSliders[ParamId.Scale] = new("Scale", 0.6f, 0f, 1f) { Percentage = true };
        paramSliders[ParamId.Origin] = new("Origin", 0.5f, 0f, 1f) { Percentage = true };
        paramSliders[ParamId.StartIndex] = new("Start", 0, 0, 15) { Integer = true };
        paramSliders[ParamId.CornerLength] = new("Corner", 50f, 1f, 120f);
        paramSliders[ParamId.CornerFactor] = new("Corner", 0.2f, 0f, 1f) { Percentage = true };
        paramSliders[ParamId.RotationDeg] = new("Rotation", 45f, 0f, 360f);
        paramSliders[ParamId.Roundness] = new("Roundness", 0.25f, 0f, 1f) { Percentage = true };
        paramSliders[ParamId.Segments] = new("Segments", 8, 1, 32) { Integer = true };
        paramSliders[ParamId.RingThickness] = new("Ring", 40f, 4f, 120f);
        paramSliders[ParamId.StartAngleDeg] = new("Start°", 0f, 0f, 360f);
        paramSliders[ParamId.EndAngleDeg] = new("End°", 220f, 0f, 360f);
        paramSliders[ParamId.DrawType] = new("Type", 0, 0, 2) { Integer = true };
        paramSliders[ParamId.Width] = new("Width", 20f, 2f, 40f);
        paramSliders[ParamId.EndWidth] = new("End", 4f, 1f, 20f);
        paramSliders[ParamId.Steps] = new("Steps", 8, 1, 16) { Integer = true };
        paramSliders[ParamId.VertexRadius] = new("Vertex", 12f, 2f, 32f);
        paramSliders[ParamId.MiterLimit] = new("Miter", 4f, 2f, 8f);
    }

    private void BuildCases()
    {
        cases[ShapeType.Segment] =
        [
            new("Draw", () => segment.Draw(LineInfo)),
            new("Draw Separate Caps", () => segment.DrawSeparateCaps(LineThickness, DrawColor, LineCapType.Capped, 4, LineCapType.CappedExtended, 4), ParamId.Thickness),
            new("Draw Percentage", () => segment.DrawPercentage(Percentage, LineInfo), ParamId.Thickness, ParamId.Percentage),
            new("Draw Scaled", () => segment.DrawScaled(LineInfo, Scale, Origin), ParamId.Thickness, ParamId.Scale, ParamId.Origin),
            new("Draw Glow", () => segment.DrawGlow(Width, EndWidth, DrawColor, DrawColor.SetAlpha(0), Steps, LineCapType.CappedExtended, 4), ParamId.Width, ParamId.EndWidth, ParamId.Steps),
            new("Draw Vertices", () => { segment.Draw(LineInfo); segment.DrawVertices(VertexRadius, DrawColor, Smoothness); }, ParamId.Thickness, ParamId.VertexRadius, ParamId.Smoothness),
        ];

        cases[ShapeType.Circle] =
        [
            new("Draw", () => circle.Draw(DrawColor, Smoothness), ParamId.Smoothness),
            new("Draw Fast", () => circle.DrawFast(DrawColor)),
            new("Draw Scaled", () => circle.DrawScaled(0f, DrawColor, Smoothness, Scale, Origin), ParamId.Smoothness, ParamId.Scale, ParamId.Origin),
            new("Draw Percentage", () => circle.DrawPercentage(Percentage, 0f, DrawColor, Smoothness), ParamId.Smoothness, ParamId.Percentage),
            new("Draw Lines", () => circle.DrawLines(LineInfo, Smoothness), ParamId.Thickness, ParamId.Smoothness),
            new("Draw Lines Percentage", () => circle.DrawLinesPercentage(Percentage, 0f, LineInfo, Smoothness), ParamId.Thickness, ParamId.Smoothness, ParamId.Percentage),
            new("Draw Sector", () => circle.DrawSector(StartAngleDeg, EndAngleDeg, 0f, DrawColor, Smoothness), ParamId.StartAngleDeg, ParamId.EndAngleDeg, ParamId.Smoothness),
            new("Draw Sector Scaled", () => circle.DrawSectorScaled(StartAngleDeg, EndAngleDeg, 0f, DrawColor, Smoothness, Scale, Origin), ParamId.StartAngleDeg, ParamId.EndAngleDeg, ParamId.Smoothness, ParamId.Scale, ParamId.Origin),
            new("Draw Sector Lines", () => circle.DrawSectorLines(StartAngleDeg, EndAngleDeg, 0f, LineInfo, Smoothness), ParamId.StartAngleDeg, ParamId.EndAngleDeg, ParamId.Thickness, ParamId.Smoothness),
            new("Draw Sector Lines Closed", () => circle.DrawSectorLinesClosed(StartAngleDeg, EndAngleDeg, 0f, LineInfo, Smoothness), ParamId.StartAngleDeg, ParamId.EndAngleDeg, ParamId.Thickness, ParamId.Smoothness),
            new("Draw Ring Lines", () => circle.DrawRingLines(RingThickness, 0f, LineInfo, Smoothness), ParamId.RingThickness, ParamId.Thickness, ParamId.Smoothness),
            new("Draw Ring Lines Percentage", () => circle.DrawRingLinesPercentage(RingThickness, Percentage, 0f, LineInfo, Smoothness), ParamId.RingThickness, ParamId.Thickness, ParamId.Smoothness, ParamId.Percentage),
            new("Draw Ring Sector Lines", () => circle.DrawRingSectorLines(RingThickness, StartAngleDeg, EndAngleDeg, 0f, LineInfo, Smoothness), ParamId.RingThickness, ParamId.StartAngleDeg, ParamId.EndAngleDeg, ParamId.Thickness, ParamId.Smoothness),
        ];

        cases[ShapeType.Triangle] =
        [
            new("Draw", () => triangle.Draw(DrawColor)),
            new("Draw Scaled", () => triangle.DrawScaled(DrawColor, Scale, Origin, DrawType), ParamId.Scale, ParamId.Origin, ParamId.DrawType),
            new("Draw Lines", () => triangle.DrawLines(LineThickness, DrawColor, MiterLimit), ParamId.Thickness, ParamId.MiterLimit),
            new("Draw Lines Percentage", () => triangle.DrawLinesPercentage(Percentage, StartIndex, LineThickness, DrawColor, MiterLimit), ParamId.Thickness, ParamId.Percentage, ParamId.StartIndex, ParamId.MiterLimit),
            new("Draw Corners", () => triangle.DrawCorners(LineThickness, DrawColor, CornerLength, MiterLimit), ParamId.Thickness, ParamId.CornerLength, ParamId.MiterLimit),
            new("Draw Corners Relative", () => triangle.DrawCornersRelative(LineThickness, DrawColor, CornerFactor, MiterLimit), ParamId.Thickness, ParamId.CornerFactor, ParamId.MiterLimit),
            new("Draw Vertices", () =>
            {
                triangle.DrawLines(LineThickness, DrawColor, MiterLimit);
                triangle.DrawVertices(VertexRadius, DrawColor, Smoothness);
            }, ParamId.Thickness, ParamId.VertexRadius, ParamId.Smoothness, ParamId.MiterLimit),
        ];

        cases[ShapeType.Rect] =
        [
            new("Draw", () => rect.Draw(DrawColor)),
            new("Draw Rounded", () => rect.DrawRounded(DrawColor, Roundness, Segments), ParamId.Roundness, ParamId.Segments),
            new("Draw Scaled", () => rect.DrawScaled(DrawColor, Scale, Origin, DrawType), ParamId.Scale, ParamId.Origin, ParamId.DrawType),
            new("Draw Lines", () => rect.DrawLines(LineInfo), ParamId.Thickness),
            new("Draw Lines Rounded", () => rect.DrawLinesRounded(LineThickness, DrawColor, Roundness, Segments), ParamId.Thickness, ParamId.Roundness, ParamId.Segments),
            new("Draw Lines Percentage", () => rect.DrawLinesPercentage(Percentage, StartIndex, LineInfo), ParamId.Thickness, ParamId.Percentage, ParamId.StartIndex),
            new("Draw Corners", () => rect.DrawCorners(LineThickness, DrawColor, CornerLength), ParamId.Thickness, ParamId.CornerLength),
            new("Draw Corners Relative", () => rect.DrawCornersRelative(LineThickness, DrawColor, CornerFactor), ParamId.Thickness, ParamId.CornerFactor),
            new("Draw Chamfered Corners", () => rect.DrawChamferedCorners(DrawColor, CornerLength), ParamId.CornerLength),
            new("Draw Chamfered Corners Relative", () => rect.DrawChamferedCornersRelative(DrawColor, CornerFactor), ParamId.CornerFactor),
            new("Draw Chamfered Corners Lines", () => rect.DrawChamferedCornersLines(LineThickness, DrawColor, CornerLength), ParamId.Thickness, ParamId.CornerLength),
            new("Draw Chamfered Corners Lines Relative", () => rect.DrawChamferedCornersLinesRelative(LineThickness, DrawColor, CornerFactor), ParamId.Thickness, ParamId.CornerFactor),
        ];

        cases[ShapeType.Quad] =
        [
            new("Draw", () => quad.Draw(DrawColor)),
            new("Draw Scaled", () => quad.DrawScaled(DrawColor, Scale, Origin, DrawType), ParamId.Scale, ParamId.Origin, ParamId.DrawType),
            new("Draw Lines", () => quad.DrawLines(LineInfo), ParamId.Thickness),
            new("Draw Lines Percentage", () => quad.DrawLinesPercentage(Percentage, StartIndex, LineInfo), ParamId.Thickness, ParamId.Percentage, ParamId.StartIndex),
            new("Draw Vignette", () => quad.DrawVignette(CornerLength, RotationDeg, DrawColor, Smoothness), ParamId.CornerLength, ParamId.Smoothness),
            new("Draw Corners", () => quad.DrawCorners(LineThickness, DrawColor, CornerLength), ParamId.Thickness, ParamId.CornerLength),
            new("Draw Corners Relative", () => quad.DrawCornersRelative(LineThickness, DrawColor, CornerFactor), ParamId.Thickness, ParamId.CornerFactor),
            new("Draw Chamfered Corners", () => quad.DrawChamferedCorners(DrawColor, CornerLength), ParamId.CornerLength),
            new("Draw Chamfered Corners Relative", () => quad.DrawChamferedCornersRelative(DrawColor, CornerFactor), ParamId.CornerFactor),
            new("Draw Chamfered Corners Lines", () => quad.DrawChamferedCornersLines(LineThickness, DrawColor, CornerLength), ParamId.Thickness, ParamId.CornerLength),
            new("Draw Chamfered Corners Lines Relative", () => quad.DrawChamferedCornersLinesRelative(LineThickness, DrawColor, CornerFactor), ParamId.Thickness, ParamId.CornerFactor),
        ];

        cases[ShapeType.Polygon] =
        [
            new("Draw", () => polygon.Draw(DrawColor)),
            new("Draw Polygon Convex", () => polygon.DrawPolygonConvex(DrawColor)),
            new("Draw Lines", () => polygon.DrawLines(LineThickness, DrawColor, MiterLimit), ParamId.Thickness, ParamId.MiterLimit),
            new("Draw Lines Convex", () => polygon.DrawLinesConvex(LineThickness, DrawColor, MiterLimit), ParamId.Thickness, ParamId.MiterLimit),
            new("Draw Lines Perimeter", () => polygon.DrawLinesPerimeter(polygon.GetPerimeter() * Percentage, StartIndex, LineThickness, DrawColor, MiterLimit), ParamId.Thickness, ParamId.Percentage, ParamId.StartIndex, ParamId.MiterLimit),
            new("Draw Lines Percentage", () => polygon.DrawLinesPercentage(Percentage, StartIndex, LineThickness, DrawColor, MiterLimit), ParamId.Thickness, ParamId.Percentage, ParamId.StartIndex, ParamId.MiterLimit),
            new("Draw Lines Perimeter Capped", () => polygon.DrawLinesPerimeterCapped(polygon.GetPerimeter() * Percentage, StartIndex, LineThickness, DrawColor, LineCapType.CappedExtended, 4), ParamId.Thickness, ParamId.Percentage, ParamId.StartIndex),
            new("Draw Lines Percentage Capped", () => polygon.DrawLinesPercentageCapped(Percentage, StartIndex, LineInfo), ParamId.Thickness, ParamId.Percentage, ParamId.StartIndex),
            new("Draw Cornered Absolute Transparent", () => polygon.DrawCorneredAbsoluteTransparent(CornerLength, LineInfo, MiterLimit), ParamId.Thickness, ParamId.CornerLength, ParamId.MiterLimit),
            new("Draw Cornered Relative Transparent", () => polygon.DrawCorneredRelativeTransparent(CornerFactor, LineInfo, MiterLimit), ParamId.Thickness, ParamId.CornerFactor, ParamId.MiterLimit),
            new("Draw Cornered", () => polygon.DrawCornered(CornerLength, LineInfo), ParamId.Thickness, ParamId.CornerLength),
        ];

        cases[ShapeType.Polyline] =
        [
            new("Draw", () => polyline.Draw(LineInfo), ParamId.Thickness),
            new("Draw Perimeter", () => polyline.DrawPerimeter(polyline.GetLength() * Percentage, LineInfo), ParamId.Thickness, ParamId.Percentage),
            new("Draw Percentage", () => polyline.DrawPercentage(Percentage, LineInfo), ParamId.Thickness, ParamId.Percentage),
            new("Draw Glow", () => polyline.DrawGlow(Width, EndWidth, DrawColor, DrawColor.SetAlpha(0), Steps, LineCapType.CappedExtended, 4), ParamId.Width, ParamId.EndWidth, ParamId.Steps),
        ];
    }

    private void SyncMethodSlider()
    {
        var list = cases[GetCurrentShape()];
        methodSlider.MaxValue = Math.Max(0, list.Count - 1);
        if (methodSlider.CurValue > methodSlider.MaxValue) methodSlider.SetCurValue(methodSlider.MaxValue);
    }

    private int GetCurrentShapeIndex() => ShapeMath.Clamp((int)MathF.Round(shapeSlider.CurValue), 0, Enum.GetValues<ShapeType>().Length - 1);
    private int GetCurrentMethodIndex()
    {
        var shape = GetCurrentShape();
        return ShapeMath.Clamp((int)MathF.Round(methodSlider.CurValue), 0, cases[shape].Count - 1);
    }
    private ShapeType GetCurrentShape() => (ShapeType)GetCurrentShapeIndex();
    private DrawingCase GetCurrentCase() => cases[GetCurrentShape()][GetCurrentMethodIndex()];
    private bool UsesRotationSlider() => GetCurrentShape() is ShapeType.Segment or ShapeType.Quad or ShapeType.Polygon;

    private float LineThickness => paramSliders[ParamId.Thickness].CurValue;
    private float Smoothness => paramSliders[ParamId.Smoothness].CurValue;
    private float Percentage => paramSliders[ParamId.Percentage].CurValue;
    private float Scale => paramSliders[ParamId.Scale].CurValue;
    private float Origin => paramSliders[ParamId.Origin].CurValue;
    private int StartIndex => (int)paramSliders[ParamId.StartIndex].CurValue;
    private float CornerLength => paramSliders[ParamId.CornerLength].CurValue;
    private float CornerFactor => paramSliders[ParamId.CornerFactor].CurValue;
    private float RotationDeg => paramSliders[ParamId.RotationDeg].CurValue;
    private float Roundness => paramSliders[ParamId.Roundness].CurValue;
    private int Segments => (int)paramSliders[ParamId.Segments].CurValue;
    private float RingThickness => paramSliders[ParamId.RingThickness].CurValue;
    private float StartAngleDeg => paramSliders[ParamId.StartAngleDeg].CurValue;
    private float EndAngleDeg => paramSliders[ParamId.EndAngleDeg].CurValue;
    private int DrawType => (int)paramSliders[ParamId.DrawType].CurValue;
    private float Width => paramSliders[ParamId.Width].CurValue;
    private float EndWidth => paramSliders[ParamId.EndWidth].CurValue;
    private int Steps => (int)paramSliders[ParamId.Steps].CurValue;
    private float VertexRadius => paramSliders[ParamId.VertexRadius].CurValue;
    private float MiterLimit => paramSliders[ParamId.MiterLimit].CurValue;

    private LineDrawingInfo LineInfo => new(LineThickness, DrawColor, LineCapType.CappedExtended, 4);
}
