using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes;


public class CircleSector
{
    #region Public Members
    public Transform2D Transform { get; set; }
    public int Accuracy { get; set; }
    public float AngleSectorRad { get; set; }

    public Vector2 Center
    {
        get => Transform.Position;
        set => Transform = Transform.SetPosition(value);
    }

    public float Radius
    {
        get => Transform.ScaledSize.Radius;
        set => Transform = Transform.SetSize(value);
    }

    public float RotationRad
    {
        get => Transform.RotationRad;
        set => Transform = Transform.SetRotationRad(value);
    }
    #endregion

    #region Constructor

    public CircleSector(Vector2 center, float radius, float rotationRad, float angleSectorRad, int accuracy = 3)
    {
        Transform = new Transform2D(center, rotationRad, new Size(radius));
        Accuracy = accuracy;
        AngleSectorRad = angleSectorRad;
    }
    public CircleSector(Transform2D transform, float angleSectorRad, int accuracy = 3)
    {
        Transform = transform;
        Accuracy = accuracy;
        AngleSectorRad = angleSectorRad;
    }
    public CircleSector(Vector2 center, float radius, Vector2 direction, float angleSectorRad, int accuracy = 3)
    {
        var rotationRad = 0f;
        if (direction != Vector2.Zero) rotationRad = direction.AngleRad();
        Transform = new Transform2D(center, rotationRad, new Size(radius));
        Accuracy = accuracy;
        AngleSectorRad = angleSectorRad;
    }
    
    #endregion
    
    #region Public Methods

    /// <summary>
    /// Clears the given polygon and fills it with new points.
    /// </summary>
    /// <param name="polygon"></param>
    /// <returns>Returns true if new points were generated</returns>
    public bool UpdatePolygon(Polygon polygon)
    {
        if (AngleSectorRad <= 0 || Radius <= 0) return false;
            
        polygon.Clear();
        
        //ccw order
        polygon.Add(Center);
        var angleStep = AngleSectorRad / (Accuracy + 1);
        var v = ShapeVec.VecFromAngleRad(RotationRad - angleStep / 2) * Radius;
        for (int i = 0; i < Accuracy + 2; i++)
        {
            polygon.Add(Center + v);
            v = v.Rotate(angleStep);
        }

        return true;
    }
    
    public CircleSector Copy() => new(Center, Radius, RotationRad, AngleSectorRad, Accuracy);

    public Polygon? GeneratePolygon() => GeneratePolygon(Center, Radius, RotationRad, AngleSectorRad, Accuracy);
    public Points? GeneratePoints() => GeneratePoints(Center, Radius, RotationRad, AngleSectorRad, Accuracy);
    public Segments? GenerateSegments() => GenerateSegments(Center, Radius, RotationRad, AngleSectorRad, Accuracy);

    #endregion

    #region Static

    public static Polygon? GeneratePolygon(Vector2 center, float radius, float rotationRad, float angleSectorRad, int accuracy = 3)
    {
        if (angleSectorRad <= 0 || radius <= 0) return null;
        var result = new Polygon(3 + accuracy);
            
        //ccw order
        result.Add(center);
        var angleStep = angleSectorRad / (accuracy + 1);
        var v = ShapeVec.VecFromAngleRad(rotationRad - angleStep / 2) * radius;
        for (int i = 0; i < accuracy + 2; i++)
        {
            result.Add(center + v);
            v = v.Rotate(angleStep);
        }
            
        return result;
    }
    public static Points? GeneratePoints(Vector2 center, float radius, float rotationRad, float angleSectorRad, int accuracy = 3)
    {
        if (angleSectorRad <= 0 || radius <= 0) return null;
        var result = new Points(3 + accuracy);
            
        //ccw order
        result.Add(center);
        var angleStep = angleSectorRad / (accuracy + 1);
        var v = ShapeVec.VecFromAngleRad(rotationRad - angleStep / 2) * radius;
        for (int i = 0; i < accuracy + 2; i++)
        {
            result.Add(center + v);
            v = v.Rotate(angleStep);
        }
            
        return result;
    }
    public static Segments? GenerateSegments(Vector2 center, float radius, float rotationRad, float angleSectorRad, int accuracy = 3)
    {
        if (angleSectorRad <= 0 || radius <= 0) return null;
        var result = new Segments(3 + accuracy);
            
        //ccw order
        var prevPoint = center;
        var angleStep = angleSectorRad / (accuracy + 1);
        var v = ShapeVec.VecFromAngleRad(rotationRad - angleStep / 2) * radius;
        for (int i = 0; i < accuracy + 2; i++)
        {
            var p = center + v;
            result.Add(new Segment(prevPoint, p));
            prevPoint = p;
            v = v.Rotate(angleStep);
        }
            
        return result;
    }

    #endregion
    
}