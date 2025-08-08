
using Raylib_cs;
using ShapeEngine.StaticLib;
using System.Numerics;
using System.Text;
using Clipper2Lib;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.Input;
using Color = System.Drawing.Color;

namespace Examples.Scenes.ExampleScenes;


public class PolygonHolesExample : ExampleScene
{

    private class PolygonWithHoles
    {
        public Polygon Polygon;
        public Triangulation Triangulation;
        public Polygons Holes;

        public PolygonWithHoles(Polygon polygon, Polygons holes)
        {
            this.Polygon = polygon;
            this.Holes = holes;
            List<Triangle> holeTriangles = new(holes.Count * 2);
            Points points = new();
            points.AddRange(polygon);
            foreach (var hole in holes)
            {
                points.AddRange(hole);
                holeTriangles.AddRange(hole.Triangulate());
            }

            var triangulation = Polygon.TriangulateDelaunay(points);
            for (int i = triangulation.Count - 1; i >= 0; i--)
            {
                var triangle = triangulation[i];
                foreach (var t in holeTriangles)
                {
                    if (triangle.OverlapShape(t))
                    {
                        triangulation.RemoveAt(i);
                        break;
                    }
                }
                
            }

            Triangulation = triangulation;

        }
        public PolygonWithHoles(Polygon polygon)
        {
            this.Polygon = polygon;
            this.Holes = new();
            this.Triangulation = polygon.Triangulate();

        }

        public bool Overlap(Polygon other)
        {
            foreach (var t in Triangulation)
            {
                if (t.OverlapShape(other)) return true;
            }

            return false;
        }

        public void Draw(ColorRgba color, ColorRgba outlineColor)
        {
            Triangulation.Draw(color);
            Polygon.DrawLines(2f, outlineColor);
            foreach (var hole in Holes)
            {
                hole.DrawLines(2f, outlineColor);
            }
        }
        
    }

    private Vector2 curPolygonPosition;
    private Polygon curPolygon;
    private Polygon main;
    private Triangulation triangulation;
    private readonly Polygons holes = new();
    
    private InputAction iaAddPolygon;
    private readonly InputActionTree inputActionTree;
    
