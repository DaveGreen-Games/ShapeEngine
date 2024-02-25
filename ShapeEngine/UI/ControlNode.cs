using System.ComponentModel;
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

public readonly struct NavigationDirection
{
    private readonly int horizontal;
    private readonly int vertical;

    public NavigationDirection()
    {
        horizontal = 0;
        vertical = 0;
    }
    public NavigationDirection(int hor, int vert)
    {
        this.horizontal = Sign(hor);
        this.vertical = Sign(vert);
    }

    public NavigationDirection(Vector2 dir)
    {
        this.horizontal = MathF.Sign(dir.X);
        this.vertical = MathF.Sign(dir.X);
    }

    public bool IsValid => IsVertical || IsHorizontal;
    public bool IsVertical => vertical != 0;
    public bool IsHorizontal => horizontal != 0;
    
    public bool IsUp => vertical == -1;
    public bool IsDown => vertical == 1;
    public bool IsLeft => horizontal == -1;
    public bool IsRight => horizontal == 1;


    public static NavigationDirection GetEmpty() => new(0, 0);
    public static NavigationDirection GetLeft() => new(-1, 0);
    public static NavigationDirection GetRight() => new(1, 0);
    public static NavigationDirection GetUp() => new(0, -1);
    public static NavigationDirection GetDown() => new(0, 1);
    
    public static NavigationDirection GetUpLeft() => new(-1, -1);
    public static NavigationDirection GetDownLeft() => new(-1, 1);
    public static NavigationDirection GetUpRight() => new(1, -1);
    public static NavigationDirection GetDownRight() => new(1, 1);
    
    private static int Sign(int value)
    {
        if (value < 0) return -1;
        if (value > 0) return 1;
        return 0;
    }

}

public class ControlNodeNavigator
{
    #region Events
    public event Action<ControlNodeNavigator>? OnNavigationStarted;
    public event Action<ControlNodeNavigator>? OnNavigationEnded;
    public event Action<ControlNodeNavigator, ControlNode>? OnControlNodeAdded;
    public event Action<ControlNodeNavigator, ControlNode>? OnControlNodeRemoved;
    public event Action<ControlNodeNavigator, ControlNode?, ControlNode?>? OnSelectedControlNodeChanged;
    public event Action<ControlNodeNavigator, NavigationDirection>? OnNavigated;
    #endregion

    #region Private Members

    private readonly HashSet<ControlNode> nodes = new();
    private readonly List<ControlNode> navigableNodes = new();
    private bool dirty = false;
    private ControlNode? selectedNode = null;

    #endregion

    #region Getter & Setter

    public ControlNode? SelectedNode
    {
        get => selectedNode;
        set
        {
            if (value == null && selectedNode == null) return;
            if (selectedNode == value) return;
            
            var prev = selectedNode;
            selectedNode = value;
            ResolveOnSelectedControlNodeChanged(prev, selectedNode);
        }
    }
    public bool IsNavigating { get; private set; } = false;

    #endregion

    #region Public
    public void StartNavigation()
    {
        if (IsNavigating) return;
        IsNavigating = true;
        selectedNode?.NavigationSelect();
        ResolveOnNavigationStarted();
    }
    public void EndNavigation()
    {
        if (!IsNavigating) return;
        IsNavigating = false;
        selectedNode?.NavigationDeselect();
        ResolveOnNavigationEnded();
    }
    
    public bool AddNode(ControlNode node)
    {
        if (!nodes.Add(node)) return false;
        if (IsNavigating)
        {
            if (node.Selected && selectedNode == null) selectedNode = node;
        }
        else
        {
            if (node.Selected) node.Deselect();
        }
        
        // if (IsNavigating)
        // {
        //     if (node.Selected)
        //     {
        //         if (selectedNode == null)
        //         {
        //             selectedNode = node;
        //         }
        //         else node.Deselect();
        //     }
        //     else
        //     {
        //         if (selectedNode == null)
        //         {
        //             if (node.NavigationSelect()) selectedNode = node;
        //         }
        //     }
        // }
        // else if (node.Selected) node.Deselect();
        //
        
        dirty = true;
        ResolveOnControlNodeAdded(node);
        node.OnNavigableChanged += OnControlNodeNavigableChanged;
        node.OnChildAdded += OnControlNodeChildAdded;
        node.OnChildRemoved += OnControlNodeChildRemoved;
        // node.OnSelectedChanged += OnNodeSelectionChanged;
        return true;
    }
    public bool RemoveNode(ControlNode node)
    {
        if (!nodes.Remove(node)) return false;
        dirty = true;
        ResolveOnControlNodeRemoved(node);
        node.OnNavigableChanged -= OnControlNodeNavigableChanged;
        node.OnChildAdded -= OnControlNodeChildAdded;
        node.OnChildRemoved -= OnControlNodeChildRemoved;
        // node.OnSelectedChanged -= OnNodeSelectionChanged;
        if (node == selectedNode) selectedNode = null; // selectedNode = GetClosestNode(node);
        
        return true;
    }
    
