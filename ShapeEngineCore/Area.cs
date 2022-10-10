using Raylib_CsLo;
using System.Numerics;
using ShapeEngineCore.SimpleCollision;
using ShapeEngineCore.Globals.Screen;
using ShapeEngineCore.Globals;

namespace ShapeEngineCore
{


    //Automatically set areaLayerName on GameObject?
    //Then Removing a gameobject never needs the areaLayerName


    public class AreaLayer
    {
        protected List<GameObject> gameObjects = new();
        protected List<GameObject> uiObjects = new();
        protected Rectangle inner;
        protected Rectangle outer; 
        protected CollisionHandler colHandler;
        protected Vector2 parallaxeOffset = new(0f);
        protected float parallaxeScaling = 0f;
        protected string name = "";
        public float ParallaxeSmoothing { get; set; } = 0.1f;
        public float DrawOrder { get; protected set; } = 0f;

        public AreaLayer(string name, Rectangle inner, Rectangle outer, CollisionHandler colHandler, float drawOrder = 0f, float parallaxeScaling = 0f)
        {
            this.name = name;
            this.inner = inner;
            this.outer = outer;
            this.colHandler = colHandler;
            this.parallaxeScaling = parallaxeScaling;
            this.DrawOrder = drawOrder;
        }

        public bool IsParallaxeLayer() { return parallaxeScaling != 0f; }
        public bool HasGameObject(GameObject obj) { return gameObjects.Contains(obj); }
        public List<GameObject> GetGameObjects() { return gameObjects; }
        public List<GameObject> GetGameObjects(string group)
        {
            if (group == "") return gameObjects;
            return gameObjects.FindAll(x => x.IsInGroup(group));
        }
        public List<GameObject> GetGameObjects(Predicate<GameObject> match)
        {
            return gameObjects.FindAll(match);
        }
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

        public void AddGameObject(GameObject obj, bool uiDrawing = false)
        {
            if (obj == null) return;
            //if (gameObjects.Contains(obj)) return; //dont need that i think
            if (obj is ICollidable collidable) colHandler.Add(collidable);
            gameObjects.Add(obj);
            if (uiDrawing && !uiObjects.Contains(obj)) uiObjects.Add(obj);
            obj.AreaLayerName = this.name;
            obj.Spawn();
        }
        public void AddGameObjects(List<GameObject> newObjects, bool uiDrawing = false)
        {
            foreach (GameObject obj in newObjects)
            {
                AddGameObject(obj, uiDrawing);
            }
        }

        
        public void RemoveGameObject(GameObject obj)
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
        public void RemoveGameObjects(List<GameObject> objs)
        {
            foreach (var obj in objs)
            {
                RemoveGameObject(obj);
            }
        }
        public void RemoveGameObjects(Predicate<GameObject> match)
        {
            var remove = gameObjects.FindAll(match);
            foreach (var obj in remove)
            {
                RemoveGameObject(obj);
            }
        }
        public void RemoveGameObjects(string group)
        {
            if (group == "") return;
            var remove = GetGameObjects(group);
            foreach (var obj in remove)
            {
                RemoveGameObject(obj);
            }
        }


