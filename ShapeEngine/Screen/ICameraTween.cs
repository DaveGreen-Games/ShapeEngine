using ShapeEngine.Timing;
using System.Numerics;

namespace ShapeEngine.Screen
{
    /// <summary>
    /// Represents a tween operation for a camera, providing offset, rotation, and zoom factor values.
    /// </summary>
    /// <remarks>
    /// Implement this interface to define custom camera tweening behaviors for smooth transitions and effects.
    /// </remarks>
    public interface ICameraTween : ISequenceable
    {
        /// <summary>
        /// Gets the current offset for the camera.
        /// </summary>
        /// <returns>The offset as a <see cref="Vector2"/>.</returns>
        public Vector2 GetOffset() { return new(0f); }
    
        /// <summary>
        /// Gets the current rotation in degrees for the camera.
        /// </summary>
        /// <returns>The rotation in degrees.</returns>
        public float GetRotationDeg() { return 0f; }
    
        /// <summary>
        /// Gets the current zoom factor for the camera.
        /// </summary>
        /// <returns>The zoom factor.</returns>
        public float GetZoomFactor() { return 1f; }
    }
}
