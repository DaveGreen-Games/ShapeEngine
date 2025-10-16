using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;
using ShapeEngine.Geometry.RectDef;

//TODO: Is it possible to have bounds be empty by default and instead:
// - set (valid) bounds and the spawn area will enforce those bounds (How to enforce them? Notify objects that they are out of bounds? Remove them? Call a function to request valid position with bounds parameter?)
// - leave them empty and there will be no bounds enforcement?
namespace ShapeEngine.Core
{
    /// <summary>
    /// Provides a simple area for managing <see cref="GameObject"/> instances with  adding/removing, updating, and drawing functions.
    /// Does not provide a collision system.
    /// <see cref="CollisionHandler"/> provides a collision system if needed.
    /// </summary>
    /// <remarks>
    /// SpawnArea manages collections of <see cref="GameObject"/>s, organized by layer, and provides update, draw, and removal logic.
    /// It supports parallax, area clearing, and event hooks for object addition/removal.
    /// </remarks>
    public class SpawnArea : IDrawable, IBounds
    {
        /// <summary>
        /// Occurs when a <see cref="GameObject"/> is added to the area.
        /// <list type="bullet">
        /// <item><description>GameObject: The object being added.</description></item>
        /// </list>
        /// </summary>
        public event Action<GameObject>? OnGameObjectAdded;
        /// <summary>
        /// Occurs when a <see cref="GameObject"/> is removed from the area.
        /// <list type="bullet">
        /// <item><description>GameObject: The object being removed.</description></item>
        /// </list>
        /// </summary>
        public event Action<GameObject>? OnGameObjectRemoved;
        
        /// <summary>
        /// The initial capacity for new layers.
        /// </summary>
        public int NewLayerStartCapacity = 128;
        /// <summary>
        /// Gets the total number of game objects in the area.
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// Gets or sets the bounds of the spawn area.
        /// </summary>
        public Rect Bounds { get; protected set; }
        /// <summary>
        /// Gets or sets the parallax position for the area.
        /// </summary>
        public Vector2 ParallaxePosition { get; set; } = new(0f);
        
        private readonly SortedList<uint, List<GameObject>> allObjects = new();
        private readonly List<GameObject> drawToGameTextureObjects = [];
        private readonly List<GameObject> drawToGameUiTextureObjects = [];

        private Rect clearArea;
        private bool clearAreaActive;
        private BitFlag clearAreaMask;

