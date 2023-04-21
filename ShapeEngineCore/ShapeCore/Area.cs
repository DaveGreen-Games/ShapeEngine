﻿using ShapeLib;
using System.Numerics;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace ShapeCore
{
    //clean up and rework
    //to much duplication of the same functions in area
    public class AreaLayer
    {
        public readonly List<IGameObject> gameObjects = new();
        public float ParallaxeScaling { get; set; } = 0f;
        public float ParallaxeSmoothing { get; set; } = 0.1f;
        public float UpdateSlowFactor { get; set; } = 1f;

        internal Slow slow = new(); 

        public int Layer { get; protected set; }
        public Vector2 ParallaxeOffset { get; protected set; } = new(0f);
        

        //public float DrawOrder { get; protected set; } = 0f;
        //internal Dictionary<uint, TimerValue> updateSlowFactors = new();

        public AreaLayer(int layer, float parallaxeScaling)
        {
            this.Layer = layer;
            this.ParallaxeScaling = parallaxeScaling;
        }
        public AreaLayer(int layer)
        {
            this.Layer = layer;
            this.ParallaxeScaling = 0f;
        }
        public bool IsParallaxe() { return ParallaxeScaling != 0f; }
        public void UpdateParallaxe(Vector2 pos)
        {
            ParallaxeOffset = ParallaxeOffset.Lerp(pos * ParallaxeScaling, ParallaxeSmoothing);
        }
        public void SortGameObjects()
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
    
    //make generic and public like factor handler or something
    internal class Slow
    {
        private Dictionary<uint, TimerValue> factors = new();
        public float TotalFactor { get; protected set; } = 1f;
        public uint Add(float factor, float duration = -1)
        {
            if (factor < 0) return 0;
            uint id = SID.NextID;
            if (factors.ContainsKey(id))
            {
                factors[id] = new(duration, factor);
            }
            else
            {
                factors.Add(id, new(duration, factor));
            }
            return id;
        }
        public void Remove(uint id)
        {
            if (!factors.ContainsKey(id)) return;
            factors.Remove(id);
        }
        public void Clear() { factors.Clear(); }
        public void Update(float dt)
        {
            float accumualted = 1f;
            var keys = factors.Keys.ToList();
            for (int i = keys.Count - 1; i >= 0; i--)
            {
                var entry = factors[keys[i]];
                entry.Timer -= dt;
                if (entry.Timer <= 0f)
                {
                    factors.Remove(keys[i]);
                }
                else
                {
                    accumualted *= entry.Value;
                }
            }

            TotalFactor = accumualted;
        }
    }
    internal class TimerValue
    {
        public float Timer = 0f;
        public float Value = 0f;

        public TimerValue(float duration, float value)
        {
            this.Timer = duration;
            this.Value = value;
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

    public class Area
    {
        public Rect InnerRect { get;protected set;}
        public Rect OuterRect { get; protected set; }
        public CollisionHandler colHandler;

        public float UpdateSlowFactor { get; set; } = 1f;
        public Vector2 ParallaxePosition = new(0f);
        
        private Dictionary<int, AreaLayer> layers = new();
        private List<AreaLayer> sortedLayers = new();
        private Slow slow = new Slow();
        protected List<IGameObject> uiObjects = new();
        
        
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

        
        

        public void AddLayerGroups(params AreaLayer[] layerGroups)
        {
            foreach (var layerGroup in layerGroups)
            {
                if(!this.layers.ContainsKey(layerGroup.Layer))
                    this.layers.Add(layerGroup.Layer, layerGroup);
            }
            SortAreaLayerGroups();
        }
        public void AddLayerGroups(params int[] layers)
        {
            foreach (var layer in layers)
            {
                if (!this.layers.ContainsKey(layer))
                    this.layers.Add(layer, new(layer));
            }
            SortAreaLayerGroups();
        }
        public void RemoveLayerGroups(params int[] layers)
        {
            foreach (var layer in layers)
            {
                if (this.layers.ContainsKey(layer))
                {
                    ClearLayerGroup(layer);
                    this.layers.Remove(layer);
                }
            }
            SortAreaLayerGroups();
        }
        public bool HasLayerGroup(int layer) { return layers.ContainsKey(layer); }
        public List<IGameObject> GetGameObjects(int layerGroup, Predicate<IGameObject> match) { return HasLayerGroup(layerGroup) ? layers[layerGroup].gameObjects : new(); }// gameObjects.ToList().FindAll(match); }
        public List<IGameObject> GetAllGameObjects()
        {
            List<IGameObject> objects = new();
            foreach (var layerGroup in layers.Values)
            {
                objects.AddRange(layerGroup.gameObjects);
            }
            return objects;
        }
        public List<IGameObject> GetAllGameObjects(Predicate<IGameObject> match) { return GetAllGameObjects().FindAll(match); }
        
        
        public void AddGameObject(IGameObject gameObject) 
        {
            int layer = gameObject.LayerGroup;
            if (!layers.ContainsKey(layer)) AddLayerGroups(layer); // layerGroups.Add(layer, new(layer));

            layers[layer].gameObjects.Add(gameObject);
        }
        public void AddGameObjects(params IGameObject[] gameObjects) { foreach (var go in gameObjects) AddGameObject(go); }
        public bool RemoveGameObject(IGameObject gameObject)
        {
            if (layers.ContainsKey(gameObject.LayerGroup))
            {
                bool removed = layers[gameObject.LayerGroup].gameObjects.Remove(gameObject);
                if (removed)
                {
                    gameObject.Destroy();
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
                ClearLayerGroup(layer);
            }
        }
        public void ClearLayerGroup(int layer)
        {
            if (layers.ContainsKey(layer))
            {
                var layerGroup = layers[layer];
                foreach (var obj in layerGroup.gameObjects)
                {
                    obj.Destroy();
                    if (obj is ICollidable collidable)
                    {
                        colHandler.Remove(collidable);
                    }
                }
                layerGroup.gameObjects.Clear();
            }
            
        }


        /*
        public void SetLayerSlowFactor(uint layerID, float slowFactor)
        {
            if (!groups.ContainsKey(layerID)) return;
            groups[layerID].UpdateSlowFactor = slowFactor;
        }
        public bool HasLayer(uint id)
        {
            return groups.ContainsKey(id);
        }
        public void AddLayer(uint id, float drawOrder = 0f, float parallaxeScaling = 0f)
        {
            if (id == DefaultID) return;
            if (groups.ContainsKey(id)) groups[id] = new(id, InnerRect, OuterRect, colHandler, drawOrder, parallaxeScaling);
            else groups.Add(id, new(id, InnerRect, OuterRect, colHandler, drawOrder, parallaxeScaling));
            
            sortedGroups = SortAreaGroups();
        }
        public void AddLayers(params (uint id, float drawOrder, float parallaxeScaling)[] add)
        {
            foreach (var layer in add)
            {
                if(layer.id == DefaultID) continue;
                if (groups.ContainsKey(layer.id)) groups[layer.id] = new(layer.id, InnerRect, OuterRect, colHandler, layer.drawOrder, layer.parallaxeScaling);
                else groups.Add(layer.id, new(layer.id, InnerRect, OuterRect, colHandler, layer.drawOrder, layer.parallaxeScaling));
            }
            sortedGroups = SortAreaGroups();
        }

        public void RemoveLayer(uint id)
        {
            if (id == DefaultID || !HasLayer(id)) return;
            groups[id].Clear();
            groups.Remove(id);
            sortedGroups = SortAreaGroups();
        }
        public void RemoveLayers(params uint[] ids)
        {
            foreach (var id in ids)
            {
                if (id == DefaultID || !HasLayer(id)) continue;
                groups[id].Clear();
                groups.Remove(id);
            }
            sortedGroups = SortAreaGroups();
        }
        public void UpdateLayerParallaxe(Vector2 pos, uint id)
        {
            if (!HasLayer(id)) return;
            groups[id].UpdateParallaxe(pos);
        }
        public void UpdateLayerParallaxe(Vector2 pos)
        {
            foreach (var layer in sortedGroups)
            {
                layer.UpdateParallaxe(pos);
            }
        }
        /// <summary>
        /// If layerName is "" all gameObjects of all layers are returned.
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public List<IGameObject> GetGameObjects(uint layerID)
        {

            if (!HasLayer(layerID)) return new();
            return groups[layerID].GetGameObjects();
        }
        public List<IGameObject> GetAllGameObjects()
        {
            List<IGameObject> gameObjects = new();
            foreach (var layer in groups.Values)
            {
                gameObjects.AddRange(layer.GetGameObjects());
            }
            return gameObjects;
        }
        /// <summary>
        /// If layerName is "" all gameObjects of all layers are returned.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="layerID"></param>
        /// <returns></returns>
        public List<IGameObject> GetGameObjects(uint group, uint layerID)
        {
            if (!HasLayer(layerID)) return new();
            return groups[layerID].GetGameObjects(group);
        }
        public List<IGameObject> GetAllGameObjects(uint group)
        {
            List<IGameObject> gameObjects = new();
            foreach (var layer in groups.Values)
            {
                gameObjects.AddRange(layer.GetGameObjects(group));
            }
            return gameObjects;
        }
        /// <summary>
        /// If layerName is "" all gameObjects of all layers are returned.
        /// </summary>
        /// <param name="match">Predicate to match game objects that should be returned.</param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public List<IGameObject> GetGameObjects(Predicate<IGameObject> match, uint layerID)
        {
            if (!HasLayer(layerID)) return new();
            return groups[layerID].GetGameObjects(match);
        }
        public List<IGameObject> GetAllGameObjects(Predicate<IGameObject> match)
        {
            List<IGameObject> gameObjects = new();
            foreach (var layer in groups.Values)
            {
                gameObjects.AddRange(layer.GetGameObjects(match));
            }
            return gameObjects;
        }
        
        public void ClearGameObjects(uint layerID)
        {
            if (!HasLayer(layerID)) return;
            groups[layerID].Clear();
        }
        public void ClearAllGameObjects()
        {
            foreach(var layer in groups.Values)
            {
                layer.Clear();
            }
        }
        public void AddICollidable(ICollidable obj)
        {
            colHandler.Add(obj);
        }
        public void RemoveICollidable(ICollidable obj)
        {
            colHandler.Remove(obj);
        }

        public bool AddGameObject(IGameObject obj, uint layerID, bool uiDrawing = false)
        { 
            if(!HasLayer(layerID)) return false;
            groups[layerID].AddGameObject(obj, uiDrawing);
            return true;
        }
        public void AddGameObjects(List<IGameObject> newObjects, uint layerName, bool uiDrawing = false)
        {
            if (!HasLayer(layerName)) return;
            groups[layerName].AddGameObjects(newObjects, uiDrawing);
        }

        public void RemoveGameObject(IGameObject obj)
        {
            if (!HasLayer(obj.AreaLayerID)) return;
            groups[obj.AreaLayerID].RemoveGameObject(obj);
        }
        public void RemoveGameObjects(List<IGameObject> objs)
        {
            foreach (var obj in objs)
            {
                RemoveGameObject(obj);
            }
        }
        public void RemoveGameObjects(Predicate<IGameObject> match, uint layerID)
        {
            if (!HasLayer(layerID)) return;
            groups[layerID].RemoveGameObjects(match);
        }
        public void RemoveAllGameObjects(Predicate<IGameObject> match)
        {
            foreach (var layer in sortedGroups)
            {
                layer.RemoveGameObjects(match);
            }
        }
        public void RemoveGameObjects(uint group, uint layerID)
        {
            if (!HasLayer(layerID)) return;
            groups[layerID].RemoveGameObjects(group);
        }
        public void RemoveAllGameObjects(uint group)
        {
            foreach (var layer in sortedGroups)
            {
                layer.RemoveGameObjects(group);
            }
        }
        */
        
        
        public virtual void Start() { }
        public virtual void Close()
        {
            Clear();
            colHandler.Close();
        }
        
        public virtual void Update(float dt)
        {
            colHandler.Update(dt);
            slow.Update(dt);
            uiObjects.Clear();
            for (int i = 0; i < sortedLayers.Count; i++)
            {
                var layer = sortedLayers[i];

                if(layer.IsParallaxe())
                    layer.UpdateParallaxe(ParallaxePosition);

                UpdateLayer(dt, layer);
                
                layer.SortGameObjects();
            }
        }
        protected void UpdateLayer(float dt, AreaLayer layer)
        {
            layer.slow.Update(dt);
            float slowFactor = UpdateSlowFactor * slow.TotalFactor * layer.slow.TotalFactor * layer.UpdateSlowFactor;
            for (int i = layer.gameObjects.Count - 1; i >= 0; i--)
            {
                IGameObject obj = layer.gameObjects[i];
                if (obj == null)
                {
                    layer.gameObjects.RemoveAt(i);
                    return;
                }

                if (layer.IsParallaxe()) obj.ParallaxeOffset = layer.ParallaxeOffset;
                float curSlowFactor = slowFactor * obj.UpdateSlowFactor;
                float dif = dt - (dt * curSlowFactor);
                dif *= obj.UpdateSlowResistance;
                obj.Update(dt - dif);
                bool insideInner = SGeometry.OverlapRectRect(InnerRect, obj.GetBoundingBox());
                bool insideOuter = false;
                if (insideInner) insideOuter = true;
                else insideOuter = SGeometry.OverlapRectRect(OuterRect, obj.GetBoundingBox());
                obj.OnPlayfield(insideInner, insideOuter);

                if (obj.IsDead() || !insideOuter)
                {
                    obj.Destroy();
                    layer.gameObjects.RemoveAt(i);
                    if (obj is ICollidable collidable)
                    {
                        colHandler.Remove(collidable);
                    }
                }
                else if(obj.DrawToUI) uiObjects.Add(obj);
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

            for (int i = 0; i < sortedLayers.Count; i++)
            {
                var layer = sortedLayers[i];
                foreach (IGameObject obj in layer.gameObjects)
                {
                    if (SGeometry.OverlapRectRect(OuterRect, obj.GetBoundingBox())) { obj.Draw(); }
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
        protected void SortAreaLayerGroups()
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
    }
}
