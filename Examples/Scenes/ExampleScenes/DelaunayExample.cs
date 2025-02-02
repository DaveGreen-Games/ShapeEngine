

using System.Diagnostics;
using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using System.Numerics;
using System.Text;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Lib.Drawing;
using Color = System.Drawing.Color;

namespace Examples.Scenes.ExampleScenes
{
    public class DelaunayExample : ExampleScene
    {
        //private const float PointDistance = 10f;

        private readonly Font font;

        private readonly Points points = new();
        private Triangulation curTriangulation = new();

        private int closePointIndex = -1;
        private int closeTriangleIndex = -1;

        private readonly InputAction iaAddPoint;
        private readonly InputAction iaAddMultiplePoints;

        private float lineThickness = 0f;
        private float lineThicknessBig = 0f;
        private float vertexSize = 0f;
        private float vertexSizeBig = 0f;
        
        public DelaunayExample()
        {
            Title = "Delaunay Triangulation Example";
            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            var addPointKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var addPointGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            var addPointMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            iaAddPoint = new(addPointKB, addPointGP, addPointMB);
            
            var addMultiplePointsKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            var addMultiplePointsGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            var addMultiplePointsMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaAddMultiplePoints = new(addMultiplePointsKB, addMultiplePointsGP, addMultiplePointsMB);
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
        }
        public override void Reset()
        {
            points.Clear();
            curTriangulation.Clear();
        }
        
        

        //public override void Update(float dt, ScreenInfo game, ScreenInfo ui)
        //{
        //    
        //    base.Update(dt, game, ui); //calls area update therefore area bounds have to be updated before that
        //}
        private Triangle GenerateTriangle(Vector2 pos, float size)
        {
            var poly = Polygon.Generate(pos, 3, size / 2, size);
            return new(poly[0], poly[1], poly[2]);
        }
        
        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
        {
            
            lineThickness = 2f * GAMELOOP.Camera.ZoomFactor;
            lineThicknessBig = lineThickness * 2f;
            vertexSize = lineThickness * 3f;
            vertexSizeBig = vertexSize * 2f;
            
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            var gamepad = GAMELOOP.CurGamepad;
            iaAddPoint.Gamepad = gamepad;
            iaAddPoint.Update(dt);
            
            iaAddMultiplePoints.Gamepad = gamepad;
            iaAddMultiplePoints.Update(dt);
            
            float pointDistanceSquared = vertexSizeBig * vertexSizeBig;

            closePointIndex = -1;
            closeTriangleIndex = -1;

            var result = points.GetClosestPoint(mousePosGame, out float closestDistanceSquared, out int index);
            
            if(closestDistanceSquared >= 0  && closestDistanceSquared  <= pointDistanceSquared)
            {
                //rmb deletes point
                closePointIndex = index;
            }
            else
            {
                //rmb subdivides triangle
                var triangleResult = curTriangulation.ContainsPoint(mousePosGame, out int triangleIndex);
                if (triangleResult)//.Valid)
                {
                    closeTriangleIndex = triangleIndex;
                }
            }

            //bool isNear = points.CloseTo(mousePosGame, 10f);

            

            if (iaAddPoint.State.Pressed)
            { 
                if(closePointIndex < 0)
                {
                    points.Add(mousePosGame);
                    Triangulate();
                }
                else
                {
                    points.RemoveAt(closePointIndex);
                    closePointIndex = -1;
                    if (points.Count < 3) curTriangulation = new();
                    else Triangulate();
                }
                
            }
            else if(iaAddMultiplePoints.State.Pressed)
            {
                if(closeTriangleIndex >= 0)
                {
                    Triangle t = curTriangulation[closeTriangleIndex];
                    points.AddRange(t.GetRandomPointsInside(3));
                    Triangulate();
                }
            }
        }

        private void Triangulate()
        {
            if (points.Count < 3) return;
            curTriangulation = Polygon.TriangulateDelaunay(points);

        }
        
        protected override void OnDrawGameExample(ScreenInfo game)
        {
            
            for (int i = 0; i < curTriangulation.Count; i++)
            {
                var tri = curTriangulation[i];
                if (i == closeTriangleIndex) continue;
                tri.DrawLines(lineThickness, Colors.Light, LineCapType.CappedExtended, 4);
            }
            
            
            if(closeTriangleIndex >= 0) curTriangulation[closeTriangleIndex].DrawLines(lineThicknessBig, Colors.Highlight, LineCapType.CappedExtended, 4);

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                if (i == closePointIndex)
                {
                    p.Draw(vertexSizeBig, Colors.Highlight);
                }
                else
                {
                    p.Draw(vertexSize, Colors.Special);
                }
            }
        }
        protected override void OnDrawGameUIExample(ScreenInfo gameUi)
        {
            
        }

        // protected override void DrawUIExample(ScreenInfo ui)
        // {
        //     Vector2 uiSize = ui.Area.Size;
        //     Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.07f), new Vector2(0.5f, 1f));
        //
        //     string text = String.Format("[LMB] Add Point / Remove Point | [RMB] Add 3 Points to Triangle");
        //     font.DrawText(text, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        // }
        
        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            var bottomCenter = GAMELOOP.UIRects.GetRect("bottom center");
            DrawInputText(bottomCenter);
        }

        private void DrawInputText(Rect rect)
        {
            var sb = new StringBuilder();
            var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
            
            string addPointText = iaAddPoint.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string addMultiplePointsText = iaAddMultiplePoints.GetInputTypeDescription(curInputDeviceAll, true, 1, false);

            if (curTriangulation.Count <= 0)
            {
                sb.Append($"Add Point{addPointText} | ");
                sb.Append($"Triangles {curTriangulation.Count}");
            }
            else
            {
                sb.Append($"Add/Remove Point {addPointText} | ");
                sb.Append($"Add Multiple Points {addMultiplePointsText} | ");
                sb.Append($"Triangles {curTriangulation.Count}");
            }
            
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(sb.ToString(), rect, new(0.5f));
            
            // font.DrawText(sb.ToString(), rect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
    }

}
