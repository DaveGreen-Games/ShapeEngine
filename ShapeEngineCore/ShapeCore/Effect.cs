using System.Numerics;
using Raylib_CsLo;
using ShapeTiming;
using ShapeLib;

namespace ShapeCore
{
    public class Effect : GameObject
    {
        protected Vector2 gamePos;
        protected BasicTimer lifetimeTimer = new();

        public Effect(Vector2 gamePos) { this.gamePos = gamePos; }
        public Effect(Vector2 gamePos, float lifeTime) { this.gamePos = gamePos; lifetimeTimer.Start(lifeTime); }


        public override void Update(float dt)
        {
            if (IsDead()) return;
            lifetimeTimer.Update(dt);
        }
        public override bool IsDead()
        {
            return lifetimeTimer.IsFinished();
        }

    }
    public class ShapeEffect : Effect
    {
        protected float size = 0f;
        protected float rotation = 0f;//radians
        protected float rotSpeed = 0f;
        protected Color color = WHITE;
        public ShapeEffect(Vector2 pos, float duration, float size, Color color, float rotSpeed = 0f) : base(pos, duration)
        {
            DrawOrder = 90;
            this.size = size;
            this.color = color;
            rotation = SRNG.randF(2f * PI);
            this.rotSpeed = rotSpeed * SRNG.randF() < 0.5f ? 1f : -1f;
        }
        public ShapeEffect(Vector2 pos, float duration, float size, Color color, float rot = 0f, float rotSpeed = 0f) : base(pos, duration)
        {
            DrawOrder = 90;
            this.size = size;
            this.color = color;
            rotation = rot;
            this.rotSpeed = rotSpeed * SRNG.randF() < 0.5f ? 1f : -1f;
        }
        public override void Update(float dt)
        {
            if (IsDead()) return;
            base.Update(dt);
            rotation += rotSpeed * dt;
        }
        protected virtual float GetCurSize() { return SUtils.LerpFloat(size, 0f, 1.0f - lifetimeTimer.GetF()); }
        public override Rectangle GetBoundingBox()
        {
            return new(gamePos.X - size, gamePos.Y - size, size * 2, size * 2);
        }
    }
    public class SquareEffect : ShapeEffect
    {
        public SquareEffect(Vector2 pos, float duration, float size, Color color, float rotSpeed = 0f) : base(pos, duration, size, color, rotSpeed) { }
        public override void Draw()
        {
            if (IsDead()) return;
            float curSize = GetCurSize();
            Rectangle rect = new(gamePos.X, gamePos.Y, curSize * 2f, curSize * 2f);
            DrawRectanglePro(rect, new(curSize, curSize), rotation * RAD2DEG, color);
        }
    }
    public class CircleEffect : ShapeEffect
    {
        public CircleEffect(Vector2 pos, float duration, float radius, Color color) : base(pos, duration, radius, color, 0f) { }
        public override void Draw()
        {
            if (IsDead()) return;
            DrawCircleV(gamePos, GetCurSize(), color);
        }
    }

    public class LineEffect : ShapeEffect
    {
        float lineThickness = 4f;
        public LineEffect(Vector2 pos, float duration, float size, float thickness, Color color, float rot = 0f, float rotSpeed = 0f) : base(pos, duration, size, color, rot, rotSpeed)
        {
            lineThickness = thickness;
        }
        public LineEffect(Vector2 pos, float duration, float size, float thickness, Color color, float rotSpeed = 0f) : base(pos, duration, size, color, rotSpeed)
        {
            lineThickness = thickness;
        }
        public LineEffect(Vector2 pos, float duration, float size, float thickness, Color color) : base(pos, duration, size, color, 0f)
        {
            lineThickness = thickness;
        }
        public override void Draw()
        {
            if (IsDead()) return;
            DrawLineEx(gamePos, gamePos + SVec.Rotate(SVec.Right(), rotation) * GetCurSize(), lineThickness, color);
        }
        public override Rectangle GetBoundingBox()
        {
            Vector2 end = gamePos + SVec.Rotate(SVec.Right() * size, rotation);
            return new(gamePos.X, gamePos.Y, end.X - gamePos.X, end.Y - gamePos.Y);
        }
    }
}

/*
public class SquareEffect : Effect
    {
        float size = 0f;
        float curSize = 0f;
        float rotation = 0f;
        bool second = false;
        float duration = 0f;
        Color color = WHITE;
        public SquareEffect(Vector2 pos, float duration, float size, Color color) : base(pos, duration * 0.8f)
        {
            this.duration = duration;
            this.size = size;
            this.curSize = size;
            this.color = color;
            this.rotation = RNG.randF(360f);
        }
        public override void Update(float dt)
        {
            base.Update(dt);

            if (lifetimeTimer.IsFinished() && !second)
            {
                lifetimeTimer.Start(duration * 0.2f);
                second = true;
            }
            if (second) curSize = Utils.LerpFloat(size, 0, 1.0f - lifetimeTimer.GetF());
        }

        public override void Draw()
        {
            Rectangle rect = new(pos.X, pos.Y, curSize * 2f, curSize * 2f);
            DrawRectanglePro(rect, new(curSize, curSize), rotation, color);
        }
        public override bool IsDead() { return second && lifetimeTimer.IsFinished(); }
    }
*/

/*
class AsteroidDeathEffect : GameObject
{
    private Color color = WHITE;
    private BasicTimer timer = new();
    private float duration = 0f;
    private Vector2 pos = new();
    private float size = 0f;
    private float curSize = 0f;
    private float rotation = 0f;
    private bool second = false;
    public AsteroidDeathEffect(Vector2 pos, float duration, float size, Color color)
    {
        drawOrder = 30;
        timer.Start(duration * 0.8f);
        this.duration = duration;
        this.size = size;
        this.curSize = size;
        this.pos = pos;
        this.color = color;
        this.rotation = RNG.randF(360f);
    }

    public override void Update(float dt)
    {
        timer.Update(dt);

        if (timer.IsFinished() && !second)
        {
            timer.Start(duration * 0.2f);
            second = true;
        }
        if (second) curSize = Utils.LerpFloat(size, 0, 1.0f - timer.GetF());
        //else rotation += dt * 450;
    }

    public override void Draw()
    {
        Rectangle rect = new(pos.X, pos.Y, curSize * 2f, curSize * 2f);
        //DrawRectangleRec(rect, color);
        DrawRectanglePro(rect, new(curSize, curSize), rotation, color);
    }
    public override bool IsDead() { return second && timer.IsFinished(); }
}
*/