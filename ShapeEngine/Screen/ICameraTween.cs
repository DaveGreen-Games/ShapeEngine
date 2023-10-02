using ShapeEngine.Timing;
using System.Numerics;

namespace ShapeEngine.Screen
{
    public interface ICameraTween : ISequenceable
    {
        public Vector2 GetOffset() { return new(0f); }
        public float GetRotationDeg() { return 0f; }
        public float GetZoomFactor() { return 1f; }
    }
}
