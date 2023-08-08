using ShapeEngine.Lib;
using ShapeEngine.Timing;
using System.Collections;
using System.Net.Sockets;
using System.Numerics;

namespace ShapeEngine.Core
{
    /*
    public class AreaSlowBehavior : IBehavior
    {
        public SequencerTimedFloat<TimedFloat> UpdateSlowFactors = new();
        protected HashSet<int> affectedLayers;

        public AreaSlowBehavior(params int[] affectedLayers)
        {
            this.affectedLayers = affectedLayers.ToHashSet();
        }

        public BehaviorResult Apply(IAreaObject obj, float delta) { return new(false, delta * UpdateSlowFactors.Total); }
        public HashSet<int> GetAffectedLayers() { return affectedLayers; }
        public void Update(float dt) { UpdateSlowFactors.Update(dt); }
    }
    */
    /*
    public class AreaObjects : List<IAreaObject>
    {
        internal class IAreaObjectComparer : Comparer<IAreaObject>
        {
            public override int Compare(IAreaObject? x, IAreaObject? y)
            {
                if (x == null || y == null) return 0;

                if (x.AreaLayer > y.AreaLayer) return 1;
                else if (x.AreaLayer < y.AreaLayer) return -1;
                else return 0;
            }
        }
        public new void Add(IAreaObject value)
        {
            this.BinarySearch(value, new IAreaObjectComparer());
            int x = this.BinarySearch(value);
            this.Insert((x >= 0) ? x : ~x, value);
        }
    }
    */

    internal class AreaLayer
    {
        public List<IAreaObject> objs = new();
        //public List<IBehavior> behaviors = new();
        public int Layer { get; private set; }
        public AreaLayer(int layer, IArea area) { this.Layer = layer; this.area = area; }

        private IArea area;
        //public TimedFactors UpdateSlowFactors = new();
        //public Dictionary<IGameObject, TimedFactors> GameObjectUpdateSlowFactors = new();
        //public SortedList<float, IGameObject> gameobjects = new();
        /*
        public uint AddSlow(float factor, float duration = -1)
        {
            return UpdateSlowFactors.Add(factor, duration);
        }
        public uint AddSlow(IGameObject obj, float factor, float duration = -1)
        {
            if (!GameObjectUpdateSlowFactors.ContainsKey(obj)) GameObjectUpdateSlowFactors.Add(obj, new());
            return GameObjectUpdateSlowFactors[obj].Add(factor, duration);
        }
        public bool RemoveSlow(uint id) { return UpdateSlowFactors.Remove(id); }
        public bool RemoveSlow(IGameObject obj, uint id)
        {
            if (GameObjectUpdateSlowFactors.ContainsKey(obj)) return GameObjectUpdateSlowFactors[obj].Remove(id);
            return false;
        }
        public void ClearSlow() { UpdateSlowFactors.Clear(); }
        public void ClearSlow(IGameObject obj)
        {
            if (GameObjectUpdateSlowFactors.ContainsKey(obj)) GameObjectUpdateSlowFactors[obj].Clear();
        }
        */
        /*
        public float GetTotalUpdateSlowFactor(IGameObject obj)
        {
            if (GameObjectUpdateSlowFactors.ContainsKey(obj))
            {
                return UpdateSlowFactors.Total * GameObjectUpdateSlowFactors[obj].Total;
            }
            else return UpdateSlowFactors.Total;
        }
        public void Update(float dt)
        {
            UpdateSlowFactors.Update(dt);
            foreach (var kvp in GameObjectUpdateSlowFactors)
            {
                if(!kvp.Key.IsDead()) kvp.Value.Update(dt);
            }
        }
        public void SortGameObjects()
        {
            objs.Sort(delegate (IGameObject x, IGameObject y)
            {
                if (x == null || y == null) return 0;

                if (x.DrawOrder < y.DrawOrder) return -1;
                else if (x.DrawOrder > y.DrawOrder) return 1;
                else return 0;
            });
        }
        */

