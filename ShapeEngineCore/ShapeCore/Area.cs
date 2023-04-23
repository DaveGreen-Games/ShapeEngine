using ShapeLib;
using ShapeTiming;
using System.Numerics;
using ShapeCore;
using System.Reflection.Emit;

namespace ShapeCore
{
    //public class AreaLayer
    //{
    //    public readonly List<IGameObject> gameObjects = new();
    //    public float ParallaxeScaling { get; set; } = 0f;
    //    public float ParallaxeSmoothing { get; set; } = 0.1f;
    //    public float UpdateSlowFactor { get; set; } = 1f;
    //    public ITimedValues UpdateSlowFactors { get; } = new TimedFactors(); 
    //    public int Layer { get; private set; }
    //    public Vector2 ParallaxeOffset { get; protected set; } = new(0f);
    //    
    //    public AreaLayer(int layer, float parallaxeScaling, float parallaxeSmoothing = 0.1f)
        //{
        //    this.Layer = layer;
        //    this.ParallaxeScaling = parallaxeScaling;
        //    this.ParallaxeSmoothing = parallaxeSmoothing;
        //}
    //    public AreaLayer(int layer)
        //{
        //    this.Layer = layer;
        //    this.ParallaxeScaling = 0f;
        //}
    //    public bool IsParallaxe() { return ParallaxeScaling != 0f; }
    //    public virtual void UpdateParallaxe(Vector2 pos)
    //    {
    //        ParallaxeOffset = ParallaxeOffset.Lerp(pos * ParallaxeScaling, ParallaxeSmoothing);
    //    }


    //}
    
    public class Area
    {
        protected class AreaLayer
        {
            public List<IGameObject> objs = new();
            public int Layer { get; private set; }
            public AreaLayer(int layer) { this.Layer = layer; }
            public TimedFactors UpdateSlowFactors = new();
            public Dictionary<IGameObject, TimedFactors> GameObjectUpdateSlowFactors = new();
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
            public void Add(IGameObject obj) { objs.Add(obj); }
            public bool Remove(IGameObject obj) 
            { 
                bool removed = objs.Remove(obj);
                GameObjectUpdateSlowFactors.Remove(obj);
                obj.Destroy();
                return removed;
            }
            public bool RemoveAt(int index)
            {
                if(index < 0 || index >= objs.Count) return false;
                var obj = objs[index];
                objs.RemoveAt(index);
                GameObjectUpdateSlowFactors.Remove(obj);
                obj.Destroy();
                return true;

            }
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
        }

        public Rect InnerRect { get;protected set;}
        public Rect OuterRect { get; protected set; }
        public CollisionHandler colHandler { get; protected set; }
        public Vector2 ParallaxePosition { get; set; } = new(0f);

        protected Dictionary<int, AreaLayer> layers = new();
        protected List<AreaLayer> sortedLayers = new();
        protected List<IGameObject> uiObjects = new();
        private TimedFactors UpdateSlowFactors { get; } = new TimedFactors();
       
        
        public Area()
        {
            InnerRect = new();
            OuterRect = new();
            colHandler = new(0,0,0,0,0,0);
        }
        public Area(float x, float y, float w, float h, int rows, int cols)
        {
            InnerRect = new(x, y, w, h);
            OuterRect = SRect.ScaleRectangle(InnerRect, 2f);
            colHandler = new(InnerRect.x, InnerRect.y, InnerRect.width, InnerRect.height, rows, cols);
            //DefaultID = SID.NextID;
            //AddLayer(DefaultID);
        }
        public Area(Vector2 topLeft, Vector2 bottomRight, int rows, int cols)
        {
            float w = bottomRight.X - topLeft.X;
            float h = bottomRight.Y - topLeft.Y;
            InnerRect = new(topLeft.X, topLeft.Y, w, h);
            OuterRect = SRect.ScaleRectangle(InnerRect, 2f);
            colHandler = new(InnerRect.x, InnerRect.y, InnerRect.width, InnerRect.height, rows, cols);
            //DefaultID = SID.NextID;
            //AddLayer(DefaultID);
        }
        public Area(Vector2 topLeft, float w, float h, int rows, int cols)
        {
            InnerRect = new(topLeft.X, topLeft.Y, w, h);
            OuterRect = SRect.ScaleRectangle(InnerRect, 2f);
            colHandler = new(InnerRect.x, InnerRect.y, InnerRect.width, InnerRect.height, rows, cols);
            //DefaultID = SID.NextID;
            //AddLayer(DefaultID);
        }
        public Area(Rect area, int rows, int cols)
        {
            InnerRect = area;
            OuterRect = SRect.ScaleRectangle(InnerRect, 2f);
            colHandler = new(InnerRect.x, InnerRect.y, InnerRect.width, InnerRect.height, rows, cols);
            //DefaultID = SID.NextID;
            //AddLayer(DefaultID);
        }
       
