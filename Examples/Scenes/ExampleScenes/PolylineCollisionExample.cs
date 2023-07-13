
using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;




namespace Examples.Scenes.ExampleScenes
{

    public class PolylineCollisionExample : ExampleScene
    {
        ScreenTexture game;
        Polyline polyline = new();
        int dragIndex = -1;

        Segments boundary = new();

        Rect boundaryRect;

        CircleCollider ball;
        float collisionTimer = -1f;
        const float collisionTime = 1f;

        Vector2 lastNormal = new();
        Vector2 lastIntersectionPoint = new();

        Font font;

        RectCollider rect;
        Intersection lastIntersection = new();

        List<Circle> vertices = new();
        float vertexRadius = 8f;
        int pickedVertex = -1;
        bool drawClosest = true;
        Vector2 closest = new();
        int closestIndex = -1;
        public PolylineCollisionExample()
        {
            Title = "Polyline Collision Example";
            game = GAMELOOP.Game;
            BasicCamera camera = new BasicCamera(new Vector2(0f), new Vector2(1920, 1080), new Vector2(0.5f), 1f, 0f);
            game.SetCamera(camera);

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            var cameraRect = camera.GetArea();
            boundaryRect = SRect.ApplyMarginsAbsolute(cameraRect, 25f, 25f, 75 * 2f, 75 * 2f);
            boundaryRect.FlippedNormals = true;
            boundary = boundaryRect.GetEdges();

            ball = new CircleCollider(boundaryRect.Center, 25f);
            ball.ComputeCollision = true;
            ball.ComputeIntersections = true;
            ball.Vel = SRNG.randVec2(150, 300);

            rect = new RectCollider(boundaryRect.Center, new Vector2(100, 100), new Vector2(0.5f));
            rect.ComputeIntersections = true;
            rect.ComputeCollision = true;
            rect.Vel = SRNG.randVec2(150, 300);

            polyline.AutomaticNormals = true;
            polyline.FlippedNormals = true;
        }
        public override void Reset()
        {
            polyline = new();
            dragIndex = -1;
            ball.Pos = boundaryRect.Center;
            ball.Vel = SRNG.randVec2(150, 300);
            rect.Pos = ball.Pos;
            rect.Vel = SRNG.randVec2(150, 300);
        }
        public override void HandleInput(float dt)
        {
            base.HandleInput(dt);
            if (IsKeyPressed(KeyboardKey.KEY_R)) { polyline = new(); }

        }

        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.Update(dt, mousePosGame, mousePosUI);

            UpdatePolyline(mousePosGame);

            ball.UpdateState(dt);
            rect.UpdateState(dt);

            var ballShape = ball.GetShape();
            Segments allSegments = new();
            allSegments.AddRange(boundary);
            allSegments.AddRange(polyline.GetEdges());

            foreach (var segment in allSegments)
            {
                bool overlap = ballShape.Overlap(segment);
                if (overlap)
                {
                    var intersection = ballShape.Intersect(segment, ball.Vel);
                    if (intersection.valid)
                    {
                        Vector2 normal = intersection.n;
                        //lastNormal = normal;
                        //lastIntersection = intersection.p;
                        ball.Vel = ball.Vel.Reflect(normal);

                        collisionTimer = collisionTime;
                        break;
                    }
                }

            }

            var rectShape = rect.GetShape();
            foreach (var segment in allSegments)
            {
                bool overlap = rectShape.Overlap(segment);
                if (overlap)
                {
                    var intersection = rectShape.Intersect(segment, rect.Vel);
                    if (intersection.valid)
                    {
                        Vector2 normal = intersection.n;
                        lastNormal = normal;
                        lastIntersectionPoint = intersection.p;
                        rect.Vel = rect.Vel.Reflect(normal);
                        lastIntersection = intersection;
                        break;
                    }
                }

            }

            if (collisionTimer > 0f)
            {
                collisionTimer -= dt;
                if (collisionTimer <= 0f)
                { 
                    collisionTimer = -1f;
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
            //colSegments.Draw(2f, ColorLight);
            DrawPolyline();

            float colF = collisionTimer > 0f ? collisionTimer / collisionTime : 0f;
            
            //Color ballColor = ColorHighlight2.Lerp(ColorHighlight1, colF * colF);
            Color ballColor = STween.Tween(ColorHighlight2, ColorHighlight1, colF, TweenType.QUAD_IN);
            ball.GetShape().DrawShape(2f, ballColor);
            rect.GetShape().DrawShape(2f, ballColor);
            if(lastNormal.X != 0f || lastNormal.Y != 0f)
            {
                DrawCircleV(lastIntersectionPoint, 6f, ColorHighlight2);
                Segment s = new(lastIntersectionPoint, lastIntersectionPoint + lastNormal * 25f);
                s.Draw(2f, ColorHighlight2);
            }
            lastIntersection.Draw(2f, RED, BLUE);
            
        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            base.DrawUI(uiSize, mousePosUI);

            


            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            string infoText = "[LMB] Add point - [RMB] Remove point - [Space] Add Segment";
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            
        }

        private void UpdatePolyline(Vector2 mousePos)
        {
            bool isMouseOnLine = false;
            drawClosest = true;

            closest = polyline.GetClosestPoint(mousePos);
            closestIndex = polyline.GetClosestIndex(mousePos);
            pickedVertex = -1;
            vertices.Clear();

            for (int i = 0; i < polyline.Count; i++)
            {
                var p = polyline[i];
                float disSq = (mousePos - p).LengthSquared();
                if (pickedVertex == -1 && disSq < (vertexRadius * vertexRadius) * 2f)
                {
                    vertices.Add(new(p, vertexRadius * 2f));
                    pickedVertex = i;
                }
                else vertices.Add(new(p, vertexRadius));

                if (drawClosest)
                {
                    disSq = (closest - p).LengthSquared();
                    if (disSq < (vertexRadius * vertexRadius) * 4f)
                    {
                        drawClosest = false;
                    }
                }

            }

            if (drawClosest)
            {
                float disSq = (closest - mousePos).LengthSquared();

                float tresholdSq = 30 * 30;
                if (disSq > tresholdSq)
                {
                    drawClosest = false;
                }
                else isMouseOnLine = true;
            }


            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                if (pickedVertex == -1)
                {
                    if (isMouseOnLine)
                    {
                        polyline.Insert(closestIndex + 1, mousePos);
                    }
                    else polyline.Add(mousePos);

                }
                else
                {
                    dragIndex = pickedVertex;
                }
            }
            else if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dragIndex = -1;
            }
            else if (dragIndex == -1 && IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                if (pickedVertex > -1)
                {
                    polyline.RemoveAt(pickedVertex);
                }
            }

            if (dragIndex > -1) polyline[dragIndex] = mousePos;
        }
        private void DrawPolyline()
        {
            
            foreach (var vertex in vertices)
            {
                if (vertex.radius > 0f)
                {
                    if (vertex.radius < vertexRadius * 2f) vertex.Draw(ColorLight);
                    else vertex.Draw(ColorHighlight1);
                }
            }

            var segments = polyline.GetEdges();
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];

                if (drawClosest)
                {
                    if (closestIndex == i) segment.Draw(4f, ColorHighlight2);
                    else segment.Draw(4f, ColorLight);
                }
                else segment.Draw(4f, ColorLight);

                Segment normal = new(segment.Center, segment.Center + segment.n * 25f);
                normal.Draw(2f, BLUE);
            }

            if (drawClosest) DrawCircleV(closest, vertexRadius, ColorHighlight1);
        }
    }
}