        public void Add(IAreaObject obj) { objs.Add(obj); }
        public bool Remove(IAreaObject obj)
        {
            bool removed = objs.Remove(obj);
            obj.RemovedFromArea(area);
            return removed;
        }
        public bool Remove(int index)
        {
            if (index < 0 || index >= objs.Count) return false;
            objs[index].RemovedFromArea(area);
            objs.RemoveAt(index);
            return true;

        }

    }

    /// <summary>
    /// Empty area that does nothing.
    /// </summary>
    public class AreaEmpty : IArea
    {
        public bool IsValid() { return false; }
        public Rect Bounds { get { return new(); } }

        public int Count { get { return 0; } }

        private CollisionHandlerEmpty collisionHandlerEmpty = new();
        public ICollisionHandler CollisionHandler { get { return collisionHandlerEmpty; } }

        public Vector2 ParallaxePosition { get { return new Vector2(); } set { } }
        //public float UpdateSlowResistance { get { return 1f; } set { } }
        //public bool DrawToUI { get { return false; } set { } }

        public void AddAreaObject(IAreaObject areaObjects) { }

        public void AddAreaObjects(params IAreaObject[] areaObjects) { }

        public void AddAreaObjects(IEnumerable<IAreaObject> areaObjects) { }

        public void Clear() { }

        public void ClearLayer(int layer) { }

        public void Close() { }

        public void ResizeBounds(Rect newBounds) { }

        //public void DrawDebug(params Raylib_CsLo.Color[] colors) { }

        public List<IAreaObject> GetAllGameObjects() { return new(); }

        public List<IAreaObject> GetAllGameObjects(Predicate<IAreaObject> match) { return new(); }

        public List<IAreaObject> GetAreaObjects(int layer, Predicate<IAreaObject> match) { return new(); }

        public bool HasLayer(int layer) { return false; }

        public void RemoveAreaObject(IAreaObject areaObject) { }

        public void RemoveAreaObjects(Predicate<IAreaObject> match) { }

        public void RemoveAreaObjects(int layer, Predicate<IAreaObject> match) { }

        public void RemoveAreaObjects(params IAreaObject[] areaObjects) { }

        public void RemoveAreaObjects(IEnumerable<IAreaObject> areaObjects) { }

        public void Start() { }

        public void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            
        }

