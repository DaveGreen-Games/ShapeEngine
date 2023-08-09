using ShapeEngine.Lib;
using System.Numerics;

namespace ShapeEngine.Core
{
    /// <summary>
    /// Provides a simple area for managing adding/removing, updating, and drawing of area objects. Does not provide a collision system.
    /// </summary>
    public class Area : IArea
    {
        public int Count
        {
            get
            {
                int count = 0;
                foreach (var objects in layers.Values)
                {
                    count += objects.Count;
                }
                return count;
            }
        }
        public Rect Bounds { get; protected set; }
        public virtual ICollisionHandler? GetCollisionHandler() { return null; }
        public Vector2 ParallaxePosition { get; set; } = new(0f);

        private SortedList<int, List<IAreaObject>> layers = new();
        private List<IAreaObject> uiObjects = new();

        public Area()
        {
            Bounds = new Rect();
        }
        public Area(float x, float y, float w, float h)
        {
            Bounds = new(x, y, w, h);
        }
        public Area(Rect bounds)
        {
            Bounds = bounds;
        }


        public virtual void ResizeBounds(Rect newBounds) { Bounds = newBounds; }
        public bool HasLayer(int layer) { return layers.ContainsKey(layer); }
        public List<IAreaObject> GetAreaObjects(int layer, Predicate<IAreaObject> match) { return HasLayer(layer) ? layers[layer].FindAll(match) : new(); }
        public List<IAreaObject> GetAllGameObjects()
        {
            List<IAreaObject> objects = new();
            foreach (var layerGroup in layers.Values)
            {
                objects.AddRange(layerGroup);
            }
            return objects;
        }
        public List<IAreaObject> GetAllGameObjects(Predicate<IAreaObject> match) { return GetAllGameObjects().FindAll(match); }

        public void AddAreaObject(IAreaObject areaObject)
        {
            int layer = areaObject.AreaLayer;
            if (!layers.ContainsKey(layer)) AddLayer(layer);

            layers[layer].Add(areaObject);
            AreaObjectAdded(areaObject);
            areaObject.AddedToArea(this);
        }
        public void AddAreaObjects(params IAreaObject[] areaObjects) { foreach (var ao in areaObjects) AddAreaObject(ao); }
        public void AddAreaObjects(IEnumerable<IAreaObject> areaObjects) { foreach (var ao in areaObjects) AddAreaObject(ao); }
        public void RemoveAreaObject(IAreaObject areaObject)
        {
            if (layers.ContainsKey(areaObject.AreaLayer))
            {
                bool removed = layers[areaObject.AreaLayer].Remove(areaObject);
                if (removed) AreaObjectRemoved(areaObject);
            }
        }
        public void RemoveAreaObjects(params IAreaObject[] areaObjects)
        {
            foreach (var ao in areaObjects)
            {
                RemoveAreaObject(ao);
            }
        }
        public void RemoveAreaObjects(IEnumerable<IAreaObject> areaObjects)
        {
            foreach (var ao in areaObjects)
            {
                RemoveAreaObject(ao);
            }
        }
        public void RemoveAreaObjects(int layer, Predicate<IAreaObject> match)
        {
            if (layers.ContainsKey(layer))
            {
                var objs = GetAreaObjects(layer, match);
                foreach (var o in objs)
                {
                    RemoveAreaObject(o);
                }
            }
        }
        public void RemoveAreaObjects(Predicate<IAreaObject> match)
        {
            var objs = GetAllGameObjects(match);
            foreach (var o in objs)
            {
                RemoveAreaObject(o);
            }
        }

        protected virtual void AreaObjectAdded(IAreaObject obj) { }
        protected virtual void AreaObjectRemoved(IAreaObject obj) { }

