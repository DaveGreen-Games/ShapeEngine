using System.Numerics;
using Raylib_CsLo;
using ShapeLib;
using ShapeCore;
using ShapeColor;

namespace ShapeEngineDemo.Guns
{
    public class CasingParticle : Particle
    {
        Vector2 size = new(4, 1.5f);
        float rotation = 0f;
        float rotationSpeed = 0f;
        float aDrag = 0f;
        Color color = WHITE;
        Color startColor = WHITE;
        public CasingParticle(Vector2 pos, Vector2 dir, float lifetime, Color color, float sizeFactor = 1f, float speedFactor = 1f) : base(pos, lifetime)
        {
            this.size *= sizeFactor;
            float acc = SRNG.randF(-5f, 5f) * DEG2RAD;
            float speed = SRNG.randF(150, 400);
            this.Vel = SVec.Rotate(dir, acc) * speed * speedFactor;
            this.rotation = SVec.AngleRad(this.Vel);
            this.color = color;
            this.startColor = color;
            this.startColor.a = (byte)(this.startColor.a * 0.5f);
            this.Drag = 3f;
            this.aDrag = 1f;
            this.rotationSpeed = (SRNG.randF(165, 200) * (SRNG.randF() < 0.5f ? -1f : 1f)) * DEG2RAD;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            rotation += rotationSpeed * dt;
            rotationSpeed = SPhysics.ApplyDragForce(rotationSpeed, aDrag, dt);
            float f = 1f - lifetimeTimer.GetF();
            color = SColor.LerpColor(startColor, new(0, 0, 0, 0), f * f * f * f * f);
        }
        public override void Draw()
        {
            Rectangle rect = new(pos.X, pos.Y, size.X, size.Y);
            DrawRectanglePro(rect, size / 2, rotation * RAD2DEG, color);
        }
    }
}
