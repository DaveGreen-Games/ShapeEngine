using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
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
    public abstract class EffectObject : IAreaObject
    {
        public Vector2 Pos { get; set; }
        public Vector2 Size { get; protected set; }
        public TweenType TweenType { get; set; } = TweenType.LINEAR;
        public float LifetimeF { get { return 1f - lifetimeTimer.F; } }
        protected BasicTimer lifetimeTimer = new();

        public int AreaLayer { get; set; } = 0;

        public EffectObject(Vector2 pos, Vector2 size) { this.Pos = pos; this.Size = size; }
        public EffectObject(Vector2 pos, Vector2 size, float lifeTime) { this.Pos = pos; this.Size = size; lifetimeTimer.Start(lifeTime); }

        public bool Kill()
        {
            if (IsDead()) return false;
            lifetimeTimer.Stop();
            return true;
        }
        public bool IsDead() { return lifetimeTimer.IsFinished; }

        public virtual Vector2 GetPosition() { return Pos; }
        public virtual Rect GetBoundingBox() { return new(Pos, Size, new(0.5f)); }
        protected float GetTweenFloat(float start, float end) { return STween.Tween(start, end, LifetimeF, TweenType); }
        protected Vector2 GetTweenVector2(Vector2 start, Vector2 end) { return start.Tween(end, LifetimeF, TweenType); }
        protected Raylib_CsLo.Color GetTweenColor(Raylib_CsLo.Color startColor, Raylib_CsLo.Color endColor) { return startColor.Tween(endColor, LifetimeF, TweenType); }

        /*
        public virtual bool HasBehaviors() { return false; }
        public virtual bool AddBehavior(IBehavior behavior) { return false; }
        public virtual bool RemoveBehavior(IBehavior behavior) { return false; }
        */
        public void AddedToArea(Area area)     {}
        public void RemovedFromArea(Area area) {}
        
        public Vector2 GetCameraFollowPosition(Vector2 camPos) { return GetPosition(); }

        public virtual void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui)
        {
            lifetimeTimer.Update(dt);
        }
        public abstract void DrawGame(Vector2 gameSize, Vector2 mousePosGame);
        public virtual void DrawUI(Vector2 uiSize, Vector2 mousePosUI) { }


        public virtual bool CheckAreaBounds()
        {
            return false;
        }

        public virtual void LeftAreaBounds(Vector2 safePosition, CollisionPoints collisionPoints)
        {
        }

        public virtual void DeltaFactorApplied(float f)
        {
            
        }

        //public ScreenTextureMask? GetTextureMask()
        //{
        //    return null;
        //}

        public virtual bool IsDrawingToScreen()
        {
            return false;
        }
        public virtual bool IsDrawingToGameTexture()
        {
            return true;
        }
        public virtual bool IsDrawingToUITexture()
        {
            return false;
        }
        
        //public void DrawToTexture(ScreenTexture texture)
        //{
        //    
        //}
        public virtual void DrawToScreen(Vector2 size, Vector2 mousePos)
        {
            
        }
    }

    

}
