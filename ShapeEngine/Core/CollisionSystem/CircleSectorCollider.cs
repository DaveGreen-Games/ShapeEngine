using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.CollisionSystem;

/// <summary>
/// Represents a circle sector collider for collision detection.
/// </summary>
/// <remarks>
/// The circle sector is defined by its center, radius, rotation, sector angle, and arc accuracy.
/// The shape is recalculated based on the current transform and sector parameters.
/// </remarks>
/// <example>
/// Example usage: Attach to an enemy to represent its field of view or detection cone,
/// such as for vision checks, area-of-effect attacks, or directional triggers.
/// </example>
public class CircleSectorCollider : Collider
{
    /// <summary>
    /// How many points are used for the circle sector arc.
    /// 0 creates a triangle where the arc is a straight line.
    /// </summary>
    public int ArcPoints { get; private set; }
    /// <summary>
    /// How wide the circle sector is, in radians.
    /// </summary>
    public float AngleSectorRad { get; private set; }
    /// <summary>
    /// Represents <see cref="Collider.CurTransform"/>.Position (center of the sector).
    /// </summary>
    public Vector2 Center => CurTransform.Position;
    /// <summary>
    /// Represents <see cref="Collider.CurTransform"/>.ScaledSize.Radius (radius of the sector).
    /// </summary>
    public float Radius => CurTransform.ScaledSize.Radius;
    /// <summary>
    /// Represents <see cref="Collider.CurTransform"/>.RotationRad (rotation of the sector).
    /// </summary>
    public float RotationRad => CurTransform.RotationRad;
    
    
    private readonly Polygon shape;
    private bool dirty;
   
    /// <summary>
    /// Initializes a new instance of <see cref="CircleSectorCollider"/> with a transform offset, sector angle, and arc accuracy.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    /// <param name="angleSectorRad">The angle of the sector in radians.</param>
    /// <param name="arcPoints">The number of points used for the arc (accuracy).</param>
    public CircleSectorCollider(Transform2D offset, float angleSectorRad, int arcPoints = 5) : base(offset)
    {
        AngleSectorRad = angleSectorRad;
        ArcPoints = arcPoints;
        shape = new Polygon();

    }

    /// <summary>
    /// Called when the collider is initialized. Calculates the initial points of the sector shape.
    /// </summary>
    protected override void OnInitialized()
    {
        CalculatePoints();
    }

    /// <summary>
    /// Recalculates the points of the sector shape. Called when the shape needs to be updated.
    /// </summary>
    public override void RecalculateShape()
    {
        CalculatePoints();
    }

    /// <summary>
    /// Called when the shape's transform changes. Recalculates the sector points if the transform or state is dirty.
    /// </summary>
    /// <param name="transformChanged">Indicates if the transform has changed.</param>
    protected override void OnShapeTransformChanged(bool transformChanged)
    {
        if(transformChanged || dirty) CalculatePoints();
    }

    private void CalculatePoints()
    {
        dirty = false;
        shape.Clear();
        var radius = CurTransform.ScaledSize.Radius;
        if (AngleSectorRad <= 0 || radius <= 0) return;
            
        //ccw order
        var center = CurTransform.Position;
        shape.Add(center);
        var angleStep = AngleSectorRad / (ArcPoints + 1);
        var v = ShapeVec.VecFromAngleRad(CurTransform.RotationRad - angleStep / 2) * radius;
        for (int i = 0; i < ArcPoints + 2; i++)
        {
            shape.Add(center + v);
            v = v.Rotate(angleStep);
        }
    }
    /// <summary>
    /// Gets the bounding box of the circle sector collider.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    public override Rect GetBoundingBox() => GetPolygonShape().GetBoundingBox();
    /// <summary>
    /// Gets the shape type for this collider.
    /// </summary>
    /// <returns><see cref="ShapeType.Poly"/></returns>
    public override ShapeType GetShapeType() => ShapeType.Poly;
    /// <summary>
    /// Gets the polygon shape representing the sector in world (absolute) space.
    /// </summary>
    /// <returns>The <see cref="Polygon"/> shape.</returns>
    public override Polygon GetPolygonShape() => shape;
    #region Set Methods
    /// <summary>
    /// Sets the arc accuracy (number of points for the arc).
    /// </summary>
    /// <param name="value">The number of arc points.</param>
    public void SetAccuracy(int value)
    {
        if(ArcPoints == value) return;
        dirty = true;
        ArcPoints = value <= 0 ? 0 : value;
    }
    /// <summary>
    /// Sets the sector angle in radians.
    /// </summary>
    /// <param name="radians">The angle in radians.</param>
    public void SetAngleSector(float radians)
    {
        if(Math.Abs(AngleSectorRad - radians) < 0.00001f) return;
        dirty = true;
        AngleSectorRad = ShapeMath.Clamp(radians, 0f, ShapeMath.PI * 2f);
    }
    #endregion
    #region Change Methods
    /// <summary>
    /// Changes the arc accuracy by a given amount.
    /// </summary>
    /// <param name="amount">The amount to change the arc points by.</param>
    public void ChangeAccuracy(int amount)
    {
        if(amount == 0) return;
        SetAccuracy(ArcPoints + amount);
    }
    /// <summary>
    /// Changes the sector angle by a given amount in radians.
    /// </summary>
    /// <param name="radians">The amount to change the angle by (in radians).</param>
    public void ChangeAngleSector(float radians)
    {
        if(radians == 0) return;
        SetAngleSector(AngleSectorRad + radians);
    }
    #endregion
}