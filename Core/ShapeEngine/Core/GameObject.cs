
using System.Numerics;

namespace ShapeEngine.Core
{
    public class Gameobject : IGameObject
    {
        public float UpdateSlowResistance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool DrawToUI { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int AreaLayer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool AddBehavior(IBehavior behavior)
        {
            throw new NotImplementedException();
        }

        public Rect GetBoundingBox()
        {
            throw new NotImplementedException();
        }

        public Vector2 GetPosition()
        {
            throw new NotImplementedException();
        }

        public bool HasBehaviors()
        {
            throw new NotImplementedException();
        }

        public bool IsDead()
        {
            throw new NotImplementedException();
        }

        public bool Kill()
        {
            throw new NotImplementedException();
        }

        public bool RemoveBehavior(IBehavior behavior)
        {
            throw new NotImplementedException();
        }
    }
}
