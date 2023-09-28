
using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;




namespace Examples.Scenes.ExampleScenes
{
    public class ShapeCollider
    {
        const float collisionTime = 1f;

        public Collider Collider { get; private set; }
        public IShape Shape { get { return Collider.GetShape(); } }


        float collisionTimer = -1f;
        
        Intersection lastIntersection = new();
        
        public ShapeCollider(int shapeIndex, Vector2 pos, float size)
        {
            if(shapeIndex == 1)//segment
            {
                Vector2 dir = SRNG.randVec2();
                Vector2 start = pos - dir * size * 0.5f;
                Vector2 end = pos + dir * size * 0.5f;
                SegmentCollider sc = new(start, end);
                Collider = sc;
            }
            else if (shapeIndex == 2)//circle
            {
                CircleCollider cc = new(pos, size * 0.5f);
                Collider = cc;
            }
            else if (shapeIndex == 3)//triangle
            {
                Vector2 dir = SRNG.randVec2();
                Vector2 A = pos + dir * size * 0.5f;
                
                Vector2 p = pos - dir * size * 0.5f;
                Vector2 left = dir.GetPerpendicularLeft();
                Vector2 right = dir.GetPerpendicularRight();
                Vector2 B = p + left * SRNG.randF(size * 0.1f, size * 0.5f);
                Vector2 C = p + right * SRNG.randF(size * 0.1f, size * 0.5f);
                PolyCollider pc = new(pos, new(0f), A, B, C);
                Collider = pc;
            }
            else if (shapeIndex == 4)//rect
            {
                RectCollider rc = new RectCollider(pos, new Vector2(size, size) * 0.5f, new Vector2(0.5f));
                Collider = rc;
            }
            else if (shapeIndex == 5)//rect poly
            {
                Rect r = new (pos, new Vector2(size, size) * 0.5f, new Vector2(0.5f));
                var points = r.ToPolygon();
                points.Rotate(pos, SRNG.randAngleRad());
                Collider = new PolyCollider(points, pos, new(0f));
            }
            else if (shapeIndex == 6)//poly
            {
                var poly = Polygon.Generate(pos, SRNG.randI(6, 24), size * 0.1f, size * 0.5f);
                Collider = new PolyCollider(poly, pos, new(0f));
            }
            else if (shapeIndex == 7)//polyline
            {
                var poly = Polygon.Generate(pos, SRNG.randI(6, 24), size * 0.1f, size * 0.5f);
                Collider = new PolylineCollider(poly.ToPolyline(), pos, new(0f));
            }
            else
            {
                CircleCollider cc = new(pos, size * 0.5f);
                Collider = cc;
            }

            Collider.ComputeCollision = true;
            Collider.ComputeIntersections = true;
            Collider.Vel = SRNG.randVec2(50, 300);
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

            Collider.DrawShape(4f, color);
            lastIntersection.Draw(2f, ExampleScene.ColorLight, ExampleScene.ColorLight);

            //DrawCircleV(Collider.Pos, 25, BLUE);
            DrawCircleV(Shape.GetCentroid(), 2, ExampleScene.ColorHighlight2);
        }
    }


    public class PolylineCollisionExample : ExampleScene
    {
        Polyline polyline = new();
        int dragIndex = -1;

        Rect boundaryRect;
        Segments boundary = new();

        List<ShapeCollider> colliders = new();


        Font font;
        List<Circle> vertices = new();
        float vertexRadius = 8f;
        int pickedVertex = -1;
        bool drawClosest = true;
        Vector2 closest = new();
        int closestIndex = -1;
        
        public PolylineCollisionExample()
        {
            Title = "Polyline Collision Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);


            boundaryRect = new Rect(new(0), new(1920, 1080), new(0.5f)).ApplyMargins(0.05f, 0.05f, 0.1f, 0.1f);
            boundaryRect.FlippedNormals = true;
            boundary = boundaryRect.GetEdges();

        }

