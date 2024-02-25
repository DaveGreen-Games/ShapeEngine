using System.Numerics;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.UI;


/// <summary>
/// How can a control node be selected.
/// </summary>
public enum SelectFilter
{
    None = 0,
    Mouse = 1,
    Navigation = 2,
    All = 3
}

/// <summary>
/// How a control node handles mouse interaction.
/// </summary>
public enum MouseFilter
{
    Ignore = 0,
    Pass = 1,
    Stop = 2
}
public abstract class ControlNode
{
    #region Events

    
    public event Action<ControlNode?, ControlNode?>? OnParentChanged;
    public event Action<ControlNode>? OnChildAdded;
    public event Action<ControlNode>? OnChildRemoved;
    public event Action<bool>? OnVisibleChanged;
    public event Action<bool>? OnActiveChanged;
    public event Action<bool>? OnParentActiveChanged;
    public event Action<bool>? OnParentVisibleChanged;
    
    #endregion

    #region Private Members
    private ControlNode? parent = null;
    private readonly List<ControlNode> children = new();
    private bool active = true;
    private bool visible = true;
    private bool parentActive = true;
    private bool parentVisible = true;
    #endregion

    #region Public Members
    public bool Active
    {
        get => active;
        set
        {
            if (active == value) return;
            
            active = value;
            ResolveActiveChanged();
            if(parent == null || (parentActive && active != parentActive))
                ChangeChildrenActive(active);
        }
        
    }
    public bool Visible
    {
        get => visible;
        set
        {
            if (visible == value) return;
            
            visible = value;
            ResolveVisibleChanged();
            if(parent == null || (parentVisible && visible != parentVisible))
                ChangeChildrenVisible(visible);
        } 
    }

    /// <summary>
    /// Is this instance visible and are its parents visible to the root node?
    /// </summary>
    public bool IsVisibleInHierarchy => visible && parentVisible;
    /// <summary>
    /// Is this instance active and are its parents active to the root node?
    /// </summary>
    public bool IsActiveInHierarchy => active && parentActive;
    
    public Rect Rect { get; set; } = new();
    public Vector2 Anchor = new();
    public Vector2 Stretch = new(1, 1);
    public SelectFilter SelectionFilter = SelectFilter.None;
    public MouseFilter MouseFilter = MouseFilter.Ignore;

    #endregion

    #region Getters & Setters

    public ControlNode? Parent
    {
        get => parent;
        private set
        {
            if (parent == value) return;
            var oldParent = parent;
            parent = value;
            ResolveParentChanged(oldParent, parent);
        }
        
    }
    public int ChildCount => children.Count;
    public bool HasParent => parent != null;
    public bool HasChildren => children.Count > 0;

    #endregion

    #region Children

    public bool AddChild(ControlNode child)
    {
        if (child.Parent != null)
        {
            if (child.Parent == this) return false;
            child.Parent.RemoveChild(child);
        }
        
        children.Add(child);
        ResolveChildAdded(child);
        child.Parent = this;

        
        //a child can only be added if it has no parent -> if it has a parent, RemoveChild is called first
        //therefore a child that is added always has:
            //parentActive & parentVisible set to true (because it has no parent)
            //all the children are set to the active & visible value instead of parentActive & parentVisible value (again because it has no parent)
        bool checkActive = parent == null ? active : parentActive;
        bool checkVisible = parent == null ? visible : parentVisible;
        
        if (!checkActive)
        {
            child.parentActive = checkActive;
            child.ResolveParentActiveChanged();
            if(child.active) child.ChangeChildrenActive(checkActive);
            
        }
        
        if (!checkVisible)
        {
            child.parentVisible = checkVisible;
            child.ResolveParentVisibleChanged();
            if(child.visible) child.ChangeChildrenVisible(checkVisible);
            
        }
        
        return true;
    }
    public bool RemoveChild(ControlNode child)
    {
        if (child.Parent == null || child.Parent != this) return false;
        if (!children.Remove(child)) return false;
        
        ResolveChildRemoved(child);
        child.Parent = null;

        if (child.parentActive == false)
        {
            child.parentActive = true;
            ResolveParentActiveChanged();

            if (active) ChangeChildrenActive(active);
        }

        if (child.parentVisible == false)
        {
            child.parentVisible = true;
            ResolveParentVisibleChanged();
            
            if(visible) ChangeChildrenVisible(visible);
        }
        
        return true;

    }
    private void RemoveChild(int index)
    {
        var child = children[index];
        children.RemoveAt(index);
        ResolveChildRemoved(child);
        child.Parent = null;
        child.parentActive = true; //default value if parent is null
        child.parentVisible = true; //default value if parent is null
    }
    
