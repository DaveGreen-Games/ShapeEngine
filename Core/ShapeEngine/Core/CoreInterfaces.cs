

using ShapeEngine.Lib;
using System.Numerics;

namespace ShapeEngine.Core
{
    public interface IScene : IUpdateable, IDrawable
    {
        public bool CallUpdate { get; set; }
        public bool CallHandleInput { get; set; }
        public bool CallDraw { get; set; }


        public IArea? GetCurArea();


        public void Activate(IScene oldScene);// { }
        public void Deactivate();

        
        /// <summary>
        /// Used for cleanup. Should be called once right before the scene gets deleted.
        /// </summary>
        public void Close();

        //public void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI);// { }
        //public void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI);// { }
        //public void Draw(Vector2 gameSize, Vector2 mousePosGame);// { }
        //public void DrawUI(Vector2 uiSize, Vector2 mousePosUI);// { }
        public void DrawToScreen(Vector2 screenSize, Vector2 mousePos);

    }
    


    public interface ISpatial
    {
        /// <summary>
        /// Get the current position of the object.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetPosition();
        /*
        /// <summary>
        /// The current bounding circle of the object.
        /// </summary>
        /// <returns></returns>
        public Circle GetBoundingCircle();
        */
        /// <summary>
        /// Get the current bounding box of the object.
        /// </summary>
        /// <returns></returns>
        public Rect GetBoundingBox();
        /*
        /// <summary>
        /// The current bounding radius of the object. Is used to generate the bounding circle.
        /// </summary>
        /// <returns></returns>
        public float GetBoundingRadius();
        /// <summary>
        /// The current bounding circle of the radius. Is constructed out of the position and bounding radius of the object.
        /// </summary>
        /// <returns></returns>
        public Circle GetBoundingCircle();

        
        */
    }
    public interface IUpdateable
    {
        /*
        /// <summary>
        /// Multiplier for the total slow factor. 2 means slow factor has twice the effect, 0.5 means half the effect.
        /// </summary>
        public float UpdateSlowResistance { get; set; }
        */
        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="dt"></param>
        public void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI);
    }
    public interface IDrawable
    {

        /// <summary>
        /// Called every frame to draw the object.
        /// </summary>
        public void Draw(Vector2 gameSize, Vector2 mousePosGame);
        /// <summary>
        /// Called every frame after Draw was called, if DrawToUI is true.
        /// </summary>
        /// <param name="uiSize">The current size of the UI texture.</param>
        public void DrawUI(Vector2 uiSize, Vector2 mousePosUI);
    }
    public interface IKillable
    {
        /// <summary>
        /// Try to kill the object.
        /// </summary>
        /// <returns>Return true if kill was successful.</returns>
        public bool Kill();
        /// <summary>
        /// Check if object is dead.
        /// </summary>
        /// <returns></returns>
        public bool IsDead();
    }
    
    
    //how to tell object when delta factor was applied and when delta factor applying stopped?
    public interface IAreaObject : ISpatial, IUpdateable, IDrawable, IKillable//, IBehaviorReceiver
    {
        /// <summary>
        /// Should DrawUI be called on this object.
        /// </summary>
        public bool DrawToUI { get; set; }

        /// <summary>
        /// The area layer the object is stored in. Higher layers are draw on top of lower layers.
        /// </summary>
        public int AreaLayer { get; set; }
        /// <summary>
        /// Is called by the area. Can be used to update the objects position based on the new parallax position.
        /// </summary>
        /// <param name="newParallaxPosition">The new parallax position from the layer the object is in.</param>
        public virtual void UpdateParallaxe(Vector2 newParallaxPosition) { }

        /// <summary>
        /// Check if the object is in a layer.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public sealed bool IsInLayer(int layer) { return this.AreaLayer == layer; }

        /// <summary>
        /// Is called when gameobject is added to an area.
        /// </summary>
        public void AddedToArea(IArea area);
        /// <summary>
        /// Is called by the area once a game object is dead.
        /// </summary>
        public void RemovedFromArea(IArea area);

        /// <summary>
        /// Should this object be checked for leaving the bounds of the area?
        /// </summary>
        /// <returns></returns>
        public bool CheckAreaBounds();
        /// <summary>
        /// Will be called if the object left the bounds of the area. The BoundingCircle is used for this check.
        /// </summary>
        /// <param name="safePosition">The closest position within the bounds.</param>
        /// <param name="collisionPoints">The points where the object left the bounds. Can be 1 or 2 max.</param>
        public void LeftAreaBounds(Vector2 safePosition, CollisionPoints collisionPoints);
        
        public Vector2 GetCameraFollowPosition(Vector2 camPos);

        public virtual bool HasCollidables() { return false; }
        public virtual List<ICollidable> GetCollidables() { return new(); }

        /// <summary>
        ///  Is called right after update if a delta factor was applied to the objects dt.
        /// </summary>
        /// <param name="f">The factor that was applied.</param>
        public void DeltaFactorApplied(float f);
    }

    
    public interface ICollidable
    {
        public ICollider GetCollider();
        public void Overlap(CollisionInformation info);
        public void OverlapEnded(ICollidable other);
        public uint GetCollisionLayer();
        public uint[] GetCollisionMask();
    }

    //public interface IInputReciever
    //{
    //    public virtual bool RecievesInput() { return false; }
    //    public void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI);
    //}
    /// <summary>
    /// Used by the area. Each IGameObject is updated and drawn by the area. If it dies it is removed from the area.
    /// </summary>
    //public interface IGameObject : ILocation, IUpdateable, IDrawable, IAreaObject, IKillable, IBehaviorReceiver//, IInputReciever
    //{
    //    /// <summary>
    //    /// The camera calls this function on its target object. Used to determine the next position for the camera to follow.
    //    /// </summary>
    //    /// <param name="camPos">The current position of the camera.</param>
    //    /// <returns>Returns the new position for the camera to follow.</returns>
    //    public virtual Vector2 GetCameraFollowPosition(Vector2 camPos) { return GetPosition(); }//, float dt, float smoothness = 1f, float boundary = 0f) { return GetPosition(); }
    //}
    /*
    public interface IBehaviorReceiver
    {
        public bool HasBehaviors();
        public bool AddBehavior(IBehavior behavior);
        public bool RemoveBehavior(IBehavior behavior);
    }
    public interface IBehavior
    {
        public HashSet<int> GetAffectedLayers();
        public void Update(float dt);
        public BehaviorResult Apply(IAreaObject obj, float delta);
    }
    public struct BehaviorResult
    {
        public bool finished = false;
        public float newDelta = 0f;

        public BehaviorResult(bool finished, float newDelta) { this.finished = finished; this.newDelta = newDelta; }
    }
   */

    public interface IShape
    {
        /// <summary>
        /// All normals face outwards of shapes per default or face right along the direction of segments.
        /// If flipped normals is true all normals face inwards of shapes or face left along the direction of segments.
        /// </summary>
        public bool FlippedNormals { get; set; }
        public Vector2 GetCentroid();
        public float GetArea();
        public float GetCircumference();
        public float GetCircumferenceSquared();
        public Polygon ToPolygon();
        public Polyline ToPolyline();
        public Segments GetEdges();
        public Triangulation Triangulate();
        public Rect GetBoundingBox();
        public Circle GetBoundingCircle();
        public bool IsPointInside(Vector2 p);
        public CollisionPoint GetClosestPoint(Vector2 p);
        public Vector2 GetClosestVertex(Vector2 p);
        public Vector2 GetRandomPoint();
        public List<Vector2> GetRandomPoints(int amount);
        public Vector2 GetRandomVertex();
        public Segment GetRandomEdge();
        public Vector2 GetRandomPointOnEdge();
        public List<Vector2> GetRandomPointsOnEdge(int amount);
        public void DrawShape(float linethickness, Raylib_CsLo.Color color);
    }
    public interface IPhysicsObject
    {
        public Vector2 Pos { get; set; }
        public Vector2 Vel { get; set; }
        public float Mass { get; set; }
        public float Drag { get; set; }
        public Vector2 ConstAcceleration { get; set; }
        public void AddForce(Vector2 force);
        public void AddImpulse(Vector2 force);
        public bool IsStatic(float deltaSq) { return Vel.LengthSquared() <= deltaSq; }
        public Vector2 GetAccumulatedForce();
        public void ClearAccumulatedForce();
        public void UpdateState(float dt);
    }
    public interface ICollider : IPhysicsObject
    {
        //public Vector2 PrevPos { get; set; }

        /// <summary>
        /// If disabled this collider will not take part in collision detection.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// If disabled this collider will not compute collision but other colliders can still collide with it.
        /// </summary>
        public bool ComputeCollision { get; set; }

        /// <summary>
        /// Should this collider compute intersections with other shapes or just overlaps.
        /// </summary>
        public bool ComputeIntersections { get; set; }

        /// <summary>
        /// Treat all other closed shapes as circles. (not segment or polyline)
        /// Still uses the actual shape for this collider.
        /// If used with CCD any closed shape collision will be a circle/ circle collision
        /// </summary>
        public bool SimplifyCollision { get; set; }

        public bool FlippedNormals { get; set; }

        ///// <summary>
        ///// Enables Continous Collision Detection. Works best for small & fast objects that might tunnel through other shapes especially segments.
        ///// Only works for closed shapes. (not segments or polylines)
        ///// Automatically uses the bounding circle for collision checking, not the actual shape.
        ///// Tunneling occurs when a shape does not collide in the current frame and then moves to the other side of an object in the next frame.
        ///// </summary>
        //public bool CCD { get; set; }

        //public Vector2 GetPrevPos(); // { return Pos; }
        //public void UpdatePrevPos(float dt);// { }
        public IShape GetShape();
        public IShape GetSimplifiedShape();

        public Vector2 GetPreviousPosition();
        public void UpdatePreviousPosition(float dt);
    }

    public interface IBounds
    {
        public Rect Bounds { get; }
        public void ResizeBounds(Rect newBounds);
    }

    public interface IAreaDeltaFactor
    {
        public int ApplyOrder { get; set; }

        public uint GetID();
       
        public bool IsAffectingLayer(int layer);

        /// <summary>
        /// Update the delta factor.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>Returns true when finished.</returns>
        public bool Update(float dt);

        /// <summary>
        /// Recieves the current total delta factor.
        /// </summary>
        /// <param name="totalFactor">The current total delta factor.</param>
        /// <returns>Returns the new total delta factor.</returns>
        public float Apply(float totalFactor);
    }

    public interface IArea : IUpdateable, IDrawable, IBounds
    {
        public int Count { get; }

        public ICollisionHandler? GetCollisionHandler();

        /// <summary>
        /// The parallaxe position for this area. Every IAreaObject that uses parallaxe scales its position based on this position.
        /// For instance could be set to the players position or the cameras position that follows the player.
        /// </summary>
        public Vector2 ParallaxePosition { get; set; }

        public void AddDeltaFactor(IAreaDeltaFactor deltaFactor);
        public bool RemoveDeltaFactor(IAreaDeltaFactor deltaFactor);
        public bool RemoveDeltaFactor(uint id);


        public List<IAreaObject> GetAreaObjects(int layer, Predicate<IAreaObject> match);
        public List<IAreaObject> GetAllGameObjects();
        public List<IAreaObject> GetAllGameObjects(Predicate<IAreaObject> match);


        public bool HasLayer(int layer);

        public void AddAreaObject(IAreaObject areaObjects);
        public void AddAreaObjects(params IAreaObject[] areaObjects);
        public void AddAreaObjects(IEnumerable<IAreaObject> areaObjects);

        public void RemoveAreaObject(IAreaObject areaObject);
        public void RemoveAreaObjects(Predicate<IAreaObject> match);
        public void RemoveAreaObjects(int layer, Predicate<IAreaObject> match);
        public void RemoveAreaObjects(params IAreaObject[] areaObjects);
        public void RemoveAreaObjects(IEnumerable<IAreaObject> areaObjects);

        public void Clear();
        public void ClearLayer(int layer);

        
        
        //public bool HasBehaviors() { return behaviors.Count > 0; }
        //public bool AddBehavior(IBehavior behavior) { return behaviors.Add(behavior); }
        //public bool RemoveBehavior(IBehavior behavior) { return behaviors.Remove(behavior); }
        

        public void Start();
        public void Close();

        //public void DrawDebug(params Raylib_CsLo.Color[] colors);

    }
    
    public interface ICollisionHandler : IUpdateable, IBounds
    {
        public int Count { get; }


        public void Add(ICollidable collidable);
        public void AddRange(IEnumerable<ICollidable> collidables);
        public void AddRange(params ICollidable[] collidables);
        public void Remove(ICollidable collidable);
        public void RemoveRange(IEnumerable<ICollidable> collidables);
        public void RemoveRange(params ICollidable[] collidables);

        public void Clear();
        public void Close();


        public List<QueryInfo> QuerySpace(ICollidable collidable, bool sorted = false);
        public List<QueryInfo> QuerySpace(ICollider collider, bool sorted = false, params uint[] collisionMask);
        public List<QueryInfo> QuerySpace(IShape shape, bool sorted = false, params uint[] collisionMask);
        public List<QueryInfo> QuerySpace(IShape shape, ICollidable[] exceptions, bool sorted = false, params uint[] collisionMask);


        public List<ICollidable> CastSpace(ICollidable collidable, bool sorted = false);
        public List<ICollidable> CastSpace(ICollider collider, bool sorted = false, params uint[] collisionMask);
        public List<ICollidable> CastSpace(IShape castShape, bool sorted = false, params uint[] collisionMask);

    }
    
    /*
    public interface ISpatialHash
    {
        //public ISpatialHash<T> Resize(Rect newBounds);
        //public BucketInfo GetBucketInfo(int bucketIndex);

        public void DebugDraw(params Raylib_CsLo.Color[] colors);

        public void AddRange(IEnumerable<ICollidable> colliders);
        public void Add(ICollidable collidable);

        public void Clear();
        public void Close();

        public List<ICollidable> GetObjects(ICollidable collidable);
        public List<ICollidable> GetObjects(ICollider collider, params uint[] mask);
        public List<ICollidable> GetObjects(IShape shape, params uint[] mask);

    }
    */
}