        public bool HasLayer(int layer) { return layers.ContainsKey(layer); }
        
        public uint AddSlow(float factor, float duration = -1) { return UpdateSlowFactors.Add(factor, duration); }
        public uint AddSlow(int layer, float factor, float duration = -1) { return HasLayer(layer) ? layers[layer].AddSlow(factor, duration) : 0; }
        public uint AddSlow(IGameObject obj, float factor, float duration = -1)
        {
            if (HasLayer(obj.AreaLayer))
            {
                return layers[obj.AreaLayer].AddSlow(obj, factor, duration);
            }
            return 0;
        }
        
        public bool RemoveSlow(uint id)
        {
            return UpdateSlowFactors.Remove(id);
        }
        public bool RemoveSlow(uint id, int layer)
        {
            if (layers.ContainsKey(layer)) return layers[layer].RemoveSlow(id);
            else return false;
        }
        public bool RemoveSlow(uint id, IGameObject obj)
        {
            if (layers.ContainsKey(obj.AreaLayer))
            {
                return layers[obj.AreaLayer].RemoveSlow(obj, id);
            }
            return false;
        }
       
        public void ClearSlow() { UpdateSlowFactors.Clear(); }
        public void ClearSlow(int layer) { if (layers.ContainsKey(layer)) layers[layer].ClearSlow(); }
        public void ClearSlow(IGameObject obj) { if(layers.ContainsKey(obj.AreaLayer)) layers[obj.AreaLayer].ClearSlow(obj); }
        
        public List<IGameObject> GetGameObjects(int layer, Predicate<IGameObject> match) { return HasLayer(layer) ? layers[layer].objs.FindAll(match) : new(); }// gameObjects.ToList().FindAll(match); }
        public List<IGameObject> GetAllGameObjects()
        {
            List<IGameObject> objects = new();
            foreach (var layerGroup in layers.Values)
            {
                objects.AddRange(layerGroup.objs);
            }
            return objects;
        }
        public List<IGameObject> GetAllGameObjects(Predicate<IGameObject> match) { return GetAllGameObjects().FindAll(match); }
        