        public void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            
        }

        public void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            
        }
    }

    //hierarcy!!!
    /// <summary>
    /// Provides a simple area for managing adding/removing, updating, drawing, and colliding of area objects. 
    /// </summary>
    public class AreaCollision : IArea
    {
        
        public int Count 
        { 
            get 
            {
                int count = 0;
                foreach (var layerGroup in layers.Values)
                {
                    count += layerGroup.objs.Count;
                }
                return count;
            } 
        }
        public Rect Bounds { get; protected set; }
        protected CollisionHandler col;
        public ICollisionHandler CollisionHandler { get { return col; } }
        public Vector2 ParallaxePosition { get; set; } = new(0f);
        //public float UpdateSlowResistance { get { return 1f; } set { } }
        //public bool DrawToUI { get { return false; } set { } }

        private SortedList<int, AreaLayer> layers = new();
        //protected HashSet<IBehavior> behaviors = new();
        private List<IAreaObject> uiObjects = new();
        public bool IsValid() { return true; }

        public AreaCollision()
        {
            Bounds = new Rect();
            col = new CollisionHandler(0,0,0,0,0,0);
        }
        public AreaCollision(float x, float y, float w, float h, int rows, int cols)
        {
            Bounds = new(x, y, w, h);
            col = new CollisionHandler(Bounds, rows, cols);
        }
        public AreaCollision(Rect bounds, int rows, int cols)
        {
            Bounds = bounds;
            col = new CollisionHandler(bounds, rows, cols);
        }

        public void ResizeBounds(Rect newBounds) { Bounds = newBounds; col.ResizeBounds(newBounds); }
        public bool HasLayer(int layer) { return layers.ContainsKey(layer); }
        public List<IAreaObject> GetAreaObjects(int layer, Predicate<IAreaObject> match) { return HasLayer(layer) ? layers[layer].objs.FindAll(match) : new(); }// gameObjects.ToList().FindAll(match); }
        public List<IAreaObject> GetAllGameObjects()
        {
            List<IAreaObject> objects = new();
            foreach (var layerGroup in layers.Values)
            {
                objects.AddRange(layerGroup.objs);
            }
            return objects;
        }
        //public int GetGameobjectCount()
        //{
        //    int count = 0;
        //    foreach (var layerGroup in layers.Values)
        //    {
        //        count += layerGroup.objs.Count;
        //    }
        //    return count;
        //}
        //public int GetCollidableCount()
        //{
        //    return col.Count;
        //}
        public List<IAreaObject> GetAllGameObjects(Predicate<IAreaObject> match) { return GetAllGameObjects().FindAll(match); }
        
        private void AddLayer(int layer)
        {
            if (!layers.ContainsKey(layer))
            {
                layers.Add(layer, new(layer, this));
                //SortAreaLayerGroups();
            }
        }
        /*
        public void AddCollidable(UCollidable collidable)
        {
            Col.Add(collidable);
        }
        public void AddCollidables(params ICollidable[] collidables)
        {
            foreach (var col in collidables) AddCollidable(col);
        }
        public void AddCollidables(IEnumerable<ICollidable> collidables)
        {
            foreach (var col in collidables) AddCollidable(col);
        }

        public void RemoveCollidable(ICollidable collidable)
        {
            Col.Remove(collidable);
        }
        public void RemoveCollidables(params ICollidable[] collidables)
        {
            foreach (var col in collidables) RemoveCollidable(col);
        }
        public void RemoveCollidables(IEnumerable<ICollidable> collidables)
        {
            foreach (var col in collidables) RemoveCollidable(col);
        }
        */
        public void AddAreaObject(IAreaObject areaObject) 
        {
            int layer = areaObject.AreaLayer;
            if (!layers.ContainsKey(layer)) AddLayer(layer);

            layers[layer].Add(areaObject);
            if (areaObject.HasCollidables()) col.AddRange(areaObject.GetCollidables());
            areaObject.AddedToArea(this);
        }
        public void AddAreaObjects(params IAreaObject[] areaObjects) { foreach (var ao in areaObjects) AddAreaObject(ao); }
        public void AddAreaObjects(IEnumerable<IAreaObject> areaObjects) { foreach (var ao in areaObjects) AddAreaObject(ao); }
        public void RemoveAreaObject(IAreaObject areaObject)
        {
            if (layers.ContainsKey(areaObject.AreaLayer))
            {
                bool removed = layers[areaObject.AreaLayer].Remove(areaObject);
                if (removed)
                {
                    if (areaObject.HasCollidables()) col.RemoveRange(areaObject.GetCollidables());
                }
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

        public void Clear()
        {
            foreach (var layer in layers.Keys)
            {
                ClearLayer(layer);
            }
        }
        public void ClearLayer(int layer)
        {
            if (layers.ContainsKey(layer))
            {
                var layerGroup = layers[layer];
                for (int i = layerGroup.objs.Count - 1; i >= 0; i--)
                {
                    var obj = layerGroup.objs[i];
                    if (obj.HasCollidables()) col.RemoveRange(obj.GetCollidables());
                    //if (obj is ICollidable col) Col.Remove(col);
                    
                    layerGroup.Remove(i);
                }
                layerGroup.objs.Clear();
            }
        }

        /*
        public bool HasBehaviors() { return behaviors.Count > 0; }
        public bool AddBehavior(IBehavior behavior) { return behaviors.Add(behavior); }
        public bool RemoveBehavior(IBehavior behavior) { return behaviors.Remove(behavior); }
        */
        
        public void Start() { }
        public void Close()
        {
            Clear();
            col.Close();
        }
        //public virtual void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        //{
        //    for (int i = 0; i < layers.Count; i++)
        //    {
        //        var layer = layers[i];
        //
        //        for (int j = 0; j < layer.objs.Count; j++)
        //        {
        //            var obj = layer.objs[j];
        //            if(obj.RecievesInput())
        //                obj.HandleInput(dt, mousePosGame, mousePosGame);
        //        }
        //    }
        //}
        public void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            col.Update(dt, mousePosGame, mousePosUI);
            
            //foreach (var b in behaviors) b.Update(dt);

            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if(layer.objs.Count > 0) UpdateLayer(dt, mousePosGame, mousePosUI, layer);
            }
        }
        
        public void DrawDebug(Raylib_CsLo.Color bounds, Raylib_CsLo.Color border, Raylib_CsLo.Color fill)
        {
            DrawRectangleLinesEx(this.Bounds.Rectangle, 15f, bounds);
            col.DebugDraw(border, fill);
        }

        public void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            uiObjects.Clear();
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                
                for (int j = 0; j < layer.objs.Count; j++)
                {
                    var obj = layer.objs[j];
                    obj.Draw(gameSize, mousePosGame);
                    if (obj.DrawToUI) uiObjects.Add(obj);
                }
            }
        }
        public void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            foreach (var obj in uiObjects)
            {
                obj.DrawUI(uiSize, mousePosUI);
            }
        }
        
        /*
        protected List<IBehavior> GetLayerBehaviors(int layer)
        {
            List<IBehavior> result = new();
            foreach (var b in behaviors)
            {
                var layers = b.GetAffectedLayers();
                if(layers.Count <= 0) result.Add(b);
                else if(layers.Contains(layer)) result.Add(b);
            }
            return result;
        }
        */
        private void UpdateLayer(float dt, Vector2 mousePosGame, Vector2 mousePosUI, AreaLayer layer)
        {
            //var behaviors = GetLayerBehaviors(layer.Layer);
            
            for (int i = layer.objs.Count - 1; i >= 0; i--)
            {
                IAreaObject obj = layer.objs[i];
                if (obj == null)
                {
                    layer.Remove(i);
                    return;
                }


                obj.UpdateParallaxe(ParallaxePosition);

                /*
                float delta = dt;
                for (int j = behaviors.Count - 1; j >= 0; j--)
                {
                    var b = layer.behaviors[j];
                    var result = b.Apply(obj, delta);
                    delta = result.newDelta;
                    if (result.finished) { layer.behaviors.RemoveAt(j); }
                }

                float dif = dt - delta;
                dif *= obj.UpdateSlowResistance;
                if (dif > 0f) obj.Update(dt - dif, mousePosGame, mousePosUI);
                else obj.Update(dt, mousePosGame, mousePosUI);
                */
                obj.Update(dt, mousePosGame, mousePosUI);
                if (!obj.IsDead())
                {
                    bool insideBounds = Bounds.OverlapShape(obj.GetBoundingBox());
                    if (!insideBounds) obj.LeftAreaBounds(Bounds);
                }
                else
                {
                    layer.Remove(i);
                    if (obj.HasCollidables()) col.RemoveRange(obj.GetCollidables());
                    //if (obj is ICollidable collidable) Col.Remove(collidable);
                }
                
            }
        }

        
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
                foreach (var layerGroup in layers.Values)
                {
                    count += layerGroup.objs.Count;
                }
                return count;
            }
        }
        public Rect Bounds { get; protected set; }
        public ICollisionHandler CollisionHandler { get { return new CollisionHandlerEmpty(); } }
        public Vector2 ParallaxePosition { get; set; } = new(0f);

        private SortedList<int, AreaLayer> layers = new();
        private List<IAreaObject> uiObjects = new();
        public bool IsValid() { return true; }

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

        public void ResizeBounds(Rect newBounds) { Bounds = newBounds; }
        public bool HasLayer(int layer) { return layers.ContainsKey(layer); }
        public List<IAreaObject> GetAreaObjects(int layer, Predicate<IAreaObject> match) { return HasLayer(layer) ? layers[layer].objs.FindAll(match) : new(); }// gameObjects.ToList().FindAll(match); }
        public List<IAreaObject> GetAllGameObjects()
        {
            List<IAreaObject> objects = new();
            foreach (var layerGroup in layers.Values)
            {
                objects.AddRange(layerGroup.objs);
            }
            return objects;
        }
        public List<IAreaObject> GetAllGameObjects(Predicate<IAreaObject> match) { return GetAllGameObjects().FindAll(match); }

        protected void AddLayer(int layer)
        {
            if (!layers.ContainsKey(layer))
            {
                layers.Add(layer, new(layer, this));
            }
        }
        public void AddAreaObject(IAreaObject areaObject)
        {
            int layer = areaObject.AreaLayer;
            if (!layers.ContainsKey(layer)) AddLayer(layer);

            layers[layer].Add(areaObject);
            areaObject.AddedToArea(this);
        }
        public void AddAreaObjects(params IAreaObject[] areaObjects) { foreach (var ao in areaObjects) AddAreaObject(ao); }
        public void AddAreaObjects(IEnumerable<IAreaObject> areaObjects) { foreach (var ao in areaObjects) AddAreaObject(ao); }
        public void RemoveAreaObject(IAreaObject areaObject)
        {
            if (layers.ContainsKey(areaObject.AreaLayer))
            {
                layers[areaObject.AreaLayer].Remove(areaObject);
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

        public void Clear()
        {
            foreach (var layer in layers.Keys)
            {
                ClearLayer(layer);
            }
        }
        public void ClearLayer(int layer)
        {
            if (layers.ContainsKey(layer))
            {
                var layerGroup = layers[layer];
                for (int i = layerGroup.objs.Count - 1; i >= 0; i--)
                {
                    var obj = layerGroup.objs[i];
                    layerGroup.Remove(i);
                }
                layerGroup.objs.Clear();
            }
        }

        public void Start() { }
        public void Close()
        {
            Clear();
        }
        
        public void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if (layer.objs.Count > 0) UpdateLayer(dt, mousePosGame, mousePosUI, layer);
            }
        }

        public void DrawDebug(Raylib_CsLo.Color bounds, Raylib_CsLo.Color border, Raylib_CsLo.Color fill)
        {
            DrawRectangleLinesEx(this.Bounds.Rectangle, 15f, bounds);
        }

        public void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            uiObjects.Clear();
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];

                for (int j = 0; j < layer.objs.Count; j++)
                {
                    var obj = layer.objs[j];
                    obj.Draw(gameSize, mousePosGame);
                    if (obj.DrawToUI) uiObjects.Add(obj);
                }
            }
        }
        public void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            foreach (var obj in uiObjects)
            {
                obj.DrawUI(uiSize, mousePosUI);
            }
        }

        private void UpdateLayer(float dt, Vector2 mousePosGame, Vector2 mousePosUI, AreaLayer layer)
        {
            for (int i = layer.objs.Count - 1; i >= 0; i--)
            {
                IAreaObject obj = layer.objs[i];
                if (obj == null)
                {
                    layer.Remove(i);
                    return;
                }


                obj.UpdateParallaxe(ParallaxePosition);
                obj.Update(dt, mousePosGame, mousePosUI);
                if (!obj.IsDead())
                {
                    bool insideBounds = Bounds.OverlapShape(obj.GetBoundingBox());
                    if (!insideBounds) obj.LeftAreaBounds(Bounds);
                }
                else
                {
                    layer.Remove(i);
                }

            }
        }


    }

}







