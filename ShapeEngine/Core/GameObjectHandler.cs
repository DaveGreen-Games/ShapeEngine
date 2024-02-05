using ShapeEngine.Lib;
using ShapeEngine.Core.Shapes;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core
{


    /// <summary>
    /// Provides a simple area for managing adding/removing, updating, and drawing of area objects. Does not provide a collision system.
    /// </summary>
    public class GameObjectHandler : IUpdateable, IDrawable, IBounds
    {
        public int Count
        {
            get
            {
                int count = 0;
                foreach (var objects in allObjects.Values)
                {
                    count += objects.Count;
                }
                return count;
            }
        }
        public Rect Bounds { get; protected set; }
        public virtual CollisionHandler? GetCollisionHandler() { return null; }
        public Vector2 ParallaxePosition { get; set; } = new(0f);

        private SortedList<int, List<IGameObject>> allObjects = new();

        //private Dictionary<uint, List<IAreaObject>> drawToScreenTextureObjects = new();
        //private List<IAreaObject> drawToScreenObjects = new();
        private List<IGameObject> drawToGameTextureObjects = new();
        private List<IGameObject> drawToUITextureObjects = new();


        // private Dictionary<uint, IHandlerDeltaFactor> deltaFactors = new();
        // private List<IHandlerDeltaFactor> sortedDeltaFactors = new();

        public GameObjectHandler()
        {
            Bounds = new Rect();
        }
        public GameObjectHandler(float x, float y, float w, float h)
        {
            Bounds = new(x, y, w, h);
        }
        public GameObjectHandler(Rect bounds)
        {
            Bounds = bounds;
        }

        // public void AddDeltaFactor(IHandlerDeltaFactor deltaFactor)
        // {
        //     var id = deltaFactor.GetID();
        //     if (deltaFactors.ContainsKey(id)) deltaFactors[id] = deltaFactor;
        //     else deltaFactors.Add(id, deltaFactor);
        // }
        // public bool RemoveDeltaFactor(IHandlerDeltaFactor deltaFactor) { return deltaFactors.Remove(deltaFactor.GetID()); }
        // public bool RemoveDeltaFactor(uint id) { return deltaFactors.Remove(id); }

        public virtual void ResizeBounds(Rect newBounds) { Bounds = newBounds; }
        public bool HasLayer(int layer) { return allObjects.ContainsKey(layer); }
        public List<IGameObject> GetAreaObjects(int layer, Predicate<IGameObject> match) { return HasLayer(layer) ? allObjects[layer].FindAll(match) : new(); }
        public List<IGameObject> GetAllGameObjects()
        {
            List<IGameObject> objects = new();
            foreach (var layerGroup in allObjects.Values)
            {
                objects.AddRange(layerGroup);
            }
            return objects;
        }
        public List<IGameObject> GetAllGameObjects(Predicate<IGameObject> match) { return GetAllGameObjects().FindAll(match); }

        public void AddAreaObject(IGameObject gameObject)
        {
            int layer = gameObject.Layer;
            if (!allObjects.ContainsKey(layer)) AddLayer(layer);

            allObjects[layer].Add(gameObject);
            AreaObjectAdded(gameObject);
            gameObject.AddedToHandler(this);
        }
        public void AddAreaObjects(params IGameObject[] areaObjects) { foreach (var ao in areaObjects) AddAreaObject(ao); }
        public void AddAreaObjects(IEnumerable<IGameObject> areaObjects) { foreach (var ao in areaObjects) AddAreaObject(ao); }
        public void RemoveAreaObject(IGameObject gameObject)
        {
            if (allObjects.ContainsKey(gameObject.Layer))
            {
                bool removed = allObjects[gameObject.Layer].Remove(gameObject);
                if (removed) AreaObjectRemoved(gameObject);
            }
        }
        public void RemoveAreaObjects(params IGameObject[] areaObjects)
        {
            foreach (var ao in areaObjects)
            {
                RemoveAreaObject(ao);
            }
        }
        public void RemoveAreaObjects(IEnumerable<IGameObject> areaObjects)
        {
            foreach (var ao in areaObjects)
            {
                RemoveAreaObject(ao);
            }
        }
        public void RemoveAreaObjects(int layer, Predicate<IGameObject> match)
        {
            if (allObjects.ContainsKey(layer))
            {
                var objs = GetAreaObjects(layer, match);
                foreach (var o in objs)
                {
                    RemoveAreaObject(o);
                }
            }
        }
        public void RemoveAreaObjects(Predicate<IGameObject> match)
        {
            var objs = GetAllGameObjects(match);
            foreach (var o in objs)
            {
                RemoveAreaObject(o);
            }
        }

        protected virtual void AreaObjectAdded(IGameObject obj) { }
        protected virtual void AreaObjectRemoved(IGameObject obj) { }

        public virtual void Clear()
        {
            //drawToScreenObjects.Clear();
            //drawToScreenTextureObjects.Clear();
            drawToGameTextureObjects.Clear();
            drawToUITextureObjects.Clear();

            foreach (var layer in allObjects.Keys)
            {
                ClearLayer(layer);
            }
        }
        public virtual void ClearLayer(int layer)
        {
            if (allObjects.ContainsKey(layer))
            {
                var objects = allObjects[layer];
                for (int i = objects.Count - 1; i >= 0; i--)
                {
                    var obj = objects[i];
                    AreaObjectRemoved(obj);
                    objects.RemoveAt(i);
                }
                objects.Clear();
            }
        }

        public virtual void Start() { }
        public virtual void Close()
        {
            Clear();
        }
        
        protected (Vector2 safePosition, CollisionPoints points) HasLeftBounds(IGameObject obj)
        {
            Rect bb = obj.GetBoundingBox();
            Vector2 pos = bb.Center;
            Vector2 halfSize = bb.Size * 0.5f;

            Vector2 newPos = pos;
            CollisionPoints points = new();

            if (pos.X + halfSize.X > Bounds.Right)
            {
                newPos.X = Bounds.Right - halfSize.X;
                Vector2 p = new(Bounds.Right, ShapeMath.Clamp(pos.Y, Bounds.Bottom, Bounds.Top));
                Vector2 n = new(-1, 0);
                points.Add(new(p, n));
            }
            else if (pos.X - halfSize.X < Bounds.Left)
            {
                newPos.X = Bounds.Left + halfSize.X;
                Vector2 p = new(Bounds.Left, ShapeMath.Clamp(pos.Y, Bounds.Bottom, Bounds.Top));
                Vector2 n = new(1, 0);
                points.Add(new(p, n));
            }

            if (pos.Y + halfSize.Y > Bounds.Bottom)
            {
                newPos.Y = Bounds.Bottom - halfSize.Y;
                Vector2 p = new(ShapeMath.Clamp(pos.X, Bounds.Left, Bounds.Right), Bounds.Bottom);
                Vector2 n = new(0, -1);
                points.Add(new(p, n));
            }
            else if (pos.Y - halfSize.Y < Bounds.Top)
            {
                newPos.Y =Bounds.Top + halfSize.Y;
                Vector2 p = new(ShapeMath.Clamp(pos.X, Bounds.Left, Bounds.Right), Bounds.Top);
                Vector2 n = new(0, 1);
                points.Add(new(p, n));
            }
            return (newPos, points);
        }

        public virtual void DrawDebug(ColorRgba bounds, ColorRgba border, ColorRgba fill)
        {
            this.Bounds.DrawLines(15f, bounds);
            // Raylib.DrawRectangleLinesEx(this.Bounds.Rectangle, 15f, bounds.ToRayColor());
        }

        
        public virtual void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            drawToGameTextureObjects.Clear();
            drawToUITextureObjects.Clear();
            
            foreach (var layer in allObjects)
            {
                List<IGameObject> objs = allObjects[layer.Key];
                if (objs.Count <= 0) return;

                for (int i = objs.Count - 1; i >= 0; i--)
                {
                    IGameObject obj = objs[i];
                    if (obj == null)
                    {
                        objs.RemoveAt(i);
                        return;
                    }

                    obj.UpdateParallaxe(ParallaxePosition);
                    
                    if (obj.DrawToGame(game.Area)) drawToGameTextureObjects.Add(obj);
                    if (obj.DrawToGameUI(ui.Area)) drawToUITextureObjects.Add(obj);
                    
                    obj.Update(dt,deltaSlow, game, ui);
                    
                    if (obj.IsDead())
                    {
                        objs.RemoveAt(i);

                    }
                    else
                    {
                        if (obj.CheckHandlerBounds())
                        {
                            var check = HasLeftBounds(obj);
                            if (check.points.Count > 0)
                            {
                                obj.LeftHandlerBounds(check.safePosition, check.points);
                            }
                        }
                    }

                }
            }
        }
        public virtual void DrawGame(ScreenInfo game)
        {
            foreach (var obj in drawToGameTextureObjects)
            {
                obj.DrawGame(game);
            }
        }
        public virtual void DrawGameUI(ScreenInfo ui)
        {
            foreach (var obj in drawToUITextureObjects)
            {
                obj.DrawGameUI(ui);
            }
        }


        

        protected void AddLayer(int layer)
        {
            if (!allObjects.ContainsKey(layer))
            {
                allObjects.Add(layer, new());
            }
        }
        

        
    }
    
    /// <summary>
    /// Provides a simple area for managing adding/removing, updating, drawing, and colliding of area objects. 
    /// </summary>
    public class GameObjectHandlerCollision: GameObjectHandler
    {
        protected CollisionHandler col;
        public override CollisionHandler GetCollisionHandler() { return col; }

        
        public GameObjectHandlerCollision() : base()
        {
            col = new CollisionHandler(0,0,0,0,0,0);
        }
        public GameObjectHandlerCollision(float x, float y, float w, float h, int rows, int cols) : base(x, y, w, h)
        {
            col = new CollisionHandler(Bounds, rows, cols);
        }
        public GameObjectHandlerCollision(Rect bounds, int rows, int cols) : base(bounds)
        {
            col = new CollisionHandler(bounds, rows, cols);
        }

        public override void ResizeBounds(Rect newBounds) { Bounds = newBounds; col.ResizeBounds(newBounds); }

        protected override void AreaObjectAdded(IGameObject obj)
        {
            if (obj.HasCollidables()) col.AddRange(obj.GetCollidables());
        }
        protected override void AreaObjectRemoved(IGameObject obj)
        {
            if (obj.HasCollidables()) col.RemoveRange(obj.GetCollidables());
        }

        public override void Clear()
        {
            base.Clear();
            col.Clear();
        }

        public override void Close()
        {
            base.Close();
            col.Close();
        }

        public override void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            col.Update();

            base.Update(dt, deltaSlow, game, ui);
        }
        
        public override void DrawDebug(ColorRgba bounds, ColorRgba border, ColorRgba fill)
        {
            base.DrawDebug(bounds, border, fill);
            col.DebugDraw(border, fill);
        }

        
    }
    
    /*
    public class AreaCollision<TCollisionHandler> : Area where TCollisionHandler : ICollisionHandler
    {
        public TCollisionHandler Col { get; protected set; }
        public override ICollisionHandler GetCollisionHandler() { return Col; }


        public AreaCollision(TCollisionHandler col) : base()
        {
            this.Col = col;
        }
        public AreaCollision(float x, float y, float w, float h, TCollisionHandler col) : base(x, y, w, h)
        {
            this.Col = col;
        }
        public AreaCollision(Rect bounds, TCollisionHandler col) : base(bounds)
        {
            this.Col = col;
        }

        public override void ResizeBounds(Rect newBounds) { Bounds = newBounds; Col.ResizeBounds(newBounds); }

        protected override void AreaObjectAdded(IAreaObject obj)
        {
            if (obj.HasCollidables()) Col.AddRange(obj.GetCollidables());
        }
        protected override void AreaObjectRemoved(IAreaObject obj)
        {
            if (obj.HasCollidables()) Col.RemoveRange(obj.GetCollidables());
        }


        public override void Close()
        {
            Clear();
            Col.Close();
        }

        public override void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui)
        {
            Col.Update(dt);

            base.Update(dt, mousePosScreen, game, ui);
        }

        public override void DrawDebug(Raylib_CsLo.Color bounds, Raylib_CsLo.Color border, Raylib_CsLo.Color fill)
        {
            base.DrawDebug(bounds, border, fill);
            //col.DebugDraw(border, fill);
        }


    }
    */
}




