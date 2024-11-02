using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Shapes;

public abstract class ShapeContainer : Shape
{
    #region Members

    public ShapeContainer? Parent { get; set; } = null;
    private List<ShapeContainer> children { get; set; } = new();
    
    #endregion
    
    #region Public Functions
    
    public bool ChangeParent(ShapeContainer newParent)
    {
        if (newParent == Parent) return false;
        newParent.AddChild(this);
        return true;
    }
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
    public bool RemoveChild(ShapeContainer child)
    {
        if(child.Parent != this) return false;
        
        child.Parent = null;
        var removed = children.Remove(child);
        if(removed) child.OnRemovedFromParent(this);
        return removed;
    }
    
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

    protected virtual void OnChildInitialized(ShapeContainer child) { }
    protected virtual void OnChildUpdated(ShapeContainer child) { }
    protected virtual void OnChildDrawn(ShapeContainer child) { }
    
    protected virtual void OnUpdateFinished() { }
    protected virtual void OnDrawFinished() { }
    
    protected virtual void OnAddedToParent(ShapeContainer parent) { }
    protected virtual void OnRemovedFromParent(ShapeContainer parent) { }
    
    #endregion
}