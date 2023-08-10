using ShapeEngine.Lib;
using ShapeEngine.Timing;
using System.Net;
using System.Net.NetworkInformation;
using System.Numerics;

namespace ShapeEngine.Core
{
    /*
    public class AreaLayerInjector
    {
        public delegate void InjectorAction(float dt, List<IAreaObject> objs);
        public InjectorAction Injector;
        private float duration;
        private float timer;

        public AreaLayerInjector(InjectorAction injector, float duration)
        {
            this.Injector = injector;
            this.duration = duration;
            this.timer = duration;
        }

        public bool Update(float dt)
        {
            if (duration <= 0f) return false;
            timer -= dt;
            return timer <= 0f && duration > 0f;
        }

    }
    */

    /// <summary>
    /// Can be used to manipulate the delta value each area object recieves. 
    /// The layers affected can be specified. If no layers are specified all layers are affected!
    /// The final factor can not be negative and will be clamped to 0.
    /// </summary>
    
    
    
    public sealed class AreaDeltaFactor : IAreaDeltaFactor
    {
        private static uint idCounter = 0;
        private static uint NextID { get { return idCounter++; } }
        private uint id  = NextID;

        private float delayTimer = 0f;
        private float cur;
        private float timer;
        private float duration;
        private float from;
        private float to;
        private TweenType tweenType;
        HashSet<int> layerMask;

        /// <summary>
        /// Delta factors are applied from lowest first to highest last
        /// </summary>
        public int ApplyOrder { get; set; } = 0;

        /// <summary>
        /// Create a new Area Layer Slow Factor. The slow factor affects the delta time of each area object in the affected layers.
        /// A slow factor > 1 speeds up objects, a slow factor < 1 slow objects down, and a slow factor == 1 does not affect the speed at all.
        /// </summary>
        /// <param name="from">The start slow factor.</param>
        /// <param name="to">The end slow factor.</param>
        /// <param name="duration">The duration of the slow effect. Duration <= 0 means infinite duration and no tweening!</param>
        /// <param name="delay">The delay until the duration starts and the factor is applied.</param>
        /// <param name="tweenType">The tweentype for the slow factor.</param>
        /// <param name="layerMask">The mask for all area layers that will be affected. An empty layer mask affects all layers!</param>
        public AreaDeltaFactor(float from, float to, float duration, float delay = -1f, TweenType tweenType = TweenType.LINEAR, params int[] layerMask)
        {
            this.delayTimer = delay;
            this.cur = from;
            this.from = from;
            this.to = to;
            this.duration = duration;
            this.timer = duration;
            this.tweenType = tweenType;
            this.layerMask = layerMask.ToHashSet();
        }
        /// <summary>
        /// Create a new Area Layer Slow Factor. The slow factor affects the delta time of each area object in the affected layers.
        /// A slow factor > 1 speeds up objects, a slow factor < 1 slow objects down, and a slow factor == 1 does not affect the speed at all.
        /// The linear tween type is used.
        /// </summary>
        /// <param name="from">The start slow factor.</param>
        /// <param name="to">The end slow factor.</param>
        /// <param name="duration">The duration of the slow effect. Duration <= 0 means infinite duration and no tweening!</param>
        /// <param name="delay">The delay until the duration starts and the factor is applied.</param>
        /// <param name="layerMask">The mask for all area layers that will be affected. An empty layer mask affects all layers!</param>
        public AreaDeltaFactor(float from, float to, float duration, float delay = -1f, params int[] layerMask)
        {
            this.delayTimer = delay;
            this.cur = from;
            this.from = from;
            this.to = to;
            this.duration = duration;
            this.timer = duration;
            this.tweenType = TweenType.LINEAR;
            this.layerMask = layerMask.ToHashSet();
        }
        /// <summary>
        /// Create a new Area Layer Slow Factor. The slow factor affects the delta time of each area object in the affected layers.
        /// A slow factor > 1 speeds up objects, a slow factor < 1 slow objects down, and a slow factor == 1 does not affect the speed at all.
        /// Creates a slow factor with infinite duration and therefore now tweening.
        /// </summary>
        /// <param name="factor">The slow factor that should be applied.</param>
        /// <param name="delay">The delay until the factor is applied.</param>
        /// <param name="layerMask">The mask for all area layers that will be affected. An empty layer mask affects all layers!</param>
        public AreaDeltaFactor(float factor, float delay, params int[] layerMask)
        {
            this.delayTimer = delay;
            this.cur = factor;
            this.from = -1;
            this.to = -1f;
            this.duration = -1;
            this.timer = -1;
            this.tweenType = TweenType.LINEAR;
            this.layerMask = layerMask.ToHashSet();
        }
        /// <summary>
        /// Create a new Area Layer Slow Factor. The slow factor affects the delta time of each area object in the affected layers.
        /// A slow factor > 1 speeds up objects, a slow factor < 1 slow objects down, and a slow factor == 1 does not affect the speed at all.
        /// Creates a slow factor with infinite duration and therefore now tweening.
        /// </summary>
        /// <param name="factor">The slow factor that should be applied.</param>
        /// <param name="layerMask">The mask for all area layers that will be affected. An empty layer mask affects all layers!</param>
        public AreaDeltaFactor(float factor, params int[] layerMask)
        {
            this.delayTimer = -1f;
            this.cur = factor;
            this.from = -1;
            this.to = -1f;
            this.duration = -1;
            this.timer = -1;
            this.tweenType = TweenType.LINEAR;
            this.layerMask = layerMask.ToHashSet();
        }

