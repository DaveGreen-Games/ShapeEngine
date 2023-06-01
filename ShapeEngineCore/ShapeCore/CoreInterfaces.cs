

using ShapeLib;
using Raylib_CsLo;
using System.Numerics;

namespace ShapeCore
{
    public interface IScene
    {
        public bool CallUpdate { get; set; }
        public bool CallHandleInput { get; set; }
        public bool CallDraw { get; set; }


        public Area? GetCurArea();// { return null; }


        public void Activate(IScene oldScene);// { }
        public void Deactivate();// { }// { if (newScene != null) GAMELOOP.SwitchScene(this, newScene); }

        public void Start();// { }
        public void Close();// { }

        public void HandleInput(float dt);// { }
        public void Update(float dt, Vector2 mousePosGame);// { }
        public void Draw(Vector2 gameSize, Vector2 mousePosGame);// { }
        public void DrawUI(Vector2 uiSize, Vector2 mousePosUI);// { }

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
        public virtual void Update(float dt) {  }
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
        public virtual void Draw() {  }
        /// <summary>
        /// Called every frame after Draw was called, if DrawToUI is true.
        /// </summary>
        /// <param name="uiSize">The current size of the UI texture.</param>
        public virtual void DrawUI(Vector2 uiSize) {  }
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
    /// <summary>
    /// Used by the area. Each IGameObject is updated and drawn by the area. If it dies it is removed from the area.
    /// </summary>
    public interface IGameObject : ILocation, IBehaviorReceiver, IUpdateable, IDrawable, IAreaObject, IKillable
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
        public void Overlap(CollisionInfo info);
        public void OverlapEnded(ICollidable other);
        public uint GetCollisionLayer();
        public uint[] GetCollisionMask();
        public Vector2 GetVelocity() { return GetCollider().Vel; }
        Rect ILocation.GetBoundingBox() { return GetCollider().GetShape().GetBoundingBox(); }
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
        public Vector2 GetCentroid();
        public float GetArea();
        public float GetCircumference();
        public float GetCircumferenceSquared();
        public Polygon ToPolygon();
        public PolyLine ToPolyLine();
        public Segments GetEdges();
        //public Segments GetEdges(Vector2 normalReferencePoint);
        public Triangulation Triangulate();
        public Rect GetBoundingBox();
        public Circle GetBoundingCircle();
        public bool IsPointInside(Vector2 p);
        public Vector2 GetClosestPoint(Vector2 p);
        public Vector2 GetClosestVertex(Vector2 p);
        public Vector2 GetRandomPoint();
        public Vector2 GetRandomVertex();
        public Segment GetRandomEdge();
        public Vector2 GetRandomPointOnEdge();
        public void DrawShape(float linethickness, Color color);
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
        public bool Enabled { get; set; }
        public bool ComputeCollision { get; set; }
        public bool ComputeIntersections { get; set; }

        public IShape GetShape();

        public bool CheckOverlap(ICollider other) { return GetShape().Overlap(other.GetShape()); }
        public Intersection CheckIntersection(ICollider other) { return GetShape().Intersect(other.GetShape()); }
        public bool CheckOverlapRect(Rect rect) { return rect.Overlap(GetShape()); }

        //public Rect GetBoundingBox();
        //public void DrawDebugShape(Color color);
        //todo get segments intersection/overlap
    }

}
