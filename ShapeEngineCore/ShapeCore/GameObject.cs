using System.Numerics;
using Raylib_CsLo;

namespace ShapeCore
{
    /*public interface IGameObject
    {   
        public void Spawn();
        public void Draw();
        public void Update(float dt);
        public void Destroy();
        public bool IsDead();
        public float Damage(float amount, Vector2 pos, IGameObject dealer);
        public float GetDamage();


    }*/
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

        public virtual Rectangle GetBoundingBox() { return new(GetPosition().X, GetPosition().Y, 1, 1); }
        public virtual Vector2 GetPosition() { return Vector2.Zero; }
        public virtual Vector2 GetCameraPosition(Vector2 camPos, float dt, float smoothness = 1f, float boundary = 0f) { return GetPosition(); }
        public virtual bool IsDead() { return false; }
        public virtual bool Kill() { return false; }
        protected virtual void WasKilled() { }
        public virtual bool IsEnabled() { return true; }
        public virtual bool IsVisible() { return true; }

    }
    
    //public interface IGameObject
    //{
    //    public float DrawOrder { get; set; }
    //    public string Group { get; set; }
    //    public string AreaLayerName { get; set; }
    //    public Vector2 AreaLayerOffset { get; set; }
    //
    //    /// <summary>
    //    /// Slows down or speeds up the gameobject. 2 means twice as fast, 0.5 means half speed. Is affect by slow resistance and area slow factor.
    //    /// </summary>
    //    public float UpdateSlowFactor { get; set; }
    //    /// <summary>
    //    /// Multiplier for the total slow factor. 2 means slow factor has twice the effect, 0.5 means half the effect.
    //    /// </summary>
    //    public float UpdateSlowResistance { get; set; }
    //
    //    public bool IsInGroup(string group) { return this.Group == group; }
    //
    //
    //    public void Start();
    //    public void Destroy();
    //    public void Draw();
    //    public void DrawUI(Vector2 uiSize, Vector2 stretchFactor);
    //    public void Update(float dt);
    //    public void OnPlayfield(bool inner, bool outer);
    //    public Rectangle GetBoundingBox();
    //    public Vector2 GetPosition();
    //    public Vector2 GetCameraPosition(Vector2 camPos, float dt, float smoothness = 1f, float boundary = 0f);
    //    public bool Kill();
    //    protected void WasKilled();
    //    public virtual bool IsDead() { return false; }
    //    public virtual bool IsEnabled() { return true; }
    //    public virtual bool IsVisible() { return true; }
    //}


}
