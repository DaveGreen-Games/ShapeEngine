using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes;


public class CircleSector
{
    #region Public Members
    public Transform2D Transform { get; set; }
    
    /// <summary>
    /// How many points are used for the circle sector arc. 0 creates a triangle where the arc is a straight line.
    /// </summary>
    public int ArcPoints { get; set; }
    
    /// <summary>
    /// How wide the circle sector is. 
    /// </summary>
    public float AngleSectorRad { get; set; }

    /// <summary>
    /// Represents Transform.Position
    /// </summary>
    public Vector2 Center
    {
        get => Transform.Position;
        set => Transform = Transform.SetPosition(value);
    }

    /// <summary>
    /// Represents Transform.ScaledSize.Radius
    /// </summary>
    public float Radius
    {
        get => Transform.ScaledSize.Radius;
        set => Transform = Transform.SetSize(value);
    }

    /// <summary>
    /// Represents Transform.RotationRad
    /// </summary>
    public float RotationRad
    {
        get => Transform.RotationRad;
        set => Transform = Transform.SetRotationRad(value);
    }
    #endregion

    #region Constructor

    public CircleSector(Vector2 center, float radius, float rotationRad, float angleSectorRad, int arcPoints = 3)
    {
        Transform = new Transform2D(center, rotationRad, new Size(radius));
        ArcPoints = arcPoints;
        AngleSectorRad = angleSectorRad;
    }
    public CircleSector(Transform2D transform, float angleSectorRad, int arcPoints = 3)
    {
        Transform = transform;
        ArcPoints = arcPoints;
        AngleSectorRad = angleSectorRad;
    }
    public CircleSector(Vector2 center, float radius, Vector2 direction, float angleSectorRad, int arcPoints = 3)
    {
        var rotationRad = 0f;
        if (direction != Vector2.Zero) rotationRad = direction.AngleRad();
        Transform = new Transform2D(center, rotationRad, new Size(radius));
        ArcPoints = arcPoints;
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
        var angleStep = AngleSectorRad / (ArcPoints + 1);
        var v = ShapeVec.VecFromAngleRad(RotationRad - angleStep / 2) * Radius;
        for (int i = 0; i < ArcPoints + 2; i++)
        {
            polygon.Add(Center + v);
            v = v.Rotate(angleStep);
        }

        return true;
    }
    
    public CircleSector Copy() => new(Center, Radius, RotationRad, AngleSectorRad, ArcPoints);

    public Polygon? GeneratePolygon() => GeneratePolygon(Center, Radius, RotationRad, AngleSectorRad, ArcPoints);
    public Points? GeneratePoints() => GeneratePoints(Center, Radius, RotationRad, AngleSectorRad, ArcPoints);
    public Segments? GenerateSegments() => GenerateSegments(Center, Radius, RotationRad, AngleSectorRad, ArcPoints);

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