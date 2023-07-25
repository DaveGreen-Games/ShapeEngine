using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Timing;
using System.Numerics;

namespace ShapeEngine.Effects
{
    /*
    //experiment----------------------------
    public abstract class DrawFuncBasic
    {
        public delegate void DrawFunc<T>(T t) where T : DrawFuncBasic;
    }
    public class DrawFuncExecuter<T> : DrawFuncBasic where T : DrawFuncBasic
    {
        public DrawFunc<T>? Func = null;
        public virtual bool Draw()
        {
            if (this is T t) Func?.Invoke(t);
            return false;
        }

    }
    //-------------------------------------
    */
    public abstract class EffectObject: IGameObject
    {
        public Vector2 Pos { get; set; }
        public Vector2 Size { get; protected set; }
        public TweenType TweenType { get; set; } = TweenType.LINEAR;
        public float LifetimeF { get { return 1f - lifetimeTimer.F; } }
        protected BasicTimer lifetimeTimer = new();

        public float DrawOrder { get; set; } = 0;
        public int AreaLayer { get; set; } = 0;
        public bool DrawToUI { get; set; } = false;
        public Vector2 ParallaxeOffset { get; set; } = new(0f);
        public float UpdateSlowFactor { get; set; } = 1f;
        public float UpdateSlowResistance { get; set; } = 1f;

        public EffectObject(Vector2 pos, Vector2 size) { this.Pos = pos; this.Size = size; }
        public EffectObject(Vector2 pos, Vector2 size, float lifeTime) { this.Pos = pos; this.Size = size; lifetimeTimer.Start(lifeTime); }

        public bool Kill()
        {
            if (IsDead()) return false;
            lifetimeTimer.Stop();
            return true;
        }
        public virtual bool Update(float dt)
        {
            if (IsDead()) return true;
            lifetimeTimer.Update(dt);
            return false;
        }
        public bool IsDead() { return lifetimeTimer.IsFinished; }

        public virtual Vector2 GetPosition() { return Pos; }
        public virtual Rect GetBoundingBox() { return new(Pos, Size, new(0.5f)); }
        protected float GetTweenFloat(float start, float end) { return STween.Tween(start, end, LifetimeF, TweenType); }
        protected Vector2 GetTweenVector2(Vector2 start, Vector2 end) { return start.Tween(end, LifetimeF, TweenType); }
        protected Raylib_CsLo.Color GetTweenColor(Raylib_CsLo.Color startColor, Raylib_CsLo.Color endColor) { return startColor.Tween(endColor, LifetimeF, TweenType); }

        public virtual bool HasBehaviors() { return false; }
        public virtual bool AddBehavior(IBehavior behavior) { return false; }
        public virtual bool RemoveBehavior(IBehavior behavior) { return false; }

        //public void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI) { }
    }

    

}
