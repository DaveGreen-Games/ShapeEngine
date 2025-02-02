using ShapeEngine.Lib;
using ShapeEngine.Core.Shapes;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib.Drawing;

namespace ShapeEngine.Core
{
    /// <summary>
    /// Provides a simple area for managing adding/removing, updating, and drawing of area objects. Does not provide a collision system.
    /// </summary>
    public class SpawnArea : IUpdateable, IDrawable, IBounds
    {
        public event Action<GameObject>? OnGameObjectAdded;
        public event Action<GameObject>? OnGameObjectRemoved;
        
        
        public int NewLayerStartCapacity = 128;
        public int Count { get; private set; } = 0;
        public Rect Bounds { get; protected set; }
        public Vector2 ParallaxePosition { get; set; } = new(0f);
        
        private readonly SortedList<uint, List<GameObject>> allObjects = new();
        private readonly List<GameObject> drawToGameTextureObjects = new();
        private readonly List<GameObject> drawToGameUiTextureObjects = new();

        private Rect clearArea = new();
        private bool clearAreaActive = false;
        private BitFlag clearAreaMask = new();

        private List<GameObject> removalList = new(1024);
        public SpawnArea(float x, float y, float w, float h)
        {
            Bounds = new(x, y, w, h);
        }
        public SpawnArea(Rect bounds)
        {
            Bounds = bounds;
        }

        
        public virtual void ResizeBounds(Rect newBounds)
        {
            Bounds = newBounds;
        }
        public bool HasLayer(uint layer) { return allObjects.ContainsKey(layer); }

        public List<GameObject>? GetGameObjects(uint layer, Predicate<GameObject> match)
        {
            if (Count <= 0) return null;
            return HasLayer(layer) ? allObjects[layer].FindAll(match) : null;
        }
        public void GetGameObjects(uint layer, Predicate<GameObject> match, ref List<GameObject> result)
        {
            if (Count <= 0) return;
            if (!HasLayer(layer)) return;

            foreach (var obj in allObjects[layer])
            {
                if (!match.Invoke(obj)) continue;
                result.Add(obj);
            }
        }
        public void GetGameObjects(BitFlag layerMask, ref List<GameObject> result)
        {
            if (Count <= 0) return;
            foreach (var kvp in allObjects)
            {
                if (layerMask.Has(kvp.Key))
                {
                    if (kvp.Value.Count > 0)
                    {
                        result.AddRange(kvp.Value);
                    }
                    
                }
            }
        }
        public void GetGameObjects(BitFlag layerMask, Predicate<GameObject> match, ref List<GameObject> result)
        {
            if (Count <= 0) return;
            
            foreach (var kvp in allObjects)
            {
                if (layerMask.Has(kvp.Key))
                {
                    if (kvp.Value.Count > 0)
                    {
                        result.AddRange(kvp.Value.FindAll(match));
                    }
                    
                }
            }
        }

        public List<GameObject>? GetAllGameObjects()
        {
            if (Count <= 0) return null;
            List<GameObject> objects = new(Count);
            foreach (var layerGroup in allObjects.Values)
            {
                objects.AddRange(layerGroup);
            }
            return objects;
        }
        public void GetAllGameObjects(ref List<GameObject> result)
        {
            if (Count <= 0) return;
            
            foreach (var layerGroup in allObjects.Values)
            {
                result.AddRange(layerGroup);
            }
        }

