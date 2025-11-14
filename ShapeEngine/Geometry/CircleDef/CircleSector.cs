using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;

/// <summary>
/// Represents a sector (pie slice) of a circle, defined by a center, radius, rotation, and sector angle.
/// </summary>
/// <remarks>
/// The <see cref="CircleSector"/> class provides methods to generate polygons, points, and segments representing the sector.
/// The arc of the sector can be approximated with a configurable number of points.
/// </remarks>
public class CircleSector
{
    #region Public Members
    /// <summary>
    /// Gets or sets the transform of the sector, which includes position (center), rotation, and scaled size (radius).
    /// </summary>
    public Transform2D Transform { get; set; }
    
    /// <summary>
    /// Gets or sets the number of points used to approximate the arc of the sector.
    /// 0 creates a triangle where the arc is a straight line.
    /// </summary>
    public int ArcPoints { get; set; }
    
    /// <summary>
    /// Gets or sets the width of the sector in radians.
    /// </summary>
    public float AngleSectorRad { get; set; }

    /// <summary>
    /// Gets or sets the center of the sector in world coordinates by changing <see cref="Transform"/>.Position.
    /// </summary>
    public Vector2 Center
    {
        get => Transform.Position;
        set => Transform = Transform.SetPosition(value);
    }

    /// <summary>
    /// Gets or sets the radius of the sector in world units by changing <see cref="Transform"/>.BaseSize.
    /// </summary>
    public float Radius
    {
        get => Transform.ScaledSize.Radius;
        set => Transform = Transform.SetSize(value);
    }

    /// <summary>
    /// Gets or sets the rotation of the sector in radians by changing <see cref="Transform"/>.RotationRad.
    /// </summary>
    public float RotationRad
    {
        get => Transform.RotationRad;
        set => Transform = Transform.SetRotationRad(value);
    }
    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CircleSector"/> class.
    /// </summary>
    /// <param name="center">The center position of the sector.</param>
    /// <param name="radius">The radius of the sector.</param>
    /// <param name="rotationRad">The rotation of the sector in radians.</param>
    /// <param name="angleSectorRad">The angle of the sector in radians.</param>
    /// <param name="arcPoints">The number of points used to approximate the arc. Default is 3.</param>
    public CircleSector(Vector2 center, float radius, float rotationRad, float angleSectorRad, int arcPoints = 3)
    {
        Transform = new Transform2D(center, rotationRad, new Size(radius));
        ArcPoints = arcPoints;
        AngleSectorRad = angleSectorRad;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="CircleSector"/> class from a transform.
    /// </summary>
    /// <param name="transform">The transform containing position, rotation, and size.</param>
    /// <param name="angleSectorRad">The angle of the sector in radians.</param>
    /// <param name="arcPoints">The number of points used to approximate the arc. Default is 3.</param>
    public CircleSector(Transform2D transform, float angleSectorRad, int arcPoints = 3)
    {
        Transform = transform;
        ArcPoints = arcPoints;
        AngleSectorRad = angleSectorRad;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="CircleSector"/> class from a center, radius, direction, and sector angle.
    /// </summary>
    /// <param name="center">The center position of the sector.</param>
    /// <param name="radius">The radius of the sector.</param>
    /// <param name="direction">The direction vector for the sector's rotation.</param>
    /// <param name="angleSectorRad">The angle of the sector in radians.</param>
    /// <param name="arcPoints">The number of points used to approximate the arc. Default is 3.</param>
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
    /// Clears the given polygon and fills it with new points representing the sector.
    /// </summary>
    /// <param name="polygon">The polygon to update.</param>
    /// <returns>Returns true if new points were generated; otherwise, false.</returns>
    /// <remarks>
    /// The polygon is filled in counter-clockwise order, starting from the center.
    /// </remarks>
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
    
    /// <summary>
    /// Creates a copy of this <see cref="CircleSector"/>.
    /// </summary>
    /// <returns>A new <see cref="CircleSector"/> with the same parameters.</returns>
    public CircleSector Copy() => new(Center, Radius, RotationRad, AngleSectorRad, ArcPoints);

    /// <summary>
    /// Generates a polygon representing the sector.
    /// </summary>
    /// <returns>A <see cref="Polygon"/> representing the sector, or null if parameters are invalid.</returns>
    public Polygon? GeneratePolygon() => GeneratePolygon(Center, Radius, RotationRad, AngleSectorRad, ArcPoints);
    /// <summary>
    /// Generates a set of points representing the sector.
    /// </summary>
    /// <returns>A <see cref="Points"/> collection representing the sector, or null if parameters are invalid.</returns>
    public Points? GeneratePoints() => GeneratePoints(Center, Radius, RotationRad, AngleSectorRad, ArcPoints);
    /// <summary>
    /// Generates a set of segments representing the sector's edges.
    /// </summary>
    /// <returns>A <see cref="Segments"/> collection representing the sector, or null if parameters are invalid.</returns>
    public Segments? GenerateSegments() => GenerateSegments(Center, Radius, RotationRad, AngleSectorRad, ArcPoints);

    #endregion

    #region Static

    /// <summary>
    /// Generates a polygon representing a circle sector.
    /// </summary>
    /// <param name="center">The center of the sector.</param>
    /// <param name="radius">The radius of the sector.</param>
    /// <param name="rotationRad">The rotation of the sector in radians.</param>
    /// <param name="angleSectorRad">The angle of the sector in radians.</param>
    /// <param name="accuracy">The number of points used to approximate the arc. Default is 3.</param>
    /// <returns>A <see cref="Polygon"/> representing the sector, or null if parameters are invalid.</returns>
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
    /// <summary>
    /// Generates a set of points representing a circle sector.
    /// </summary>
    /// <param name="center">The center of the sector.</param>
    /// <param name="radius">The radius of the sector.</param>
    /// <param name="rotationRad">The rotation of the sector in radians.</param>
    /// <param name="angleSectorRad">The angle of the sector in radians.</param>
    /// <param name="accuracy">The number of points used to approximate the arc. Default is 3.</param>
    /// <returns>A <see cref="Points"/> collection representing the sector, or null if parameters are invalid.</returns>
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
    /// <summary>
    /// Generates a set of segments representing the edges of a circle sector.
    /// </summary>
    /// <param name="center">The center of the sector.</param>
    /// <param name="radius">The radius of the sector.</param>
    /// <param name="rotationRad">The rotation of the sector in radians.</param>
    /// <param name="angleSectorRad">The angle of the sector in radians.</param>
    /// <param name="accuracy">The number of points used to approximate the arc. Default is 3.</param>
    /// <returns>A <see cref="Segments"/> collection representing the sector, or null if parameters are invalid.</returns>
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