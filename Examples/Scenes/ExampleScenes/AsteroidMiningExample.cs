using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{
    public abstract class SpaceObject : IAreaObject
    {
        protected bool dead = false;
        public int AreaLayer { get; set; } = 0;
        public bool Kill()
        {
            if (dead) return false;
            dead = true;
            return true;
        }

        public abstract Vector2 GetCameraFollowPosition(Vector2 camPos);
        public abstract Vector2 GetPosition();
        public abstract Rect GetBoundingBox();

        public virtual void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui) { }
        public virtual void DrawGame(Vector2 size, Vector2 mousePos) { }
        public virtual void Overlap(CollisionInformation info) { }
        public virtual void OverlapEnded(ICollidable other) { }
        public virtual void AddedToArea(Area area) { }
        public virtual void RemovedFromArea(Area area) { }
        
        public void DeltaFactorApplied(float f) { }
        public void DrawToScreen(Vector2 size, Vector2 mousePos) { }
        public void DrawUI(Vector2 size, Vector2 mousePos) { }
        public bool IsDead() { return dead; }
        public bool IsDrawingToGameTexture() { return true; }
        public bool IsDrawingToScreen() { return false; }
        public bool IsDrawingToUITexture() { return false; }
        public bool CheckAreaBounds() { return false; }
        public void LeftAreaBounds(Vector2 safePosition, CollisionPoints collisionPoints) { }

        
    }
    public class AsteroidShard : SpaceObject
    {
        private Polygon shape;
        private Vector2 pos;
        private Vector2 vel;
        private float rotDeg = 0f;
        private float angularVelDeg = 0f;
        private float lifetimeTimer = 0f;
        private float lifetime = 0f;
        private float lifetimeF = 1f;
        public AsteroidShard(Polygon shape, Vector2 fractureCenter)
        {
            this.shape = shape;
            this.rotDeg = 0f;
            this.pos = shape.GetCentroid();
            Vector2 dir = (pos - fractureCenter).Normalize();
            this.vel = dir * SRNG.randF(100, 300);
            this.angularVelDeg = SRNG.randF(-90, 90);
            this.lifetime = SRNG.randF(1, 3);
            this.lifetimeTimer = this.lifetime;
        }
        public override void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui)
        {
            if(lifetimeTimer > 0f)
            {
                lifetimeTimer -= dt;
                if(lifetimeTimer <= 0f)
                {
                    lifetimeTimer = 0f;
                    dead = true;
                }
                else
                {
                    lifetimeF = 1f - (lifetimeTimer / lifetime);
                    float prevRotDeg = rotDeg;
                    pos += vel * dt;
                    rotDeg += angularVelDeg * dt;

                    float rotDifDeg = rotDeg - prevRotDeg;
                    shape.CenterSelf(pos);
                    shape.RotateSelf(new Vector2(0.5f), rotDifDeg * DEG2RAD);
                }
            }
        }
        public override void DrawGame(Vector2 size, Vector2 mousePos)
        {
            //SDrawing.DrawCircleFast(pos, 4f, RED);
            Color color = WHITE.ChangeAlpha((int)(150 * lifetimeF));
            shape.DrawLines(2f, color);
        }
        public override Rect GetBoundingBox() { return shape.GetBoundingBox(); }
        public override Vector2 GetCameraFollowPosition(Vector2 camPos) { return pos; }
        public override Vector2 GetPosition() { return pos; }
    }
    public class Asteroid : SpaceObject, ICollidable
    {
        private const float DamageThreshold = 250f;

        private PolyCollider collider;
        private List<ICollidable> collidables = new();
        private uint[] colMask = new uint[] { };
        private bool overlapped = false;
        private float curThreshold = DamageThreshold;
        public Asteroid(Vector2 pos, params Vector2[] shape)
        {
            collider = new PolyCollider(pos, new(), shape);
            collider.ComputeCollision = false;
            collider.ComputeIntersections = false;
            collidables.Add(this);

        }
        public Asteroid(Polygon shape)
        {
            collider = new PolyCollider(shape);
            collider.ComputeCollision = false;
            collider.ComputeIntersections = false;
            collidables.Add(this);
        }
        public Polygon GetPolygon() { return collider.GetPolygonShape(); }

        public void Overlapped()
        {
            overlapped = true;
        }
        
        public void Damage(float amount, Vector2 point)
        {
            curThreshold -= amount;
            if(curThreshold <= 0f)
            {
                curThreshold = DamageThreshold + curThreshold;

                //cut piece
            }
        }

        public override void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui) { }
        public override void DrawGame(Vector2 size, Vector2 mousePos)
        {
            Color color = overlapped ? GREEN : WHITE;
            collider.GetShape().DrawShape(4f, color);
            //if(collider.GetShape() is Polygon p)
            //{
            //    p.DrawVertices(4f, RED);
            //}
            overlapped = false;
        }
        
        public virtual bool HasCollidables() { return true; }
        public virtual List<ICollidable> GetCollidables() { return collidables; }

        public override Rect GetBoundingBox() { return collider.GetShape().GetBoundingBox(); }
        public override Vector2 GetCameraFollowPosition(Vector2 camPos) { return collider.Pos; }
        public override Vector2 GetPosition() { return collider.Pos; }
        public ICollider GetCollider() { return collider; }
        public uint GetCollisionLayer() { return AsteroidMiningExample.AsteriodLayer; }
        public uint[] GetCollisionMask() { return colMask; }
    }

    public class LaserDevice : SpaceObject
    {
        private const float LaserRange = 750;
        private const float DamagePerSecond = 100;
        private bool aimingMode = true;
        private bool hybernate = false;
        private bool laserEnabled = false;

        private Vector2 pos;
        private float rotRad;
        private float size;
        private Triangle shape;
        private Vector2 tip;
        private Vector2 laserEndPoint;
        private Vector2 aimDir = new();
        private AreaCollision area;
        public LaserDevice(Vector2 pos, float size, AreaCollision area) 
        {
            this.area = area;
            this.pos = pos;
            this.size = size;
            this.rotRad = 0f;
            UpdateTriangle();
            this.laserEndPoint = tip;
        }
        public void SetHybernate(bool enabled)
        {
            if (enabled)
            {
                aimingMode = true;
                hybernate = true;
            }
            else
            {
                aimingMode = true;
                hybernate = false;
            }
            
        }
        public void SetAimingMode(bool enabled)
        {
            aimingMode = enabled;

        }
        
        private void UpdateTriangle()
        {
            Vector2 a = pos + new Vector2(size / 2, 0f).Rotate(rotRad);
            Vector2 b = pos + new Vector2(-size / 2, -size / 4).Rotate(rotRad);
            Vector2 c = pos + new Vector2(-size / 2, size / 4).Rotate(rotRad);

            this.shape = new Triangle(a, b, c);
            this.tip = a;
        }

        public override void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui)
        {
            laserEnabled = false;
            if (hybernate) return;
            
            if (aimingMode)
            {
                Vector2 dir = game.MousePos - pos;
                aimDir = dir.Normalize();
                rotRad = dir.AngleRad();

                laserEnabled = IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT);
            }
            else
            {
                pos = game.MousePos;
            }
            
            UpdateTriangle();

            if (laserEnabled)
            {
                laserEndPoint = tip + aimDir * LaserRange;
                var col = area.GetCollisionHandler();
                if(col != null)
                {
                    var queryInfos = col.QuerySpace(new Segment(tip, laserEndPoint), true, AsteroidMiningExample.AsteriodLayer);
                    if(queryInfos.Count > 0)
                    {
                        var closest = queryInfos[0];
                        if (closest.intersection.Valid)
                        {
                            var other = closest.collidable;
                            if (other != null && other is Asteroid a)
                            {
                                laserEndPoint = closest.intersection.ColPoints[0].Point;
                                a.Damage(DamagePerSecond * dt, laserEndPoint);
                            }
                        }
                        
                    }
                }

            }
        }
        public override void DrawGame(Vector2 size, Vector2 mousePos)
        {
            shape.DrawLines(4f, RED);
            SDrawing.DrawCircle(tip, 8f, RED);

            if (laserEnabled)
            {
                Segment laser = new(tip, laserEndPoint);
                laser.Draw(4f, RED);
            }
        }


        public override Rect GetBoundingBox() { return shape.GetBoundingBox(); }
        public override Vector2 GetCameraFollowPosition(Vector2 camPos) { return pos; }
        public override Vector2 GetPosition() { return pos; }
    }

    public class AsteroidMiningExample : ExampleScene
    {
        public static uint AsteriodLayer = 1;
        private const float MinPieceArea = 3000f;

        internal enum ShapeType { None = 0, Triangle = 1, Rect = 2, Poly = 3};

        private Font font;
        private AreaCollision area;
        private Rect boundaryRect = new();

        private bool polyModeActive = false;
        private ShapeType curShapeType = ShapeType.None;

        private Polygon curShape = new();
        private Vector2 curPos = new();
        private float curRot = 0f;
        private float curSize = 50;

        private LaserDevice laserDevice;

        public AsteroidMiningExample()
        {
            Title = "Asteroid Mining Example";
            font = GAMELOOP.GetFont(FontIDs.JetBrains);
            UpdateBoundaryRect(GAMELOOP.Game);
            area = new AreaCollision(boundaryRect, 4, 4);

            laserDevice = new(new Vector2(0f), 100, area);
            area.AddAreaObject(laserDevice);
        }
        public override void Reset()
        {
            area.Clear();
            polyModeActive = false;
            curRot = 0f;
            curSize = 50f;
            curShapeType = ShapeType.Triangle;
            RegenerateShape();
            laserDevice = new(new Vector2(0), 100, area);
            area.AddAreaObject(laserDevice);
        }
        public override Area? GetCurArea()
        {
            return area;
        }

        private void UpdateBoundaryRect(ScreenTexture game)
        {
            boundaryRect = new Rect(new Vector2(0f), game.GetSize(), new Vector2(0.5f)).ApplyMargins(0.005f, 0.005f, 0.1f, 0.005f);
        }

        public override void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui)
        {
            UpdateBoundaryRect(game);
            area.ResizeBounds(boundaryRect);
            area.Update(dt, mousePosScreen, game, ui);

            base.Update(dt, mousePosScreen, game, ui); //calls area update therefore area bounds have to be updated before that
        }
        private void SetCurPos(Vector2 pos)
        {
            curPos = pos;
        }
        private void CycleRotation()
        {
            float step = 45f;
            curRot += step;
            curRot = Wrap(curRot, 0f, 360f);
        }
        private void CycleSize()
        {
            float step = 50f;
            float min = 50f;
            float max = 400;
            curSize += step;
            curSize = Wrap(curSize, min, max);
        }
        private void RegenerateShape()
        {
            if (curShapeType == ShapeType.Triangle)
            {
                GenerateTriangle();
            }
            else if (curShapeType == ShapeType.Rect)
            {
                GenerateRect();
            }
            else if (curShapeType == ShapeType.Poly)
            {
                GeneratePoly();
            }
        }
        private void GenerateTriangle()
        {
            curShape = SPoly.Generate(curPos, 3, curSize / 2, curSize);
        }
        private void GenerateRect()
        {
            Rect r = new(curPos, new Vector2(curSize), new Vector2(0.5f));
            curShape = r.RotateList(new Vector2(0.5f), curRot);
        }
        private void GeneratePoly()
        {
            curShape = SPoly.Generate(curPos, 16, curSize * 0.25f, curSize);
        }
        private void PolyModeStarted()
        {
            if (curShapeType == ShapeType.None)
            {
                curShapeType = ShapeType.Triangle;
                RegenerateShape();
            }
            laserDevice.SetHybernate(true);
        }
        private void PolyModeEnded()
        {
            laserDevice.SetHybernate(false);
        }
        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);

            var col = area.GetCollisionHandler();
            if (col == null) return;

            if (IsKeyPressed(KeyboardKey.KEY_TAB))//enter/exit poly mode
            {
                polyModeActive = !polyModeActive;
                if(polyModeActive) PolyModeStarted();
                else PolyModeEnded();
            }
            
            if (polyModeActive)
            {
                SetCurPos(mousePosGame);
                curShape.CenterSelf(curPos);

                
                var collidables = col.CastSpace(curShape, false, AsteriodLayer);
                foreach (var collidable in collidables)
                {
                    if (collidable is Asteroid asteroid)
                    {
                        asteroid.Overlapped();
                    }
                }

                if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) //add polygon (merge)
                {
                    Polygons polys = new();

                    if (collidables.Count > 0)
                    {
                        foreach (var collidable in collidables)
                        {
                            if (collidable is Asteroid asteroid)
                            {
                                area.RemoveAreaObject(asteroid);
                                polys.Add(asteroid.GetPolygon());
                            }
                        }
                        var finalShapes = SClipper.Union(curShape.ToPolygon(), polys, Clipper2Lib.FillRule.NonZero).ToPolygons(true);
                        if (finalShapes.Count > 0)
                        {
                            foreach (var f in finalShapes)
                            {
                                Asteroid a = new(f);
                                area.AddAreaObject(a);
                            }
                        }
                        else
                        {
                            Asteroid a = new(curShape.ToPolygon());
                            area.AddAreaObject(a);
                        }
                    }
                    else
                    {
                        Asteroid a = new(curShape.ToPolygon());
                        area.AddAreaObject(a);
                    }

                }
                if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT)) //cut polygon
                {
                    //use cur shape to cast
                    //go through all overlapping polygons and call cut on them
                    //accumulate all shards and add them to the area
                    var cutShape = curShape.ToPolygon();
                    foreach (var collidable in collidables)
                    {
                        if (collidable is Asteroid asteroid)
                        {
                            area.RemoveAreaObject(asteroid);
                            var asteroidShape = asteroid.GetPolygon();


                            var cutOut = cutShape.Cut(asteroidShape); // asteroidShape.Cut(cutShape);
                            //Triangulation fracture = new();
                            foreach (var piece in cutOut)
                            {
                                float pieceArea = piece.GetArea();
                                if (pieceArea < MinPieceArea) continue;

                                Vector2 center = piece.GetCentroid();
                                var triangulation = piece.Fracture(0.15f); //piece.Triangulate();
                                foreach (var triangle in triangulation)
                                {
                                    AsteroidShard shard = new(triangle.ToPolygon(), center);
                                    area.AddAreaObject(shard);
                                }
                                //fracture.AddRange(SPoly.TriangulateDelaunay(piece));
                                //fracture.AddRange(piece.Triangulate());
                            }
                            
                            
                            var newShapes = SClipper.Difference(asteroidShape, cutShape).ToPolygons(true);
                            if(newShapes.Count > 0)
                            {
                                foreach (var shape in newShapes)
                                {
                                    float shapeArea = shape.GetArea();
                                    if(shapeArea > MinPieceArea)
                                    {
                                        Asteroid a = new(shape);
                                        area.AddAreaObject(a);
                                    }
                                }
                            }
                        }
                    }

                }

                if (IsKeyPressed(KeyboardKey.KEY_Q))//regenerate
                {
                    RegenerateShape();
                }

                if (IsKeyPressed(KeyboardKey.KEY_X))//rotate
                {
                    float oldRot = curRot;
                    CycleRotation();
                    
                    float dif = curRot - oldRot;
                    curShape.RotateSelf(new Vector2(0.5f), dif * DEG2RAD);
                    curShape.CenterSelf(curPos);
                }

                if (IsKeyPressed(KeyboardKey.KEY_C))//scale
                {
                    float oldSize = curSize;
                    CycleSize();
                    float scale = curSize / oldSize;
                    curShape.ScaleSelf(scale);
                    curShape.CenterSelf(curPos);
                }

                
                if (IsKeyPressed(KeyboardKey.KEY_ONE))//choose triangle
                {
                    if(curShapeType != ShapeType.Triangle)
                    {
                        GenerateTriangle();
                        curShapeType = ShapeType.Triangle;
                    }
                    
                }
                if (IsKeyPressed(KeyboardKey.KEY_TWO))//choose rectangle
                {
                    if(curShapeType != ShapeType.Rect)
                    {
                        GenerateRect();
                        curShapeType = ShapeType.Rect;
                    }
                    
                }
                if (IsKeyPressed(KeyboardKey.KEY_THREE))//choose polygon 
                {
                    if (curShapeType != ShapeType.Poly)
                    {
                        GeneratePoly();
                        curShapeType = ShapeType.Poly;
                    }
                    
                }
            }
            else
            {
                if(IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    laserDevice.SetAimingMode(false);
                }
                else if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    laserDevice.SetAimingMode(true);
                }
            }
        }



        public override void DrawGame(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.DrawGame(gameSize, mousePosGame);
            boundaryRect.DrawLines(4f, ColorLight);

            if(polyModeActive && curShapeType != ShapeType.None)
            {
                curShape.DrawLines(2f, RED);
            }

            //area.DrawDebug(GRAY, GOLD, GREEN);
            area.DrawGame(gameSize, mousePosGame);

        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            area.DrawUI(uiSize, mousePosUI);
            base.DrawUI(uiSize, mousePosUI);

            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.07f), new Vector2(0.5f, 1f));
            string infoText = String.Format("Object Count: {0}", area.Count);
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }

    }
}
