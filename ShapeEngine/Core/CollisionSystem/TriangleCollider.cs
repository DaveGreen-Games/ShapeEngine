using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.CollisionSystem;

public class TriangleCollider : Collider
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
    public TriangleCollider(Transform2D offset, List<Vector2> relativePoints) : base(offset)
    {
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
    public TriangleCollider(Transform2D offset, Vector2[] relativePoints) : base(offset)
    {
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
    public TriangleCollider(Transform2D offset, Vector2 relA, Vector2 relB, Vector2 relC) : base(offset)
    {
        ARelative = relA;
        BRelative = relB;
        CRelative = relC;
    }
    
    public override Rect GetBoundingBox() => GetTriangleShape().GetBoundingBox();
    public override ShapeType GetShapeType() => ShapeType.Triangle;
    public override Triangle GetTriangleShape() => new(AAbsolute, BAbsolute, CAbsolute);
}
