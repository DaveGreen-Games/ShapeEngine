using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{
    internal class Wall : SegmentCollider
    {

    }
    internal class Rock : Gameobject
    {

    }

    public class AreaExample : ExampleScene
    {
        Area area;
        
        ScreenTexture game;
        BasicCamera cam;

        Rect boundaryRect;

        Font font;



        public AreaExample()
        {
            Title = "Area Example";
            game = GAMELOOP.Game;
            cam = new BasicCamera(new Vector2(0f), new Vector2(1920, 1080), new Vector2(0.5f), 1f, 0f);
            game.SetCamera(cam);

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            var cameraRect = cam.GetArea();
            boundaryRect = SRect.ApplyMarginsAbsolute(cameraRect, 25f, 25f, 75 * 2f, 75 * 2f);
            //boundaryRect.FlippedNormals = true;
            //boundary = boundaryRect.GetEdges();

            area = new(boundaryRect, 10, 10);

            //add walls
        }
        public override void Reset()
        {
            
        }
        public override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);
            //area.HandleInput(dt, mousePosGame, mousePosUI);
            //if (IsKeyPressed(KeyboardKey.KEY_SPACE)) Shoot();
        
        }

        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.Update(dt, mousePosGame, mousePosUI);
            area.Update(dt, mousePosGame, mousePosUI);
            
            
        }

        public override void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.Draw(gameSize, mousePosGame);

            //boundary.Draw(4f, ColorMedium);
            //foreach (var seg in boundary)
            //{
            //    Segment normal = new(seg.Center, seg.Center + seg.n * 25f);
            //    normal.Draw(2f, BLUE);
            //}

            area.Draw(gameSize, mousePosGame);

        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            base.DrawUI(uiSize, mousePosUI);

            area.DrawUI(uiSize, mousePosUI);

            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            string infoText = String.Format("[LMB] Add Segment | [RMB] Cancel Segment | [Space] Shoot");
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
    }
}