        protected void AddLayer(int layer)
        {
            if (!layers.ContainsKey(layer))
            {
                layers.Add(layer, new(layer));
                SortAreaLayerGroups();
            }
        }
        public void AddGameObject(IGameObject gameObject) 
        {
            int layer = gameObject.AreaLayer;
            if (!layers.ContainsKey(layer)) AddLayer(layer);

            layers[layer].Add(gameObject);
        }
        public void AddGameObjects(params IGameObject[] gameObjects) { foreach (var go in gameObjects) AddGameObject(go); }
        public bool RemoveGameObject(IGameObject gameObject)
        {
            if (layers.ContainsKey(gameObject.AreaLayer))
            {
                bool removed = layers[gameObject.AreaLayer].Remove(gameObject);
                if (removed)
                {
                    if (gameObject is ICollidable collidable)
                    {
                        colHandler.Remove(collidable);
                    }
                }
                return removed;
            }
            return false;
        }
        public void RemoveGameObjects(int layer, Predicate<IGameObject> match)
        {
            if (layers.ContainsKey(layer))
            {
                var objs = GetGameObjects(layer, match);
                foreach (var o in objs)
                {
                    RemoveGameObject(o);
                }
            }
        }
        public void RemoveGameObjects(Predicate<IGameObject> match)
        {
            var objs = GetAllGameObjects(match);
            foreach (var o in objs)
            {
                RemoveGameObject(o);
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
                foreach (var obj in layerGroup.objs)
                {
                    layerGroup.Remove(obj);
                    if (obj is ICollidable collidable)
                    {
                        colHandler.Remove(collidable);
                    }
                }
                layerGroup.objs.Clear();
            }
        }

        
        public virtual void Start() { }
        public virtual void Close()
        {
            Clear();
            colHandler.Close();
        }
        public virtual void Update(float dt)
        {
            colHandler.Update(dt);
            UpdateSlowFactors.Update(dt);
            
            for (int i = 0; i < sortedLayers.Count; i++)
            {
                var layer = sortedLayers[i];
                UpdateLayer(dt, layer);
            }
        }
        public virtual void Draw()
        {
            if (DEBUG_DRAWHELPERS)
            {
                DrawRectangleLinesEx(this.InnerRect.Rectangle, 15f, DEBUG_AreaInnerColor);
                DrawRectangleLinesEx(this.OuterRect.Rectangle, 15f, DEBUG_AreaOuterColor);
                colHandler.DebugDrawGrid(DEBUG_CollisionHandlerBorder, DEBUG_CollisionHandlerFill);
            }

            uiObjects.Clear();
            for (int i = 0; i < sortedLayers.Count; i++)
            {
                var layer = sortedLayers[i];
                layer.SortGameObjects();
                foreach (IGameObject obj in layer.objs)
                {
                    if (SGeometry.OverlapRectRect(OuterRect, obj.GetBoundingBox())) 
                    { 
                        obj.Draw();
                        if (obj.DrawToUI) uiObjects.Add(obj);
                    }
                }
            }
        }
        public virtual void DrawUI(Vector2 uiSize)
        {
            foreach (var obj in uiObjects)
            {
                obj.DrawUI(uiSize);
            }
        }
        protected virtual void UpdateLayer(float dt, AreaLayer layer)
        {
            layer.Update(dt);
            
            for (int i = layer.objs.Count - 1; i >= 0; i--)
            {
                IGameObject obj = layer.objs[i];
                if (obj == null)
                {
                    layer.RemoveAt(i);
                    return;
                }

                obj.UpdateParallaxe(ParallaxePosition);
                float curSlowFactor =  UpdateSlowFactors.Total * layer.GetTotalUpdateSlowFactor(obj);// slowFactor * obj.UpdateSlowFactor;
                float dif = dt - (dt * curSlowFactor);
                dif *= obj.UpdateSlowResistance;
                
                if(dif > 0f) obj.Update(dt - dif);
                
                bool insideInner = SGeometry.OverlapRectRect(InnerRect, obj.GetBoundingBox());
                bool insideOuter = false;
                if (insideInner) insideOuter = true;
                else insideOuter = SGeometry.OverlapRectRect(OuterRect, obj.GetBoundingBox());
                obj.OnPlayfield(insideInner, insideOuter);

                if (obj.IsDead() || !insideOuter)
                {
                    obj.Destroy();
                    layer.RemoveAt(i);
                    if (obj is ICollidable collidable)
                    {
                        colHandler.Remove(collidable);
                    }
                }
                
            }
        }
        
        private void SortAreaLayerGroups()
        {
            var list = layers.Values.ToList();
            list.Sort(delegate (AreaLayer x, AreaLayer y)
            {
                if (x == null || y == null) return 0;
        
                if (x.Layer < y.Layer) return -1;
                else if (x.Layer > y.Layer) return 1;
                else return 0;
            });
            sortedLayers = list;
        }
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
    }
}

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