        public uint GetID() { return id; }
        public bool IsAffectingLayer(int layer)
        {
            if (delayTimer > 0f) return false;
            if (layerMask.Count <= 0) return true;
            else return layerMask.Contains(layer);
        }
        public bool Update(float dt)
        {
            if (delayTimer > 0f)
            {
                delayTimer -= dt;
                return false;
            }

            if (duration <= 0f) return false;
            
            timer -= dt;
            if (timer < 0f) timer = 0f;
            
            if(from != to)
            {
                float f = 1.0f - (timer / duration);
                cur = STween.Tween(from, to, f, tweenType);
            }
            
            return IsFinished();
        }
        public float Apply(float totalDeltaFactor)
        {
            if(cur <= 0f) return totalDeltaFactor;
            else return totalDeltaFactor * cur;
        }
        private bool IsFinished() { return timer <= 0f && duration > 0f; }
        
        
        //public float GetCurFactor()
        //{
        //    if (cur <= 0f) return 0f;
        //    else return cur;
        //}
    }


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
        private Dictionary<uint, IAreaDeltaFactor> deltaFactors = new();
        private List<IAreaDeltaFactor> sortedDeltaFactors = new();

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

        public void AddDeltaFactor(IAreaDeltaFactor deltaFactor)
        {
            var id = deltaFactor.GetID();
            if (deltaFactors.ContainsKey(id)) deltaFactors[id] = deltaFactor;
            else deltaFactors.Add(id, deltaFactor);
        }
        public bool RemoveDeltaFactor(IAreaDeltaFactor deltaFactor) { return deltaFactors.Remove(deltaFactor.GetID()); }
        public bool RemoveDeltaFactor(uint id) { return deltaFactors.Remove(id); }

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
            List<IAreaDeltaFactor> allDeltaFactors = deltaFactors.Values.ToList();
            for (int i = allDeltaFactors.Count - 1; i >= 0; i--)
            {
                var deltaFactor = allDeltaFactors[i];
                bool finished = deltaFactor.Update(dt);
                if (finished) RemoveDeltaFactor(deltaFactor);
            }

            allDeltaFactors.Sort((a, b) =>
            {
                if (a.ApplyOrder > b.ApplyOrder) return 1;
                else if (a.ApplyOrder < b.ApplyOrder) return -1;
                else return 0;
            }
            );
            sortedDeltaFactors = allDeltaFactors;

            foreach (var layer in layers)
            {
                UpdateLayer(dt, mousePosGame, mousePosUI, layer.Key);
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
        protected void UpdateLayer(float dt, Vector2 mousePosGame, Vector2 mousePosUI, int layer)
        {
            List<IAreaObject> objs = layers[layer];
            if (objs.Count <= 0) return;

            float totalDeltaFactor = 1f;
            foreach (var deltaFactor in sortedDeltaFactors)
            {
                if (deltaFactor.IsAffectingLayer(layer))
                {
                    totalDeltaFactor = deltaFactor.Apply(totalDeltaFactor);
                }
            }

            dt *= totalDeltaFactor;

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
                if(totalDeltaFactor != 1f) obj.DeltaFactorApplied(totalDeltaFactor);
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