//private void SortAreaLayerGroups()
//{
//    var list = layers.Values.ToList();
//    list.Sort(delegate (AreaLayer x, AreaLayer y)
//    {
//        if (x == null || y == null) return 0;
//
//        if (x.Layer < y.Layer) return -1;
//        else if (x.Layer > y.Layer) return 1;
//        else return 0;
//    });
//    sortedLayers = list;
//}
//public void SortGameObjects(List<IGameObject> objectsToSort)
//{
//    objectsToSort.Sort(delegate (IGameObject x, IGameObject y)
//    {
//        if (x == null || y == null) return 0;
//
//        if (x.LayerGroup < y.LayerGroup) return -1;
//        else if (x.LayerGroup > y.LayerGroup) return 1;
//        else
//        {
//            if (x.DrawOrder < y.DrawOrder) return -1;
//            else if (x.DrawOrder > y.DrawOrder) return 1;
//            else return 0;
//        }
//    });
//}
//public Area(Vector2 topLeft, Vector2 bottomRight, int rows, int cols)
//{
//    //float w = bottomRight.X - topLeft.X;
//    //float h = bottomRight.Y - topLeft.Y;
//    //InnerRect = new(topLeft.X, topLeft.Y, w, h);
//    //OuterRect = InnerRect.ScaleSize(2f, new(0.5f)); //SRect.ScaleRectangle(InnerRect, 2f);
//    //Col = new(InnerRect.x, InnerRect.y, InnerRect.width, InnerRect.height, rows, cols);
//    Bounds = new(topLeft, bottomRight);
//    Col = new(Bounds, rows, cols);
//}
//public Area(Vector2 topLeft, Vector2 size, int rows, int cols)
//{
//    //InnerRect = new(topLeft.X, topLeft.Y, w, h);
//    //OuterRect = InnerRect.ScaleSize(2f, new(0.5f)); //SRect.ScaleRectangle(InnerRect, 2f);
//    //Col = new(InnerRect.x, InnerRect.y, InnerRect.width, InnerRect.height, rows, cols);
//}
//public Rect InnerRect { get;protected set;}
//public Rect OuterRect { get; protected set; }
/*
    public class AreaLayer
    {
        protected List<IGameObject> gameObjects = new();
        protected List<IGameObject> uiObjects = new();
        protected Rect inner;
        protected Rect outer; 
        protected CollisionHandler colHandler;


        protected Vector2 parallaxeOffset = new(0f);
        protected float parallaxeScaling = 0f;
        public uint ID { get; protected set; }
        public float ParallaxeSmoothing { get; set; } = 0.1f;
        public float DrawOrder { get; protected set; } = 0f;
        public float UpdateSlowFactor { get; set; } = 1f;
        
        
        public AreaLayer(uint id, Rect inner, Rect outer, CollisionHandler colHandler, float drawOrder = 0f, float parallaxeScaling = 0f)
        {
            this.ID = id;
            this.inner = inner;
            this.outer = outer;
            this.colHandler = colHandler;
            this.parallaxeScaling = parallaxeScaling;
            this.DrawOrder = drawOrder;
        }
        

        public bool IsParallaxeLayer() { return parallaxeScaling != 0f; }
        public bool HasGameObject(IGameObject obj) { return gameObjects.Contains(obj); }
        public List<IGameObject> GetGameObjects() { return gameObjects; }
        public List<IGameObject> GetGameObjects(uint group) { return gameObjects.FindAll(x => x.IsInGroup(group)); }
        public List<IGameObject> GetGameObjects(Predicate<IGameObject> match) { return gameObjects.FindAll(match); }
        public void Clear()
        {
            uiObjects.Clear();
            foreach (var obj in gameObjects)
            {
                obj.Destroy();
                if (obj is ICollidable collidable)
                {
                    colHandler.Remove(collidable);
                }
            }
            gameObjects.Clear();
        }

        public void AddGameObject(IGameObject obj, bool uiDrawing = false)
        {
            if (obj == null) return;
            if (obj is ICollidable collidable) colHandler.Add(collidable);
            gameObjects.Add(obj);
            if (uiDrawing && !uiObjects.Contains(obj)) uiObjects.Add(obj);
            obj.AreaLayerID = this.ID;
            obj.Start();
        }
        public void AddGameObjects(List<IGameObject> newObjects, bool uiDrawing = false)
        {
            foreach (IGameObject obj in newObjects)
            {
                AddGameObject(obj, uiDrawing);
            }
        }

        
        public void RemoveGameObject(IGameObject obj)
        {
            if (obj == null) return;
            if (!gameObjects.Contains(obj)) return;
            if (obj is ICollidable collidable)
            {
                colHandler.Remove(collidable);
            }
            uiObjects.Remove(obj);
            obj.Destroy();
            gameObjects.Remove(obj);
        }
        public void RemoveGameObjects(List<IGameObject> objs)
        {
            foreach (var obj in objs)
            {
                RemoveGameObject(obj);
            }
        }
        public void RemoveGameObjects(Predicate<IGameObject> match)
        {
            var remove = gameObjects.FindAll(match);
            foreach (var obj in remove)
            {
                RemoveGameObject(obj);
            }
        }
        public void RemoveGameObjects(uint group)
        {
            var remove = GetGameObjects(group);
            foreach (var obj in remove)
            {
                RemoveGameObject(obj);
            }
        }


        public virtual void UpdateParallaxe(Vector2 pos)
        {
            parallaxeOffset = SVec.Lerp(parallaxeOffset, pos * parallaxeScaling, ParallaxeSmoothing);
        }
        public virtual void Draw()
        {
            SortGameObjects();
            foreach (IGameObject obj in gameObjects)
            {
                if (SGeometry.OverlapRectRect(outer, obj.GetBoundingBox())) { obj.Draw(); }
            }
        }
        public virtual void DrawUI(Vector2 uiSize)
        {
            foreach (IGameObject obj in uiObjects)
            {
                obj.DrawUI(uiSize);
            }
        }
        public virtual void Update(float dt, float slowFactor)
        {
            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {
                IGameObject obj = gameObjects[i];
                if (obj == null)
                {
                    gameObjects.RemoveAt(i);
                    return;
                }

                if(IsParallaxeLayer()) obj.AreaLayerOffset = parallaxeOffset;
                float curSlowFactor = slowFactor * UpdateSlowFactor * obj.UpdateSlowFactor;// * obj.UpdateSlowResistance;
                float dif = dt - (dt * curSlowFactor);
                dif *= obj.UpdateSlowResistance;
                obj.Update(dt - dif);
                bool insideInner = SGeometry.OverlapRectRect(inner, obj.GetBoundingBox());
                bool insideOuter = false;
                if (insideInner) insideOuter = true;
                else insideOuter = SGeometry.OverlapRectRect(outer, obj.GetBoundingBox());
                obj.OnPlayfield(insideInner, insideOuter);

                if (obj.IsDead() || !insideOuter)
                {
                    obj.Destroy();
                    gameObjects.RemoveAt(i);
                    uiObjects.Remove(obj);
                    if (obj is ICollidable collidable)
                    {
                        colHandler.Remove(collidable);
                    }
                }
            }
        }
        //public virtual void MonitorHasChanged()
        //{
        //    inner = ScreenHandler.GameArea();
        //    foreach (var go in gameObjects)
        //    {
        //        go.MonitorHasChanged();
        //    }
        //    colHandler.UpdateArea(inner);
        //}
        protected virtual void SortGameObjects()
        {
            gameObjects.Sort(delegate (IGameObject x, IGameObject y)
            {
                if (x == null || y == null) return 0;

                if (x.DrawOrder < y.DrawOrder) return -1;
                else if (x.DrawOrder > y.DrawOrder) return 1;
                else return 0;
            });
        }

    }
    */