    public void ClearChildren()
    {
        for (int i = children.Count - 1; i >= 0; i--)
        {
            RemoveChild(i);
        }
    }

    public ControlNode? GetChild(int index) => children.Count <= index ? null : children[index];
    public List<ControlNode>? GetChildrenCopy() => children.ToList();
    public List<ControlNode>? GetChildren(Predicate<ControlNode> match) => children.FindAll(match);

    private void ChangeChildrenVisible(bool value)
    {
        foreach (var child in children)
        {
            child.parentVisible = value;
            child.ResolveParentVisibleChanged();
            if(value || child.visible != value)
                child.ChangeChildrenVisible(value);
        }
    }
    private void ChangeChildrenActive(bool value)
    {
        foreach (var child in children)
        {
            child.parentActive = value;
            child.ResolveParentActiveChanged();
            if(value || child.active != value)
                child.ChangeChildrenActive(value);
        }
    }
    
    #endregion

    #region Update & Draw
    public void UpdateRect(Rect sourceRect)
    {
        var p = sourceRect.GetPoint(Anchor);
        Rect = new(p, sourceRect.Size * Stretch, Anchor);
    }
    public void Update(float dt, Vector2 mousePos)
    {
        if (!IsActiveInHierarchy) return;
        
        //check stuff with mouse filter and select filter
        //if mouse filter is not ignore check if mouse entered rect
        //if mouse filter is not stop pass mouse pos to children
        //if select filter is mouse or all also trigger select
        OnUpdate(dt, mousePos);
        UpdateChildren(dt, mousePos);
    }

    public void Draw()
    {
        if (!IsVisibleInHierarchy) return;
        
        OnDraw();
        DrawChildren();
    }

    protected virtual void UpdateChildren(float dt, Vector2 mousePos)
    {
        foreach (var child in children)
        {
            child.UpdateRect(Rect);
            child.Update(dt, mousePos);
            OnChildUpdated(child);
        }
    }

    protected virtual void DrawChildren()
    {
        foreach (var child in children)
        {
            child.Draw();
            OnChildDrawn(child);
        }
    }
    #endregion

    #region Virtual
    protected virtual void OnUpdate(float dt, Vector2 mousePos) { }
    protected virtual void OnChildUpdated(ControlNode child) { }
    protected virtual void OnDraw() { } 
    protected virtual void OnChildDrawn(ControlNode child) { }
    protected virtual void ActiveWasChanged(bool value) { }
    protected virtual void VisibleWasChanged(bool value) { }
    protected virtual void ParentActiveWasChanged(bool value) { }
    protected virtual void ParentVisibleWasChanged(bool value) { }
    protected virtual void ParentWasChanged(ControlNode? oldParent, ControlNode? newParent) { }
    protected virtual void ChildWasAdded(ControlNode newChild) { }
    protected virtual void ChildWasRemoved(ControlNode oldChild) { }
    #endregion

    #region Private

    private void ResolveActiveChanged()
    {
        ActiveWasChanged(active);
        OnActiveChanged?.Invoke(active);
    }

    private void ResolveVisibleChanged()
    {
        VisibleWasChanged(visible);
        OnVisibleChanged?.Invoke(visible);
    }

    private void ResolveParentVisibleChanged()
    {
        ParentVisibleWasChanged(parentVisible);
        OnParentVisibleChanged?.Invoke(parentVisible);
    }

    private void ResolveParentActiveChanged()
    {
        ParentActiveWasChanged(parentActive);
        OnParentActiveChanged?.Invoke(parentActive);
    }
    private void ResolveParentChanged(ControlNode? oldParent, ControlNode? newParent)
    {
        ParentWasChanged(oldParent, newParent);
        OnParentChanged?.Invoke(oldParent, newParent);
    }
    private void ResolveChildAdded(ControlNode newChild)
    {
        ChildWasAdded(newChild);
        OnChildAdded?.Invoke(newChild);
    }
    private void ResolveChildRemoved(ControlNode oldChild)
    {
        ChildWasRemoved(oldChild);
        OnChildRemoved?.Invoke(oldChild);
    }
    #endregion
    
    
    
    
    
}