    public void Update()
    {
        if (!IsNavigating) return;
        if (selectedNode == null)
        {
            var navigable = GetNavigableNodes();
            if (navigable.Count > 0)
            {
                selectedNode = navigable[0];
                if (!selectedNode.NavigationSelect())
                {
                    throw new WarningException(
                        "Control Node Navigation Selected return false when it should have returned true!");
                }
            }
            else return;
        }

        var dir = selectedNode.GetNavigationDirection();
        var nextNode = GetNextNode(dir);
        if (nextNode != null && nextNode != selectedNode)
        {
            selectedNode.NavigationDeselect();
            selectedNode = nextNode;
            selectedNode.NavigationSelect();
        }
    }
    #endregion

    #region Private
    private ControlNode? GetClosestNode(ControlNode node)
    {
        var navigable = GetNavigableNodes();
        if (navigable.Count <= 0) return null;

        var minDisSq = float.MaxValue;
        ControlNode? closestNode = null;
        
        foreach (var other in navigable)
        {
            var disSq = node.GetDistanceSquaredTo(other);
            if (disSq >= 0 && disSq < minDisSq)
            {
                closestNode = other;
                minDisSq = disSq;
            }
        }
        return closestNode;
    }
    private ControlNode? GetNextNode(NavigationDirection dir)
    {
        if (!dir.IsValid) return null;
        var navigable = GetNavigableNodes();
        if (navigable.Count <= 0) return null;

        // throw new NotImplementedException();
        return null;
    }
    private List<ControlNode> GetNavigableNodes()
    {
        if(dirty) CompileNavigableControlNodes();
        return navigableNodes;
    }

    private void CompileNavigableControlNodes()
    {
        dirty = false;
        navigableNodes.Clear();
        var result = new HashSet<ControlNode>();
        foreach (var node in nodes)
        {
            if(node.Navigable) navigableNodes.Add(node);
            node.GetAllNavigableChildren(ref result);
        }

        if (result.Count > 0) navigableNodes.AddRange(result);
    }
    private void OnControlNodeChildAdded(ControlNode child) => dirty = true;
    private void OnControlNodeChildRemoved(ControlNode child) => dirty = true;
    private void OnControlNodeNavigableChanged(bool navigable) => dirty = true;
    
    #endregion
    
    #region Virtual
    protected virtual void WasNavigated(NavigationDirection dir) { }
    protected virtual void NavigationWasStarted() { }
    protected virtual void NavigationWasEnded() { }
    protected virtual void ControlNodeWasAdded(ControlNode node) { }
    protected virtual void ControlNodeWasRemoved(ControlNode node) { }
    protected virtual void SeletecControlNodeWasChanged(ControlNode? prev, ControlNode? cur) { }
    #endregion
    
    #region Resolve

    private void ResolveOnNavigated(NavigationDirection dir)
    {
        WasNavigated(dir);
        OnNavigated?.Invoke(this, dir);
    }
    private void ResolveOnNavigationStarted()
    {
        NavigationWasStarted();
        OnNavigationStarted?.Invoke(this);
    }
    private void ResolveOnNavigationEnded()
    {
        NavigationWasEnded();
        OnNavigationEnded?.Invoke(this);
    }
    private void ResolveOnControlNodeAdded(ControlNode node)
    {
        ControlNodeWasAdded(node);
        OnControlNodeAdded?.Invoke(this, node);
    }
    private void ResolveOnControlNodeRemoved(ControlNode node)
    {
        ControlNodeWasRemoved(node);
        OnControlNodeRemoved?.Invoke(this, node);
    }
    private void ResolveOnSelectedControlNodeChanged(ControlNode? prev, ControlNode? cur)
    {
        SeletecControlNodeWasChanged(prev, cur);
        OnSelectedControlNodeChanged?.Invoke(this, prev, cur);
    }
    #endregion
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

