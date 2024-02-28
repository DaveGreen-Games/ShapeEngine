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
    All = 3
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
    
    public bool IsUp => vertical == -1 && horizontal == 0;
    public bool IsDown => vertical == 1 && horizontal == 0;
    public bool IsLeft => horizontal == -1 && vertical == 0;
    public bool IsRight => horizontal == 1 && vertical == 0;
    public bool IsUpLeft => vertical == -1 && horizontal == -1;
    public bool IsDownLeft => vertical == 1 && horizontal == -1;
    public bool IsUpRight => vertical == -1 && horizontal == 1;
    public bool IsDownRight => vertical == 1 && horizontal == 1;
    
    public Vector2 ToVector2() => new(horizontal, vertical);
    public Vector2 ToAlignement() => new Vector2(horizontal + 1, vertical + 1) / 2;
    // public Vector2 ToReverseAlignement() => new Vector2((horizontal * -1) + 1, (vertical * -1) + 1) / 2;

    public NavigationDirection Invert() => new(horizontal * -1, vertical * -1);
    
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

    public ControlNode? SelectedNode => selectedNode;

    private void SetSelectedNode(ControlNode? newNode)
    {
        if (newNode == null && selectedNode == null) return;
        if (selectedNode == newNode) return;
            
        var prev = selectedNode;
        selectedNode = newNode;
        ResolveOnSelectedControlNodeChanged(prev, selectedNode);
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

    public void Clear()
    {
        var nodesToRemove = nodes.ToList();
        foreach (var node in nodesToRemove)
        {
            RemoveNode(node);
        }
    }
    public bool AddNode(ControlNode node)
    {
        if (!nodes.Add(node)) return false;
        dirty = true;
        HandleNodeAddition(node);
        return true;
    }
    public bool RemoveNode(ControlNode node)
    {
        if (!nodes.Remove(node)) return false;
        dirty = true;
        HandleNodeRemoval(node);
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
                SetSelectedNode(navigable[0]);
                // SelectedNode = navigable[0];
                if (selectedNode == null || !selectedNode.NavigationSelect())
                {
                    throw new WarningException(
                        "Control Node Navigation Selected return false when it should have returned true!");
                }
            }
            else return;
        }

        
        var dir = selectedNode.GetNavigationDirection();
        var nextNode = GetNextNode(dir);
        if (nextNode != null && CheckNextNode(nextNode, dir))
        {
            selectedNode.NavigationDeselect();
            SetSelectedNode(nextNode);
            selectedNode.NavigationSelect();
            ResolveOnNavigated(dir);
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
        if (selectedNode == null) return null;
        var navigable = GetNavigableNodes();
        if (navigable.Count <= 0) return null;

        var minDisSq = float.MaxValue;
        var origin = selectedNode.GetNavigationOrigin(dir);
        ControlNode? newNode = null;
        
        foreach (var node in navigable)
        {
            if(node == selectedNode) continue;
            
            var dif = node.GetNavigationOrigin(dir) - origin;
            var neighborDistanceSquared = GetNeighborDistance(dif, dir);
            if (neighborDistanceSquared >= minDisSq) continue;
            minDisSq = neighborDistanceSquared;
            newNode = node;
        }
        return newNode;
    }
    private float GetNeighborDistance(Vector2 dif, NavigationDirection dir)
    {
        if (dir.IsLeft) return dif.X < 0 ? dif.LengthSquared() : float.MaxValue;
        if (dir.IsRight) return dif.X > 0 ? dif.LengthSquared() : float.MaxValue;
        if (dir.IsUp) return dif.Y < 0 ? dif.LengthSquared() : float.MaxValue;
        if (dir.IsDown) return dif.Y > 0 ? dif.LengthSquared() : float.MaxValue;
        
        if (dir.IsUpLeft) return dif is { X: < 0, Y: < 0 } ? dif.LengthSquared() : float.MaxValue;
        if (dir.IsDownLeft) return dif is { X: < 0, Y: > 0 } ? dif.LengthSquared() : float.MaxValue;
        if (dir.IsUpRight) return dif is { X: > 0, Y: < 0 } ? dif.LengthSquared() : float.MaxValue;
        if (dir.IsDownRight) return dif is { X: > 0, Y: > 0 } ? dif.LengthSquared() : float.MaxValue;
        
        return float.MaxValue;
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
    private void HandleNodeAddition(ControlNode node)
    {
        if (node.Selected)
        {
            if (selectedNode == null && IsNavigating) SetSelectedNode(node);
            else node.Deselect();
        }
        
        node.OnNavigableChanged += OnControlNodeNavigableChanged;
        node.OnChildAdded += OnControlNodeChildAdded;
        node.OnChildRemoved += OnControlNodeChildRemoved;
        node.OnSelectedChanged += OnNodeSelectionChanged;
        
        ResolveOnControlNodeAdded(node);

        foreach (var child in node.GetChildrenEnumerable)
        {
            HandleNodeAddition(child);
        }
    }
    private void HandleNodeRemoval(ControlNode node)
    {
        node.OnNavigableChanged -= OnControlNodeNavigableChanged;
        node.OnChildAdded -= OnControlNodeChildAdded;
        node.OnChildRemoved -= OnControlNodeChildRemoved;
        node.OnSelectedChanged -= OnNodeSelectionChanged;
        if (node == selectedNode) SetSelectedNode(null);
        ResolveOnControlNodeRemoved(node);
        foreach (var child in node.GetChildrenEnumerable)
        {
            HandleNodeRemoval(child);
        }
    }
    private void OnControlNodeChildAdded(ControlNode node, ControlNode child)
    {
        dirty = true;
        HandleNodeAddition(child);
    }
    private void OnControlNodeChildRemoved(ControlNode node, ControlNode child)
    {
        dirty = true;
        HandleNodeRemoval(child);
    }
    private void OnNodeSelectionChanged(ControlNode node, bool value)
    {
        if (!value) return;
        if (!node.Navigable) return;
        
        if (selectedNode == null)
        {
            SetSelectedNode(node);
            node.NavigationSelect();
            return;
        }
        
        if (node != selectedNode)
        {
            selectedNode.Deselect();
            SetSelectedNode(node);
            selectedNode.NavigationSelect();
        }
    }
    private void OnControlNodeNavigableChanged(ControlNode node, bool navigable)
    {
        dirty = true;
        if (selectedNode != null && node == selectedNode)
        {
            node.Deselect();
            SetSelectedNode(null);
        }
    }

    #endregion
    
    #region Virtual

    protected virtual bool CheckNextNode(ControlNode nextNode, NavigationDirection dir) => true;
    protected virtual void WasNavigated(NavigationDirection dir) { }
    protected virtual void NavigationWasStarted() { }
    protected virtual void NavigationWasEnded() { }
    protected virtual void ControlNodeWasAdded(ControlNode node) { }
    protected virtual void ControlNodeWasRemoved(ControlNode node) { }
    protected virtual void SelectedControlNodeWasChanged(ControlNode? prev, ControlNode? cur) { }
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
        SelectedControlNodeWasChanged(prev, cur);
        OnSelectedControlNodeChanged?.Invoke(this, prev, cur);
    }
    #endregion
}



