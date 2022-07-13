using System.Numerics;
using ShapeEngineCore.SimpleCollision;
using ShapeEngineCore.Globals;
using ShapeEngineCore;
using Raylib_CsLo;

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
            List<ICollidable> bodies = GAMELOOP.CUR_SCENE.GetCurArea().colHandler.CastSpace(pos, radius, collisionMask);
            foreach (ICollidable body in bodies)
            {
                Collider col = body.GetCollider();
                col.AccumulateForce(Utils.Attraction(pos, col.Pos, col.Vel, radius, strength, friction));
            }
        }

        public override void Draw()
        {
            Color color = PaletteHandler.C("text");
            color.a = (byte)RNG.randI(140, 160);
            DrawCircle((int)pos.X, (int)pos.Y, RNG.randF(radius * 0.99f, radius * 1.01f), color);
        }
    }

}
