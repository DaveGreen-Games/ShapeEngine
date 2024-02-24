﻿using ShapeEngine.Lib;
using ShapeEngine.Core.Shapes;
using System.Numerics;
using System.Runtime.Serialization;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core
{
    /// <summary>
    /// Provides a simple area for managing adding/removing, updating, and drawing of area objects. Does not provide a collision system.
    /// </summary>
    public class SpawnArea : IUpdateable, IDrawable, IBounds
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
        public CollisionHandler? CollisionHandler { get; private set; } = null;
        public Vector2 ParallaxePosition { get; set; } = new(0f);

        private readonly SortedList<uint, List<GameObject>> allObjects = new();
        private readonly List<GameObject> drawToGameTextureObjects = new();
        private readonly List<GameObject> drawToUITextureObjects = new();

        private Rect clearArea = new();
        private bool clearAreaActive = false;
        private BitFlag clearAreaMask = new();
        
        
        // public SpawnArea()
        // {
        //     Bounds = new Rect();
        // }
        public SpawnArea(float x, float y, float w, float h)
        {
            Bounds = new(x, y, w, h);
        }
        public SpawnArea(Rect bounds)
        {
            Bounds = bounds;
        }
        
        public virtual bool InitCollisionHandler(int rows, int cols)
        {
            if (CollisionHandler != null) return false;
            CollisionHandler = new(Bounds, rows, cols);
            return true;
        }
        public virtual bool InitCollisionHandler(CollisionHandler collisionHandler)
        {
            if (CollisionHandler != null) return false;
            if (CollisionHandler == collisionHandler) return false;
            CollisionHandler = collisionHandler; // new(Bounds, rows, cols);
            return true;
        }
        public virtual bool RemoveCollisionHandler()
        {
            if (CollisionHandler == null) return false;
            CollisionHandler.Close();
            CollisionHandler = null;
            return true;
        }
        
        public virtual void ResizeBounds(Rect newBounds)
        {
            Bounds = newBounds;
            CollisionHandler?.ResizeBounds(newBounds);
        }
        public bool HasLayer(uint layer) { return allObjects.ContainsKey(layer); }
        public List<GameObject> GetGameObjects(uint layer, Predicate<GameObject> match) { return HasLayer(layer) ? allObjects[layer].FindAll(match) : new(); }
        public List<GameObject> GetGameObjects(BitFlag layerMask)
        {
            var result = new List<GameObject>();
            foreach (var kvp in allObjects)
            {
                if(layerMask.Has(kvp.Key)) result.AddRange(kvp.Value);
            }
            return result;
        }
        public List<GameObject> GetGameObjects(BitFlag layerMask, Predicate<GameObject> match)
        {
            var result = new List<GameObject>();
            foreach (var kvp in allObjects)
            {
                if(layerMask.Has(kvp.Key)) result.AddRange(kvp.Value.FindAll(match));
            }
            return result;
        }

        public List<GameObject> GetAllGameObjects()
        {
            List<GameObject> objects = new();
            foreach (var layerGroup in allObjects.Values)
            {
                objects.AddRange(layerGroup);
            }
            return objects;
        }
        public List<GameObject> GetAllGameObjects(Predicate<GameObject> match) { return GetAllGameObjects().FindAll(match); }

        
        public void AddGameObject(GameObject gameObject)
        {
            var layer = gameObject.Layer;
            AddLayer(layer);

            allObjects[layer].Add(gameObject);

            if (CollisionHandler != null)
            {
                if (gameObject is CollisionObject co)
                {
                    CollisionHandler.Add(co);
                }
            }
            
            OnGameObjectAdded(gameObject);
            gameObject.OnSpawned(this);
        }
        public void AddGameObjects(params GameObject[] areaObjects) { foreach (var ao in areaObjects) AddGameObject(ao); }
        public void AddGameObjects(IEnumerable<GameObject> areaObjects) { foreach (var ao in areaObjects) AddGameObject(ao); }
        public bool RemoveGameObject(GameObject gameObject)
        {
            if (!allObjects.TryGetValue(gameObject.Layer, out var o)) return false;
            if (!o.Remove(gameObject)) return false;
            if (CollisionHandler != null)
            {
                if (gameObject is CollisionObject co)
                {
                    CollisionHandler.Remove(co);
                }
            }
            OnGameObjectRemoved(gameObject);
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
        public List<GameObject>? RemoveGameObjects(uint layer, Predicate<GameObject> match)
        {
            if (!allObjects.ContainsKey(layer)) return null;
            List<GameObject>? result = null;
            var objs = GetGameObjects(layer, match);
            foreach (var o in objs)
            {
                if(!RemoveGameObject(o)) continue;
                result ??= new();
                result.Add(o);
            }

            return result;
        }
        public List<GameObject>? RemoveGameObjects(BitFlag layerMask)
        {
            var objs = GetGameObjects(layerMask);
            List<GameObject>? result = null;
            foreach (var o in objs)
            {
                if (!RemoveGameObject(o)) continue;
                result ??= new();
                result.Add(o);
            }

            return result;
        }
        public List<GameObject>? RemoveGameObjects(BitFlag layerMask, Predicate<GameObject> match)
        {
            var objs = GetGameObjects(layerMask, match);
            List<GameObject>? result = null;
            foreach (var o in objs)
            {
                if (!RemoveGameObject(o)) continue;
                result ??= new();
                result.Add(o);
            }

            return result;
        }

        
        public void RemoveGameObjects(Predicate<GameObject> match)
        {
            var objs = GetAllGameObjects(match);
            foreach (var o in objs)
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
        public HashSet<CollisionObject>? ClearAreaCollisionObjects(Rect area, BitFlag collisionLayerMask)
        {
            if (CollisionHandler == null) return null;
            
            var result = new List<Collider>();
            CollisionHandler.CastSpace(area, collisionLayerMask, ref result);

            if (result.Count <= 0) return null;
            
            var removedParents = new HashSet<CollisionObject>();
            
            foreach (var collider in result)
            {
                var parent = collider.Parent;
                if (parent != null && !removedParents.Contains(parent))
                {
                    RemoveGameObject(parent);
                    removedParents.Add(parent);
                }
            }

            return removedParents;
        }
        
        
        protected virtual void OnGameObjectAdded(GameObject obj) { }
        protected virtual void OnGameObjectRemoved(GameObject obj) { }

        public virtual void Clear()
        {
            drawToGameTextureObjects.Clear();
            drawToUITextureObjects.Clear();

            foreach (var layer in allObjects.Keys)
            {
                ClearLayer(layer);
            }
            CollisionHandler?.Clear();
        }
        public virtual void ClearLayer(uint layer)
        {
            if (!allObjects.TryGetValue(layer, out var objects)) return;

            for (int i = objects.Count - 1; i >= 0; i--)
            {
                var obj = objects[i];
                objects.RemoveAt(i);
                OnGameObjectRemoved(obj);
                obj.OnDespawned(this);
                
                if (CollisionHandler == null) continue;
                
                if (obj is CollisionObject co) CollisionHandler.Remove(co);

            }
            // objects.Clear();
        }

        public virtual void Start() { }
        public virtual void Close()
        {
            Clear();
            CollisionHandler?.Close();
        }
        
        // private BoundsCollisionInfo HasLeftBounds(GameObject obj) => Bounds.BoundsCollision(obj.GetBoundingBox());
        // {
        //     
        //     var bb = obj.GetBoundingBox();
        //     var pos = bb.Center;
        //     var halfSize = bb.Size * 0.5f;
        //
        //     var newPos = pos;
        //     CollisionPoint Horizontal;
        //     CollisionPoint Vertical;
        //     if (pos.X + halfSize.X > Bounds.Right)
        //     {
        //         newPos.X = Bounds.Right - halfSize.X;
        //         Vector2 p = new(Bounds.Right, ShapeMath.Clamp(pos.Y, Bounds.Bottom, Bounds.Top));
        //         Vector2 n = new(-1, 0);
        //         Horizontal = new(p, n);
        //     }
        //     else if (pos.X - halfSize.X < Bounds.Left)
        //     {
        //         newPos.X = Bounds.Left + halfSize.X;
        //         Vector2 p = new(Bounds.Left, ShapeMath.Clamp(pos.Y, Bounds.Bottom, Bounds.Top));
        //         Vector2 n = new(1, 0);
        //         Horizontal = new(p, n);
        //     }
        //     else Horizontal = new();
        //
        //     if (pos.Y + halfSize.Y > Bounds.Bottom)
        //     {
        //         newPos.Y = Bounds.Bottom - halfSize.Y;
        //         Vector2 p = new(ShapeMath.Clamp(pos.X, Bounds.Left, Bounds.Right), Bounds.Bottom);
        //         Vector2 n = new(0, -1);
        //         Vertical = new(p, n);
        //     }
        //     else if (pos.Y - halfSize.Y < Bounds.Top)
        //     {
        //         newPos.Y = Bounds.Top + halfSize.Y;
        //         Vector2 p = new(ShapeMath.Clamp(pos.X, Bounds.Left, Bounds.Right), Bounds.Top);
        //         Vector2 n = new(0, 1);
        //         Vertical = new(p, n);
        //     }
        //     else Vertical = new();
        //
        //     return new(newPos, Horizontal, Vertical);
        // }

        public virtual void DrawDebug(ColorRgba bounds, ColorRgba border, ColorRgba fill)
        {
            this.Bounds.DrawLines(15f, bounds);
            CollisionHandler?.DebugDraw(border, fill);
            // Raylib.DrawRectangleLinesEx(this.Bounds.Rectangle, 15f, bounds.ToRayColor());
        }

        
        public virtual void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            CollisionHandler?.Update();

            drawToGameTextureObjects.Clear();
            drawToUITextureObjects.Clear();

            if (clearAreaActive)
            {
                //clear area is not within the bounds of the spawn area and therefore irrelevant
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
                    if (obj.IsDrawingToGameUI(ui.Area)) drawToUITextureObjects.Add(obj);
                    
                    obj.Update(time, game, ui);
                    
                    if (obj.IsDead || obj.HasLeftBounds(Bounds))
                    {
                        RemoveGameObject(obj);
                    }
                    // else
                    // {
                    //     
                    //     if (obj.IsCheckingHandlerBounds())
                    //     {
                    //         var check = HasLeftBounds(obj);
                    //         if (check.Valid)
                    //         {
                    //             obj.OnLeftHandlerBounds(check);
                    //         }
                    //     }
                    // }

                }
            }

            clearAreaActive = false;
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

        private void AddLayer(uint layer)
        {
            if (!allObjects.ContainsKey(layer))
            {
                allObjects.Add(layer, new());
            }
        }
        

        
    }
    

    // /// <summary>
    // /// Provides a simple area for managing adding/removing, updating, drawing, and colliding of area objects. 
    // /// </summary>
    // public class SpawnAreaCollision: SpawnArea
    // {
    //     protected CollisionHandler col;
    //     public override CollisionHandler GetCollisionHandler() { return col; }
    //
    //     
    //     public SpawnAreaCollision() : base()
    //     {
    //         col = new CollisionHandler(0,0,0,0,0,0);
    //     }
    //     public SpawnAreaCollision(float x, float y, float w, float h, int rows, int cols) : base(x, y, w, h)
    //     {
    //         col = new CollisionHandler(Bounds, rows, cols);
    //     }
    //     public SpawnAreaCollision(Rect bounds, int rows, int cols) : base(bounds)
    //     {
    //         col = new CollisionHandler(bounds, rows, cols);
    //     }
    //
    //     public override void ResizeBounds(Rect newBounds) { Bounds = newBounds; col.ResizeBounds(newBounds); }
    //
    //     protected override void OnGameObjectAdded(GameObject obj)
    //     {
    //         if (obj is CollisionObject co)
    //         {
    //             col.Add(co);
    //         }
    //         // if (!obj.HasCollisionBody()) return;
    //         // var body = obj.GetCollisionBody();
    //         // if(body != null) col.Add(body);
    //         
    //         // if(obj.HasCollisionBody()) col.Add(obj.GetCollisionBody());
    //     }
    //     protected override void OnGameObjectRemoved(GameObject obj)
    //     {
    //         if (obj is CollisionObject co)
    //         {
    //             col.Remove(co);
    //         }
    //         // if (!obj.HasCollisionBody()) return;
    //         // var body = obj.GetCollisionBody();
    //         // if(body != null) col.Remove(body);
    //         // if(obj.HasCollisionBody()) col.Remove(obj.GetCollisionBody());
    //     }
    //
    //     public override void Clear()
    //     {
    //         base.Clear();
    //         col.Clear();
    //     }
    //
    //     public override void Close()
    //     {
    //         base.Close();
    //         col.Close();
    //     }
    //
    //     public override void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
    //     {
    //         col.Update();
    //
    //         base.Update(time, game, ui);
    //     }
    //     
    //     public override void DrawDebug(ColorRgba bounds, ColorRgba border, ColorRgba fill)
    //     {
    //         base.DrawDebug(bounds, border, fill);
    //         col.DebugDraw(border, fill);
    //     }
    //
    //     
    // }
    //
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




