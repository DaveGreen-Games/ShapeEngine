using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

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

public enum InputFilter
{
    None = 0,
    MouseOnly = 1,
    MouseNever = 2,
    All = 2
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
    public event Action<Vector2>? OnMouseEntered;
    public event Action<Vector2>? OnMouseExited;
    public event Action<ControlNode, bool>? OnSelectedChanged;
    public event Action<ControlNode, bool>? OnPressedChanged;
    // public event Action<ControlNode, bool>? OnFocusChanged;
    
    #endregion

    #region Private Members
    private ControlNode? parent = null;
    private readonly List<ControlNode> children = new();
    private bool active = true;
    private bool visible = true;
    private bool parentActive = true;
    private bool parentVisible = true;
    private bool selected = false;

    private bool navigationSelected = false;
    // private bool focused = false;
    #endregion

    #region Public Members
    
    public Vector2 Anchor = new(0f);
    /// <summary>
    /// Stretch determines the size of the rect based on the parent rect size. Values are relative and in range 0 - 1.
    /// If Stretch values are 0 than the size of the rect can be set manually without it being changed by the parent rect size.
    /// </summary>
    public Vector2 Stretch = new(1, 1);
    public Vector2 MinSize = new(0f);
    public Vector2 MaxSize = new(0f);
    
    public SelectFilter SelectionFilter = SelectFilter.None;
    public MouseFilter MouseFilter = MouseFilter.Ignore;
    public InputFilter InputFilter = InputFilter.None;

    #endregion

    #region Getters & Setters
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
    
    /// <summary>
    /// Set the rect only on root nodes.
    /// Otherwise it will have no effect except of changing the size if Stretch values are 0.
    /// </summary>
    public Rect Rect { get; set; } = new();
    public bool MouseInside { get; private set; } = false;
    public Vector2 MouseInsidePosition { get; private set; }
    public bool Selected
    {
        get => selected;
        private set
        {
            if (selected == value) return;
            selected = value;
            ResolveSelectedChanged();
        } 
    }
    // public bool Focused
    // {
    //     get => focused;
    //     private set
    //     {
    //         if (focused == value) return;
    //         focused = value;
    //         ResolveFocusChanged();
    //     } 
    // }
    public bool Pressed { get; private set; } = false;
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

    #region Select & Deselect

    public void Select()
    {
        if (SelectionFilter != SelectFilter.All) return;
        Selected = true;
    }

    public void Deselect()
    {
        if (SelectionFilter != SelectFilter.All) return;
        Selected = false;
        navigationSelected = false;
    }
    
    private void NavigationSelect()
    {
        if (SelectionFilter is SelectFilter.Mouse or SelectFilter.None) return;
        Selected = true;
        navigationSelected = true;
        
    }

    private void NavigationDeselect()
    {
        if (SelectionFilter is SelectFilter.Mouse or SelectFilter.None) return;
        Selected = false;
        navigationSelected = false;
    }
    
    private void MouseSelect()
    {
        if (SelectionFilter is SelectFilter.Navigation or SelectFilter.None) return;
        Selected = true;
    }

    private void MouseDeselect()
    {
        if (SelectionFilter is SelectFilter.Navigation or SelectFilter.None) return;
        if (navigationSelected) return;
        Selected = false;
    }

    
    

    #endregion
    
    #region Update & Draw
    public void UpdateRect(Rect sourceRect)
    {
        var p = sourceRect.GetPoint(Anchor);
        var size = Stretch.LengthSquared() == 0f ? Rect.Size : sourceRect.Size * Stretch;
        size = size.Clamp(MinSize, MaxSize);
        Rect = new(p, size, Anchor);
    }
    public void Update(float dt, Vector2 mousePos)
    {
        if (parent != null) return;
        InternalUpdate(dt, mousePos, true);
    }
    public void Draw()
    {
        if (parent != null) return;
        InternalDraw();
    }

    protected virtual void UpdateChildren(float dt, Vector2 mousePos, bool mousePosValid)
    {
        foreach (var child in children)
        {
            child.UpdateRect(Rect);
            child.InternalUpdate(dt, mousePos, mousePosValid);
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

    private void InternalUpdate(float dt, Vector2 mousePos, bool mousePosValid)
    {
        //if it is not visible it should also not be updated!
        if (!IsVisibleInHierarchy) return;

        if (IsActiveInHierarchy)
        {
            if (MouseFilter != MouseFilter.Ignore)
            {
                if (MouseFilter == MouseFilter.Stop) mousePosValid = false;
                
                bool isMouseInside = mousePosValid && Rect.ContainsPoint(mousePos);
            
                if (MouseInside != isMouseInside)
                {
                    MouseInside = isMouseInside;
                    if (isMouseInside)
                    {
                        ResolveMouseEntered(mousePos);
                        if (SelectionFilter is SelectFilter.All or SelectFilter.Mouse)
                        {
                            MouseSelect();
                        }
                    }
                    else
                    {
                        ResolveMouseExited(MouseInsidePosition);
                        if (SelectionFilter is SelectFilter.All or SelectFilter.Mouse)
                        {
                            MouseDeselect();
                        }
                    }
                }

                if (isMouseInside) MouseInsidePosition = mousePos;
            
            }

            if (InputFilter != InputFilter.None)
            {
                var pressed = false;
                if (InputFilter == InputFilter.MouseOnly) pressed = GetMousePressedState(); 
                else if (InputFilter == InputFilter.MouseNever) pressed = GetPressedState();
                else pressed = GetMousePressedState() || GetPressedState();

                if (Pressed != pressed)
                {
                    Pressed = pressed;
                    ResolvePressedChanged();
                }
            
            }
        }
        
        OnUpdate(dt, mousePos, mousePosValid);
        UpdateChildren(dt, mousePos, mousePosValid);
    }

    private void InternalDraw()
    {
        if (!IsVisibleInHierarchy) return;
        
        OnDraw();
        DrawChildren();
    }
    #endregion

    #region Input
    protected virtual bool GetPressedState() => false;
    protected virtual bool GetMousePressedState() => false;

    #endregion
    
    #region Virtual
    protected virtual void OnUpdate(float dt, Vector2 mousePos, bool mousePosValid) { }
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
    protected virtual void MouseHasEntered(Vector2 mousePos) { }
    protected virtual void MouseHasExited(Vector2 mousePos) { }
    protected virtual void SelectedWasChanged(bool value) { }
    protected virtual void PressedWasChanged(bool value) { }
    // protected virtual void FocusWasChanged(bool value) { }
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

    private void ResolveMouseEntered(Vector2 mousePos)
    {
        MouseHasEntered(mousePos);
        OnMouseEntered?.Invoke(mousePos);
    }

    private void ResolveMouseExited(Vector2 mousePos)
    {
        MouseHasExited(mousePos);
        OnMouseExited?.Invoke(mousePos);
    }

    private void ResolveSelectedChanged()
    {
        SelectedWasChanged(selected);
        OnSelectedChanged?.Invoke(this, selected);
    }

    private void ResolvePressedChanged()
    {
        PressedWasChanged(Pressed);
        OnPressedChanged?.Invoke(this, Pressed);
    }
    // private void ResolveFocusChanged()
    // {
    //     FocusWasChanged(focused);
    //     OnFocusChanged?.Invoke(this, focused);
    // }
    #endregion
    
    
    
    
    
}