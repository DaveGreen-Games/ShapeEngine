

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


        public virtual Area? GetCurArea() { return null; }


        public virtual void Activate(IScene oldScene) { }
        public virtual void Deactivate() { }// { if (newScene != null) GAMELOOP.SwitchScene(this, newScene); }

        public virtual void Start() { }
        public virtual void Close() { }

        public virtual void HandleInput() { }
        public virtual void Update(float dt) { }
        public virtual void Draw() { }
        public virtual void DrawUI(Vector2 uiSize) { }

    }


    public interface ILocation
    {
        public Vector2 GetPosition();
        public Rect GetBoundingBox();
    }
    public interface IGameObject : ILocation, IBehaviourReciever
    {
        //public float DrawOrder { get; set; }
        public int AreaLayer { get; set; }
        public bool DrawToUI { get; set; }
        /// <summary>
        /// Multiplier for the total slow factor. 2 means slow factor has twice the effect, 0.5 means half the effect.
        /// </summary>
        public float UpdateSlowResistance { get; set; }
        public virtual void UpdateParallaxe(Vector2 pos)
        {
            //ParallaxeOffset = ParallaxeOffset.Lerp(pos * ParallaxeScaling, ParallaxeSmoothing);
        }

        public sealed bool IsInLayer(int layer) { return this.AreaLayer == layer; }
        public sealed bool Kill()
        {
            if (IsDead()) return false;
            return WasKilled();
        }

        public virtual void Start() { }
        public virtual void Destroy() { }
        public virtual bool Update(float dt) { return false; }
        public virtual bool Draw() { return false; }
        public virtual bool DrawUI(Vector2 uiSize) { return false; }
        public virtual void LeftWorldBounds() { Destroy(); }
        public virtual Vector2 GetCameraPosition(Vector2 camPos, float dt, float smoothness = 1f, float boundary = 0f) { return GetPosition(); }
        public bool IsDead();

        protected virtual bool WasKilled() { return true; }

        //public virtual void OnPlayfield(bool inner, bool outer) { }
        //public virtual bool IsEnabled() { return true; }
        //public virtual bool IsVisible() { return true; }
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


    public interface IBehaviourReciever
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
        public Segments GetEdges();
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
        //todo getsegments intersection/overlap
    }

}