        public override void Reset()
        {
            polyline.Clear();
            dragIndex = -1;
            colliders.Clear();
        }
        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);
            //if (IsKeyPressed(KeyboardKey.KEY_R)) { polyline = new(); }

            //if (IsKeyPressed(KeyboardKey.KEY_SPACE)) polyline.AutomaticNormals = !polyline.AutomaticNormals;

            float shapeSize = SRNG.randF(75, 150);
            if (IsKeyPressed(KeyboardKey.KEY_ONE))
            {
                colliders.Add(new ShapeCollider(1, mousePosGame, shapeSize));
            }
            else if (IsKeyPressed(KeyboardKey.KEY_TWO))
            {
                colliders.Add(new ShapeCollider(2, mousePosGame, shapeSize));
            }
            else if (IsKeyPressed(KeyboardKey.KEY_THREE))
            {
                colliders.Add(new ShapeCollider(3, mousePosGame, shapeSize));
            }
            else if (IsKeyPressed(KeyboardKey.KEY_FOUR))
            {
                colliders.Add(new ShapeCollider(4, mousePosGame, shapeSize));
            }
            else if (IsKeyPressed(KeyboardKey.KEY_FIVE))
            {
                colliders.Add(new ShapeCollider(5, mousePosGame, shapeSize));
            }
            else if (IsKeyPressed(KeyboardKey.KEY_SIX))
            {
                colliders.Add(new ShapeCollider(6, mousePosGame, shapeSize));
            }
            else if (IsKeyPressed(KeyboardKey.KEY_SEVEN))
            {
                colliders.Add(new ShapeCollider(7, mousePosGame, shapeSize));
            }
        }

        public override void Update(float dt, ScreenInfo game, ScreenInfo ui)
        {
            base.Update(dt, game, ui);

            UpdatePolyline(game.MousePos);

            Segments allSegments = new();
            allSegments.AddRange(boundary);
            allSegments.AddRange(polyline.GetEdges());

            foreach (var col in colliders)
            {
                col.Update(dt);
                var shape = col.Shape;
                foreach (var segment in allSegments)
                {
                    bool overlap = SGeometry.Overlap(shape, segment);
                    if (overlap)
                    {
                        var intersection = SGeometry.Intersect(shape, segment);
                        if (intersection.Valid)
                        {
                            col.Collision(new(intersection, col.Collider.Vel, col.Collider.Pos));
                        }
                    }
                }
            }
        }
        public override void DrawGame(ScreenInfo game)
        {
            base.DrawGame(game);

            boundary.Draw(4f, ColorMedium);
            foreach (var seg in boundary)
            {
                Segment normal = new(seg.Center, seg.Center + seg.Normal * 25f);
                normal.Draw(2f, BLUE);
            }
            DrawPolyline();


            foreach (var col in colliders)
            {
                col.Draw();
            }
            
        }
        public override void DrawUI(ScreenInfo ui)
        {
            base.DrawUI(ui);



            Vector2 uiSize = ui.Area.Size;
            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            string infoText = $"[LMB] Add point | [RMB] Remove point | [1 - 7] Add Shape | Shapes: {colliders.Count}";
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            
        }

        private void UpdatePolyline(Vector2 mousePos)
        {
            bool isMouseOnLine = false;
            drawClosest = true;

            closest = polyline.GetClosestCollisionPoint(mousePos).Point;
            closestIndex = polyline.GetClosestIndexOnEdge(mousePos);
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
                if (vertex.Radius > 0f)
                {
                    if (vertex.Radius < vertexRadius * 2f) vertex.Draw(ColorLight);
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

                Segment normal = new(segment.Center, segment.Center + segment.Normal * 25f);
                normal.Draw(2f, BLUE);

                //if (!polyline.AutomaticNormals)
                //{
                //    Segment normal = new(segment.Center, segment.Center + segment.Normal * 25f);
                //    normal.Draw(2f, BLUE);
                //}
            }

            if (drawClosest) DrawCircleV(closest, vertexRadius, ColorHighlight1);

            //DrawCircleV(polyline.GetCentroid(), 15f, YELLOW);
        }
    }
}