        private List<GameObject> removalList = new(1024);
        /// <summary>
        /// Initializes a new instance of the <see cref="SpawnArea"/> class with the specified position and size.
        /// </summary>
        /// <param name="x">The x-coordinate of the spawn area's position.</param>
        /// <param name="y">The y-coordinate of the spawn area's position.</param>
        /// <param name="w">The width of the spawn area.</param>
        /// <param name="h">The height of the spawn area.</param>
        public SpawnArea(float x, float y, float w, float h)
        {
            Bounds = new(x, y, w, h);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SpawnArea"/> class with the specified bounds.
        /// </summary>
        /// <param name="bounds">The bounds of the spawn area.</param>
        public SpawnArea(Rect bounds)
        {
            Bounds = bounds;
        }
        
        /// <summary>
        /// Determines whether the specified layer exists in the spawn area.
        /// </summary>
        /// <param name="layer">The layer to check.</param>
        /// <returns><c>true</c> if the layer exists; otherwise, <c>false</c>.</returns>
        public bool HasLayer(uint layer) { return allObjects.ContainsKey(layer); }

        /// <summary>
        /// Retrieves a list of game objects in the specified layer that match the given criteria.
        /// </summary>
        /// <param name="layer">The layer to search.</param>
        /// <param name="match">The criteria that game objects must match.</param>
        /// <returns>A list of matching game objects, or <c>null</c> if none found.</returns>
        public List<GameObject>? GetGameObjects(uint layer, Predicate<GameObject> match)
        {
            if (Count <= 0) return null;
            return HasLayer(layer) ? allObjects[layer].FindAll(match) : null;
        }
        /// <summary>
        /// Fills the provided list with game objects in the specified layer that match the given criteria.
        /// </summary>
        /// <param name="layer">The layer to search.</param>
        /// <param name="match">The criteria that game objects must match.</param>
        /// <param name="result">The list to fill with matching game objects.</param>
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
        /// <summary>
        /// Retrieves a list of game objects in the spawn area that match the given criteria, across all layers specified by the layer mask.
        /// </summary>
        /// <param name="layerMask">The layer mask specifying which layers to include in the search.</param>
        /// <param name="result">The list to fill with matching game objects.</param>
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
        /// <summary>
        /// Retrieves a list of game objects in the specified layers that match the given criteria.
        /// </summary>
        /// <param name="layerMask">The layer mask specifying which layers to include in the search.</param>
        /// <param name="match">The criteria that game objects must match.</param>
        /// <param name="result">The list to fill with matching game objects.</param>
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

        /// <summary>
        /// Retrieves all game objects in the spawn area.
        /// </summary>
        /// <returns>A list of all game objects in the spawn area, or <c>null</c> if there are none.</returns>
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
        /// <summary>
        /// Fills the provided list with all game objects in the spawn area.
        /// </summary>
        /// <param name="result">The list to fill with all game objects.</param>
        public void GetAllGameObjects(ref List<GameObject> result)
        {
            if (Count <= 0) return;
            
            foreach (var layerGroup in allObjects.Values)
            {
                result.AddRange(layerGroup);
            }
        }

        /// <summary>
        /// Retrieves a list of all game objects in the spawn area that match the given criteria.
        /// </summary>
        /// <param name="match">The criteria that game objects must match.</param>
        /// <param name="result">The list to fill with matching game objects.</param>
        public void GetAllGameObjects(Predicate<GameObject> match, ref List<GameObject> result)
        {
            if (Count <= 0) return;

            
            foreach (var layerGroup in allObjects.Values)
            {
                result.AddRange(layerGroup.FindAll(match));
            }
        }
        /// <summary>
        /// Retrieves a list of all game objects in the spawn area that match the given criteria.
        /// </summary>
        /// <param name="match">The criteria that game objects must match.</param>
        /// <returns>A list of matching game objects, or <c>null</c> if none found.</returns>
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

        /// <summary>
        /// Adds a game object to the spawn area.
        /// </summary>
        /// <param name="gameObject">The game object to add.</param>
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
        /// <summary>
        /// Adds multiple game objects to the spawn area.
        /// </summary>
        /// <param name="areaObjects">The game objects to add.</param>
        public void AddGameObjects(params GameObject[] areaObjects) { foreach (var ao in areaObjects) AddGameObject(ao); }
        /// <summary>
        /// Adds multiple game objects to the spawn area.
        /// </summary>
        /// <param name="areaObjects">The game objects to add.</param>
        public void AddGameObjects(IEnumerable<GameObject> areaObjects) { foreach (var ao in areaObjects) AddGameObject(ao); }
        /// <summary>
        /// Removes a game object from the spawn area.
        /// </summary>
        /// <param name="gameObject">The game object to remove.</param>
        /// <returns><c>true</c> if the game object was successfully removed; otherwise, <c>false</c>.</returns>
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
        /// <summary>
        /// Removes multiple game objects from the spawn area.
        /// </summary>
        /// <param name="areaObjects">The game objects to remove.</param>
        public void RemoveGameObjects(params GameObject[] areaObjects)
        {
            foreach (var ao in areaObjects)
            {
                RemoveGameObject(ao);
            }
        }
        /// <summary>
        /// Removes multiple game objects from the spawn area.
        /// </summary>
        /// <param name="areaObjects">The game objects to remove.</param>
        public void RemoveGameObjects(IEnumerable<GameObject> areaObjects)
        {
            foreach (var ao in areaObjects)
            {
                RemoveGameObject(ao);
            }
        }
        
        
        /// <summary>
        /// Removes game objects from the specified layer that match the given criteria.
        /// </summary>
        /// <param name="layer">The layer to remove game objects from.</param>
        /// <param name="match">The criteria that game objects must match to be removed.</param>
        /// <param name="result">The list to fill with removed game objects.</param>
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
        /// <summary>
        /// Removes game objects from the spawn area that match the given criteria,
        /// across all layers specified by the layer mask.
        /// </summary>
        /// <param name="layerMask">The layer mask specifying which layers to include in the removal.</param>
        /// <param name="result">The list to fill with removed game objects.</param>
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
        /// <summary>
        /// Removes game objects from the specified layers that match the given criteria.
        /// </summary>
        /// <param name="layerMask">The layer mask specifying which layers to include in the removal.</param>
        /// <param name="match">The criteria that game objects must match to be removed.</param>
        /// <param name="result">The list to fill with removed game objects.</param>
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
        /// <summary>
        /// Removes all game objects from the spawn area that match the given criteria.
        /// </summary>
        /// <param name="match">The criteria that game objects must match to be removed.</param>
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

        /// <summary>
        /// Clears a specified area of the spawn area, removing all game objects within it that match the layer mask.
        /// </summary>
        /// <param name="area">The area to clear.</param>
        /// <param name="areaLayerMask">The layer mask specifying which layers to clear.</param>
        public void ClearArea(Rect area, BitFlag areaLayerMask)
        {
            clearArea = area;
            clearAreaMask = areaLayerMask;
            clearAreaActive = true;
        }
        
        /// <summary>
        /// Called when a game object is added to the spawn area.
        /// </summary>
        /// <param name="obj">The game object that was added.</param>
        /// <remarks>Override for custom logic.</remarks>
        protected virtual void GameObjectWasAdded(GameObject obj) { }
        /// <summary>
        /// Called when a game object is removed from the spawn area.
        /// </summary>
        /// <param name="obj">The game object that was removed.</param>
        /// /// <remarks>Override for custom logic.</remarks>
        protected virtual void GameObjectWasRemoved(GameObject obj) { }

        /// <summary>
        /// Clears all game objects from the spawn area.
        /// </summary>
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
        /// <summary>
        /// Clears all game objects from the specified layer.
        /// </summary>
        /// <param name="layer">The layer to clear.</param>
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

        /// <summary>
        /// Initializes the spawn area. Override to add custom initialization logic.
        /// </summary>
        public virtual void Start() { }
        /// <summary>
        /// Closes the spawn area, clearing all objects. Override to add custom closing logic.
        /// </summary>
        /// <remarks>Calls <see cref="Clear"/> function.</remarks>
        public virtual void Close()
        {
            Clear();
        }

        /// <summary>
        /// Draws debug information for the spawn area.
        /// </summary>
        /// <param name="bounds">The color for the bounds.</param>
        /// <param name="border">The color for the border.</param>
        /// <param name="fill">The color for the fill.</param>
        public virtual void DrawDebug(ColorRgba bounds, ColorRgba border, ColorRgba fill)
        {
            this.Bounds.DrawLines(15f, bounds);
        }
        
        #region Open Framerate
        /// <summary>
        /// Updates the spawn area and all contained game objects. This method is called with variable time steps.
        /// </summary>
        /// <param name="time">The current game time.</param>
        /// <param name="game">The screen information for the game area.</param>
        /// <param name="gameUi">The screen information for the game UI area.</param>
        /// <param name="ui">The screen information for the UI area.</param>
        /// <param name="fixedFramerateMode">If this update is called in the open frame rate or fixed frame rate mode.</param>
        public virtual void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui, bool fixedFramerateMode)
        {
            if (fixedFramerateMode)
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
                    
                        if (clearAreaActive && (clearAreaMask.IsEmpty() || clearAreaMask.Has(layer.Key)))
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
            else
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

                        if (clearAreaActive && (clearAreaMask.IsEmpty() || clearAreaMask.Has(layer.Key)))
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
                            RemoveGameObject(obj); //Note: Currenty out of bounds objects are removed.
                        }
                    }
                }

                clearAreaActive = false;
            }
        }

        #endregion
        
        #region Fixed Framerate
        
        /// <summary>
        /// Fixed updates the spawn area and all contained game objects.
        /// This method is called with fixed time steps.
        /// </summary>
        /// <param name="fixedTime">The current fixed game time.</param>
        /// <param name="game">The screen information for the game area.</param>
        /// <param name="gameUi">The screen information for the game UI area.</param>
        /// <param name="ui">The screen information for the UI area.</param>
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
                    
                    obj.FixedUpdate(fixedTime, game, gameUi, ui);
                    
                    if (obj.IsDead || obj.HasLeftBounds(Bounds))
                    {
                        RemoveGameObject(obj);
                    }
                }
            }
        }
        /// <summary>
        /// Interpolates and updates the spawn area and all contained game objects.
        /// This method is called for smooth frame-rate independent rendering.
        /// </summary>
        /// <param name="time">The current game time.</param>
        /// <param name="game">The screen information for the game area.</param>
        /// <param name="gameUi">The screen information for the game UI area.</param>
        /// <param name="ui">The screen information for the UI area.</param>
        /// <param name="f">The interpolation factor.</param>
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
        
        /// <summary>
        /// Draws the game objects that are marked for drawing to the game area.
        /// </summary>
        /// <param name="game">The screen information for the game area.</param>
        public virtual void DrawGame(ScreenInfo game)
        {
            foreach (var obj in drawToGameTextureObjects)
            {
                obj.DrawGame(game);
            }
        }
        /// <summary>
        /// Draws the game objects that are marked for drawing to the game UI area.
        /// </summary>
        /// <param name="gameUi">The screen information for the game UI area.</param>
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

        public Rect GetBounds()
        {
            return Bounds;
        }

        public virtual void SetBounds(Rect newBounds)
        {
            Bounds = newBounds;
        }
    }
}
