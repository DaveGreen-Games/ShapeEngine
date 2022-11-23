using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals.Timing;
using ShapeEngineCore.Globals;
using System.Collections.Specialized;

namespace ShapeEngineCore
{
    public class Particle : GameObject
    {
        protected Vector2 pos;
        protected Vector2 vel;
        protected float drag = 2f;
        protected BasicTimer lifetimeTimer = new();
        private Vector2 accumulatedForce = new(0f, 0f);
        private Vector2 constAccel = new(0f, 0f);

        public Particle(Vector2 pos) { DrawOrder = 25; this.pos = pos; }
        public Particle(Vector2 pos, float lifetime)
        {
            DrawOrder = 25;
            this.pos = pos;
            vel = new(0f, 0f);
            lifetimeTimer.Start(lifetime);
        }
        public Particle(Vector2 pos, Vector2 vel, float lifetime)
        {
            DrawOrder = 25;
            this.pos = pos;
            this.vel = vel;
            lifetimeTimer.Start(lifetime);
        }
        public Particle(Vector2 pos, float angle, float lifetime)
        {
            DrawOrder = 25;
            this.pos = pos;
            vel = Vec.Rotate(Vec.Right(), angle * DEG2RAD);
            lifetimeTimer.Start(lifetime);
        }

        public override Rectangle GetBoundingBox()
        {
            return new(pos.X, pos.Y, 1, 1);
        }
        public override Vector2 GetPosition() { return pos; }
        public Vector2 GetVelocity() { return vel; }
        public void AddImpulse(Vector2 impulse) { vel += impulse; }
        public void AddForce(Vector2 force) { accumulatedForce += force; }
        public void AddAcceleration(Vector2 accel) { constAccel += accel; }
        public void SetAcceleration(Vector2 accel) { constAccel = accel; }
        public void ClearAcceleration() { SetAcceleration(new(0f, 0f)); }

        public void SetDrag(float dragCoefficient)
        {
            drag = dragCoefficient;
        }

        public override void Update(float dt)
        {
            if (IsDead()) return;
            lifetimeTimer.Update(dt);

            ApplyAccumulatedForce(dt);
            ApplyAcceleration(dt);

            pos += vel * dt;
            //vel = Utils.ApplyDragForce(vel, drag, dt);
            //vel *= drag;
        }
        public override bool IsDead()
        {
            return lifetimeTimer.IsFinished();
        }


        private void ApplyAccumulatedForce(float dt)
        {
            vel += accumulatedForce * dt;
            accumulatedForce = new(0f, 0f);
        }
        private void ApplyAcceleration(float dt)
        {
            Vector2 dragForce = Utils.GetDragForce(vel, drag, dt); // drag * -vel;
            Vector2 force = constAccel * dt + dragForce;
            vel += force;
        }

    }

    public class LineParticle : Particle
    {
        private float size = 1f;
        private float rotRad = 0f;
        private float lineThickness = 1f;
        private Color color;

        public LineParticle(Vector2 pos, float speed, Color color, float size, float lifetime) : base(pos, lifetime)
        {
            float angle = RNG.randF(0f, 2f * PI);
            rotRad = angle;
            vel = Vec.Rotate(Vec.Right() * speed, angle);
            this.size = size;
            this.color = color;
        }
        public LineParticle(Vector2 pos, float speed, Color color, float size, float lifetime, float lineThickness = 1f) : base(pos, lifetime)
        {
            float angle = RNG.randF(0f, 2f * PI);
            rotRad = angle;
            vel = Vec.Rotate(Vec.Right() * speed, angle);
            this.size = size;
            this.color = color;
            this.lineThickness = lineThickness;
        }
        public LineParticle(Vector2 pos, float speed, Color color, float size, float lifetime, float lineThickness = 1f, float drag = 2f) : base(pos, lifetime)
        {
            float angle = RNG.randF(0f, 2f * PI);
            rotRad = angle;
            vel = Vec.Rotate(Vec.Right() * speed, angle);
            this.size = size;
            this.color = color;
            this.lineThickness = lineThickness;
            this.drag = drag;
        }
        public LineParticle(Vector2 pos, float angleRad, float speed, Color color, float size, float lifetime, float lineThickness = 1f) : base(pos, lifetime)
        {
            rotRad = angleRad;
            vel = Vec.Rotate(Vec.Right() * speed, rotRad);
            this.size = size;
            this.color = color;
            this.lineThickness = lineThickness;
        }
        public LineParticle(Vector2 pos, float angleRad, float speed, Color color, float size, float lifetime, float lineThickness = 1f, float drag = 2f) : base(pos, lifetime)
        {
            rotRad = angleRad;
            vel = Vec.Rotate(Vec.Right() * speed, rotRad);
            this.size = size;
            this.color = color;
            this.lineThickness = lineThickness;
            this.drag = drag;
        }
        public LineParticle(Vector2 pos, float angleRad, float accRad, float speed, Color color, float size, float lifetime, float lineThickness = 1f) : base(pos, lifetime)
        {
            rotRad = angleRad + RNG.randF(-accRad, accRad);
            vel = Vec.Rotate(Vec.Right() * speed, rotRad);
            this.size = size;
            this.color = color;
            this.lineThickness = lineThickness;
        }
        public override void Draw()
        {
            DrawLineEx(pos, pos + Vec.Rotate(Vec.Right() * size, rotRad), lineThickness, color);
        }
        public override Rectangle GetBoundingBox()
        {
            Vector2 end = pos + Vec.Rotate(Vec.Right() * size, rotRad);
            return new(pos.X, pos.Y, end.X - pos.X, end.Y - pos.Y);
        }
    }
    public class CircleParticle : Particle
    {
        private float r = 1f;
        private Color color;

        public CircleParticle(Vector2 pos, float speed, Color color, float radius, float lifetime) : base(pos, lifetime)
        {
            float angle = RNG.randF(0f, 2f * PI);
            vel = Vec.Rotate(Vec.Right() * speed, angle);
            r = radius;
            this.color = color;
        }
        public CircleParticle(Vector2 pos, float angleRad, float speed, Color color, float radius, float lifetime) : base(pos, lifetime)
        {
            vel = Vec.Rotate(Vec.Right() * speed, angleRad);
            r = radius;
            this.color = color;
        }
        public CircleParticle(Vector2 pos, float speed, Color color, float radius, float lifetime, float drag = 2f) : base(pos, lifetime)
        {
            float angle = RNG.randF(0f, 2f * PI);
            vel = Vec.Rotate(Vec.Right() * speed, angle);
            r = radius;
            this.color = color;
            this.drag = drag;
        }
        public CircleParticle(Vector2 pos, float angleRad, float speed, Color color, float radius, float lifetime, float drag = 2f) : base(pos, lifetime)
        {
            vel = Vec.Rotate(Vec.Right() * speed, angleRad);
            r = radius;
            this.color = color;
            this.drag = drag;
        }
        public CircleParticle(Vector2 pos, float angleRad, float accRad, float speed, Color color, float radius, float lifetime) : base(pos, lifetime)
        {
            vel = Vec.Rotate(Vec.Right() * speed, angleRad + RNG.randF(-accRad, accRad));
            r = radius;
            this.color = color;
        }
        public override void Draw()
        {
            DrawCircle((int)pos.X, (int)pos.Y, r, color);
        }
        public override Rectangle GetBoundingBox()
        {
            return new(pos.X - r, pos.Y - r, r * 2, r * 2);
        }
    }



}


