using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class CircleSectorCollider : Collider
{
    
    public int Accuracy { get; private set; }
    public float AngleSectorRad { get; private set; }

    
    private readonly Polygon shape;
    private bool dirty = false;
   
    public CircleSectorCollider(Transform2D offset, float angleSectorRad, int accuracy = 5) : base(offset)
    {
        AngleSectorRad = angleSectorRad;
        Accuracy = accuracy;
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
        var angleStep = AngleSectorRad / (Accuracy + 1);
        var v = ShapeVec.VecFromAngleRad(CurTransform.RotationRad - angleStep / 2) * radius;
        for (int i = 0; i < Accuracy + 2; i++)
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
        if(Accuracy == value) return;
        dirty = true;
        Accuracy = value <= 0 ? 0 : value;
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
        SetAccuracy(Accuracy + amount);
    }
    public void ChangeAngleSector(float radians)
    {
        if(radians == 0) return;
        SetAngleSector(AngleSectorRad + radians);
    }
    
    #endregion

}