    public event Action<MouseFilter, MouseFilter>? OnMouseFilterChanged;
    public event Action<SelectFilter, SelectFilter>? OnSelectionFilterChanged;
    public event Action<InputFilter, InputFilter>? OnInputFilterChanged;
    public event Action<bool>? OnNavigableChanged;
    // public event Action<ControlNode, bool>? OnFocusChanged;
    
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

    private bool navigationSelected = false;

    private bool prevNavigable = false;
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

    #endregion

    #region Getters & Setters
    public bool Active
    {
        get => active;
        set
        {
            if (active == value) return;
            prevNavigable = Navigable;
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
            prevNavigable = Navigable;
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
            child.prevNavigable = child.Navigable;
            child.parentActive = checkActive;
            child.ResolveParentActiveChanged();
            if(child.active) child.ChangeChildrenActive(checkActive);
            
        }
        
        if (!checkVisible)
        {
            child.prevNavigable = child.Navigable;
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
            child.prevNavigable = child.Navigable;
            child.parentActive = true;
            child.ResolveParentActiveChanged();

            if (child.active) child.ChangeChildrenActive(child.active);
        }

        if (child.parentVisible == false)
        {
            child.prevNavigable = child.Navigable;
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
    
    private void ChangeChildrenVisible(bool value)
    {
        foreach (var child in children)
        {
            child.prevNavigable = child.Navigable;
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
            child.prevNavigable = child.Navigable;
            child.parentActive = value;
            child.ResolveParentActiveChanged();
            if(value || child.active != value)
                child.ChangeChildrenActive(value);
        }
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
                if (InputFilter == InputFilter.MouseOnly)
                {
                    pressed = GetMousePressedState();
                } 
                else if (InputFilter == InputFilter.MouseNever)
                {
                    pressed = GetPressedState();
                    // if (Navigable)
                    // {
                    // 
                    // }
                }
                else if (InputFilter == InputFilter.All)
                {
                    pressed = GetMousePressedState() || GetPressedState();
                    // if (Navigable)
                    // {
                    //
                    // }
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
    protected virtual bool GetPressedState() => false;
    protected virtual bool GetMousePressedState() => false;

    public virtual NavigationDirection GetNavigationDirection() => new();
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
    protected virtual void MouseFilterWasChanged(MouseFilter old, MouseFilter cur) { }
    protected virtual void SelectionFilterWasChanged(SelectFilter old, SelectFilter cur) { }
    protected virtual void InputFilterWasChanged(InputFilter old, InputFilter cur) { }
    protected virtual void NavigableWasChanged(bool value) { }
    #endregion

    #region Private
    private void ResolveActiveChanged()
    {
        ActiveWasChanged(active);
        OnActiveChanged?.Invoke(active);
        ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveVisibleChanged()
    {
        VisibleWasChanged(visible);
        OnVisibleChanged?.Invoke(visible);
        ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveParentVisibleChanged()
    {
        ParentVisibleWasChanged(parentVisible);
        OnParentVisibleChanged?.Invoke(parentVisible);
        ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveParentActiveChanged()
    {
        ParentActiveWasChanged(parentActive);
        OnParentActiveChanged?.Invoke(parentActive);
        ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveParentChanged(ControlNode? oldParent, ControlNode? newParent)
    {
        ParentWasChanged(oldParent, newParent);
        OnParentChanged?.Invoke(oldParent, newParent);
        // ResolveOnNavigableChanged(Navigable);
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

    private void ResolveOnMouseFilterChanged(MouseFilter old, MouseFilter cur)
    {
        MouseFilterWasChanged(old, cur);
        OnMouseFilterChanged?.Invoke(old, cur);
    }
    private void ResolveOnSelectionFilterChanged(SelectFilter old, SelectFilter cur)
    {
        SelectionFilterWasChanged(old, cur);
        OnSelectionFilterChanged?.Invoke(old, cur);
        ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveOnInputFilterChanged(InputFilter old, InputFilter cur)
    {
        InputFilterWasChanged(old, cur);
        OnInputFilterChanged?.Invoke(old, cur);
        ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveOnNavigableChanged(bool value)
    {
        if (prevNavigable == Navigable) return;
        NavigableWasChanged(value);
        OnNavigableChanged?.Invoke(value);
    }
    #endregion
    
    
    
    
    // public event Action<MouseFilter, MouseFilter>? OnMouseFilterChanged;
    // public event Action<SelectFilter, SelectFilter>? OnSelectionFilterChanged;
    // public event Action<InputFilter, InputFilter>? OnInputFilterChanged;
    // public event Action<bool>? OnNavigableChanged;
}