        public void GetAllGameObjects(Predicate<GameObject> match, ref List<GameObject> result)
        {
            if (Count <= 0) return;

            
            foreach (var layerGroup in allObjects.Values)
            {
                result.AddRange(layerGroup.FindAll(match));
            }
        }
        public List<GameObject>? GetAllGameObjects(Predicate<GameObject> match)
        {
            if (Count <= 0) return null;
            List<GameObject> objects = new(Count / 2);
            foreach (var layerGroup in allObjects.Values)
            {
                objects.AddRange(layerGroup.FindAll(match));
            }
            return objects;
        }

        
        public void AddGameObject(GameObject gameObject)
        {
            var layer = gameObject.Layer;
            AddLayer(layer, NewLayerStartCapacity <= 0 ? 4 : NewLayerStartCapacity);

            allObjects[layer].Add(gameObject);
            
            Count++;
            GameObjectWasAdded(gameObject);
            OnGameObjectAdded?.Invoke(gameObject);
            gameObject.OnSpawned(this);
        }
        public void AddGameObjects(params GameObject[] areaObjects) { foreach (var ao in areaObjects) AddGameObject(ao); }
        public void AddGameObjects(IEnumerable<GameObject> areaObjects) { foreach (var ao in areaObjects) AddGameObject(ao); }
        public bool RemoveGameObject(GameObject gameObject)
        {
            if (!allObjects.TryGetValue(gameObject.Layer, out var o)) return false;
            if (!o.Remove(gameObject)) return false;
            
            Count--;
            GameObjectWasRemoved(gameObject);
            OnGameObjectRemoved?.Invoke(gameObject);
            gameObject.OnDespawned(this);
            return true;

        }
        public void RemoveGameObjects(params GameObject[] areaObjects)
        {
            foreach (var ao in areaObjects)
            {
                RemoveGameObject(ao);
            }
        }
        public void RemoveGameObjects(IEnumerable<GameObject> areaObjects)
        {
            foreach (var ao in areaObjects)
            {
                RemoveGameObject(ao);
            }
        }
        
        
        public void RemoveGameObjects(uint layer, Predicate<GameObject> match, ref List<GameObject> result)
        {
            if (!allObjects.ContainsKey(layer)) return;
            removalList.Clear();
            GetGameObjects(layer, match, ref removalList);
            foreach (var o in removalList)
            {
                if(!RemoveGameObject(o)) continue;
                result.Add(o);
            }

        }
        public void RemoveGameObjects(BitFlag layerMask, ref List<GameObject> result)
        {
            removalList.Clear();
            GetGameObjects(layerMask, ref removalList);
            foreach (var o in removalList)
            {
                if (!RemoveGameObject(o)) continue;
                result.Add(o);
            }

        }
        public void RemoveGameObjects(BitFlag layerMask, Predicate<GameObject> match, ref List<GameObject> result)
        {
            removalList.Clear();
            GetGameObjects(layerMask, match, ref removalList);
            foreach (var o in removalList)
            {
                if (!RemoveGameObject(o)) continue;
                result.Add(o);
            }

        }
        public void RemoveGameObjects(Predicate<GameObject> match)
        {
            // var objs = GetAllGameObjects(match);
            removalList.Clear();
            GetAllGameObjects(match, ref removalList);
            foreach (var o in removalList)
            {
                RemoveGameObject(o);
            }
        }

        
        public void ClearArea(Rect area, BitFlag areaLayerMask)
        {
            clearArea = area;
            clearAreaMask = areaLayerMask;
            clearAreaActive = true;
        }
        
        protected virtual void GameObjectWasAdded(GameObject obj) { }
        protected virtual void GameObjectWasRemoved(GameObject obj) { }

        public virtual void Clear()
        {
            drawToGameTextureObjects.Clear();
            drawToGameUiTextureObjects.Clear();

            foreach (var layer in allObjects.Keys)
            {
                ClearLayer(layer);
            }
            Count = 0;
        }
        public virtual void ClearLayer(uint layer)
        {
            if (!allObjects.TryGetValue(layer, out var objects)) return;

            for (int i = objects.Count - 1; i >= 0; i--)
            {
                var obj = objects[i];
                objects.RemoveAt(i);
                GameObjectWasRemoved(obj);
                obj.OnDespawned(this);
                Count--;
                
            }
        }

        public virtual void Start() { }
        public virtual void Close()
        {
            Clear();
        }

        public virtual void DrawDebug(ColorRgba bounds, ColorRgba border, ColorRgba fill)
        {
            this.Bounds.DrawLines(15f, bounds);
        }
        
        #region Open Framerate
        public virtual void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            drawToGameTextureObjects.Clear();
            drawToGameUiTextureObjects.Clear();