public abstract class ControlNode
{
    #region Events
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

    public void SetDisplayed(ControlNode changer, bool value)
    {
        if (changer is not ControlNodeContainer) return;
        displayed = value;
    }

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

    public float ContainerStretch = 1f;
    
    public Vector2 MinSize = new(0f);
    public Vector2 MaxSize = new(0f);
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
            visible = value;
            ResolveVisibleChanged();
            if(parent == null || parentVisible)
                ChangeChildrenVisible(visible);
            
        } 
    }

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
            child.prevNavigable = child.Navigable;
            child.parentActive = checkActive;
            child.ResolveParentActiveChanged();
            if(child.active != checkActive) child.ChangeChildrenActive(checkActive);
            
        }
        
        if (!checkVisible)
        {
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
    public int GetAllVisibleInHierarchyChildren(ref List<ControlNode> visibleChildren)
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
    public int GetAllActiveInHierarchyChildren(ref List<ControlNode> activeChildren)
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

        if (MinSize.LengthSquared() > 0)
        {
            size = size.Max(MinSize);
        }

        if (MaxSize.LengthSquared() > 0)
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
        foreach (var child in children)
        {
            if(!child.displayed) continue;
            child.UpdateRect(SetChildRect(child, Rect));
            child.InternalUpdate(dt, mousePos, mousePosValid);
            OnChildUpdated(child);
        }
    }

    private void DrawChildren()
    {
        foreach (var child in children)
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

    public Vector2 GetNavigationOrigin(NavigationDirection dir) => Rect.GetPoint(dir.Invert().ToAlignement());

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
        OnActiveChanged?.Invoke(this, active);
        ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveVisibleChanged()
    {
        VisibleWasChanged(visible);
        OnVisibleChanged?.Invoke(this, visible);
        ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveParentVisibleChanged()
    {
        ParentVisibleWasChanged(parentVisible);
        OnParentVisibleChanged?.Invoke(this, parentVisible);
        ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveParentActiveChanged()
    {
        ParentActiveWasChanged(parentActive);
        OnParentActiveChanged?.Invoke(this, parentActive);
        ResolveOnNavigableChanged(Navigable);
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
        ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveOnInputFilterChanged(InputFilter old, InputFilter cur)
    {
        InputFilterWasChanged(old, cur);
        OnInputFilterChanged?.Invoke(this, old, cur);
        ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveOnNavigableChanged(bool value)
    {
        if (prevNavigable == Navigable) return;
        NavigableWasChanged(value);
        OnNavigableChanged?.Invoke(this, value);
    }
    #endregion
    
}



public class ControlNodeContainer : ControlNode
{
    public enum ContainerType
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Grid = 3
    }

    public event Action<ControlNode, ControlNode>? OnFirstNodeSelected;
    public event Action<ControlNode, ControlNode>? OnLastNodeSelected;
    public event Action<ControlNode, ControlNode>? OnNodeSelected;

    
    
    public int GridRows
    {
        get => gridRows;
        set
        {
            if (value == gridRows) return;
            if (value > 0) gridRows = value;
        }
    }
    public int GridColumns
    {
        get => gridColumns;
        set
        {
            if (value == gridColumns) return;
            if (value > 0) gridColumns = value;
        }
    }
    public int DisplayCount
    {
        get => type == ContainerType.Grid ? gridColumns * gridRows : displayCount;
        set
        {
            if (value == displayCount || type == ContainerType.Grid) return;
            dirty = true;
            
            if (value < 0)
            {
                displayCount = -1;
                displayIndex = 0;
                return;
            }

            displayCount = value;
            
        }
    }
    public int DisplayIndex
    {
        get => displayIndex;
        set
        {
            if (value == displayIndex) return;
            dirty = true;
            
            if (value < 0)
            {
                displayIndex = 0;
                return;
            }

            displayIndex = value;

        }
    }

    
    private ContainerType type = ContainerType.None;
    public ContainerType Type
    {
        get => type;
        set
        {
            if (value == type) return;
            dirty = true;
            type = value;
        } 
    }
    public Vector2 Gap { get; set; } = new();

    

    #region Private Members
    private int displayCount = -1;
    private int displayIndex = 0;
    private int gridRows = 1;
    private int gridColumns = 1;

    private readonly List<ControlNode> displayedNodes = new();
    
    private bool dirty = false;
    
    private Vector2 curOffset = new();
    private Vector2 gapSize = new();
    private Vector2 startPos = new();
    private Vector2 elementSize = new();

    #endregion
    
    #region Override
    protected override void OnUpdate(float dt, Vector2 mousePos, bool mousePosValid)
    {
        if (Type == ContainerType.None) return;
        if(dirty) CompileDisplayedNodes();
        
        if (Type == ContainerType.Vertical)
        {
            startPos = Rect.TopLeft;
            float stretchFactorTotal = 0f;
            int count = displayCount <= 0 ? displayedNodes.Count : displayCount;
            for (int i = 0; i < count; i++)
            {
                if (i < displayedNodes.Count)
                {
                    stretchFactorTotal += displayedNodes[i].ContainerStretch;
                }
                else stretchFactorTotal += 1;
            }
            int gaps = count - 1;

            float totalHeight = Rect.Height;
            gapSize = new(0f, totalHeight * Gap.Y);
            float elementHeight = (totalHeight - gaps * gapSize.Y) / stretchFactorTotal;
            elementSize = new(0f, elementHeight);
            curOffset = new(0f, 0f);
        }

        if (Type == ContainerType.Horizontal)
        {
            startPos = Rect.TopLeft;
            var stretchFactorTotal = 0f;
            int count = displayCount <= 0 ? displayedNodes.Count : displayCount;
            for (var i = 0; i < count; i++)
            {
                if (i < displayedNodes.Count)
                {
                    stretchFactorTotal += displayedNodes[i].ContainerStretch;
                }
                else stretchFactorTotal += 1;
            }
            int gaps = count - 1;

            float totalWidth = Rect.Width;
            gapSize = new(totalWidth * Gap.X, 0f);
            float elementWidth = (totalWidth - gaps * gapSize.X) / stretchFactorTotal;
            elementSize = new(elementWidth, 0f);
            curOffset = new(0f, 0f);
        }

        if (Type == ContainerType.Grid)
        {
            startPos = Rect.TopLeft;

            int hGaps = GridColumns - 1;
            float totalWidth = Rect.Width;
            float hGapSize = totalWidth * Gap.X;
            float elementWidth = (totalWidth - hGaps * hGapSize) / GridColumns;

            int vGaps = GridRows - 1;
            float totalHeight = Rect.Height;
            float vGapSize = totalHeight * Gap.Y;
            float elementHeight = (totalHeight - vGaps * vGapSize) / GridRows;

            gapSize = new(hGapSize + elementWidth, vGapSize + elementHeight);
            elementSize = new(elementWidth, elementHeight);
            
            
        }
        
        
    }
    protected override Rect SetChildRect(ControlNode node, Rect inputRect)
    {
        if (Type == ContainerType.None) return inputRect;
        if (Type == ContainerType.Vertical)
        {
            float height = elementSize.Y * node.ContainerStretch;
            var size = new Vector2(Rect.Width, height);
            var maxSize = node.MaxSize;
            if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
            if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
            var r = new Rect(startPos + curOffset, size, new(0f));
            curOffset += new Vector2(0f, gapSize.Y + height);
            return r;
        }

        if (Type == ContainerType.Horizontal)
        {
            float width = elementSize.X * node.ContainerStretch;
            Vector2 size = new(width, Rect.Height);
            var maxSize = node.MaxSize;
            if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
            if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
            var r = new Rect(startPos + curOffset, size, new(0f));
            curOffset += new Vector2(gapSize.X + width, 0f);
            return r;
        }
        
        if (Type == ContainerType.Grid)
        {
            // int count = GridColumns * GridRows;
            // if (displayedNodes.Count < count) count = displayedNodes.Count;
            int i = displayedNodes.IndexOf(node);
            if (i < 0) return inputRect;
            var coords = ShapeMath.TransformIndexToCoordinates(i, GridRows, GridColumns, true);
            var r = new Rect
            (
                startPos + new Vector2(gapSize.X * coords.col, 0f) + new Vector2(0f, gapSize.Y * coords.row), 
                elementSize,
                new(0f)
            );
            return r;
        }

        return inputRect;
    }
    protected override void ChildWasAdded(ControlNode newChild)
    {
        dirty = true;
        newChild.OnSelectedChanged += OnChildSelectionChanged;
        newChild.OnVisibleChanged += OnChildVisibleChanged;
        newChild.OnParentVisibleChanged += OnChildParentVisibleChanged;
    }
    protected override void ChildWasRemoved(ControlNode oldChild)
    {
        dirty = true;
        oldChild.OnSelectedChanged -= OnChildSelectionChanged;
        oldChild.OnVisibleChanged -= OnChildVisibleChanged;
        oldChild.OnParentVisibleChanged -= OnChildParentVisibleChanged;
        oldChild.SetDisplayed(this, true);

    }

    #endregion
    
    #region Virtual
    protected virtual void FirstNodeWasSelected(ControlNode node) { }
    protected virtual void LastNodeWasSelected(ControlNode node) { }
    protected virtual void NodeWasSelected(ControlNode node) { }
    protected virtual bool IsFirst(ControlNode node)
    {
        if (displayedNodes.Count <= 0) return false;
        return displayedNodes[0] == node;
    }
    protected virtual bool IsLast(ControlNode node)
    {
        if (displayedNodes.Count <= 0) return false;
        return displayedNodes[^1] == node;
    }
    #endregion
    
    #region Private
    private void CompileDisplayedNodes()
    {
        dirty = false;
        displayedNodes.Clear();

        var visibleChildren = GetChildren((node => node.IsVisibleInHierarchy));

        if (visibleChildren == null) return;
        
        if (displayCount > 0)
        {
            if (displayIndex + displayCount > visibleChildren.Count)
            {
                displayIndex = visibleChildren.Count - displayCount;
            }
        }
        
        for (var i = 0; i < visibleChildren.Count; i++)
        {
            var child = visibleChildren[i];
            if (DisplayCount == 0) child.SetDisplayed(this, false);
            else if (DisplayCount > 0)
            {
                if (i < DisplayIndex) child.SetDisplayed(this, false);
                else if (i >= DisplayIndex + DisplayCount) child.SetDisplayed(this, false);
                else
                {
                    displayedNodes.Add(child);
                    child.SetDisplayed(this, true);
                }
                
            }
            else
            {
                child.SetDisplayed(this, true);
                displayedNodes.Add(child);
            }
        }
    }
    private void OnChildVisibleChanged(ControlNode child, bool visible)
    {
        dirty = true;
    }
    private void OnChildParentVisibleChanged(ControlNode child, bool parentVisible)
    {
        dirty = true;
    }
    private void OnChildSelectionChanged(ControlNode child, bool selected)
    {
        if(selected) ResolveOnNodeSelected(child);
    }

    private void ResolveOnFirstNodeSelected(ControlNode node)
    {
        FirstNodeWasSelected(node);
        OnFirstNodeSelected?.Invoke(this, node);

        
        if (displayCount >= 0 && type is ContainerType.Horizontal or ContainerType.Vertical)
        {
            DisplayIndex -= 1;
        }
    }
    private void ResolveOnLastNodeSelected(ControlNode node)
    {
        LastNodeWasSelected(node);
        OnLastNodeSelected?.Invoke(this, node);

        if (displayCount >= 0 && type is ContainerType.Horizontal or ContainerType.Vertical)
        {
            DisplayIndex += 1;
        }
    }
    private void ResolveOnNodeSelected(ControlNode node)
    {
        if(IsFirst(node)) ResolveOnFirstNodeSelected(node);
        else if(IsLast(node)) ResolveOnLastNodeSelected(node);
        
        NodeWasSelected(node);
        OnNodeSelected?.Invoke(this, node);
        
    }
    #endregion
}
