using System.Numerics;

namespace ShapeEngine.Core.Structs;

public readonly struct BoundsCollisionInfo
{
    /// <summary>
    /// The closest position within the bounds.
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
    public readonly bool Valid => Horizontal.Valid || Vertical.Valid;

    public BoundsCollisionInfo()
    {
        SafePosition = new();
        Horizontal = new();
        Vertical = new();
    }
    public BoundsCollisionInfo(Vector2 safePosition, CollisionPoint horizontal, CollisionPoint vertical)
    {
        SafePosition = safePosition;
        Horizontal = horizontal;
        Vertical = vertical;
    }

    public static BoundsCollisionInfo GetHorizontal(Vector2 safePosition, CollisionPoint horizontal) =>
        new(safePosition, horizontal, new());

    public static BoundsCollisionInfo GetVertical(Vector2 safePosition, CollisionPoint vertical) =>
        new(safePosition, new(), vertical);
}