        public virtual void Clear()
        {
            foreach (var layer in layers.Keys)
            {
                ClearLayer(layer);
            }
        }
        public virtual void ClearLayer(int layer)
        {
            if (layers.ContainsKey(layer))
            {
                var objects = layers[layer];
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
        
        protected (Vector2 safePosition, CollisionPoints points) HasLeftBounds(IAreaObject obj)
        {
            Rect bb = obj.GetBoundingBox();
            Vector2 pos = bb.Center;
            Vector2 halfSize = bb.Size * 0.5f;

            Vector2 newPos = pos;
            CollisionPoints points = new();

            if (pos.X + halfSize.X > Bounds.Right)
            {
                newPos.X = Bounds.Right - halfSize.X;
                Vector2 p = new(Bounds.Right, Clamp(pos.Y, Bounds.Bottom, Bounds.Top));
                Vector2 n = new(-1, 0);
                points.Add(new(p, n));
            }
            else if (pos.X - halfSize.X < Bounds.Left)
            {
                newPos.X = Bounds.Left + halfSize.X;
                Vector2 p = new(Bounds.Left, Clamp(pos.Y, Bounds.Bottom, Bounds.Top));
                Vector2 n = new(1, 0);
                points.Add(new(p, n));
            }

            if (pos.Y + halfSize.Y > Bounds.Bottom)
            {
                newPos.Y = Bounds.Bottom - halfSize.Y;
                Vector2 p = new(Clamp(pos.X, Bounds.Left, Bounds.Right), Bounds.Bottom);
                Vector2 n = new(0, -1);
                points.Add(new(p, n));
            }
            else if (pos.Y - halfSize.Y < Bounds.Top)
            {
                newPos.Y =Bounds.Top + halfSize.Y;
                Vector2 p = new(Clamp(pos.X, Bounds.Left, Bounds.Right), Bounds.Top);
                Vector2 n = new(0, 1);
                points.Add(new(p, n));
            }
            return (newPos, points);
        }

        public virtual void DrawDebug(Raylib_CsLo.Color bounds, Raylib_CsLo.Color border, Raylib_CsLo.Color fill)
        {
            DrawRectangleLinesEx(this.Bounds.Rectangle, 15f, bounds);
        }

        public virtual void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            foreach (var layer in layers.Values)
            {
                if (layer.Count > 0) UpdateLayer(dt, mousePosGame, mousePosUI, layer);
            }
        }
        public virtual void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            uiObjects.Clear();
            foreach (var layer in layers.Values)
            {
                var objects = layer;

                for (int j = 0; j < objects.Count; j++)
                {
                    var obj = objects[j];
                    obj.Draw(gameSize, mousePosGame);
                    if (obj.DrawToUI) uiObjects.Add(obj);
                }
            }
        }
        public virtual void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            foreach (var obj in uiObjects)
            {
                obj.DrawUI(uiSize, mousePosUI);
            }
        }

        protected void AddLayer(int layer)
        {
            if (!layers.ContainsKey(layer))
            {
                layers.Add(layer, new());
            }
        }
        protected void UpdateLayer(float dt, Vector2 mousePosGame, Vector2 mousePosUI, List<IAreaObject> objs)
        {
            for (int i = objs.Count - 1; i >= 0; i--)
            {
                IAreaObject obj = objs[i];
                if (obj == null)
                {
                    objs.RemoveAt(i);
                    return;
                }


                obj.UpdateParallaxe(ParallaxePosition);
                obj.Update(dt, mousePosGame, mousePosUI);
                if (obj.IsDead())
                {
                    objs.RemoveAt(i);
                }
                else
                {
                    if (obj.CheckAreaBounds())
                    {
                        var check = HasLeftBounds(obj);
                        if(check.points.Count > 0)
                        {
                            obj.LeftAreaBounds(check.safePosition, check.points);
                        }
                    }
                }

            }
        }


    }
    
    /// <summary>
    /// Provides a simple area for managing adding/removing, updating, drawing, and colliding of area objects. 
    /// </summary>
    public class AreaCollision: Area
    {
        public CollisionHandler Col { get; protected set; }
        public override ICollisionHandler GetCollisionHandler() { return Col; }

        
        public AreaCollision() : base()
        {
            Col = new CollisionHandler(0,0,0,0,0,0);
        }
        public AreaCollision(float x, float y, float w, float h, int rows, int cols) : base(x, y, w, h)
        {
            Col = new CollisionHandler(Bounds, rows, cols);
        }
        public AreaCollision(Rect bounds, int rows, int cols) : base(bounds)
        {
            Col = new CollisionHandler(bounds, rows, cols);
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

        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            Col.Update(dt, mousePosGame, mousePosUI);

            base.Update(dt, mousePosGame, mousePosUI);
        }
        
        public override void DrawDebug(Raylib_CsLo.Color bounds, Raylib_CsLo.Color border, Raylib_CsLo.Color fill)
        {
            base.DrawDebug(bounds, border, fill);
            Col.DebugDraw(border, fill);
        }

        
    }
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

        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            Col.Update(dt, mousePosGame, mousePosUI);

            base.Update(dt, mousePosGame, mousePosUI);
        }

        public override void DrawDebug(Raylib_CsLo.Color bounds, Raylib_CsLo.Color border, Raylib_CsLo.Color fill)
        {
            base.DrawDebug(bounds, border, fill);
            //col.DebugDraw(border, fill);
        }


    }
}




