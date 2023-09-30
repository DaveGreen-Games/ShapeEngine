using System.Numerics;

namespace ShapeEngine.Core.Interfaces;

public interface ICollider : IPhysicsObject
{
    //public Vector2 PrevPos { get; set; }

    /// <summary>
    /// If disabled this collider will not take part in collision detection.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// If disabled this collider will not compute collision but other colliders can still collide with it.
    /// </summary>
    public bool ComputeCollision { get; set; }

    /// <summary>
    /// Should this collider compute intersections with other shapes or just overlaps.
    /// </summary>
    public bool ComputeIntersections { get; set; }

    /// <summary>
    /// Treat all other closed shapes as circles. (not segment or polyline)
    /// Still uses the actual shape for this collider.
    /// If used with CCD any closed shape collision will be a circle/ circle collision
    /// </summary>
    public bool SimplifyCollision { get; set; }

    public bool FlippedNormals { get; set; }

    ///// <summary>
    ///// Enables Continous Collision Detection. Works best for small & fast objects that might tunnel through other shapes especially segments.
    ///// Only works for closed shapes. (not segments or polylines)
    ///// Automatically uses the bounding circle for collision checking, not the actual shape.
    ///// Tunneling occurs when a shape does not collide in the current frame and then moves to the other side of an object in the next frame.
    ///// </summary>
    //public bool CCD { get; set; }

    //public Vector2 GetPrevPos(); // { return Pos; }
    //public void UpdatePrevPos(float dt);// { }
    public IShape GetShape();
    public IShape GetSimplifiedShape();

    public Vector2 GetPreviousPosition();
    public void UpdatePreviousPosition(float dt);

    public void DrawShape(float lineThickness, Raylib_CsLo.Color color);
}