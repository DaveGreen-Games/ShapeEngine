using ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;

namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Specifies the motion type of a <see cref="CollisionObject"/> for collision detection optimizations.
/// </summary>
public enum MotionType
{
    /// <summary>
    /// The object is static and does not move. Optimized for non-moving objects.
    /// </summary>
    /// <remarks>
    /// If you move a <see cref="CollisionObject"/> with <see cref="MotionType.Static"/>,
    /// you either have to remove and re-add it to the <see cref="CollisionHandler"/> or change its motion type to <see cref="MotionType.Dynamic"/> temporarily.
    /// Failing to do so will result in the <see cref="Collider"/>s of the <see cref="CollisionObject"/> to be still in their old position in the collision system.
    /// </remarks>
    Static,
    /// <summary>
    /// The object is dynamic and can move. Default for moving objects.
    /// </summary>
    Dynamic
}