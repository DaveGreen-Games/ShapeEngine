using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;
using ShapeEngine.Lib.Drawing;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class PayloadKey
{
    
    public readonly KeyDirection Direction;
    
    public PayloadKey(KeyDirection direction)
    {
        this.Direction = direction;
    }
    
    public void Draw(Rect rect, ColorRgba color)
    {
        // rect = new Rect(rect.Center, new Size(minSize), new Vector2(0.5f));
        rect = rect.ApplyMargins(0.05f, 0.05f, 0.05f, 0.05f);
        var lineThickness = rect.Size.Min() / 18;
        rect.DrawLines(lineThickness, color);
        rect = rect.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
        
        
        if (Direction == KeyDirection.Up)
        {
            var a = rect.BottomSegment.GetPoint(0.25f);
            var b = rect.BottomSegment.GetPoint(0.75f);
            var c = rect.TopSegment.GetPoint(0.5f);
            SegmentDrawing.DrawSegment(a, c, lineThickness, color);
            SegmentDrawing.DrawSegment(b, c, lineThickness, color);
        }
        else if (Direction == KeyDirection.Right)
        {
            var a = rect.LeftSegment.GetPoint(0.25f);
            var b = rect.LeftSegment.GetPoint(0.75f);
            var c = rect.RightSegment.GetPoint(0.5f);
            SegmentDrawing.DrawSegment(a, c, lineThickness, color);
            SegmentDrawing.DrawSegment(b, c, lineThickness, color);
        }
        else if (Direction == KeyDirection.Down)
        {
            var a = rect.TopSegment.GetPoint(0.25f);
            var b = rect.TopSegment.GetPoint(0.75f);
            var c = rect.BottomSegment.GetPoint(0.5f);
            SegmentDrawing.DrawSegment(a, c, lineThickness, color);
            SegmentDrawing.DrawSegment(b, c, lineThickness, color);
        }
        else if (Direction == KeyDirection.Left)
        {
            var a = rect.RightSegment.GetPoint(0.25f);
            var b = rect.RightSegment.GetPoint(0.75f);
            var c = rect.LeftSegment.GetPoint(0.5f);
            SegmentDrawing.DrawSegment(a, c, lineThickness, color);
            SegmentDrawing.DrawSegment(b, c, lineThickness, color);
        }
    }
}