            if (clearAreaActive)
            {
                if (!Bounds.OverlapShape(clearArea)) clearAreaActive = false;
            }
            
            foreach (var layer in allObjects)
            {
                var objs = allObjects[layer.Key];
                if (objs.Count <= 0) continue;

                for (int i = objs.Count - 1; i >= 0; i--)
                {
                    var obj = objs[i];

                    if (clearAreaActive && (clearAreaMask.IsEmpty() || clearAreaMask.Has((uint)layer.Key)))
                    {
                        if (clearArea.OverlapShape(obj.GetBoundingBox()))
                        {
                            RemoveGameObject(obj);
                            continue;
                        }
                    }

                    
                    obj.UpdateParallaxe(ParallaxePosition);
                    
                    if (obj.IsDrawingToGame(game.Area)) drawToGameTextureObjects.Add(obj);
                    if (obj.IsDrawingToGameUI(gameUi.Area)) drawToGameUiTextureObjects.Add(obj);
                    
                    obj.Update(time, game, gameUi, ui);
                    
                    if (obj.IsDead || obj.HasLeftBounds(Bounds))
                    {
                        RemoveGameObject(obj);
                    }
                }
            }

            clearAreaActive = false;
        }

        #endregion
        
        #region Fixed Framerate
        
        public virtual void PreFixedUpdate(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            if (clearAreaActive)
            {
                if (!Bounds.OverlapShape(clearArea)) clearAreaActive = false;
            }
            
            foreach (var layer in allObjects)
            {
                var objs = allObjects[layer.Key];
                if (objs.Count <= 0) continue;

                for (int i = objs.Count - 1; i >= 0; i--)
                {
                    var obj = objs[i];
                    
                    if (clearAreaActive && (clearAreaMask.IsEmpty() || clearAreaMask.Has((uint)layer.Key)))
                    {
                        if (clearArea.OverlapShape(obj.GetBoundingBox()))
                        {
                            RemoveGameObject(obj);
                            continue;
                        }
                    }
                    
                    obj.Update(time, game, gameUi, ui);
                }
            }
            
            clearAreaActive = false;
        }
        public virtual void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            foreach (var layer in allObjects)
            {
                var objs = allObjects[layer.Key];
                if (objs.Count <= 0) continue;

                for (int i = objs.Count - 1; i >= 0; i--)
                {
                    var obj = objs[i];

                    obj.UpdateParallaxe(ParallaxePosition);
                    
                    obj.Update(fixedTime, game, gameUi, ui);
                    
                    if (obj.IsDead || obj.HasLeftBounds(Bounds))
                    {
                        RemoveGameObject(obj);
                    }
                }
            }
        }
        public virtual void InterpolateFixedUpdate(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui, float f)
        {
            drawToGameTextureObjects.Clear();
            drawToGameUiTextureObjects.Clear();
            
            foreach (var layer in allObjects)
            {
                var objs = allObjects[layer.Key];
                if (objs.Count <= 0) continue;

                for (int i = objs.Count - 1; i >= 0; i--)
                {
                    var obj = objs[i];
                    if (obj.IsDrawingToGame(game.Area)) drawToGameTextureObjects.Add(obj);
                    if (obj.IsDrawingToGameUI(gameUi.Area)) drawToGameUiTextureObjects.Add(obj);
                    obj.InterpolateFixedUpdate(time, game, gameUi, ui, f);
                }
            }
        }
        
        #endregion
        
        public virtual void DrawGame(ScreenInfo game)
        {
            foreach (var obj in drawToGameTextureObjects)
            {
                obj.DrawGame(game);
            }
        }
        public virtual void DrawGameUI(ScreenInfo gameUi)
        {
            foreach (var obj in drawToGameUiTextureObjects)
            {
                obj.DrawGameUI(gameUi);
            }
        }

        private void AddLayer(uint layer, int capacityEstimate = 128)
        {
            if (!allObjects.ContainsKey(layer))
            {
                allObjects.Add(layer, new(capacityEstimate));
            }
        }
    }
}




