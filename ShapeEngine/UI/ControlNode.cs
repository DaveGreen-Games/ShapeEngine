using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.UI;

public abstract class ControlNode
{
    #region Events

    public event Action<ControlNode, Direction>? OnNavigated; 
    /// <summary>
    /// Parameters: Invoker, Old Parent, New Parent
    /// </summary>
    public event Action<ControlNode, ControlNode?, ControlNode?>? OnParentChanged;
    /// <summary>
    /// Parameters: Invoker, New Child
    /// </summary>
    public event Action<ControlNode, ControlNode>? OnChildAdded;
    /// <summary>
    /// Parameters: Invoker, Old Child
    /// </summary>
    public event Action<ControlNode, ControlNode>? OnChildRemoved;

    public event Action<ControlNode, bool>? OnDisplayedChanged;
    public event Action<ControlNode, bool>? OnActiveInHierarchyChanged;
    public event Action<ControlNode, bool>? OnVisibleInHierarchyChanged;
    /// <summary>
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode,bool>? OnVisibleChanged;
    /// <summary>
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode,bool>? OnActiveChanged;
    /// <summary>
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode,bool>? OnParentActiveChanged;
    /// <summary>
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode,bool>? OnParentVisibleChanged;
    
    //changed in hierarchy events
    
    /// <summary>
    /// Parameters: Invoker, Mouse Pos
    /// </summary>
    public event Action<ControlNode,Vector2>? OnMouseEntered;
    /// <summary>
    /// Parameters: Invoker, Last Mouse Pos Inside
    /// </summary>
    public event Action<ControlNode,Vector2>? OnMouseExited;
    /// <summary>
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode, bool>? OnSelectedChanged;
    /// <summary>
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode, bool>? OnPressedChanged;
    /// <summary>
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode, bool>? OnNavigableChanged;
    
    /// <summary>
    /// Parameters: Invoker, Old Filter, New Filter
    /// </summary>
    public event Action<ControlNode, MouseFilter, MouseFilter>? OnMouseFilterChanged;
    /// <summary>
    /// Parameters: Invoker, Old Filter, New Filter
    /// </summary>
    public event Action<ControlNode, SelectFilter, SelectFilter>? OnSelectionFilterChanged;
    /// <summary>
    /// Parameters: Invoker, Old Filter, New Filter
    /// </summary>
    public event Action<ControlNode, InputFilter, InputFilter>? OnInputFilterChanged;
    
    #endregion

    #region Private Members
    private ControlNode? parent = null;
    private readonly List<ControlNode> children = new();
    private SelectFilter selectionFilter = SelectFilter.None;
    private MouseFilter mouseFilter = MouseFilter.Ignore;
    private InputFilter inputFilter = InputFilter.None;
    private bool active = true;
    private bool visible = true;
    private bool parentActive = true;
    private bool parentVisible = true;
    private bool selected = false;
    
    private bool displayed = true;

    // public void SetDisplayed(ControlNode changer, bool value)
    // {
    //     if (changer is not ControlNodeContainer) return;
    //     displayed = value;
    // }

    public bool Displayed
    {
        get => displayed;
        set
        {
            if (value == displayed) return;
            prevNavigable = Navigable;
            prevIsVisibleInHierarchy = IsVisibleInHierarchy;
            displayed = value;
            ResolveOnDisplayedChanged();
            foreach (var child in children)
            {
                child.Displayed = value;
            }
        }
    }
    
    
    private bool navigationSelected = false;

    private bool prevNavigable = false;
    private bool prevIsVisibleInHierarchy = false;
    private bool prevIsActiveInHierarchy = false;
    // private bool focused = false;
    #endregion

    #region Public Members
    
    public Vector2 Anchor = new(0f);
    /// <summary>
    /// Stretch determines the size of the rect based on the parent rect size. Values are relative and in range 0 - 1.
    /// If Stretch values are 0 than the size of the rect can be set manually without it being changed by the parent rect size.
    /// </summary>
    public Vector2 Stretch = new(1, 1);

