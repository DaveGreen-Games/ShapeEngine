
using Raylib_CsLo;
using Raylib_CsLo.InternalHelpers;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;


namespace Examples.Scenes.ExampleScenes
{
    public class Bullet
    {
        const float collisionTime = 1f;

        
        private Vector2 prevPos = new();
        public CircleCollider Collider { get; private set; }
        public IShape Shape { get { return Collider.GetShape(); } }

        //Points prevPoints = new();

        float collisionTimer = -1f;

        Intersection lastIntersection = new();

        

        public Bullet(Vector2 pos, Vector2 vel, float r)
        {
            Collider = new(pos, r);
            Collider.ComputeCollision = true;
            Collider.ComputeIntersections = true;
            Collider.Vel = vel;
        }
        public void UpdatePrevPos()
        {
            prevPos = Collider.Pos;
        }
        public Vector2 GetPrevPos()
        {
            return prevPos;
        }
        public void Collision(Intersection intersection)
        {
            Collider.Vel = Collider.Vel.Reflect(intersection.CollisionSurface.Normal);
            collisionTimer = collisionTime;
            lastIntersection = intersection;
        }
        public void Update(float dt)
        {
            Collider.UpdateState(dt);
            //prevPoints.Add(Collider.PrevPos);
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
            Color color = ShapeTween.Tween(ExampleScene.ColorHighlight2, ExampleScene.ColorHighlight1, colF, TweenType.QUAD_IN);

            Collider.DrawShape(4f, color);
            lastIntersection.Draw(2f, ExampleScene.ColorLight, ExampleScene.ColorLight);

            //DrawCircleV(Collider.Pos, 25, BLUE);
            DrawCircleV(Shape.GetCentroid(), 2, ExampleScene.ColorHighlight2);

            //prevPoints.Draw(5f, RED);
        }
    }

    

    public class CCDExample : ExampleScene
    {
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

        bool CCD = true;

        public CCDExample()
        {
            Title = "Continous Collision Detection Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            boundaryRect = new Rect(new(0), new(1920, 1080), new(0.5f)).ApplyMargins(0.05f, 0.05f, 0.1f, 0.1f);
            boundaryRect.FlippedNormals = true;
            boundary = boundaryRect.GetEdges();
        }
        public override void Reset()
        {
            if (bullets.Count <= 0) segments.Clear();
            else bullets.Clear();
        }
        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            if (IsKeyPressed(KeyboardKey.KEY_SPACE)) Shoot();
            if(IsKeyPressed(KeyboardKey.KEY_ONE)) IncreaseBulletSpeed();
            if(IsKeyReleased(KeyboardKey.KEY_TWO)) DecreaseBulletR();
            if (IsKeyPressed(KeyboardKey.KEY_C)) CCD = !CCD;

        }

        protected override void UpdateExample(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            muzzlePos = GAMELOOP.Game.Area.GetPoint(new Vector2(0.05f, 0.5f)); // game.GetSize() * new Vector2(0.1f, 0.5f);

            HandleSegments(game.MousePos);

            Segments allSegments = new();
            allSegments.AddRange(boundary);
            allSegments.AddRange(segments);

            foreach (var bullet in bullets)
            {
                bullet.UpdatePrevPos();
                bullet.Update(dt);
                var shape = bullet.Shape;
                var collider = bullet.Collider;


                if (CCD)
                {
                    Vector2 prevPos = bullet.GetPrevPos();
                    Segment centerRay = new(prevPos, collider.Pos);
                    float r = shape.GetBoundingCircle().Radius;
                    float r2 = r + r;

                    List<Vector2> points = new();
                    foreach (var seg in allSegments)
                    {
                        //moved more than twice the shapes radius -> means gap between last & cur frame
                        if (centerRay.LengthSquared > r2 * r2)
                        {
                            var i = centerRay.Intersect(seg);
                            if (i.Valid)
                            {
                                foreach (var p in i)
                                {
                                    points.Add(p.Point);
                                }
                            }
                        }
                    }

                    if (points.Count > 0)
                    {
                        points.Sort
                        (
                            (a, b) =>
                            {
                                Vector2 pos = prevPos;
                                float la = (pos - a).LengthSquared();
                                float lb = (pos - b).LengthSquared();

                                if (la > lb) return 1;
                                else if (la == lb) return 0;
                                else return -1;
                            }
                        );

                        Vector2 closestPoint = points[0];
                        collider.Pos = closestPoint - centerRay.Dir * r;
                        shape = collider.GetShape();
                    }
                }
                
                foreach (var segment in allSegments)
                {
                    bool overlap = ShapeGeometry.Overlap(shape, segment);
                    if (overlap)
                    {
                        var intersection = ShapeGeometry.Intersect(shape, segment);
                        if (intersection.Valid)
                        {
                            bullet.Collision(new(intersection, bullet.Collider.Vel, bullet.Collider.Pos));
                        }
                    }
                }
            }
        }

        protected override void DrawGameExample(ScreenInfo game)
        {
            boundary.Draw(4f, ColorMedium);
            foreach (var seg in boundary)
            {
                Segment normal = new(seg.Center, seg.Center + seg.Normal * 25f);
                normal.Draw(2f, BLUE);
            }

            DrawCircleV(muzzlePos, bulletR * 2f, ColorHighlight2);

            
            if (segmentStarted)
            {
                DrawCircleV(startPoint, 15f, ColorHighlight1);
                Segment s = new(startPoint, game.MousePos);
                s.Draw(4, ColorHighlight1);
                
            }
            segments.Draw(3f, ColorLight);

            
            foreach (var bullet in bullets)
            {
                bullet.Draw();
            }

        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            
        }

        protected override void DrawUIExample(ScreenInfo ui)
        {
            Vector2 uiSize = ui.Area.Size;
            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.98f, 0.11f), new Vector2(0.5f, 1f));
            string infoText =
                $"[LMB] Add Segment [RMB] Cancel Segment [Space] Shoot [1] Speed: {bulletSpeed} [2] Size: {bulletR} [C] CCD: {(CCD ? "ON" : "OFF")}";
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }

        private void Shoot()
        {
            Vector2 mousePos = GAMELOOP.Game.MousePos;
            Vector2 dir = (mousePos - muzzlePos).Normalize();

            Bullet b = new(muzzlePos, dir * bulletSpeed, bulletR);
            bullets.Add(b);
        }
        private void IncreaseBulletSpeed()
        {
            bulletSpeed += 1000f;
            if (bulletSpeed > 15000f) bulletSpeed = 1000f;
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
