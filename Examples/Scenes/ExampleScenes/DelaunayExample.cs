

using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{
    public class DelaunayExample : ExampleScene
    {
        private const float PointDistance = 10f;

        private Font font;

        Points points = new();
        Triangulation curTriangulation = new();

        int closePointIndex = -1;
        int closeTriangleIndex = -1;

        public DelaunayExample()
        {
            Title = "Delaunay Triangulation Example";
            font = GAMELOOP.GetFont(FontIDs.JetBrains);
            
        }
        public override void Reset()
        {
            points.Clear();
            curTriangulation.Clear();
        }
        public override GameObjectHandler? GetGameObjectHandler()
        {
            return null;
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
        
        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);

            float pointDistanceSquared = PointDistance * PointDistance;

            closePointIndex = -1;
            closeTriangleIndex = -1;

            //for (int i = 0; i < curTriangulation.Count; i++)
            //{
            //    var tri = curTriangulation[i];
            //    if (tri.IsPointInside(mousePosGame))
            //    {
            //        closeTriangleIndex = i;
            //        break;
            //    }
            //}


            var result = points.GetClosest(mousePosGame);
            if(result.Valid && result.DistanceSquared <= pointDistanceSquared)
            {
                //rmb deletes point
                closePointIndex = points.IndexOf(result.Closest.Point);
            }
            else
            {
                //rmb subdivides triangle
                var triangleResult = curTriangulation.GetClosest(mousePosGame);
                if (triangleResult.Valid)
                {
                    var triangle = triangleResult.Item; 
                    if(triangle.ContainsPoint(mousePosGame))
                    {
                        closeTriangleIndex = curTriangulation.IndexOf(triangle);
                    }
                }
            }

            //bool isNear = points.CloseTo(mousePosGame, 10f);

            

            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
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
                    Triangulate();
                }
                
            }
            else if(IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
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
        
        public override void DrawGame(ScreenInfo game)
        {
            base.DrawGame(game);

            
            for (int i = 0; i < curTriangulation.Count; i++)
            {
                var tri = curTriangulation[i];
                if (i == closeTriangleIndex) continue;
                tri.DrawLines(2f, WHITE);
                //{
                //    tri.DrawLines(4f, GREEN);
                //}
                //else
                //{
                //    tri.DrawLines(2f, YELLOW);
                //}
            }
            if(closeTriangleIndex >= 0) curTriangulation[closeTriangleIndex].DrawLines(6f, GREEN);

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                if (i == closePointIndex)
                {
                    p.Draw(PointDistance, GREEN);
                }
                else
                {
                    p.Draw(5f, PURPLE);
                }
            }
        }
        public override void DrawUI(ScreenInfo ui)
        {
            base.DrawUI(ui);
            Vector2 uiSize = ui.Area.Size;
            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.07f), new Vector2(0.5f, 1f));

            //string polymodeText = "[Tab] Polymode | [LMB] Place/Merge | [RMB] Cut | [1] Triangle | [2] Rect | [3] Poly | [Q] Regenerate | [X] Rotate | [C] Scale";
            //string laserText = "[Tab] Lasermode | [LMB] Move | [RMB] Shoot Laser";
            string text = String.Format("[LMB] Add Point / Remove Point | [RMB] Add 3 Points to Triangle");
            font.DrawText(text, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);


        }

    }

}
