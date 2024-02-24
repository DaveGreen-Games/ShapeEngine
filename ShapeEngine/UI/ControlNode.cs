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
    #endregion

    #region Private Members

    private ControlNode? parent = null;
    private readonly List<ControlNode> children = new();
    
    #endregion

    #region Public Members

    //how to do visible & active???
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
        return true;
    }
    public bool RemoveChild(ControlNode child)
    {
        if (child.Parent == null || child.Parent != this) return false;
        if (!children.Remove(child)) return false;
        
        ResolveChildRemoved(child);
        child.Parent = null;
        return true;

    }
    private void RemoveChild(int index)
    {
        var child = children[index];
        children.RemoveAt(index);
        ResolveChildRemoved(child);
        child.Parent = null;
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


    #endregion

   
    public void UpdateRect(Rect sourceRect)
    {
        var p = sourceRect.GetPoint(Anchor);
        Rect = new(p, sourceRect.Size * Stretch, Anchor);
    }
    public void Update(float dt, Vector2 mousePos)
    {
        //check stuff with mouse filter and select filter
        //if mouse filter is not ignore check if mouse entered rect
        //if mouse filter is not stop pass mouse pos to children
        //if select filter is mouse or all also trigger select
        OnUpdate(dt, mousePos);
        UpdateChildren(dt, mousePos);
    }

    public void Draw()
    {
        OnDraw();
        foreach (var child in children)
        {
            child.Draw();
            OnChildDrawn(child);
        }
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
    protected virtual void OnUpdate(float dt, Vector2 mousePos) { }
    protected virtual void OnChildUpdated(ControlNode child) { }
    protected virtual void OnDraw() { } 
    protected virtual void OnChildDrawn(ControlNode child) { }

    #region Virtual
    protected virtual void ParentWasChanged(ControlNode? oldParent, ControlNode? newParent) { }
    protected virtual void ChildWasAdded(ControlNode newChild) { }
    protected virtual void ChildWasRemoved(ControlNode oldChild) { }
    #endregion

    #region Private
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