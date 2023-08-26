using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{
    public class SpaceObject : IAreaObject
    {
        public int AreaLayer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void AddedToArea(Area area)
        {
            throw new NotImplementedException();
        }

        public bool CheckAreaBounds()
        {
            throw new NotImplementedException();
        }

        public void DeltaFactorApplied(float f)
        {
            throw new NotImplementedException();
        }

        public void DrawGame(Vector2 size, Vector2 mousePos)
        {
            throw new NotImplementedException();
        }

        public void DrawToScreen(Vector2 size, Vector2 mousePos)
        {
            throw new NotImplementedException();
        }

        public void DrawUI(Vector2 size, Vector2 mousePos)
        {
            throw new NotImplementedException();
        }

        public Rect GetBoundingBox()
        {
            throw new NotImplementedException();
        }

        public Vector2 GetCameraFollowPosition(Vector2 camPos)
        {
            throw new NotImplementedException();
        }

        public Vector2 GetPosition()
        {
            throw new NotImplementedException();
        }

        public bool IsDead()
        {
            throw new NotImplementedException();
        }

        public bool IsDrawingToGameTexture()
        {
            throw new NotImplementedException();
        }

        public bool IsDrawingToScreen()
        {
            throw new NotImplementedException();
        }

        public bool IsDrawingToUITexture()
        {
            throw new NotImplementedException();
        }

        public bool Kill()
        {
            throw new NotImplementedException();
        }

        public void LeftAreaBounds(Vector2 safePosition, CollisionPoints collisionPoints)
        {
            throw new NotImplementedException();
        }

        public void RemovedFromArea(Area area)
        {
            throw new NotImplementedException();
        }

        public void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui)
        {
            throw new NotImplementedException();
        }
    }
    public class Shard : SpaceObject
    {
        public Shard()
        {

        }

    }
    public class Asteroid : IAreaObject, ICollidable
    {
        private PolyCollider collider;
        private List<ICollidable> collidables = new();
        private uint[] colMask = new uint[] { };
        private bool dead = false;
        public int AreaLayer { get; set; } = 0;
        private bool overlapped = false;
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
        


        public void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui) { }
        public void DrawGame(Vector2 size, Vector2 mousePos)
        {
            Color color = overlapped ? GREEN : WHITE;
            collider.GetShape().DrawShape(4f, color);
            //if(collider.GetShape() is Polygon p)
            //{
            //    p.DrawVertices(4f, RED);
            //}
            overlapped = false;
        }
        public bool Kill()
        {
            if (dead) return false;
            dead = true;
            return true;
        }
        public virtual bool HasCollidables() { return true; }
        public virtual List<ICollidable> GetCollidables() { return collidables; }

        public void AddedToArea(Area area) { }
        public bool CheckAreaBounds() { return false; }
        public void DeltaFactorApplied(float f) { }
        public void DrawToScreen(Vector2 size, Vector2 mousePos) { }
        public void DrawUI(Vector2 size, Vector2 mousePos) { }

        public Rect GetBoundingBox() { return collider.GetShape().GetBoundingBox(); }
        public Vector2 GetCameraFollowPosition(Vector2 camPos) { return collider.Pos; }
        public ICollider GetCollider() { return collider; }
        public uint GetCollisionLayer() { return AsteroidMiningExample.AsteriodLayer; }
        public uint[] GetCollisionMask() { return colMask; }
        public Vector2 GetPosition() { return collider.Pos; }
        public bool IsDead() { return dead; }
        public bool IsDrawingToGameTexture() { return true; }
        public bool IsDrawingToScreen() { return false; }
        public bool IsDrawingToUITexture() { return false; }

        

        public void LeftAreaBounds(Vector2 safePosition, CollisionPoints collisionPoints) { }

        public void Overlap(CollisionInformation info) { }

        public void OverlapEnded(ICollidable other) { }

        public void RemovedFromArea(Area area) { }

        
    }

    public class AsteroidMiningExample : ExampleScene
    {
        public static uint AsteriodLayer = 1;

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
        public AsteroidMiningExample()
        {
            Title = "Asteroid Mining Example";
            font = GAMELOOP.GetFont(FontIDs.JetBrains);
            UpdateBoundaryRect(GAMELOOP.Game);
            area = new AreaCollision(boundaryRect, 4, 4);
        }
        public override void Reset()
        {
            area.Clear();
            polyModeActive = false;
            curRot = 0f;
            curSize = 50f;
            curShapeType = ShapeType.Triangle;
            RegenerateShape();
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
        }
        private void PolyModeEnded()
        {

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


                            //gets stuck....
                            //var cutOut = cutShape.Cut(asteroidShape); // asteroidShape.Cut(cutShape);
                            //Triangulation fracture = new();
                            //foreach (var piece in cutOut)
                            //{
                            //    
                            //    fracture.AddRange(piece.Triangulate());
                            //}
                            
                            
                            var newShapes = SClipper.Difference(asteroidShape, cutShape).ToPolygons(true);
                            if(newShapes.Count > 0)
                            {
                                foreach (var shape in newShapes)
                                {
                                    float shapeArea = shape.GetArea();
                                    if(shapeArea > 10)
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
