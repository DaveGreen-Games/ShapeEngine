using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes;

public class CircleSector
{
    #region Public Members

    public int Accuracy { get; private set; }
    public float Radius { get; private set; }
    public Vector2 Center { get; private set; }
    public float AngleDirectionRad { get; private set; }
    public float AngleSectorRad { get; private set; }

    #endregion

    #region Private Members

    private Polygon shape = new();
    private bool dirty = false;

    #endregion
    
    #region Constructor

    public CircleSector(Vector2 center, float radius, float angleDirectionRad, float angleSectorRad, int accuracy = 3)
    {
        SetCenter(center);
        SetRadius(radius);
        SetAngleDirection(angleDirectionRad);
        SetAngleSector(angleSectorRad);
        SetAccuracy(accuracy);

        UpdateShape();
    }
    
    public CircleSector(Vector2 center, float radius, Vector2 direction, float angleSectorRad, int accuracy = 3)
    {
        SetCenter(center);
        SetRadius(radius);
        SetAngleDirection(direction);
        SetAngleSector(angleSectorRad);
        SetAccuracy(accuracy);
        
        UpdateShape();
    }
    
    #endregion
    
    #region Public Methods

    public Polygon GetShape()
    {
        if (dirty) UpdateShape();
        return shape;
    }
    public Polygon GetShapeCopy() => shape.ToPolygon();
    public Polygon? GenerateShape()
    {
        if (AngleSectorRad <= 0 || Radius <= 0) return null;
        var result = new Polygon(3 + Accuracy);
            
        //ccw order
        result.Add(Center);
        var angleStep = AngleSectorRad / (Accuracy + 1);
        var v = ShapeVec.VecFromAngleRad(AngleDirectionRad - angleStep / 2) * Radius;
        for (int i = 0; i < Accuracy + 2; i++)
        {
            result.Add(Center + v);
            v = v.Rotate(angleStep);
        }
            
        return result;
    }

    #endregion
    
    #region Set Methods
    
    public void SetAccuracy(int value)
    {
        if(Accuracy == value) return;
        dirty = true;
        Accuracy = value <= 0 ? 0 : value;
    }
    public void SetRadius(float value)
    {
        if(Math.Abs(Radius - value) < 0.00001f) return;
        dirty = true;
        Radius = value <= 0f ? 0f : value;
    }
    public void SetCenter(Vector2 value)
    {
        if(Center == value) return;
        dirty = true;
        Center = value;
    }
    public void SetAngleDirection(float radians)
    {
        if(Math.Abs(AngleDirectionRad - radians) < 0.00001f) return;
        dirty = true;
        AngleDirectionRad = ShapeMath.WrapAngleRad(radians);
    }
    public void SetAngleDirection(Vector2 newDirection)
    {
        if(newDirection == Vector2.Zero) return;
        SetAngleDirection(newDirection.AngleRad());
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
    public void ChangeRadius(float amount)
    {
        if(amount == 0) return;
        SetRadius(Radius + amount);
    }
    public void ChangeCenter(Vector2 amount)
    {
        if(amount.X == 0 && amount.Y == 0) return;
        SetCenter(Center + amount);
    }
    public void ChangeAngleDirection(float radians)
    {
        if(radians == 0) return;
        SetAngleDirection(AngleDirectionRad + radians);
    }
    public void ChangeAngleSector(float radians)
    {
        if(radians == 0) return;
        SetAngleSector(AngleSectorRad + radians);
    }
    
    #endregion
    
    #region Private Methods
    
    private void UpdateShape()
    {
        shape.Clear();
        dirty = false;
        if (AngleSectorRad <= 0 || Radius <= 0) return;
            
        //ccw order
        shape.Add(Center);
        var angleStep = AngleSectorRad / (Accuracy + 1);
        var v = ShapeVec.VecFromAngleRad(AngleDirectionRad - angleStep / 2) * Radius;
        for (int i = 0; i < Accuracy + 2; i++)
        {
            shape.Add(Center + v);
            v = v.Rotate(angleStep);
        }
    }
    
    #endregion
}