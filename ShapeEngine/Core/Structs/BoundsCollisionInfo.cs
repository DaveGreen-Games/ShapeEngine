using System.Numerics;
using ShapeEngine.Core.CollisionSystem;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents information about a collision with bounds,
/// including the safe position and collision points on both horizontal and vertical planes.
/// </summary>
/// <remarks>
/// This struct is used to encapsulate the result of a bounds collision check,
/// providing details about where an object left the bounds and the last valid position within the bounds.
/// </remarks>
public readonly struct BoundsCollisionInfo
{
    /// <summary>
    /// The closest position within the bounds that is considered safe (not colliding).
    /// </summary>
    public readonly Vector2 SafePosition;
    /// <summary>
    /// The collision point where the object left the bounds on the horizontal plane (left to right).
    /// </summary>
    public readonly CollisionPoint Horizontal;
    /// <summary>
    /// The collision point where the object left the bounds on the vertical plane (top to bottom).
    /// </summary>
    public readonly CollisionPoint Vertical;
    /// <summary>
    /// Indicates whether a valid collision occurred on either the horizontal or vertical plane.
    /// </summary>
    public readonly bool Valid => Horizontal.Valid || Vertical.Valid;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundsCollisionInfo"/> struct with default values.
    /// </summary>
    public BoundsCollisionInfo()
    {
        SafePosition = new();
        Horizontal = new();
        Vertical = new();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="BoundsCollisionInfo"/> struct with the specified safe position and collision points.
    /// </summary>
    /// <param name="safePosition">The closest position within the bounds that is considered safe.</param>
    /// <param name="horizontal">The collision point on the horizontal plane.</param>
    /// <param name="vertical">The collision point on the vertical plane.</param>
    public BoundsCollisionInfo(Vector2 safePosition, CollisionPoint horizontal, CollisionPoint vertical)
    {
        SafePosition = safePosition;
        Horizontal = horizontal;
        Vertical = vertical;
    }

    /// <summary>
    /// Creates a <see cref="BoundsCollisionInfo"/> instance for a horizontal collision only.
    /// </summary>
    /// <param name="safePosition">The closest position within the bounds that is considered safe.</param>
    /// <param name="horizontal">The collision point on the horizontal plane.</param>
    /// <returns>A <see cref="BoundsCollisionInfo"/> with only the horizontal collision set.</returns>
    public static BoundsCollisionInfo GetHorizontal(Vector2 safePosition, CollisionPoint horizontal) =>
        new(safePosition, horizontal, new());

    /// <summary>
    /// Creates a <see cref="BoundsCollisionInfo"/> instance for a vertical collision only.
    /// </summary>
    /// <param name="safePosition">The closest position within the bounds that is considered safe.</param>
    /// <param name="vertical">The collision point on the vertical plane.</param>
    /// <returns>A <see cref="BoundsCollisionInfo"/> with only the vertical collision set.</returns>
    public static BoundsCollisionInfo GetVertical(Vector2 safePosition, CollisionPoint vertical) =>
        new(safePosition, new(), vertical);
}