using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{

    public class Circ
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public float Radius;
        //public Polygon relative;
        //public Polygon poly;
        public Circ(Vector2 pos, Vector2 vel, float radius)
        {
            this.Pos = pos;
            this.Vel = vel;
            this.Radius = radius;

            //poly = SPoly.Generate(pos, 12, Radius * 0.1f, Radius);
        }

        public void Update(Rect boundary, float dt)
        {
            Pos += Vel * dt;

            if(Pos.X + Radius > boundary.Right)
            {
                Vel.X = -Vel.X;
                Pos.X = boundary.Right - Radius;
            }
            else if(Pos.X - Radius < boundary.Left)
            {
                Pos.X = boundary.Left + Radius;
                Vel.X = -Vel.X;
            }

            if (Pos.Y + Radius > boundary.Bottom)
            {
                Vel.Y = -Vel.Y;
                Pos.Y = boundary.Bottom - Radius;
            }
            else if (Pos.Y - Radius < boundary.Top)
            {
                Pos.Y = boundary.Top + Radius;
                Vel.Y = -Vel.Y;
            }
        }
        public void Draw()
        {
            DrawCircleSector(Pos, Radius, 0, 360, 12, RED);
            //DrawPoly(Pos, 12, Radius, 0f, GREEN);
            //Circle circle = new(Pos, Radius);
            //circle.Draw(ExampleScene.ColorHighlight2);
            //circle.DrawLines(2f, ExampleScene.ColorHighlight2);
            //DrawCircleV(Pos, Radius, ExampleScene.ColorHighlight2);
            //poly.CenterSelf(Pos);

            //var poly = relative.GetShape(Pos, 0f, new Vector2(1f));
            //Polygon poly = SPoly.Generate(Pos, 12, Radius * 0.1f, Radius);
            //poly.DrawLines(1f, ExampleScene.ColorHighlight2);

            //Rect r = new(Pos, new Vector2(Radius), new Vector2(0.5f));
            //r.Draw(RED);
            //r.DrawLines(1f, RED);
            //r.ToPolygon().DrawLines(1f, RED);

            //DrawCircleSector(Pos, Radius, 0, 360, 8, RED);

            //Vector2 v1 = Pos + new Vector2(0, Radius);
            //Vector2 v2 = Pos + new Vector2(Radius, -Radius);
            //Vector2 v3 = Pos + new Vector2(-Radius, -Radius);
            //DrawTriangle(v1, v2, v3, RED);
        }
        //public void DrawCircle(Vector2 pos, float r, Color c, int pointCount)
        //{
        //    unsafe
        //    {
        //        Vector2[] points = new Vector2[pointCount + 1];
        //        float angleStep = RayMath.PI * 2.0f / pointCount;
        //        points[0] = pos;
        //        for (int i = 1; i < pointCount + 1; i++)
        //        {
        //            Vector2 p = SVec.Rotate(SVec.Right(), -angleStep * i) * r;
        //            points[i] = pos + p;
        //        }
        //
        //        var pointer = &points;
        //
        //        //DrawTriangleFan(points, pointCount, c);
        //        //Drawtr
        //    }
        //    
        //}
    }
    public class BouncyCircles : ExampleScene
    {
        ScreenTexture game;
        BasicCamera cam;

        Rect boundaryRect;

        Font font;

        List<Circ> circles = new();

        public BouncyCircles()
        {
            Title = "Bouncy Circles";
            game = GAMELOOP.Game;
            cam = new BasicCamera(new Vector2(0f), new Vector2(0), new Vector2(0.5f), 1f, 0f);
            game.SetCamera(cam);

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            boundaryRect = cam.GetArea().ApplyMargins(0.025f, 0.025f, 0.1f, 0.1f);

        }
        public override void Reset()
        {
            circles.Clear();
        }
        public override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);

            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                for (int i = 0; i < 2500; i++)
                {
                    Vector2 randPos = mousePosGame + SRNG.randVec2(0, 250);
                    Vector2 vel = SRNG.randVec2(50, 100);
                    Circ c = new(randPos, vel, 10);
                    circles.Add(c);
                }
            }
        }

        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.Update(dt, mousePosGame, mousePosUI);

            foreach (var c in circles)
            {
                c.Update(boundaryRect, dt);
            }
        }

        public override void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.Draw(gameSize, mousePosGame);
            boundaryRect.DrawLines(4f, ColorLight);
            foreach (var c in circles)
            {
                c.Draw();
            }

        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            base.DrawUI(uiSize, mousePosUI);

            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.07f), new Vector2(0.5f, 1f));
            string infoText = String.Format("[LMB] Spawn | Object Count: {0}", circles.Count);
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }

    }
}
