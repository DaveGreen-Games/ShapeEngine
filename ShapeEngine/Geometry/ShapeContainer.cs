using ShapeEngine.Core.Structs;

namespace ShapeEngine.Geometry;

/// <summary>
/// Represents a container for shapes that can have a parent and multiple children,
/// supporting hierarchical transformations and updates.
/// </summary>
/// <remarks>
/// ShapeContainer enables composite shape structures, allowing for parent-child relationships and recursive updates/draws.
/// </remarks>
public abstract class ShapeContainer : Shape
{
    #region Members

    /// <summary>
    /// Gets or sets the parent of this shape container.
    /// </summary>
    public ShapeContainer? Parent { get; set; }
    
    // List of child shape containers. Not exposed publicly.
    private List<ShapeContainer> children { get; set; } = [];
    public List<ShapeContainer> GetChildrenCopy() => [..children];
    #endregion
    
    #region Public Functions
    
    /// <summary>
    /// Changes the parent of this shape container to a new parent.
    /// </summary>
    /// <param name="newParent">The new parent to assign.</param>
    /// <returns>True if the parent was changed; false if already assigned to the new parent.</returns>
    /// <remarks>
    /// This will add the current shape to the new parent's children and update the parent reference.
    /// </remarks>
    public bool ChangeParent(ShapeContainer newParent)
    {
        if (newParent == Parent) return false;
        newParent.AddChild(this);
        return true;
    }
    
    /// <summary>
    /// Adds a child shape container to this container.
    /// </summary>
    /// <param name="child">The child shape container to add.</param>
    /// <returns>True if the child was added; false if it was already a child.</returns>
      /// <remarks>
      /// <list type="bullet">
      ///   <item><description>If the child already has a parent, it will be removed from that parent first.</description></item>
      ///   <item><description>The child's parent will be set to this container.</description></item>
      ///   <item><description>The child will be initialized with the current transform.</description></item>
      /// </list>
      /// </remarks>
    public bool AddChild(ShapeContainer child)
    {
        if (child.Parent == this) return false;

        if (child.Parent != null) //if child already has a parent
        {
            //remove child from that parent
            child.Parent.RemoveChild(child);
        }
        
        //set new parent
        child.Parent = this;
        
        children.Add(child);
        child.InitializeShape(CurTransform);
        child.OnAddedToParent(this);
        return true;
    }
    
    /// <summary>
    /// Removes a child shape container from this container.
    /// </summary>
    /// <param name="child">The child shape container to remove.</param>
    /// <returns>True if the child was removed; false if it was not a child of this container.</returns>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description>The child's parent will be set to <c>null</c>.</description></item>
    ///   <item><description>The child will be notified of removal.</description></item>
    /// </list>
    /// </remarks>
    public bool RemoveChild(ShapeContainer child)
    {
        if(child.Parent != this) return false;
        
        child.Parent = null;
        var removed = children.Remove(child);
        if(removed) child.OnRemovedFromParent(this);
        return removed;
    }
    
    /// <summary>
    /// Initializes this shape and all child shapes with the given parent transform.
    /// </summary>
    /// <param name="parentTransform">The transform of the parent shape.</param>
    /// <remarks>
    /// Recursively initializes all children and calls <see cref="OnChildInitialized"/> for each child.
    /// </remarks>
    public override void InitializeShape(Transform2D parentTransform)
    {
        UpdateTransform(parentTransform);
        foreach (var child in children)
        {
            child.InitializeShape(parentTransform);
            OnChildInitialized(child);
        }
        OnInitialized();
    }
    
    /// <summary>
    /// Updates this shape and all child shapes.
    /// </summary>
    /// <param name="dt">The delta time since the last update.</param>
    /// <param name="parentTransform">The transform of the parent shape.</param>
    /// <remarks>
    /// Recursively updates all children and calls <see cref="OnChildUpdated"/> for each child.
    /// </remarks>
    public override void UpdateShape(float dt, Transform2D parentTransform)
    {
        UpdateTransform(parentTransform);
        OnUpdate(dt);
        var trans = CurTransform;
        foreach (var child in children)
        {
            child.UpdateShape(dt, trans);
            OnChildUpdated(child);
        }
        OnUpdateFinished();
    }
    
    /// <summary>
    /// Draws this shape and all child shapes.
    /// </summary>
    /// <remarks>
    /// Recursively draws all children and calls <see cref="OnChildDrawn"/> for each child.
    /// </remarks>
    public override void DrawShape()
    {
        OnDraw();
        foreach (var child in children)
        {
            child.DrawShape();
            OnChildDrawn(child);
        }
        OnDrawFinished();
    }
    
    #endregion

    #region Protected Functions

    /// <summary>
    /// Called when a child shape has been initialized.
    /// </summary>
    /// <param name="child">The child that was initialized.</param>
    protected virtual void OnChildInitialized(ShapeContainer child) { }
    /// <summary>
    /// Called when a child shape has been updated.
    /// </summary>
    /// <param name="child">The child that was updated.</param>
    protected virtual void OnChildUpdated(ShapeContainer child) { }
    /// <summary>
    /// Called when a child shape has been drawn.
    /// </summary>
    /// <param name="child">The child that was drawn.</param>
    protected virtual void OnChildDrawn(ShapeContainer child) { }
    
    /// <summary>
    /// Called after all updates for this shape and its children are finished.
    /// </summary>
    protected virtual void OnUpdateFinished() { }
    /// <summary>
    /// Called after all drawing for this shape and its children is finished.
    /// </summary>
    protected virtual void OnDrawFinished() { }
    
    /// <summary>
    /// Called when this shape is added to a parent container.
    /// </summary>
    /// <param name="parent">The parent container this shape was added to.</param>
    protected virtual void OnAddedToParent(ShapeContainer parent) { }
    /// <summary>
    /// Called when this shape is removed from a parent container.
    /// </summary>
    /// <param name="parent">The parent container this shape was removed from.</param>
    protected virtual void OnRemovedFromParent(ShapeContainer parent) { }
    
    #endregion
}