        public virtual void UpdateParallaxe(Vector2 pos)
        {
            parallaxeOffset = Vec.Lerp(parallaxeOffset, pos * parallaxeScaling, ParallaxeSmoothing);
        }
        public virtual void Draw()
        {
            SortGameObjects();
            foreach (GameObject obj in gameObjects)
            {
                if (Overlap.Simple(outer, obj.GetBoundingBox())) { obj.Draw(); }
            }
        }
        public virtual void DrawUI(Vector2 uiSize, Vector2 stretchFactor)
        {
            foreach (GameObject obj in uiObjects)
            {
                obj.DrawUI(uiSize, stretchFactor);
            }
        }
        public virtual void Update(float dt)
        {
            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {
                GameObject obj = gameObjects[i];
                if (obj == null)
                {
                    gameObjects.RemoveAt(i);
                    return;
                }

                if(IsParallaxeLayer()) obj.AreaLayerOffset = parallaxeOffset;
                obj.Update(dt);
                bool insideInner = Overlap.Simple(inner, obj.GetBoundingBox());
                bool insideOuter = false;
                if (insideInner) insideOuter = true;
                else insideOuter = Overlap.Simple(outer, obj.GetBoundingBox());
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
            gameObjects.Sort(delegate (GameObject x, GameObject y)
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
        protected Dictionary<string, AreaLayer> layers = new();
        protected List<AreaLayer> sortedLayers = new();
        protected Rectangle inner;
        protected Rectangle outer;
        protected Playfield? playfield = null;
        public CollisionHandler colHandler;
        public Area(float x, float y, float w, float h, int rows, int cols)
        {
            inner = new(x, y, w, h);
            outer = Utils.ScaleRectangle(inner, 2f);
            colHandler = new(outer.x, outer.y, outer.width, outer.height, rows, cols, true);
            AddLayer("default");
            Start();
        }
        public Area(Vector2 topLeft, Vector2 bottomRight, int rows, int cols)
        {
            float w = bottomRight.X - topLeft.X;
            float h = bottomRight.Y - topLeft.Y;
            inner = new(topLeft.X, topLeft.Y, w, h);
            outer = Utils.ScaleRectangle(inner, 2f);
            colHandler = new(outer.x, outer.y, outer.width, outer.height, rows, cols, true);
            AddLayer("default");
            Start();
        }
        public Area(Vector2 topLeft, float w, float h, int rows, int cols)
        {
            inner = new(topLeft.X, topLeft.Y, w, h);
            outer = Utils.ScaleRectangle(inner, 2f);
            colHandler = new(outer.x, outer.y, outer.width, outer.height, rows, cols, true);
            AddLayer("default");
            Start();
        }
        public Area(Rectangle area, int rows, int cols)
        {
            inner = area;
            outer = Utils.ScaleRectangle(inner, 2f);
            colHandler = new(outer.x, outer.y, outer.width, outer.height, rows, cols, true);
            AddLayer("default");
            Start();
        }

        public Playfield? GetCurPlayfield() { return playfield; }
        public Rectangle GetInnerArea() { return inner; }
        public Rectangle GetOuterArea() { return outer; }
        public Vector2 GetInnerCenter() { return new(inner.x + inner.width / 2, inner.Y + inner.height / 2); }
        public Vector2 GetOuterCenter() { return new(outer.x + outer.width / 2, outer.Y + outer.height / 2); }


        public bool HasLayer(string name)
        {
            return layers.ContainsKey(name);
        }
        public void AddLayer(string name, float drawOrder = 0f, float parallaxeScaling = 0f)
        {
            if (name == "" || layers.ContainsKey(name)) return;
            layers.Add(name, new(name, inner, outer, colHandler, drawOrder, parallaxeScaling));
            sortedLayers = SortAreaLayers();
        }
        public void AddLayers(params (string name, float drawOrder, float parallaxeScaling)[] add)
        {
            foreach (var layer in add)
            {
                if (layer.name == "" || layer.name == "default" || layers.ContainsKey(layer.name)) continue;
                layers.Add(layer.name, new(layer.name, inner, outer, colHandler, layer.drawOrder, layer.parallaxeScaling));
            }
            sortedLayers = SortAreaLayers();
        }

        public void RemoveLayer(string name)
        {
            if (name == "" || name == "default" || !HasLayer(name)) return;
            layers[name].Clear();
            layers.Remove(name);
            sortedLayers = SortAreaLayers();
        }
        public void RemoveLayers(params string[] names)
        {
            foreach (var name in names)
            {
                if (name == "" || name == "default" || !HasLayer(name)) continue;
                layers[name].Clear();
                layers.Remove(name);
            }
            sortedLayers = SortAreaLayers();
        }
        public void UpdateLayerParallaxe(Vector2 pos, string name = "")
        {
            if(name == "")
            {
                foreach (var layer in sortedLayers)
                {
                    layer.UpdateParallaxe(pos);
                }
            }
            else
            {
                if(!HasLayer(name)) return;
                layers[name].UpdateParallaxe(pos);
            }
        }

        /// <summary>
        /// If layerName is "" all gameObjects of all layers are returned.
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public List<GameObject> GetGameObjects(string layerName = "")
        {
            
            if(layerName == "")
            {
                List<GameObject> gameObjects = new();
                foreach (var layer in layers.Values)
                {
                    gameObjects.AddRange(layer.GetGameObjects());
                }
                return gameObjects;
            }
            else
            {
                if (!HasLayer(layerName)) return new();
                return layers[layerName].GetGameObjects();
            }
        }
        /// <summary>
        /// If layerName is "" all gameObjects of all layers are returned.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public List<GameObject> GetGameObjects(string group, string layerName = "")
        {
            if (layerName == "")
            {
                List<GameObject> gameObjects = new();
                foreach (var layer in layers.Values)
                {
                    gameObjects.AddRange(layer.GetGameObjects(group));
                }
                return gameObjects;
            }
            else
            {
                if (!HasLayer(layerName)) return new();
                return layers[layerName].GetGameObjects(group);
            }
        }
        /// <summary>
        /// If layerName is "" all gameObjects of all layers are returned.
        /// </summary>
        /// <param name="match">Predicate to match game objects that should be returned.</param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public List<GameObject> GetGameObjects(Predicate<GameObject> match, string layerName = "")
        {
            if (layerName == "")
            {
                List<GameObject> gameObjects = new();
                foreach (var layer in layers.Values)
                {
                    gameObjects.AddRange(layer.GetGameObjects(match));
                }
                return gameObjects;
            }
            else
            {
                if (!HasLayer(layerName)) return new();
                return layers[layerName].GetGameObjects(match);
            }
        }

        public void ClearGameObjects(string layerName = "")
        {
            if (layerName == "")
            {
                foreach (var layer in layers.Values)
                {
                    layer.Clear();
                }
            }
            else
            {
                if (!HasLayer(layerName)) return;
                layers[layerName].Clear();
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

        public void AddGameObject(GameObject obj, bool uiDrawing = false, string layerName = "default")
        {
            if(layerName == "" || !HasLayer(layerName)) return;
            layers[layerName].AddGameObject(obj, uiDrawing);
        }
        public void AddGameObjects(List<GameObject> newObjects, bool uiDrawing = false, string layerName = "default")
        {
            if (layerName == "" || !HasLayer(layerName)) return;
            layers[layerName].AddGameObjects(newObjects, uiDrawing);
        }

        public void RemoveGameObject(GameObject obj)
        {
            string layerName = obj.AreaLayerName;
            if (layerName == "" || !HasLayer(layerName)) return;
            layers[layerName].RemoveGameObject(obj);
        }
        public void RemoveGameObjects(List<GameObject> objs)
        {
            foreach (var obj in objs)
            {
                RemoveGameObject(obj);
            }
        }
        public void RemoveGameObjects(Predicate<GameObject> match, string layerName = "")
        {
            if(layerName == "")
            {
                foreach (var layer in sortedLayers)
                {
                    layer.RemoveGameObjects(match);
                }
            }
            else
            {
                if (!HasLayer(layerName)) return;
                layers[layerName].RemoveGameObjects(match);
            }
        }
        public void RemoveGameObjects(string group, string layerName = "")
        {
            if (layerName == "")
            {
                foreach (var layer in sortedLayers)
                {
                    layer.RemoveGameObjects(group);
                }
            }
            else
            {
                if (!HasLayer(layerName)) return;
                layers[layerName].RemoveGameObjects(group);
            }
        }

        public virtual void Start() { }
        public virtual void Close()
        {
            ClearGameObjects();
            colHandler.Close();
        }
        public virtual void Draw()
        {
            if (DEBUG_DRAWHELPERS)
            {
                DrawRectangleLinesEx(this.inner, 15f, DEBUG_AreaInnerColor);
                DrawRectangleLinesEx(this.outer, 15f, DEBUG_AreaOuterColor);
                colHandler.DebugDrawGrid(DEBUG_CollisionHandlerBorder, DEBUG_CollisionHandlerFill);
            }
            bool playfieldDrawn = false;
            for (int i = 0; i < sortedLayers.Count; i++)
            {
                var layer = sortedLayers[i];
                if (playfield != null && !playfieldDrawn && layer.DrawOrder > playfield.GetDrawOrder()) playfield.Draw();
                layer.Draw();
            }
        }
        public virtual void DrawUI(Vector2 uiSize, Vector2 stretchFactor)
        {
            for (int i = 0; i < sortedLayers.Count; i++)
            {
                var layer = sortedLayers[i];
                layer.DrawUI(uiSize, stretchFactor);
            }
        }
        public virtual void Update(float dt)
        {
            colHandler.Update(dt);
            colHandler.Resolve();

            for (int i = 0; i < sortedLayers.Count; i++)
            {
                var layer = sortedLayers[i];
                layer.Update(dt);
            }
        }
        
        //public virtual void MonitorHasChanged()
        //{
        //    for (int i = 0; i < sortedLayers.Count; i++)
        //    {
        //        var layer = sortedLayers[i];
        //        layer.MonitorHasChanged();
        //    }
        //}
        
        public void SortGameObjects(List<GameObject> objectsToSort)
        {
            objectsToSort.Sort(delegate (GameObject x, GameObject y)
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