    public float ContainerStretch = 1f;
    
    public Size MinSize = new(0f);
    public Size MaxSize = new(0f);
    public Rect.Margins Margins = new();
    
    public SelectFilter SelectionFilter
    {
        get => selectionFilter;
        set
        {
            if (selectionFilter == value) return;
            prevNavigable = Navigable;
            var prev = selectionFilter;
            selectionFilter = value;
            ResolveOnSelectionFilterChanged(prev, selectionFilter);
        }
    }
    public MouseFilter MouseFilter
    {
        get => mouseFilter;
        set
        {
            if (mouseFilter == value) return;
            var prev = mouseFilter;
            mouseFilter = value;
            ResolveOnMouseFilterChanged(prev, mouseFilter);
        }
    }
    public InputFilter InputFilter
    {
        get => inputFilter;
        set
        {
            if (inputFilter == value) return;
            prevNavigable = Navigable;
            var prev = inputFilter;
            inputFilter = value;
            ResolveOnInputFilterChanged(prev, inputFilter);
            
        }
    }

    protected List<ControlNode>? DisplayedChildren = null;
    #endregion

    #region Getters & Setters
    public bool Active
    {
        get => active;
        set
        {
            if (active == value) return;
            prevNavigable = Navigable;
            prevIsActiveInHierarchy = IsActiveInHierarchy;
            active = value;
            ResolveActiveChanged();
            if(parent == null || parentActive)
                ChangeChildrenActive(active);
        }
        //parent        active      inactive    active      inactive
        //change to     active      inactive    inactive    active
        //              yes         no          yes         no
    }
    public bool Visible
    {
        get => visible;
        set
        {
            if (visible == value) return;
            prevNavigable = Navigable;
            prevIsVisibleInHierarchy = IsVisibleInHierarchy;
            visible = value;
            ResolveVisibleChanged();
            if(parent == null || parentVisible)
                ChangeChildrenVisible(visible);
            
        } 
    }

    public bool ParentVisible => parentVisible;
    public bool ParentActive => parentActive;
    /// <summary>
    /// Is this instance visible and are its parents visible to the root node?
    /// </summary>
    public bool IsVisibleInHierarchy => visible && parentVisible && displayed;
    /// <summary>
    /// Is this instance active and are its parents active to the root node?
    /// </summary>
    public bool IsActiveInHierarchy => active && parentActive;

