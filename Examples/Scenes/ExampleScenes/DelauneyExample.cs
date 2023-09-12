

using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{
    public class DelauneyExample : ExampleScene
    {

        private Font font;

        Points points = new();
        Triangulation curTriangulation = new();

        public DelauneyExample()
        {
            Title = "Delaunay Example";
            font = GAMELOOP.GetFont(FontIDs.JetBrains);
        }
        public override void Reset()
        {
            points.Clear();
            curTriangulation.Clear();
        }
        public override Area? GetCurArea()
        {
            return null;
        }

        

        public override void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui)
        {
            
            base.Update(dt, mousePosScreen, game, ui); //calls area update therefore area bounds have to be updated before that
        }
        private Triangle GenerateTriangle(Vector2 pos, float size)
        {
            var poly = Polygon.Generate(pos, 3, size / 2, size);
            return new(poly[0], poly[1], poly[2]);
        }
        
        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);

            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) points.Add(mousePosGame);

            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                curTriangulation = Polygon.TriangulateDelaunay(points);
            }
        }



        public override void DrawGame(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.DrawGame(gameSize, mousePosGame);

            points.Draw(5f, PURPLE);
            curTriangulation.DrawLines(2f, YELLOW);
        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            base.DrawUI(uiSize, mousePosUI);

            //Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.07f), new Vector2(0.5f, 1f));
            //
            //string polymodeText = "[Tab] Polymode | [LMB] Place/Merge | [RMB] Cut | [1] Triangle | [2] Rect | [3] Poly | [Q] Regenerate | [X] Rotate | [C] Scale";
            //string laserText = "[Tab] Lasermode | [LMB] Move | [RMB] Shoot Laser";
            //
            //font.DrawText(text, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);


        }

    }

}
