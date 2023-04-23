using ShapeLib;
using System.Numerics;

namespace ShapeCore
{
    public interface IShape
    {
        public Vector2 GetPosition();
        public Rect GetBoundingBox();
    }
    public interface IGameObject : IShape
    {
        public float DrawOrder { get; set; }
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
        public virtual void Draw() { }
        public virtual void DrawUI(Vector2 uiSize) { }
        public virtual void Update(float dt) { }
        public virtual void OnPlayfield(bool inner, bool outer) { }
        public virtual Vector2 GetCameraPosition(Vector2 camPos, float dt, float smoothness = 1f, float boundary = 0f) { return GetPosition(); }
        public bool IsDead();
        
        protected virtual bool WasKilled() { return true; }
        
        //public virtual bool IsEnabled() { return true; }
        //public virtual bool IsVisible() { return true; }
    }
    public interface ICollidable : IGameObject
    {
        public string GetID();
        public Collider GetCollider();
        public void Overlap(CollisionInfo info);
        public void OverlapEnded(ICollidable other);
        public uint GetCollisionLayer();
        public uint[] GetCollisionMask();
        public Vector2 GetVelocity() { return GetCollider().Vel; }
        Rect IShape.GetBoundingBox() { return GetCollider().GetBoundingBox(); }
        Vector2 IShape.GetPosition() { return GetCollider().Pos; }
        //public Vector2 GetPos();
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
    public class PhysicsObject : IPhysicsObject
    {
        public Vector2 Pos { get; set; }
        public Vector2 Vel { get; set; }
        public float Mass { get; set; }
        public float Drag { get; set; }
        public Vector2 ConstAcceleration { get; set; }
        
        protected Vector2 accumulatedForce = new(0f);
        public Vector2 GetAccumulatedForce() { return accumulatedForce; }
        public void ClearAccumulatedForce() { accumulatedForce = new(0f); }
        public void AddForce(Vector2 force) { accumulatedForce = SPhysics.AddForce(this, force); }
        public void AddImpulse(Vector2 force) { SPhysics.AddImpuls(this, force); }
        public void UpdateState(float dt) { SPhysics.UpdateState(this, dt); }
    }
}

/*
    public class GameObject
    {
        public float DrawOrder { get; set; } = 0;
        public string Group { get; set; } = "default";
        public string AreaLayerName { get; set; } = "default";
        public Vector2 AreaLayerOffset { get; set; } = new(0f);

        /// <summary>
        /// Slows down or speeds up the gameobject. 2 means twice as fast, 0.5 means half speed. Is affect by slow resistance and area slow factor.
        /// </summary>
        public float UpdateSlowFactor { get; set; } = 1f;
        /// <summary>
        /// Multiplier for the total slow factor. 2 means slow factor has twice the effect, 0.5 means half the effect.
        /// </summary>
        public float UpdateSlowResistance { get; set; } = 1f;
        public GameObject() { }

        public bool IsInGroup(string group) { return this.Group == group; }


        public virtual void Start() { }
        public virtual void Destroy() { }
        public virtual void Draw() { }
        public virtual void DrawUI(Vector2 uiSize) { }
        public virtual void Update(float dt) { }
        public virtual void OnPlayfield(bool inner, bool outer) { }
        //public virtual float Damage(float amount, Vector2 pos, GameObject dealer) { return 0; }
        //public virtual float GetDamage() { return 0; }

        public virtual Rect GetBoundingBox() { return new(GetPosition().X, GetPosition().Y, 1, 1); }
        public virtual Vector2 GetPosition() { return Vector2.Zero; }
        public virtual Vector2 GetCameraPosition(Vector2 camPos, float dt, float smoothness = 1f, float boundary = 0f) { return GetPosition(); }
        public virtual bool IsDead() { return false; }
        public virtual bool Kill() { return false; }
        protected virtual void WasKilled() { }
        public virtual bool IsEnabled() { return true; }
        public virtual bool IsVisible() { return true; }

    }
    */