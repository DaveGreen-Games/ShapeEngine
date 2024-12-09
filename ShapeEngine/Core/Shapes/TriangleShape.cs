using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes;

public class TriangleShape : ShapeContainer
{
    public Vector2 ARelative { get; set; }
    public Vector2 BRelative { get; set; }
    public Vector2 CRelative { get; set; }
    
    public Vector2 AAbsolute => CurTransform.Position + (ARelative * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    public Vector2 BAbsolute => CurTransform.Position + (BRelative * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    public Vector2 CAbsolute => CurTransform.Position + (CRelative * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);

    /// <summary>
    /// Relative points should be in range -1 to 1 on x & y axis!
    /// </summary>
    public TriangleShape(Transform2D offset, List<Vector2> relativePoints)
    {
        Offset = offset;
        
        if (relativePoints.Count <= 0)
        {
            ARelative = new();
            BRelative = new();
            CRelative = new();
        }
        else if (relativePoints.Count == 1)
        {
            ARelative = relativePoints[0];
            BRelative = new();
            CRelative = new();
        }
        else if (relativePoints.Count == 2)
        {
            ARelative = relativePoints[0];
            BRelative = relativePoints[1];
            CRelative = new();
        }
        else
        {
            ARelative = relativePoints[0];
            BRelative = relativePoints[1];
            CRelative = relativePoints[2];
        }
        
    }
    
    /// <summary>
    /// Relative points should be in range -1 to 1 on x & y axis!
    /// </summary>
    public TriangleShape(Transform2D offset, Vector2[] relativePoints)
    {
        Offset = offset;
        if (relativePoints.Length <= 0)
        {
            ARelative = new();
            BRelative = new();
            CRelative = new();
        }
        else if (relativePoints.Length == 1)
        {
            ARelative = relativePoints[0];
            BRelative = new();
            CRelative = new();
        }
        else if (relativePoints.Length == 2)
        {
            ARelative = relativePoints[0];
            BRelative = relativePoints[1];
            CRelative = new();
        }
        else
        {
            ARelative = relativePoints[0];
            BRelative = relativePoints[1];
            CRelative = relativePoints[2];
        }
    }
    
    /// <summary>
    /// Relative points should be in range -1 to 1 on x & y axis!
    /// </summary>
    public TriangleShape(Transform2D offset, Vector2 relA, Vector2 relB, Vector2 relC)
    {
        Offset = offset;
        ARelative = relA;
        BRelative = relB;
        CRelative = relC;
    }

    public override ShapeType GetShapeType() => ShapeType.Triangle;
    public override Triangle GetTriangleShape() => new(AAbsolute, BAbsolute, CAbsolute);
}