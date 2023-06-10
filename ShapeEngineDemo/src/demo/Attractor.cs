using System.Numerics;
using ShapeCollision;
using ShapeCore;
using ShapeLib;
using Raylib_CsLo;
using ShapeColor;

namespace ShapeEngineDemo
{
    public class Attractor : GameObject
    {
        private Vector2 pos;
        private float radius;
        private float strength;
        private float friction;
        private readonly string[] collisionMask = new string[] { "particle", "bullet" };
        public Attractor(Vector2 pos, float r, float strength, float friction)
        {
            this.pos = pos;
            this.radius = r;
            this.strength = strength;
            this.friction = friction;
        }

        public override void Update(float dt)
        {
            List<ICollidable> bodies = GAMELOOP.CUR_SCENE.GetCurArea().Col.CastSpace(pos, radius, false, collisionMask);
            foreach (ICollidable body in bodies)
            {
                Collider col = body.GetCollider();
                col.AccumulateForce(SPhysics.Attraction(pos, col.Pos, col.Vel, radius, strength, friction));
            }
        }

        public override void Draw()
        {
            Color color = Demo.PALETTES.C(ColorIDs.Text);
            color.a = (byte)SRNG.randI(140, 160);
            DrawCircle((int)pos.X, (int)pos.Y, SRNG.randF(radius * 0.99f, radius * 1.01f), color);
        }
    }

}
