using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Shapes;

public abstract class Shape : IShape
{
    public bool Moves = true;
    public bool Rotates = true;
    public bool Scales = true;
    
    public Transform2D Offset { get; set; }
    public Transform2D CurTransform { get; private set; }
    public Transform2D PrevTransform { get; private set; }

    
    protected Shape()
    {
        this.Offset = new(new Vector2(0f), 0f, new Size(0f), 1f);
    }
    protected Shape(Vector2 offset)
    {
        this.Offset = new(offset, 0f, new Size(0f), 1f);
    }
    protected Shape(Transform2D offset)
    {
        this.Offset = offset;
    }


    public virtual void InitializeShape(Transform2D parentTransform) { }
    public virtual void UpdateShape(float dt, Transform2D parentTransform)  { }
    public virtual void DrawShape()  { }
    public virtual void RecalculateShape() { }
    
    protected virtual void OnInitialized() { }
    protected virtual void OnUpdate(float dt) { }
    protected virtual void OnDraw() { }
    
    
    /// <summary>
    /// Called each frame after the transform was actualized from the parents
    /// </summary>
    /// <param name="transformChanged"></param>
    protected virtual void OnShapeTransformChanged(bool transformChanged) { }
    protected void UpdateTransform(Transform2D parentTransform)
    {
        PrevTransform = CurTransform;
        if ((!Moves && !Rotates && !Scales) || Offset.IsEmpty())
        {
            CurTransform = parentTransform;
        }
        else CurTransform = Transform2D.UpdateTransform(parentTransform, Offset, Moves, Rotates, Scales);

        OnShapeTransformChanged(PrevTransform != CurTransform);
    }

    
    public abstract ShapeType GetShapeType();
    public virtual Circle GetCircleShape() => new();
    public virtual Segment GetSegmentShape() => new();
    public virtual Triangle GetTriangleShape() => new();
    public virtual Quad GetQuadShape() => new();
    public virtual Rect GetRectShape() => new();
    public virtual Polygon GetPolygonShape() => new();
    public virtual Polyline GetPolylineShape() => new();

    
}


/*/// <summary>
/// Can be used to trigger recalculating the shape.
/// </summary>
public virtual void RecalculateShape()
{

}
public virtual void InitializeShape(Transform2D parentTransform)
{
    // check if it was initialized already
    UpdateTransform(parentTransform);
    OnTransformSetupFinished();
}
public virtual void UpdateShape(float dt, Transform2D parentTransform)
{
    UpdateTransform(parentTransform);
    Update(dt);
}

public virtual void DrawShape()
{
    Draw();
}

protected virtual void Update(float dt) { }
protected virtual void Draw() { }
protected virtual void OnTransformSetupFinished() { }
*/


/*
public interface ITransformHierarchyObject
{
    public Transform2D CurTransform { get; set; }
    
    /// <summary>
    /// The offset used for children only.
    /// </summary>
    public Transform2D Offset { get; set; }
    public List<ITransformHierarchyObject> GetChildren();
    
    public bool IsMoving() => true;
    public bool IsRotating() => true;
    public bool IsScaling() => true;


    /// <summary>
    /// Called on the parent to update all children.
    /// </summary>
    /// <param name="transform">The transform for this instance.</param>
    public sealed void UpdateHierarchy(Transform2D transform)
    {
        UpdateChildren(transform);
    }

    public void OnTransformUpdated(Transform2D prevTransform, Transform2D newTransform);
    private void UpdateChildren(Transform2D parentTransform)
    {
        var children = GetChildren();
        var prevTransform = CurTransform;
        
        CurTransform = Transform2D.UpdateTransform(parentTransform, Offset, IsMoving(), IsRotating(), IsScaling());
        
        OnTransformUpdated(prevTransform, CurTransform);
        
        foreach (var child in children)
        {
            child.UpdateChildren(CurTransform);
        }
    }
}
*/