    /// <summary>
    /// Set the rect only on root nodes.
    /// Otherwise it will have no effect except of changing the size if Stretch values are 0.
    /// </summary>
    public Rect Rect { get; private set; } = new();
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
    public int DisplayedChildrenCount => DisplayedChildren?.Count ?? 0;
    public bool HasDisplayedChildren => DisplayedChildrenCount > 0;
    public IEnumerable<ControlNode> GetChildrenEnumerable => children;
    // public List<ControlNode> GetChildren(Predicate<ControlNode> match) => children.FindAll(match);
    public bool Navigable => 
        IsActiveInHierarchy && IsVisibleInHierarchy &&
        SelectionFilter is SelectFilter.All or SelectFilter.Navigation && 
        InputFilter is InputFilter.All or InputFilter.MouseNever;

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
            child.prevIsActiveInHierarchy = child.IsActiveInHierarchy;
            child.prevNavigable = child.Navigable;
            child.parentActive = checkActive;
            child.ResolveParentActiveChanged();
            if(child.active != checkActive) child.ChangeChildrenActive(checkActive);
            
        }
        
        if (!checkVisible)
        {
            child.prevIsVisibleInHierarchy = child.IsVisibleInHierarchy;
            child.prevNavigable = child.Navigable;
            child.parentVisible = checkVisible;
            child.ResolveParentVisibleChanged();
            if(child.visible != checkVisible) child.ChangeChildrenVisible(checkVisible);
            
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
            child.prevNavigable = child.Navigable;
            child.prevIsActiveInHierarchy = child.IsActiveInHierarchy;
            child.parentActive = true;
            child.ResolveParentActiveChanged();

            if (child.active) child.ChangeChildrenActive(child.active);
        }

        if (child.parentVisible == false)
        {
            child.prevNavigable = child.Navigable;
            child.prevIsVisibleInHierarchy = child.IsVisibleInHierarchy;
            child.parentVisible = true;
            child.ResolveParentVisibleChanged();
            
            if(child.visible) child.ChangeChildrenVisible(child.visible);
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
    public int GetAllChildren(ref HashSet<ControlNode> result)
    {
        if (children.Count <= 0) return 0;
        
        var count = result.Count;
        foreach (var child in children)
        {
            result.Add(child);
            child.GetAllChildren(ref result);
        }

        return result.Count - count;
    }
    public int GetAllChildren(Predicate<ControlNode> match, ref HashSet<ControlNode> result)
    {
        if (children.Count <= 0) return 0;
        
        var count = result.Count;
        var matched = children.FindAll(match);
        foreach (var child in matched)
        {
            result.Add(child);
            child.GetAllChildren(match, ref result);
        }

        return result.Count - count;
    }
    public int GetAllNavigableChildren(ref HashSet<ControlNode> navigable)
    {
        if (children.Count <= 0) return 0;
        if (!IsVisibleInHierarchy || !IsActiveInHierarchy) return 0;
        
        var count = navigable.Count;
        foreach (var child in children)
        {
            if(child.Navigable) navigable.Add(child);
            child.GetAllNavigableChildren(ref navigable);
        }
        return navigable.Count - count;
    }
    public int GetAllVisibleInHierarchyChildren(ref HashSet<ControlNode> visibleChildren)
    {
        if (children.Count <= 0) return 0;
        if (!IsVisibleInHierarchy || !IsActiveInHierarchy) return 0;
        
        var count = visibleChildren.Count;
        foreach (var child in children)
        {
            if(child.IsVisibleInHierarchy) visibleChildren.Add(child);
            child.GetAllVisibleInHierarchyChildren(ref visibleChildren);
        }
        return visibleChildren.Count - count;
    }
    public int GetAllActiveInHierarchyChildren(ref HashSet<ControlNode> activeChildren)
    {
        if (children.Count <= 0) return 0;
        if (!IsVisibleInHierarchy || !IsActiveInHierarchy) return 0;
        
        var count = activeChildren.Count;
        foreach (var child in children)
        {
            if(child.IsVisibleInHierarchy) activeChildren.Add(child);
            child.GetAllActiveInHierarchyChildren(ref activeChildren);
        }
        return activeChildren.Count - count;
    }
    
    
    private void ChangeChildrenVisible(bool value)
    {
        foreach (var child in children)
        {
            child.prevNavigable = child.Navigable;
            child.prevIsVisibleInHierarchy = child.IsVisibleInHierarchy;
            child.parentVisible = value;
            child.ResolveParentVisibleChanged();
            // if(value || child.visible != value)
            if(child.visible) child.ChangeChildrenVisible(value);
        }
    }
    private void ChangeChildrenActive(bool value)
    {
        foreach (var child in children)
        {
            child.prevNavigable = child.Navigable;
            child.prevIsActiveInHierarchy = child.IsActiveInHierarchy;
            child.parentActive = value;
            child.ResolveParentActiveChanged();
            // if(value || child.active != value)
            if(child.active) child.ChangeChildrenActive(value);
        }
        
        //value     active      inactive    active      inactive
        //child     active      inactive    inactive    active
        //          yes         no          no          yes
    }
    
    #endregion

    #region Select & Deselect

    public bool Select()
    {
        if (SelectionFilter == SelectFilter.None) return false;
        Selected = true;
        return true;
    }

    public bool Deselect()
    {
        if (SelectionFilter == SelectFilter.None) return false;
        Selected = false;
        navigationSelected = false;
        return true;
    }
    
    public bool NavigationSelect()
    {
        if (SelectionFilter is SelectFilter.Mouse or SelectFilter.None) return false;
        if (InputFilter is InputFilter.None or InputFilter.MouseOnly) return false;
        Selected = true;
        navigationSelected = true;
        return true;
    }

    public bool NavigationDeselect()
    {
        if (SelectionFilter is SelectFilter.Mouse or SelectFilter.None) return false;
        if (InputFilter is InputFilter.None or InputFilter.MouseOnly) return false;
        Selected = false;
        navigationSelected = false;
        return true;
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

    public void SetRect(Rect newRect)
    {
        Rect = newRect.ApplyMargins(Margins);
    }
    public void UpdateRect(Rect sourceRect)
    {
        var p = sourceRect.GetPoint(Anchor);
        var size = Stretch.LengthSquared() == 0f ? Rect.Size : sourceRect.Size * Stretch;

        if (MinSize.Area > 0)
        {
            size = size.Max(MinSize);
        }

        if (MaxSize.Area > 0)
        {
            size = size.Min(MaxSize);
        }
        
        //size = size.Clamp(MinSize, MaxSize);
        
        Rect = new Rect(p, size, Anchor).ApplyMargins(Margins);
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

    protected virtual Rect SetChildRect(ControlNode child, Rect inputRect) => inputRect;
    private void UpdateChildren(float dt, Vector2 mousePos, bool mousePosValid)
    {
        var iterator = DisplayedChildren ?? children;
        foreach (var child in iterator)
        {
            if(!child.displayed) continue;
            child.UpdateRect(SetChildRect(child, Rect));
            child.InternalUpdate(dt, mousePos, mousePosValid);
            OnChildUpdated(child);
        }
    }

    private void DrawChildren()
    {
        var iterator = DisplayedChildren ?? children;
        foreach (var child in iterator)
        {
            if(!child.displayed) continue;
            child.InternalDraw();
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
                bool isMouseInside = mousePosValid && Rect.ContainsPoint(mousePos);
                if (MouseFilter == MouseFilter.Stop) mousePosValid = false;
                
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
                if (InputFilter == InputFilter.MouseOnly)
                {
                    pressed = MouseInside && GetMousePressedState();
                } 
                else if (InputFilter == InputFilter.MouseNever)
                {
                    pressed = GetPressedState();
                }
                else if (InputFilter == InputFilter.All)
                {
                    pressed = (MouseInside && GetMousePressedState()) || GetPressedState();
                }

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
    /// <summary>
    /// Return if the key for the pressed state is down
    /// </summary>
    protected virtual bool GetPressedState() => false;
    
    /// <summary>
    /// Return if the mouse button for the pressed state is down (only is called when mouse is inside)
    /// </summary>
    protected virtual bool GetMousePressedState() => false;

    /// <summary>
    /// Return the direction to move to another element.
    /// </summary>
    public virtual Direction GetNavigationDirection() => new();

    public void NavigatedTo(Direction dir)
    {
        HasNavigated(dir);
        OnNavigated?.Invoke(this, dir);
    }

    protected virtual void HasNavigated(Direction dir)
    {
        
    }
    #endregion

    #region Public

    public float GetDistanceTo(ControlNode other)
    {
        if (other == this) return -1f;
        return (other.Rect.TopLeft - Rect.TopLeft).Length();
    }
    public float GetDistanceSquaredTo(ControlNode other)
    {
        if (other == this) return -1f;
        return (other.Rect.TopLeft - Rect.TopLeft).LengthSquared();
    }

    public Vector2 GetNavigationOrigin(Direction dir) => Rect.GetPoint(dir.Invert().ToAlignement());

    #endregion
    
    #region Virtual

    // protected virtual IEnumerable<ControlNode>? GetChildrenIterator => null;
    
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
    protected virtual void MouseFilterWasChanged(MouseFilter old, MouseFilter cur) { }
    protected virtual void SelectionFilterWasChanged(SelectFilter old, SelectFilter cur) { }
    protected virtual void InputFilterWasChanged(InputFilter old, InputFilter cur) { }
    protected virtual void NavigableWasChanged(bool value) { }
    protected virtual void DisplayedWasChanged(bool value) { }
    protected virtual void ActiveInHierarchyChanged(bool value) { }
    protected virtual void VisibleInHierarchyChanged(bool value) { }
    #endregion

    #region Private
    private void ResolveOnActiveInHierarchyChanged()
    {
        if (prevIsActiveInHierarchy == IsActiveInHierarchy) return;
        ActiveInHierarchyChanged(IsActiveInHierarchy);
        OnActiveInHierarchyChanged?.Invoke(this, IsActiveInHierarchy);
    }

    private void ResolveOnVisibleInHierarchyChanged()
    {
        if (prevIsVisibleInHierarchy == IsVisibleInHierarchy) return;
        VisibleInHierarchyChanged(IsVisibleInHierarchy);
        OnVisibleInHierarchyChanged?.Invoke(this, IsVisibleInHierarchy);
    }
    private void ResolveOnDisplayedChanged()
    {
        DisplayedWasChanged(displayed);
        OnDisplayedChanged?.Invoke(this, displayed);
        ResolveOnNavigableChanged();
        ResolveOnVisibleInHierarchyChanged();
    }
    private void ResolveActiveChanged()
    {
        ActiveWasChanged(active);
        OnActiveChanged?.Invoke(this, active);
        ResolveOnNavigableChanged();
        ResolveOnActiveInHierarchyChanged();
    }
    private void ResolveVisibleChanged()
    {
        VisibleWasChanged(visible);
        OnVisibleChanged?.Invoke(this, visible);
        ResolveOnNavigableChanged();
        ResolveOnVisibleInHierarchyChanged();
    }
    private void ResolveParentVisibleChanged()
    {
        ParentVisibleWasChanged(parentVisible);
        OnParentVisibleChanged?.Invoke(this, parentVisible);
        ResolveOnNavigableChanged();
        ResolveOnVisibleInHierarchyChanged();
    }
    private void ResolveParentActiveChanged()
    {
        ParentActiveWasChanged(parentActive);
        OnParentActiveChanged?.Invoke(this, parentActive);
        ResolveOnNavigableChanged();
        ResolveOnActiveInHierarchyChanged();
    }
    private void ResolveParentChanged(ControlNode? oldParent, ControlNode? newParent)
    {
        ParentWasChanged(oldParent, newParent);
        OnParentChanged?.Invoke(this, oldParent, newParent);
        // ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveChildAdded(ControlNode newChild)
    {
        ChildWasAdded(newChild);
        OnChildAdded?.Invoke(this, newChild);
    }
    private void ResolveChildRemoved(ControlNode oldChild)
    {
        ChildWasRemoved(oldChild);
        OnChildRemoved?.Invoke(this, oldChild);
    }
    private void ResolveMouseEntered(Vector2 mousePos)
    {
        MouseHasEntered(mousePos);
        OnMouseEntered?.Invoke(this, mousePos);
    }
    private void ResolveMouseExited(Vector2 mousePos)
    {
        MouseHasExited(mousePos);
        OnMouseExited?.Invoke(this, mousePos);
    }
    private void ResolveSelectedChanged()
    {
        // Console.WriteLine($"Selected Changed to {selected} in {this.Anchor}");
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

    private void ResolveOnMouseFilterChanged(MouseFilter old, MouseFilter cur)
    {
        MouseFilterWasChanged(old, cur);
        OnMouseFilterChanged?.Invoke(this, old, cur);
    }
    private void ResolveOnSelectionFilterChanged(SelectFilter old, SelectFilter cur)
    {
        SelectionFilterWasChanged(old, cur);
        OnSelectionFilterChanged?.Invoke(this, old, cur);
        ResolveOnNavigableChanged();
    }
    private void ResolveOnInputFilterChanged(InputFilter old, InputFilter cur)
    {
        InputFilterWasChanged(old, cur);
        OnInputFilterChanged?.Invoke(this, old, cur);
        ResolveOnNavigableChanged();
    }
    private void ResolveOnNavigableChanged()
    {
        if (prevNavigable == Navigable) return;
        NavigableWasChanged(Navigable);
        OnNavigableChanged?.Invoke(this, Navigable);
    }
    #endregion
    
}