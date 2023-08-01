

using ShapeEngine.Lib;
using System.Numerics;

namespace ShapeEngine.Core
{
    public interface IScene
    {
        public bool CallUpdate { get; set; }
        public bool CallHandleInput { get; set; }
        public bool CallDraw { get; set; }


        public Area? GetCurArea();// { return null; }


        public void Activate(IScene oldScene);// { }
        public void Deactivate();// { }// { if (newScene != null) GAMELOOP.SwitchScene(this, newScene); }

        
        /// <summary>
        /// Used for cleanup. Should be called once right before the scene gets deleted.
        /// </summary>
        public void Close();

        //public void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI);// { }
        public void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI);// { }
        public void Draw(Vector2 gameSize, Vector2 mousePosGame);// { }
        public void DrawUI(Vector2 uiSize, Vector2 mousePosUI);// { }
        public void DrawToScreen(Vector2 screenSize, Vector2 mousePos);

    }
    


    public interface ILocation
    {
        /// <summary>
        /// Get the current position of the object.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetPosition();
        /// <summary>
        /// Get the current bounding box of the object.
        /// </summary>
        /// <returns></returns>
        public Rect GetBoundingBox();
    }
    public interface IUpdateable
    {
        /// <summary>
        /// Multiplier for the total slow factor. 2 means slow factor has twice the effect, 0.5 means half the effect.
        /// </summary>
        public float UpdateSlowResistance { get; set; }
        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="dt"></param>
        public virtual void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI) {  }
    }
    public interface IDrawable
    {
        /// <summary>
        /// Should DrawUI be called on this object.
        /// </summary>
        public bool DrawToUI { get; set; }
        /// <summary>
        /// Called every frame to draw the object.
        /// </summary>
        public virtual void Draw(Vector2 gameSize, Vector2 mousePosGame) {  }
        /// <summary>
        /// Called every frame after Draw was called, if DrawToUI is true.
        /// </summary>
        /// <param name="uiSize">The current size of the UI texture.</param>
        public virtual void DrawUI(Vector2 uiSize, Vector2 mousePosUI) {  }
    }
    public interface IAreaObject
    {
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
        public virtual void AreaEntered() { }
        /// <summary>
        /// Is called by the area once a game object is dead.
        /// </summary>
        public virtual void AreaLeft() { }
        /// <summary>
        /// Called when the object leaves the outer bounds of the area. Can be used to destroy objects that have left the bounds.
        /// </summary>
        public virtual void LeftAreaBounds() { }
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

    //public interface IInputReciever
    //{
    //    public virtual bool RecievesInput() { return false; }
    //    public void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI);
    //}
    /// <summary>
    /// Used by the area. Each IGameObject is updated and drawn by the area. If it dies it is removed from the area.
    /// </summary>
    public interface IGameObject : ILocation, IBehaviorReceiver, IUpdateable, IDrawable, IAreaObject, IKillable//, IInputReciever
    {
        /// <summary>
        /// The camera calls this function on its target object. Used to determine the next position for the camera to follow.
        /// </summary>
        /// <param name="camPos">The current position of the camera.</param>
        /// <returns>Returns the new position for the camera to follow.</returns>
        public virtual Vector2 GetCameraFollowPosition(Vector2 camPos) { return GetPosition(); }//, float dt, float smoothness = 1f, float boundary = 0f) { return GetPosition(); }
    }
    public interface ICollidable : IGameObject
    {
        //public uint GetID();
        public ICollider GetCollider();
        public void Overlap(List<CollisionInfo> infos);
        public void OverlapEnded(ICollidable other);
        public uint GetCollisionLayer();
        public uint[] GetCollisionMask();
        public Vector2 GetVelocity() { return GetCollider().Vel; }
        Rect ILocation.GetBoundingBox() { return GetCollider().GetShape().GetBoundingBox(); }
        public Circle GetBoundingCircle() { return GetCollider().GetShape().GetBoundingCircle(); }
        Vector2 ILocation.GetPosition() { return GetCollider().Pos; }
        //public Vector2 GetPos();
    }


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
        public BehaviorResult Apply(IGameObject obj, float delta);
    }
    public struct BehaviorResult
    {
        public bool finished = false;
        public float newDelta = 0f;

        public BehaviorResult(bool finished, float newDelta) { this.finished = finished; this.newDelta = newDelta; }
    }
   

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
        //public Segments GetEdges(Vector2 normalReferencePoint);
        public Triangulation Triangulate();
        public Rect GetBoundingBox();
        public Circle GetBoundingCircle();
        //public IShape GetSimplifiedShape();
        public bool IsPointInside(Vector2 p);
        public Vector2 GetClosestPoint(Vector2 p);
        public Vector2 GetClosestVertex(Vector2 p);
        public Vector2 GetRandomPoint();
        public List<Vector2> GetRandomPoints(int amount);
        public Vector2 GetRandomVertex();
        public Segment GetRandomEdge();
        public Vector2 GetRandomPointOnEdge();
        public List<Vector2> GetRandomPointsOnEdge(int amount);
        public void DrawShape(float linethickness, Raylib_CsLo.Color color);
        //public SegmentShape GetSegmentShape();
        //public Vector2 GetReferencePoint();
        //public void SetPosition(Vector2 position);
        //public bool Equals(IShape other);
        //public Circle GetBoundingCircle();
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
        public bool CheckOverlap(ICollider other);
        public Intersection CheckIntersection(ICollider other);
        public bool CheckOverlapRect(Rect rect);
        //public bool CheckOverlapBoundingCirlce(ICollider other);
        //{
        //    return GetShape().GetBoundingCircle().Overlap(other.GetShape().GetBoundingCircle());
        //}
        //{
        //    if (SimplifyCollision)
        //    {
        //        return GetShape().Overlap(other.GetShape().GetBoundingCircle());
        //    }
        //    else return GetShape().Overlap(other.GetShape()); 
        //}
    }

}