    public PolygonHolesExample()
    {
        Title = "Polygon Holes Example";

        InputActionSettings defaultSettings = new();
        var addPolygonKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
        var addPolygonGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
        var addPolygonMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
        iaAddPolygon = new(defaultSettings,addPolygonKB, addPolygonGP, addPolygonMB);

        inputActionTree = [iaAddPolygon];
        
        textFont.FontSpacing = 1f;
        textFont.ColorRgba = Colors.Light;
        GenerateCurPolygon(new());

        var mainRect = new Rect(new Vector2(0f), new Size(1000), new AnchorPoint(0.5f));
        main = mainRect.ToPolygon();
        triangulation = main.Triangulate();
    }
    public override void Reset()
    {
        var mainRect = new Rect(new Vector2(0f), new Size(1000), new AnchorPoint(0.5f));
        main = mainRect.ToPolygon();
        triangulation = main.Triangulate();
        holes.Clear();
    }
    
    
    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi,  Vector2 mousePosUI)
    {
        var gamepad = ShapeInput.GamepadManager.LastUsedGamepad;
        inputActionTree.CurrentGamepad = gamepad;
        inputActionTree.Update(dt);

        if (iaAddPolygon.State.Pressed)
        {
            PlaceCurPolygon(mousePosGame);
        }
    }

    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        var translation = (game.MousePos - curPolygonPosition);
        curPolygonPosition = game.MousePos;
        curPolygon.ChangePosition(translation);
        // for (int i = 0; i < curPolygonTriangulation.Count; i++)
        // {
        //     curPolygonTriangulation[i] = curPolygonTriangulation[i].Move(translation);
        // }
    }

    protected override void OnDrawGameExample(ScreenInfo game)
    {
        // triangulation.Draw(Colors.Cold);
        foreach (var tri in triangulation)
        {
            tri.Draw(Colors.Medium);
            // tri.DrawLines(2f, Colors.Cold);
        }

        curPolygon.DrawLines(4f, Colors.Warm);
        foreach (var hole in holes)
        {
            hole.DrawLines(2f, Colors.Warm);
        }
        
        // main.DrawLines(6f, Colors.Medium);
        
        // main.DrawLines(6f, Colors.Medium);
        //
        //
        // var holeTriangulation = new Triangulation();
        // var points = new Points();
        // foreach (var p in difference)
        // {
        //     var poly = p.ToPolygon();
        //     points.AddRange(poly);
        //     if (p.IsHole())
        //     {
        //         poly.FixWindingOrder();
        //         holeTriangulation.AddRange(poly.Triangulate());
        //     }
        // }
        // holeTriangulation.Draw(Colors.Cold);
        // Triangulation triangulation = Polygon.TriangulateDelaunay(points);
        // triangulation.DrawLines(4f, Colors.Warm);
        // points.Draw(12, Colors.Light, 16);
        //
        // for (int i = triangulation.Count - 1; i >= 0; i--)
        // {
        //     var tri = triangulation[i];
        //     bool draw = true;
        //     foreach (var h in holeTriangulation)
        //     {
        //         if (Math.Abs(tri.GetArea() - h.GetArea()) < 1)
        //         {
        //             draw = false;
        //             break;
        //         }
        //     }
        //     if(draw) tri.Draw(Colors.Warm);
        // }
        //
        // // triangulation.Draw(Colors.Warm);
        //
        // curPolygon.DrawLines(5f, Colors.Highlight);
    }
    protected override void OnDrawGameUIExample(ScreenInfo gameUi)
    {
        
    }

    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        var bottomCenter = GAMELOOP.UIRects.GetRect("bottom center");
        DrawInputText(bottomCenter);
    }

    
    private void GenerateCurPolygon(Vector2 pos)
    {
        var shape = Polygon.Generate(pos, 12, 50, 100);
        if(shape == null) return;
        curPolygon = shape;
        curPolygonPosition = pos;
    }
    private void PlaceCurPolygon(Vector2 mousePos)
    {
        // intersection = Clipper.Intersect(main.ToClipperPaths(), curPolygon.ToClipperPaths(), FillRule.NonZero, 2);
        // difference = Clipper.Difference(main.ToClipperPaths(), curPolygon.ToClipperPaths(), FillRule.NonZero, 2);
        holes.Add(curPolygon);
        UpdateTriangulation();
        GenerateCurPolygon(mousePos);
    }

    private void UpdateTriangulation()
    {
        var points = main.ToPoints();
        foreach (var hole in holes)
        {
            foreach (var p in hole)
            {
                if(main.ContainsPoint(p)) points.Add(p);
            }
            // points.AddRange(hole);
        }
    
        var newTriangulation = Polygon.TriangulateDelaunay(points);
        triangulation.Clear();
        foreach (var tri in newTriangulation)
        {
            bool outside = true;
            foreach (var hole in holes)
            {
                // var count = 0;
                // if (hole.ContainsPoint(tri.A)) count++;
                // if (hole.ContainsPoint(tri.B)) count++;
                // if (hole.ContainsPoint(tri.C)) count++;
                if (hole.ContainsPoint(tri.GetCentroid()))
                {
                    outside = false;
                    break;
                }
            }
            if(outside) triangulation.Add(tri);
        }
        
    }
    
    private void DrawInputText(Rect rect)
    {
        var sb = new StringBuilder();
        var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
        
        string addPointText = iaAddPolygon.GetInputTypeDescription(curInputDeviceAll, true, 1, false);

        sb.Append($"Add Point{addPointText} | ");
        
        textFont.ColorRgba = Colors.Light;
        textFont.DrawTextWrapNone(sb.ToString(), rect, new(0.5f));
        
    }
}