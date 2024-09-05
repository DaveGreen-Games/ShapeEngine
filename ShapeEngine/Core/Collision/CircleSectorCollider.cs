using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class CircleSectorCollider : Collider
{
    
    /// <summary>
    /// How many points are used for the circle sector arc. 0 creates a triangle where the arc is a straight line.
    /// </summary>
    public int ArcPoints { get; private set; }
    
    /// <summary>
    /// How wide the circle sector is. 
    /// </summary>
    public float AngleSectorRad { get; private set; }

    
    /// <summary>
    /// Represents CurTransform.Position
    /// </summary>
    public Vector2 Center => CurTransform.Position;

    /// <summary>
    /// Represents CurTransform.ScaledSize.Radius
    /// </summary>
    public float Radius => CurTransform.ScaledSize.Radius;

    /// <summary>
    /// Represents CurTransform.RotationRad
    /// </summary>
    public float RotationRad => CurTransform.RotationRad;
    
    
    private readonly Polygon shape;
    private bool dirty = false;
   
    public CircleSectorCollider(Transform2D offset, float angleSectorRad, int arcPoints = 5) : base(offset)
    {
        AngleSectorRad = angleSectorRad;
        ArcPoints = arcPoints;
        shape = new Polygon();

    }

    protected override void OnTransformSetupFinished()
    {
        CalculatePoints();
    }

    public override void Recalculate()
    {
        CalculatePoints();
    }

    protected override void UpdateColliderShape(bool transformChanged)
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
    public override Rect GetBoundingBox() => GetPolygonShape().GetBoundingBox();
    public override ShapeType GetShapeType() => ShapeType.Poly;
    public override Polygon GetPolygonShape() => shape;
    
    #region Set Methods
    
    public void SetAccuracy(int value)
    {
        if(ArcPoints == value) return;
        dirty = true;
        ArcPoints = value <= 0 ? 0 : value;
    }
    public void SetAngleSector(float radians)
    {
        if(Math.Abs(AngleSectorRad - radians) < 0.00001f) return;
        dirty = true;
        AngleSectorRad = ShapeMath.Clamp(radians, 0f, ShapeMath.PI * 2f);
    }
   
    #endregion
    
    #region Change Methods
    
    public void ChangeAccuracy(int amount)
    {
        if(amount == 0) return;
        SetAccuracy(ArcPoints + amount);
    }
    public void ChangeAngleSector(float radians)
    {
        if(radians == 0) return;
        SetAngleSector(AngleSectorRad + radians);
    }
    
    #endregion

}