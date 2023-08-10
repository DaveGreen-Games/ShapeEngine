﻿using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{

    public class Circ : IAreaObject
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public float Radius;
        int areaLayer = SRNG.randI(1, 5);
        Color color = RED;
        private bool deltaFactorApplied = false;
        public bool DrawToUI { get { return false; } set { } }
        public int AreaLayer { get { return areaLayer; } set { } }

        public Circ(Vector2 pos, Vector2 vel, float radius)
        {
            this.Pos = pos;
            this.Vel = vel;
            this.Radius = radius;
        }

        //public void Update(Rect boundary, float dt)
        //{
        //    
        //    Pos += Vel * dt;
        //
        //    if(Pos.X + Radius > boundary.Right)
        //    {
        //        Vel.X = -Vel.X;
        //        Pos.X = boundary.Right - Radius;
        //    }
        //    else if(Pos.X - Radius < boundary.Left)
        //    {
        //        Pos.X = boundary.Left + Radius;
        //        Vel.X = -Vel.X;
        //    }
        //
        //    if (Pos.Y + Radius > boundary.Bottom)
        //    {
        //        Vel.Y = -Vel.Y;
        //        Pos.Y = boundary.Bottom - Radius;
        //    }
        //    else if (Pos.Y - Radius < boundary.Top)
        //    {
        //        Pos.Y = boundary.Top + Radius;
        //        Vel.Y = -Vel.Y;
        //    }
        //}
        public void Draw()
        {
            DrawCircleSector(Pos, Radius, 0, 360, 12, RED);
        }


        public void AddedToArea(IArea area)
        {
        }

        public void RemovedFromArea(IArea area)
        {
        }
        public Vector2 GetCameraFollowPosition(Vector2 camPos)
        {
            return Pos;
        }

        public Vector2 GetPosition()
        {
            return Pos;
        }

        public Rect GetBoundingBox()
        {
            return new Rect(Pos, new Vector2(Radius) * 2, new Vector2(0.5f));
        }

        public void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            deltaFactorApplied = false;
            Pos += Vel * dt;
        }

        public void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            Color c = color;
            float r = Radius;
            if (deltaFactorApplied)
            {
                c = PURPLE;
                r *= 2f;
            }
            SDrawing.DrawCircleFast(Pos, Radius, c);
        }

        public void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            
        }

        public bool Kill()
        {
            return false;
        }

        public bool IsDead()
        {
            return false;
        }
        public bool CheckAreaBounds()
        {
            return true;
        }

        public void LeftAreaBounds(Vector2 safePosition, CollisionPoints collisionPoints)
        {
            Pos = safePosition;
            foreach (var c in collisionPoints)
            {
                if (c.Normal.X != 0f) Vel.X *= -1;
                else Vel.Y *= -1;
            }
        }

        public void DeltaFactorApplied(float f)
        {
            deltaFactorApplied = true;
        }
    }
    public class BouncyCircles : ExampleScene
    {
        ScreenTexture game;
        BasicCamera cam;

        Rect boundaryRect;

        Font font;

        //List<Circ> circles = new();
        IArea area;

        public BouncyCircles()
        {
            Title = "Bouncy Circles";
            game = GAMELOOP.Game;
            cam = new BasicCamera(new Vector2(0f), new Vector2(0), new Vector2(0.5f), 1f, 0f);
            game.SetCamera(cam);

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            boundaryRect = cam.GetArea().ApplyMargins(0.025f, 0.025f, 0.1f, 0.1f);

            //area = new AreaTest(boundaryRect, 2, 2);
            area = new AreaCollision(boundaryRect, 2, 2);
        }
        public override void Reset()
        {
            //circles.Clear();
            area.Clear();
        }
        public override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);

            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                for (int i = 0; i < 2500; i++)
                {
                    Vector2 randPos = mousePosGame + SRNG.randVec2(0, 250);
                    Vector2 vel = SRNG.randVec2(100, 200);
                    Circ c = new(randPos, vel, 2);
                    //circles.Add(c);
                    area.AddAreaObject(c);
                }
            }

            if (IsKeyPressed(KeyboardKey.KEY_ONE))
            {
                float slowFactor = 0.2f;
                int[] layerMask = new int[] { 1, 3};
                AreaDeltaFactor one = new(1f, slowFactor,              0.25f,  0f,         layerMask);
                AreaDeltaFactor two = new(slowFactor, slowFactor,      3f,     0.25f,      layerMask);
                AreaDeltaFactor three = new(slowFactor, 1f,            0.25f,  3.25f,      layerMask);
                area.AddDeltaFactor(one);
                area.AddDeltaFactor(two);
                area.AddDeltaFactor(three);
            }
        }

        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.Update(dt, mousePosGame, mousePosUI);

            //foreach (var c in circles)
            //{
            //    c.Update(boundaryRect, dt);
            //}

            area.Update(dt, mousePosGame, mousePosUI);
        }

        public override void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.Draw(gameSize, mousePosGame);
            boundaryRect.DrawLines(4f, ColorLight);
            //foreach (var c in circles)
            //{
            //    c.Draw();
            //}

            area.Draw(gameSize, mousePosGame);

        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            base.DrawUI(uiSize, mousePosUI);

            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.07f), new Vector2(0.5f, 1f));
            string infoText = String.Format("[LMB] Spawn | Object Count: {0}", area.Count);
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);

            area.DrawUI(uiSize, mousePosUI);
        }

    }
}
