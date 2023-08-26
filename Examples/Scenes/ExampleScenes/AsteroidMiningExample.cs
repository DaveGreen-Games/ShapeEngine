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

        public Asteroid(Vector2 pos, params Vector2[] shape)
        {
            collider = new PolyCollider(pos, new(), shape);
            collidables.Add(this);
        }
        public Asteroid(Polygon shape)
        {
            collider = new PolyCollider(shape);
            collidables.Add(this);
        }
        public Polygon GetPolygon() { return collider.GetShape().ToPolygon(); }


        public Polygon Merge()
        {
            dead = true;
            return GetPolygon();
        }
        public List<Shard> Cut(Polygon shape)//also used for mining
        {
            return new();
        }


        public void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui) { }
        public void DrawGame(Vector2 size, Vector2 mousePos)
        {
            collider.GetShape().DrawShape(4f, WHITE);
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

        internal enum TranslationMode { Translation = 1, Rotation = 2, Scaling = 3};

        private Font font;
        private AreaCollision area;
        private Rect boundaryRect = new();

        private bool polyModeActive = false;
        private TranslationMode polyTranslationMode = TranslationMode.Translation;

        private Polygon? curShape = null;
        private Vector2 curPos = new();
        private float curRot = 0f;
        private float curSize = 50;

        public AsteroidMiningExample()
        {
            Title = "Asteroid Mining Example";
            font = GAMELOOP.GetFont(FontIDs.JetBrains);
            UpdateBoundaryRect(GAMELOOP.Game);
            area = new AreaCollision(boundaryRect, 2, 2);
        }
        public override void Reset()
        {
            area.Clear();
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
            base.Update(dt, mousePosScreen, game, ui); //calls area update therefore area bounds have to be updated before that


            
        }
        private void SetCurPos(Vector2 pos)
        {
            curPos = pos;
        }
        private void SetCurRotation()
        {
            Vector2 center = boundaryRect.Center;
            Vector2 dir = (curPos - center);
            curRot = dir.AngleDeg();
        }
        private void SetCurSize()
        {
            Vector2 center = boundaryRect.Center;
            Vector2 dir = (curPos - center);
            curSize = MathF.Max(dir.Length(), 15);
        }
        private void PolyModeStarted(Vector2 mousePos)
        {
            SetCurPos(mousePos);
            //curRot = 0f;
            //curSize = 50f;
            polyTranslationMode = TranslationMode.Translation;
        }
        private void PolyModeEnded()
        {
            curShape = null;
        }
        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);

            if (IsKeyPressed(KeyboardKey.KEY_TAB))//enter/exit poly mode
            {
                polyModeActive = !polyModeActive;
                if(polyModeActive) PolyModeStarted(mousePosGame);
                else PolyModeEnded();
            }
            
            if (polyModeActive)
            {
                if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) //add polygon (merge)
                {
                    //use cur shape to cast 
                    //get all polygons from all overlapping asteroids and kill them
                    //merge all polygons with cur shape and create asteroid from that
                    if(curShape != null)
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

                }

                if (IsKeyPressed(KeyboardKey.KEY_Q))//cycle translation
                {
                    if (polyTranslationMode == TranslationMode.Translation) polyTranslationMode = TranslationMode.Rotation;
                    else if (polyTranslationMode == TranslationMode.Rotation) polyTranslationMode = TranslationMode.Scaling;
                    else if(polyTranslationMode == TranslationMode.Scaling) polyTranslationMode = TranslationMode.Translation;
                }

                switch (polyTranslationMode)
                {
                    case TranslationMode.Translation:
                        SetCurPos(mousePosGame);
                        if(curShape != null)
                        {
                            curShape.CenterSelf(curPos);
                        }
                        break;
                    case TranslationMode.Rotation:
                        float oldRot = curRot;
                        SetCurRotation();
                        if (curShape != null)
                        {
                            float dif = curRot - oldRot;
                            curShape.RotateSelf(new Vector2(0.5f), dif * DEG2RAD);
                        }
                        break;
                    case TranslationMode.Scaling:
                        float oldSize = curSize;
                        SetCurSize();
                        if (curShape != null)
                        {
                            float scale = curSize / oldSize;
                            if(scale != 1f) curShape.ScaleSelf(scale);
                        }
                        break;
                }

                //-> pressing again generates new shape
                if (IsKeyPressed(KeyboardKey.KEY_ONE))//choose triangle
                {
                    curShape = SPoly.Generate(curPos, 3, curSize / 2, curSize);
                }
                if (IsKeyPressed(KeyboardKey.KEY_TWO))//choose rectangle
                {
                    Rect r = new(curPos, new Vector2(curSize), new Vector2(0.5f));
                    curShape = r.RotateList(new Vector2(0.5f), curRot);
                }
                if (IsKeyPressed(KeyboardKey.KEY_THREE))//choose polygon 
                {
                    curShape = SPoly.Generate(curPos, 12, curSize / 2, curSize);
                }


                
            }
            

            



            
            
        }



        public override void DrawGame(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.DrawGame(gameSize, mousePosGame);
            boundaryRect.DrawLines(4f, ColorLight);

            if(curShape != null)
            {
                curShape.DrawLines(2f, RED);
            }

        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            base.DrawUI(uiSize, mousePosUI);

            //Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.07f), new Vector2(0.5f, 1f));
            //string infoText = String.Format("[LMB] Spawn | Object Count: {0}", area.Count);
            //font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }

    }
}
