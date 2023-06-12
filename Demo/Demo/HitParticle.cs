using System.Numerics;
using ShapeEngine;
using Raylib_CsLo;
using Lib;

namespace Demo
{
    public class HitParticle : Particle
    {
        const float baseRotationSpeed = 100f;
        const float baseSize = 6f;
        const float baseSpeed = 100f;
        
        float startSize = 0f;
        float curSize = 0f;
        float curRotationSpeed = 0f;
        float rotation = 0f;
        Raylib_CsLo.Color color;
        public HitParticle(Vector2 pos, Vector2 dir, float impactFactor, float lifetime, Raylib_CsLo.Color color) : base(pos)
        {
            float speed = SRNG.randF(baseSpeed * impactFactor * 0.5f, baseSpeed * impactFactor * 1.5f);
            this.Vel = SVec.Rotate(dir, SRNG.randF(-0.6f, 0.6f)) * speed;
            this.startSize = SRNG.randF(baseSize * impactFactor * 0.5f, baseSize * impactFactor * 1.5f);
            this.curSize = this.startSize;
            this.curRotationSpeed = baseRotationSpeed;// * impactFactor;
            this.rotation = SRNG.randF(360f);
            this.color = color;
            lifetimeTimer.Start(lifetime);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            rotation += dt * curRotationSpeed;
            curSize = SUtils.LerpFloat(startSize, startSize * 0.05f, 1f - lifetimeTimer.GetF());
        }

        public override void Draw()
        {
            Rectangle rect = new(pos.X, pos.Y, curSize, curSize);
            DrawRectanglePro(rect, new(curSize / 2, curSize / 2), rotation, color);
        }
    }
}
