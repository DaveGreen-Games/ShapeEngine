
using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;




namespace Examples.Scenes.ExampleScenes
{
    public class Bullet
    {
        const float collisionTime = 1f;

        public CircleCollider Collider { get; private set; }
        public IShape Shape { get { return Collider.GetShape(); } }


        float collisionTimer = -1f;

        Intersection lastIntersection = new();

        public Bullet(Vector2 pos, Vector2 vel, float r)
        {
            Collider = new(pos, r);
            Collider.ComputeCollision = true;
            Collider.ComputeIntersections = true;
            Collider.Vel = vel;
        }

        public void Collision(Intersection intersection)
        {
            Collider.Vel = Collider.Vel.Reflect(intersection.n);
            collisionTimer = collisionTime;
            lastIntersection = intersection;
        }
        public void Update(float dt)
        {
            Collider.UpdateState(dt);

            if (collisionTimer > 0f)
            {
                collisionTimer -= dt;
                if (collisionTimer <= 0f)
                {
                    collisionTimer = -1f;
                }
            }
        }
        public void Draw()
        {
            float colF = collisionTimer > 0f ? collisionTimer / collisionTime : 0f;
            Color color = STween.Tween(ExampleScene.ColorHighlight2, ExampleScene.ColorHighlight1, colF, TweenType.QUAD_IN);

            Collider.GetShape().DrawShape(4f, color);
            lastIntersection.Draw(2f, ExampleScene.ColorLight, ExampleScene.ColorLight);

            //DrawCircleV(Collider.Pos, 25, BLUE);
            DrawCircleV(Shape.GetCentroid(), 2, ExampleScene.ColorHighlight2);
        }
    }


    public class CCDExample : ExampleScene
    {
        ScreenTexture game;
        BasicCamera cam;

        Rect boundaryRect;
        Segments boundary = new();


        Segments segments = new();
        Vector2 startPoint = new();
        bool segmentStarted = false;
        List<Bullet> bullets = new();

        Vector2 muzzlePos = new();
        float bulletSpeed = 1000f;
        float bulletR = 15f;

        Font font;

        public CCDExample()
        {
            Title = "Continous Collision Detection Example";
            game = GAMELOOP.Game;
            cam = new BasicCamera(new Vector2(0f), new Vector2(1920, 1080), new Vector2(0.5f), 1f, 0f);
            game.SetCamera(cam);

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            var cameraRect = cam.GetArea();
            boundaryRect = SRect.ApplyMarginsAbsolute(cameraRect, 25f, 25f, 75 * 2f, 75 * 2f);
            boundaryRect.FlippedNormals = true;
            boundary = boundaryRect.GetEdges();
        }
        public override void Reset()
        {
            if (bullets.Count <= 0) segments.Clear();
            else bullets.Clear();
        }
        public override void HandleInput(float dt)
        {
            base.HandleInput(dt);

            if (IsKeyPressed(KeyboardKey.KEY_SPACE)) Shoot();
            if(IsKeyPressed(KeyboardKey.KEY_ONE)) IncreaseBulletSpeed();
            if(IsKeyReleased(KeyboardKey.KEY_TWO)) DecreaseBulletR();

        }

        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.Update(dt, mousePosGame, mousePosUI);

            muzzlePos = cam.GetArea().GetPoint(new Vector2(0.05f, 0.5f)); // game.GetSize() * new Vector2(0.1f, 0.5f);

            HandleSegments(mousePosGame);

            Segments allSegments = new();
            allSegments.AddRange(boundary);
            allSegments.AddRange(segments);

            foreach (var bullet in bullets)
            {
                bullet.Update(dt);
                var shape = bullet.Shape;
                foreach (var segment in allSegments)
                {
                    bool overlap = shape.Overlap(segment);
                    if (overlap)
                    {
                        var intersection = shape.Intersect(segment, bullet.Collider.Vel);
                        if (intersection.valid)
                        {
                            bullet.Collision(intersection);
                        }
                    }
                }
            }
        }
        public override void Draw(Vector2 gameSIze, Vector2 mousePosGame)
        {
            base.Draw(gameSIze, mousePosGame);

            boundary.Draw(4f, ColorMedium);
            foreach (var seg in boundary)
            {
                Segment normal = new(seg.Center, seg.Center + seg.n * 25f);
                normal.Draw(2f, BLUE);
            }

            DrawCircleV(muzzlePos, bulletR * 2f, ColorHighlight2);

            
            if (segmentStarted)
            {
                DrawCircleV(startPoint, 15f, ColorHighlight1);
                Segment s = new(startPoint, mousePosGame);
                s.Draw(4, ColorHighlight1);
                
            }
            segments.Draw(3f, ColorLight);

            
            foreach (var bullet in bullets)
            {
                bullet.Draw();
            }

        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            base.DrawUI(uiSize, mousePosUI);

            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            string infoText = String.Format("[LMB] Add Segment | [RMB] Cancel Segment | [Space] Shoot | [1] BulletSpeed: {0} | [2] Bullet R: {1}", bulletSpeed, bulletR);
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);

        }
        private void Shoot()
        {
            Vector2 mousePos = game.MousePos;
            Vector2 dir = (mousePos - muzzlePos).Normalize();

            Bullet b = new(muzzlePos, dir * bulletSpeed, bulletR);
            bullets.Add(b);
        }
        private void IncreaseBulletSpeed()
        {
            bulletSpeed += 100f;
            if (bulletSpeed > 5000f) bulletSpeed = 1000f;
        }
        private void DecreaseBulletR()
        {
            bulletR -= 5f;
            if (bulletR < 5f) bulletR = 30f;
        }
        private void HandleSegments(Vector2 mousePos)
        {
            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                if (segmentStarted)
                {
                    segmentStarted = false;
                    float lSq = (mousePos - startPoint).LengthSquared();
                    if(lSq > 400)
                    {
                        Segment s = new(startPoint, mousePos);
                        segments.Add(s);
                    }
                    
                }
                else
                {
                    startPoint = mousePos;
                    segmentStarted = true;
                }
            }
            else if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                if (segmentStarted)
                {
                    segmentStarted = false;
                }
            }


        }
    }
}
