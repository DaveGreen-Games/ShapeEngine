using Raylib_CsLo;
using System.Numerics;
using ShapeLib;
using System.Reflection.Emit;

namespace ShapeCore
{
    //clean up and rework
    //to much duplication of the same functions in area
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

    public class Area
    {
        protected Dictionary<uint, AreaLayer> layers = new();
        protected List<AreaLayer> sortedLayers = new();
        public Rect InnerRect { get;protected set;}
        public Rect OuterRect { get; protected set; }
        public CollisionHandler colHandler;
        public uint DefaultID { get; protected set; }
        public float UpdateSlowFactor { get; set; } = 1f;
        Dictionary<uint, TimerValue> updateSlowFactors = new();
        public void Slow(uint id, float factor, float duration = -1)
        {
            if (factor < 0) return;
            if (updateSlowFactors.ContainsKey(id))
            {
                //var old = updateSlowFactors[id];
                updateSlowFactors[id] = new(duration, factor);
            }
            else
            {
                updateSlowFactors.Add(id, new(duration, factor));
            }
        }
        public void RemoveSlow(uint id)
        {
            if (!updateSlowFactors.ContainsKey(id)) return;
            updateSlowFactors.Remove(id);
        }
        public void ClearSlow() { updateSlowFactors.Clear(); }
        private float UpdateSlowFactors(float dt)
        {
            float accumualted = 1f;
            var keys = updateSlowFactors.Keys.ToList();
            for (int i = keys.Count - 1; i >= 0; i--)
            {
                var entry = updateSlowFactors[keys[i]];
                entry.Timer -= dt;
                if (entry.Timer <= 0f)
                {
                    updateSlowFactors.Remove(keys[i]);
                }
                else
                {
                    accumualted *= entry.Value;
                }
            }
            return accumualted;
        }
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
            DefaultID = SID.NextID;
            AddLayer(DefaultID);
        }
        public Area(Vector2 topLeft, Vector2 bottomRight, int rows, int cols)
        {
            float w = bottomRight.X - topLeft.X;
            float h = bottomRight.Y - topLeft.Y;
            InnerRect = new(topLeft.X, topLeft.Y, w, h);
            OuterRect = SRect.ScaleRectangle(InnerRect, 2f);
            colHandler = new(InnerRect.x, InnerRect.y, InnerRect.width, InnerRect.height, rows, cols);
            DefaultID = SID.NextID;
            AddLayer(DefaultID);
        }
        public Area(Vector2 topLeft, float w, float h, int rows, int cols)
        {
            InnerRect = new(topLeft.X, topLeft.Y, w, h);
            OuterRect = SRect.ScaleRectangle(InnerRect, 2f);
            colHandler = new(InnerRect.x, InnerRect.y, InnerRect.width, InnerRect.height, rows, cols);
            DefaultID = SID.NextID;
            AddLayer(DefaultID);
        }
        public Area(Rect area, int rows, int cols)
        {
            InnerRect = area;
            OuterRect = SRect.ScaleRectangle(InnerRect, 2f);
            colHandler = new(InnerRect.x, InnerRect.y, InnerRect.width, InnerRect.height, rows, cols);
            DefaultID = SID.NextID;
            AddLayer(DefaultID);
        }


        //public Rect GetInnerArea() { return inner; }
        //public Rect GetOuterArea() { return outer; }

        public void SetLayerSlowFactor(uint layerID, float slowFactor)
        {
            if (!layers.ContainsKey(layerID)) return;
            layers[layerID].UpdateSlowFactor = slowFactor;
        }
        public bool HasLayer(uint id)
        {
            return layers.ContainsKey(id);
        }
        public void AddLayer(uint id, float drawOrder = 0f, float parallaxeScaling = 0f)
        {
            if (id == DefaultID) return;
            if (layers.ContainsKey(id)) layers[id] = new(id, InnerRect, OuterRect, colHandler, drawOrder, parallaxeScaling);
            else layers.Add(id, new(id, InnerRect, OuterRect, colHandler, drawOrder, parallaxeScaling));
            
            sortedLayers = SortAreaLayers();
        }
        public void AddLayers(params (uint id, float drawOrder, float parallaxeScaling)[] add)
        {
            foreach (var layer in add)
            {
                if(layer.id == DefaultID) continue;
                if (layers.ContainsKey(layer.id)) layers[layer.id] = new(layer.id, InnerRect, OuterRect, colHandler, layer.drawOrder, layer.parallaxeScaling);
                else layers.Add(layer.id, new(layer.id, InnerRect, OuterRect, colHandler, layer.drawOrder, layer.parallaxeScaling));
            }
            sortedLayers = SortAreaLayers();
        }

