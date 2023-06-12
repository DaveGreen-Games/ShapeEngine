using System.Numerics;
using ShapeEngine;
using Raylib_CsLo;
using Lib;
using Demo.Bodies;

namespace Demo
{
    public class Thruster
    {
        Vector2 pos;
        Vector2 offset;
        float rotRad = 0f;
        float thrusterSize = 0f;
        float particleSize = 0f;
        float sizeFactor = 0f;
        Raylib_CsLo.Color color = WHITE;

        public Thruster(Vector2 offset, float thrusterSize, float particleSize)
        {
            this.offset = offset;
            this.thrusterSize = thrusterSize;
            this.particleSize = particleSize;
        }

        public void Update(Vector2 pos, float angleRad, float sizeFactor, Raylib_CsLo.Color color, float dt)
        {
            this.sizeFactor = sizeFactor;
            this.rotRad = angleRad;
            this.color = color;
            this.pos = pos + SVec.Rotate(offset * sizeFactor, angleRad) + SRNG.randVec2(0.5f, 1f);
        }
        public void Draw()
        {
            DrawCircleV(pos, thrusterSize * sizeFactor, color);
        }

        public void SpawnParticles(float speed, Vector2 amountRange, float factor = 1f)
        {
            int randAmount = SRNG.randI((int)amountRange.X, (int)amountRange.Y);
            for (int i = 0; i < randAmount; i++)
            {
                ThrusterParticle p = new(pos, rotRad + PI, speed, color, particleSize * sizeFactor, factor);
                GAMELOOP.AddGameObject(p);
            }
        }
    }

    public class ThrusterParticle : Particle
    {
        private float r = 1f;
        private Raylib_CsLo.Color color;
        public ThrusterParticle(Vector2 pos, float angle, float speed, Raylib_CsLo.Color color, float size, float factor = 1f) : base(pos)
        {
            angle += (SRNG.randF(-25, 25) * DEG2RAD);
            speed *= SRNG.randF(0.9f, 1.1f);
            this.Vel = SVec.Rotate(SVec.Right() * speed, angle) * factor;
            this.r = MathF.Max(SRNG.randF(size * 0.75f, size * 1.25f) * factor, 1f);
            this.color = color;
            float lifetime = SRNG.randF(0.25f, 0.5f) / factor;
            lifetimeTimer.Start(lifetime);
        }
        //public ThrusterParticle(Vector2 pos, float speed, Color color, float size, float lifetime) : base(pos, lifetime)
        //{
        //    float angle = RNG.randF(0f, 2f * PI);
        //    this.vel = Vec.Rotate(Vec.Right() * speed, angle);
        //    this.r = size;
        //    this.color = color;
        //}
        //public ThrusterParticle(Vector2 pos, float angleRad, float accRad, float speed, Color color, float size, float lifetime) : base(pos, lifetime)
        //{
        //    this.vel = Vec.Rotate(Vec.Right() * speed, angleRad + RNG.randF(-accRad, accRad));
        //    this.r = size;
        //    this.color = color;
        //}
        public override void Draw()
        {
            DrawCircle((int)pos.X, (int)pos.Y, r, color);
        }
    }
}
