using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Interfaces;

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