        public void RemoveLayer(uint id)
        {
            if (id == DefaultID || !HasLayer(id)) return;
            layers[id].Clear();
            layers.Remove(id);
            sortedLayers = SortAreaLayers();
        }
        public void RemoveLayers(params uint[] ids)
        {
            foreach (var id in ids)
            {
                if (id == DefaultID || !HasLayer(id)) continue;
                layers[id].Clear();
                layers.Remove(id);
            }
            sortedLayers = SortAreaLayers();
        }
        public void UpdateLayerParallaxe(Vector2 pos, uint id)
        {
            if (!HasLayer(id)) return;
            layers[id].UpdateParallaxe(pos);
        }
        public void UpdateLayerParallaxe(Vector2 pos)
        {
            foreach (var layer in sortedLayers)
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
            return layers[layerID].GetGameObjects();
        }
        public List<IGameObject> GetAllGameObjects()
        {
            List<IGameObject> gameObjects = new();
            foreach (var layer in layers.Values)
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
            return layers[layerID].GetGameObjects(group);
        }
        public List<IGameObject> GetAllGameObjects(uint group)
        {
            List<IGameObject> gameObjects = new();
            foreach (var layer in layers.Values)
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
            return layers[layerID].GetGameObjects(match);
        }
        public List<IGameObject> GetAllGameObjects(Predicate<IGameObject> match)
        {
            List<IGameObject> gameObjects = new();
            foreach (var layer in layers.Values)
            {
                gameObjects.AddRange(layer.GetGameObjects(match));
            }
            return gameObjects;
        }
        public void ClearGameObjects(uint layerID)
        {
            if (!HasLayer(layerID)) return;
            layers[layerID].Clear();
        }
        public void ClearAllGameObjects()
        {
            foreach(var layer in layers.Values)
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
            layers[layerID].AddGameObject(obj, uiDrawing);
            return true;
        }
        public void AddGameObjects(List<IGameObject> newObjects, uint layerName, bool uiDrawing = false)
        {
            if (!HasLayer(layerName)) return;
            layers[layerName].AddGameObjects(newObjects, uiDrawing);
        }

        public void RemoveGameObject(IGameObject obj)
        {
            if (!HasLayer(obj.AreaLayerID)) return;
            layers[obj.AreaLayerID].RemoveGameObject(obj);
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
            layers[layerID].RemoveGameObjects(match);
        }
        public void RemoveAllGameObjects(Predicate<IGameObject> match)
        {
            foreach (var layer in sortedLayers)
            {
                layer.RemoveGameObjects(match);
            }
        }
        public void RemoveGameObjects(uint group, uint layerID)
        {
            if (!HasLayer(layerID)) return;
            layers[layerID].RemoveGameObjects(group);
        }
        public void RemoveAllGameObjects(uint group)
        {
            foreach (var layer in sortedLayers)
            {
                layer.RemoveGameObjects(group);
            }
        }
        public virtual void Start() { }
        public virtual void Close()
        {
            ClearAllGameObjects();
            colHandler.Close();
        }
        public virtual void Draw()
        {
            if (DEBUG_DRAWHELPERS)
            {
                DrawRectangleLinesEx(this.InnerRect.Rectangle, 15f, DEBUG_AreaInnerColor);
                DrawRectangleLinesEx(this.OuterRect.Rectangle, 15f, DEBUG_AreaOuterColor);
                colHandler.DebugDrawGrid(DEBUG_CollisionHandlerBorder, DEBUG_CollisionHandlerFill);
            }
            //bool playfieldDrawn = false;
            for (int i = 0; i < sortedLayers.Count; i++)
            {
                var layer = sortedLayers[i];
                //if (playfield != null && !playfieldDrawn && layer.DrawOrder > playfield.GetDrawOrder()) playfield.Draw();
                layer.Draw();
            }
        }
        public virtual void DrawUI(Vector2 uiSize)
        {
            for (int i = 0; i < sortedLayers.Count; i++)
            {
                var layer = sortedLayers[i];
                layer.DrawUI(uiSize);
            }
        }
        public virtual void Update(float dt)
        {
            colHandler.Update(dt);
            //colHandler.Resolve();

            float curSlowFactor = UpdateSlowFactors(dt) * UpdateSlowFactor;

            for (int i = 0; i < sortedLayers.Count; i++)
            {
                var layer = sortedLayers[i];
                layer.Update(dt, curSlowFactor);
            }
        }
        
        public void SortGameObjects(List<IGameObject> objectsToSort)
        {
            objectsToSort.Sort(delegate (IGameObject x, IGameObject y)
            {
                if (x == null || y == null) return 0;

                if (x.DrawOrder < y.DrawOrder) return -1;
                else if (x.DrawOrder > y.DrawOrder) return 1;
                else return 0;
            });
        }
        protected List<AreaLayer> SortAreaLayers()
        {
            var list = layers.Values.ToList();
            list.Sort(delegate (AreaLayer x, AreaLayer y)
            {
                if (x == null || y == null) return 0;

                if (x.DrawOrder < y.DrawOrder) return -1;
                else if (x.DrawOrder > y.DrawOrder) return 1;
                else return 0;
            });
            return list;
        }
    }
}
