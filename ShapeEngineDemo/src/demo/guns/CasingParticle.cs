using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals;
using ShapeEngineCore;

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
            float acc = RNG.randF(-5f, 5f) * DEG2RAD;
            float speed = RNG.randF(150, 400);
            this.vel = Vec.Rotate(dir, acc) * speed * speedFactor;
            this.rotation = Vec.AngleRad(this.vel);
            this.color = color;
            this.startColor = color;
            this.startColor.a = (byte)(this.startColor.a * 0.5f);
            this.drag = 0.97f; // 0.92f;
            this.aDrag = 0.95f; // 0.9f;
            this.rotationSpeed = (RNG.randF(165, 200) * (RNG.randF() < 0.5f ? -1f : 1f)) * DEG2RAD;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            rotation += rotationSpeed * dt;
            rotationSpeed *= aDrag;
            float f = 1f - lifetimeTimer.GetF();
            color = Utils.LerpColor(startColor, new(0, 0, 0, 0), f * f * f * f * f);
        }
        public override void Draw()
        {
            Rectangle rect = new(pos.X, pos.Y, size.X, size.Y);
            DrawRectanglePro(rect, size / 2, rotation * RAD2DEG, color);
        }
    }
}
