using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Lib;
using Color = System.Drawing.Color;
namespace Examples.Scenes.ExampleScenes;




public class ClosestDistanceExample : ExampleScene
{
    private const float LineThickness = 4f;
    
    private abstract class Shape
    {
        public abstract void Move(Vector2 newPosition);
        public abstract void Draw(ColorRgba color);
        public abstract ShapeType GetShapeType();
    }

    private class PointShape : Shape
    {
        private Vector2 position;
        private float size;
        public PointShape(Vector2 pos, float size)
        {
            this.position = pos;
            this.size = size;
        }
        public override void Move(Vector2 newPosition)
        {
            position = newPosition;
        }

        public override void Draw(ColorRgba color)
        {
            position.Draw(size, color, 16);
        }

        public override ShapeType GetShapeType() => ShapeType.None;
    }
    private class SegmentShape : Shape
    {
        private Segment segment;
        private Vector2 position;
        public SegmentShape(Vector2 pos, float size)
        {
            position = pos;
            var randAngle = ShapeRandom.RandAngleRad();
            var offset = new Vector2(size, 0f).Rotate(randAngle);
            var start = pos - offset;
            var end = pos + offset;
            segment = new(start, end);
        }
        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - position;
            segment.Move(offset);
            position = newPosition;
        }

        public override void Draw(ColorRgba color)
        {
            segment.Draw(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Segment;
    }
    private class CircleShape : Shape
    {
        private Circle circle;
        public CircleShape(Vector2 pos, float size)
        {
            circle = new(pos, size);
        }
        public override void Move(Vector2 newPosition)
        {
            circle = new(newPosition, circle.Radius);
        }

        public override void Draw(ColorRgba color)
        {
            circle.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Circle;
    }
    private class TriangleShape : Shape
    {
        private Vector2 position;
        private Triangle shape;

        public TriangleShape(Vector2 pos, float size)
        {
            position = pos;
            var randAngle = ShapeRandom.RandAngleRad();
            var a = pos + new Vector2(size * ShapeRandom.RandF(0.5f, 1f), size * ShapeRandom.RandF(-0.5f, 0.5f)).Rotate(randAngle);
            var b = pos + new Vector2(-size * ShapeRandom.RandF(0.5f, 1f), -size * ShapeRandom.RandF(0.5f, 1f)).Rotate(randAngle);
            var c = pos + new Vector2(-size * ShapeRandom.RandF(0.5f, 1f), size * ShapeRandom.RandF(0.5f, 1f)).Rotate(randAngle);
            shape = new(a, b, c);
        }

        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - position;
            shape = shape.Move(offset);
            position = newPosition;
        }

        public override void Draw(ColorRgba color)
        {
            shape.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Triangle;
    }
    private class QuadShape : Shape
    {
        private Quad quad;
        public QuadShape(Vector2 pos, float size)
        {
            var randAngle = ShapeRandom.RandAngleRad();
            quad = new(pos, new Vector2(size), randAngle, new Vector2(0.5f));
        }
        public override void Move(Vector2 newPosition)
        {
            quad = quad.MoveTo(newPosition, new Vector2(0.5f));
        }

        public override void Draw(ColorRgba color)
        {
           quad.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Quad;
    }
    private class RectShape : Shape
    {
        private Rect rect;

        public RectShape(Vector2 pos, float size)
        {
            rect = new(pos, new(size, size), new Vector2(0.5f));
        }
        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - rect.Center;
            rect = rect.Move(offset);
        }

        public override void Draw(ColorRgba color)
        {
            rect.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Rect;
    }
    private class PolygonShape : Shape
    {
        private Vector2 position;
        private readonly Polygon polygon;

        public PolygonShape(Vector2 pos, float size)
        {
            polygon = Polygon.Generate(pos, ShapeRandom.RandI(8, 16), size / 2, size);
            position = pos;
        }
        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - position;
            polygon.MoveSelf(offset);
            position = newPosition;
        }

        public override void Draw(ColorRgba color)
        {
            polygon.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Poly;
    }
    private class PolylineShape : Shape
    {
        private Vector2 position;
        private readonly Polyline polyline;

        public PolylineShape(Vector2 pos, float size)
        {
            
            polyline = Polygon.Generate(pos, ShapeRandom.RandI(8, 16), size / 2, size).ToPolyline();
            position = pos;
        }
        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - position;
            for (var i = 0; i < polyline.Count; i++)
            {
                var p = polyline[i];
                polyline[i] = p + offset;
            }
            position = newPosition;
        }

        public override void Draw(ColorRgba color)
        {
            polyline.Draw(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.PolyLine;
    }
    
    private InputAction nextStaticShape;
    private InputAction nextMovingShape;
    // private InputAction changeOffset;

    private Shape staticShape;
    private Shape movingShape;

    
    public ClosestDistanceExample()
    {
        Title = "Closest Distance Example";

        var nextStaticShapeMb = new InputTypeMouseButton(ShapeMouseButton.LEFT);
        var nextStaticShapeGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
        var nextStaticShapeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
        nextStaticShape = new(nextStaticShapeMb, nextStaticShapeGp, nextStaticShapeKb);
        
        var nextMovingShapeMb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
        var nextMovingShapeGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
        var nextMovingShapeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
        nextMovingShape = new(nextMovingShapeMb, nextMovingShapeGp, nextMovingShapeKb);
        
        // var offsetMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f);
        // var offsetKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.S, ShapeKeyboardButton.W);
        // var offsetGP = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_DOWN, ShapeGamepadButton.LEFT_FACE_UP);
        // changeOffset = new(offsetMW, offsetGP, offsetKB);
        
        textFont.FontSpacing = 1f;
        textFont.ColorRgba = Colors.Light;

        staticShape = CreateShape(new(), 150, ShapeType.Triangle);
        movingShape = CreateShape(new(), 50, ShapeType.Triangle);

    }
    public override void Reset()
    {
        
    }
    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
    {
        base.HandleInput(dt, mousePosGame, mousePosUI);
        var gamepad = GAMELOOP.CurGamepad;
        
        nextStaticShape.Gamepad = gamepad;
        nextStaticShape.Update(dt);
        
        nextMovingShape.Gamepad = gamepad;
        nextMovingShape.Update(dt);
        
        // changeOffset.Gamepad = gamepad;
        // changeOffset.Update(dt);

        
        if (nextStaticShape.State.Pressed)
        {
            NextStaticShape();   
        }
        if (nextMovingShape.State.Pressed)
        {
            NextMovingShape(mousePosGame);   
        }
        movingShape.Move(mousePosGame);
    }
    protected override void OnDrawGameExample(ScreenInfo game)
    {
        staticShape.Draw(new ColorRgba(Color.Lime));
        movingShape.Draw(new ColorRgba(Color.Red));
    }
    protected override void OnDrawGameUIExample(ScreenInfo ui)
    {
        
    }
    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        // var curDevice = ShapeInput.CurrentInputDeviceType;
        // var create = createPoint. GetInputTypeDescription( curDevice, true, 1, false); 
        // var delete = deletePoint. GetInputTypeDescription( curDevice, true, 1, false); 
        // var offset = changeOffset.GetInputTypeDescription( curDevice , true, 1, false);
        // Rect bottomCenter = GAMELOOP.UIRects.GetRect("bottom center");
        // string infoText =
            // $"Add Point {create} | Remove Point {delete} | Inflate {offset} {MathF.Round(offsetDelta * 100) / 100}";

        // textFont.ColorRgba = Colors.Light;
        // textFont.DrawTextWrapNone(infoText, bottomCenter, new(0.5f));
    }

    private void NextStaticShape(float size = 150f)
    {
        switch (staticShape.GetShapeType())
        {
            case ShapeType.None: staticShape = CreateShape(new(), size, ShapeType.Segment); //point
                break;
            case ShapeType.Segment: staticShape = CreateShape(new(), size, ShapeType.Circle);
                break;
            case ShapeType.Circle: staticShape = CreateShape(new(), size, ShapeType.Triangle);
                break;
            case ShapeType.Triangle: staticShape = CreateShape(new(), size, ShapeType.Quad);
                break;
            case ShapeType.Quad: staticShape = CreateShape(new(), size, ShapeType.Rect);
                break;
            case ShapeType.Rect: staticShape = CreateShape(new(), size, ShapeType.Poly);
                break;
            case ShapeType.Poly: staticShape = CreateShape(new(), size, ShapeType.PolyLine);
                break;
            case ShapeType.PolyLine: staticShape = CreateShape(new(), size / 4, ShapeType.None);
                break;
        }
    }

    private void NextMovingShape(Vector2 pos, float size = 150f)
    {
        switch (movingShape.GetShapeType())
        {
            case ShapeType.None: movingShape = CreateShape(pos, size, ShapeType.Segment); //point
                break;
            case ShapeType.Segment: movingShape = CreateShape(pos, size, ShapeType.Circle);
                break;
            case ShapeType.Circle: movingShape = CreateShape(pos, size, ShapeType.Triangle);
                break;
            case ShapeType.Triangle: movingShape = CreateShape(pos, size, ShapeType.Quad);
                break;
            case ShapeType.Quad: movingShape = CreateShape(pos, size, ShapeType.Rect);
                break;
            case ShapeType.Rect: movingShape = CreateShape(pos, size, ShapeType.Poly);
                break;
            case ShapeType.Poly: movingShape = CreateShape(pos, size, ShapeType.PolyLine);
                break;
            case ShapeType.PolyLine: movingShape = CreateShape(pos, size / 4, ShapeType.None);
                break;
        }
    }
    private Shape CreateShape(Vector2 pos, float size, ShapeType type)
    {
        switch (type)
        {
            case ShapeType.None: return new PointShape(pos, size);
            case ShapeType.Circle: return new CircleShape(pos, size);
            case ShapeType.Segment: return new SegmentShape(pos, size);
            case ShapeType.Triangle: return new TriangleShape(pos, size);
            case ShapeType.Quad: return new QuadShape(pos, size);
            case ShapeType.Rect: return new RectShape(pos, size);
            case ShapeType.Poly: return new PolygonShape(pos, size);
            case ShapeType.PolyLine: return new PolylineShape(pos, size);
        }
        
        return new PointShape(pos, size);
    }
    
}


