using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using System.Xml.Xsl;
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


public readonly struct Direction : IEquatable<Direction>
{
    public readonly int Horizontal;
    public readonly int Vertical;

    public Direction()
    {
        Horizontal = 0;
        Vertical = 0;
    }
    public Direction(int hor, int vert)
    {
        this.Horizontal = Sign(hor);
        this.Vertical = Sign(vert);
    }

    public Direction(Vector2 dir)
    {
        this.Horizontal = MathF.Sign(dir.X);
        this.Vertical = MathF.Sign(dir.X);
    }

    public bool IsValid => IsVertical || IsHorizontal;
    public bool IsVertical => Vertical != 0;
    public bool IsHorizontal => Horizontal != 0;
    
    public bool IsUp => Vertical == -1 && Horizontal == 0;
    public bool IsDown => Vertical == 1 && Horizontal == 0;
    public bool IsLeft => Horizontal == -1 && Vertical == 0;
    public bool IsRight => Horizontal == 1 && Vertical == 0;
    public bool IsUpLeft => Vertical == -1 && Horizontal == -1;
    public bool IsDownLeft => Vertical == 1 && Horizontal == -1;
    public bool IsUpRight => Vertical == -1 && Horizontal == 1;
    public bool IsDownRight => Vertical == 1 && Horizontal == 1;
    
    public Vector2 ToVector2() => new(Horizontal, Vertical);
    public Vector2 ToAlignement() => new Vector2(Horizontal + 1, Vertical + 1) / 2;
    // public Vector2 ToReverseAlignement() => new Vector2((horizontal * -1) + 1, (vertical * -1) + 1) / 2;

    public Direction Invert() => new(Horizontal * -1, Vertical * -1);
    
    public static Direction GetEmpty() => new(0, 0);
    public static Direction GetLeft() => new(-1, 0);
    public static Direction GetRight() => new(1, 0);
    public static Direction GetUp() => new(0, -1);
    public static Direction GetDown() => new(0, 1);
    
    public static Direction GetUpLeft() => new(-1, -1);
    public static Direction GetDownLeft() => new(-1, 1);
    public static Direction GetUpRight() => new(1, -1);
    public static Direction GetDownRight() => new(1, 1);
    
    private static int Sign(int value)
    {
        if (value < 0) return -1;
        if (value > 0) return 1;
        return 0;
    }

    public bool Equals(Direction other) => Horizontal == other.Horizontal && Vertical == other.Vertical;

    public override bool Equals(object? obj) => obj is Direction other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Horizontal, Vertical);
    
    public static bool operator ==(Direction left, Direction right) => left.Equals(right);

    public static bool operator !=(Direction left, Direction right) => !left.Equals(right);

    public override string ToString()
    {
        return $"({Horizontal},{Vertical})";
    }
}
public readonly struct Grid : IEquatable<Grid>
{
    public readonly struct Coordinates : IEquatable<Coordinates>
    {
        public readonly int Row;
        public readonly int Col;
        public bool IsValid => Row >= 0 && Col >= 0;

    
        public Coordinates()
        {
            this.Row = -1;
            this.Col = -1;
        }
        public Coordinates(int col, int row)
        {
            this.Row = row;
            this.Col = col;
        }

        public bool Equals(Coordinates other) => Row == other.Row && Col == other.Col;

        public override bool Equals(object? obj) => obj is Coordinates other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Row, Col);
        
        public static bool operator ==(Coordinates left, Coordinates right) => left.Equals(right);

        public static bool operator !=(Coordinates left, Coordinates right) => !left.Equals(right);
        
        public override string ToString()
        {
            return $"({Col},{Row})";
        }
    }
    
    public readonly int Rows;
    public readonly int Cols;
    public readonly bool LeftToRight;
    
    public bool IsValid => Rows > 0 && Cols > 0;
    public int Count => Rows * Cols;
    
    
    public Grid()
    {
        this.Rows = 0;
        this.Cols = 0;
        this.LeftToRight = true;
    }
    public Grid(int rows, int cols)
    {
        this.Rows = rows;
        this.Cols = cols;
        this.LeftToRight = true;
    }
    public Grid(int rows, int cols, bool leftToRight)
    {
        this.Rows = rows;
        this.Cols = cols;
        this.LeftToRight = leftToRight;
    }

    public bool IsIndexInBounds(int index) => index >= 0 && index <= Count;
    
    public Coordinates IndexToCoordinates(int index)
    {
        if (!IsValid) return new();
        
        if (LeftToRight)
        {
            int row = index / Cols;
            int col = index % Cols;
            return new(col, row);
        }
        else
        {
            int col = index / Rows;
            int row = index % Rows;
            return new(col, row);
        }
            
    }
    
    public int CoordinatesToIndex(Coordinates coordinates)
    {
        if (!IsValid || !coordinates.IsValid) return -1;
        
        if (LeftToRight)
        {
            return coordinates.Row * Cols + coordinates.Col;
        }
        else
        {
            return coordinates.Col * Rows + coordinates.Row;
        }
    }

    public Direction GetDirection(Coordinates coordinates)
    {
        if (!coordinates.IsValid) return new();


        var hor = coordinates.Col == 0 ? -1 : coordinates.Col >= Cols - 1 ? 1 : 0;
        var ver = coordinates.Row == 0 ? -1 : coordinates.Row >= Rows - 1 ? 1 : 0;
        return new(hor, ver);

        // if (coordinates.Row == 0 && coordinates.Col == 0) return new(-1, -1);//topleft
        // if (coordinates.Row == 0 && coordinates.Col > 0 && coordinates.Col < Cols) return new(0, -1);//top
        // if (coordinates.Row == 0 && coordinates.Col >= Cols) return new(1, -1);//topRight

        // if (coordinates.Row > 0 && coordinates.Row < Rows && coordinates.Col >= Cols) return new(1, 0);//right
        // if (coordinates.Row >= Rows && coordinates.Col >= Cols) return new(1, 1); //bottom right
        // if (coordinates.Row >= Rows && coordinates.Col > 0 && coordinates.Col < Cols) return new(0, 1); //bottom
        // if (coordinates.Row >= Rows && coordinates.Col == 0) return new(0, 1); //bottomLeft

    }

    public bool Equals(Grid other) => Rows == other.Rows && Cols == other.Cols && LeftToRight == other.LeftToRight;

    public override bool Equals(object? obj) => obj is Grid other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Rows, Cols, LeftToRight);
    
    public static bool operator ==(Grid left, Grid right) => left.Equals(right);

    public static bool operator !=(Grid left, Grid right) => !left.Equals(right);
    
    public override string ToString()
    {
        var leftToRightText = LeftToRight ? "L->R" : "T->B";
        return $"Cols: {Cols}, Rows: {Rows}, {leftToRightText})";
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
    public event Action<ControlNodeNavigator, Direction>? OnNavigated;
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

    private bool navigationPending = false;
    private Direction prevDir = new();
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

        if (navigationPending)
        {
            navigationPending = false;
            var dir = prevDir;
            
            var nextNode = GetNextNode(dir);
            if (nextNode != null && CheckNextNode(nextNode, dir))
            {
                selectedNode.NavigationDeselect();
                SetSelectedNode(nextNode);
                selectedNode.NavigationSelect();
                ResolveOnNavigated(dir);
            }
        }
        else
        {
            var dir = selectedNode.GetNavigationDirection();
            if (dir.IsValid)
            {
                selectedNode.NavigatedTo(dir);
                navigationPending = true;
                prevDir = dir;

            }
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
    private ControlNode? GetNextNode(Direction dir)
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
    private float GetNeighborDistance(Vector2 dif, Direction dir)
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

    protected virtual bool CheckNextNode(ControlNode nextNode, Direction dir) => true;
    protected virtual void WasNavigated(Direction dir) { }
    protected virtual void NavigationWasStarted() { }
    protected virtual void NavigationWasEnded() { }
    protected virtual void ControlNodeWasAdded(ControlNode node) { }
    protected virtual void ControlNodeWasRemoved(ControlNode node) { }
    protected virtual void SelectedControlNodeWasChanged(ControlNode? prev, ControlNode? cur) { }
    #endregion
    
    #region Resolve
    private void ResolveOnNavigated(Direction dir)
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


    public Grid Grid { get; set; } = new();
    // public int GridRows
    // {
    //     get => gridRows;
    //     set
    //     {
    //         if (value == gridRows) return;
    //         if (value > 0) gridRows = value;
    //     }
    // }
    // public int GridColumns
    // {
    //     get => gridColumns;
    //     set
    //     {
    //         if (value == gridColumns) return;
    //         if (value > 0) gridColumns = value;
    //     }
    // }
    //
    public int DisplayCount
    {
        get => type == ContainerType.Grid && Grid.IsValid ? Grid.Count : displayCount;
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

            int maxValue = displayCount > ChildCount ? 0 : ChildCount - displayCount;
            displayIndex = value > maxValue ? maxValue : value;

        }
    }

    private int navigationStep = 1;
    public int NavigationStep
    {
        get => navigationStep;
        set
        {
            if (value == navigationStep) return;
            if (value < 0) return;
            navigationStep = value;
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
    // private int gridRows = 1;
    // private int gridColumns = 1;

    // private readonly List<ControlNode> displayedNodes = new();
    
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
            int count = displayCount <= 0 ? DisplayedChildrenCount : displayCount;
            for (int i = 0; i < count; i++)
            {
                if (i < DisplayedChildrenCount)
                {
                    stretchFactorTotal += DisplayedChildren[i].ContainerStretch;
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
            int count = displayCount <= 0 ? DisplayedChildrenCount : displayCount;
            for (var i = 0; i < count; i++)
            {
                if (i < DisplayedChildrenCount)
                {
                    stretchFactorTotal += DisplayedChildren[i].ContainerStretch;
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
            if (!Grid.IsValid) return;
            startPos = Rect.TopLeft;

            int hGaps = Grid.Cols - 1;
            float totalWidth = Rect.Width;
            float hGapSize = totalWidth * Gap.X;
            float elementWidth = (totalWidth - hGaps * hGapSize) / Grid.Cols;

            int vGaps = Grid.Rows - 1;
            float totalHeight = Rect.Height;
            float vGapSize = totalHeight * Gap.Y;
            float elementHeight = (totalHeight - vGaps * vGapSize) / Grid.Rows;

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
            if (DisplayedChildren == null) return inputRect;
            if (!Grid.IsValid) return inputRect;
            int i = DisplayedChildren.IndexOf(node);
            if (i < 0) return inputRect;
            var coords = Grid.IndexToCoordinates(i); // ShapeMath.TransformIndexToCoordinates(i, GridRows, GridColumns, true);
            var r = new Rect
            (
                startPos + new Vector2(gapSize.X * coords.Col, 0f) + new Vector2(0f, gapSize.Y * coords.Row), 
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
        newChild.OnVisibleInHierarchyChanged += OnChildVisibleInHierarchyChanged;
        newChild.OnNavigated += OnChildNavigated;
        // newChild.OnVisibleChanged += OnChildVisibleChanged;
        // newChild.OnParentVisibleChanged += OnChildParentVisibleChanged;
    }
    protected override void ChildWasRemoved(ControlNode oldChild)
    {
        dirty = true;
        oldChild.OnSelectedChanged -= OnChildSelectionChanged;
        oldChild.OnVisibleInHierarchyChanged -= OnChildVisibleInHierarchyChanged;
        oldChild.OnNavigated -= OnChildNavigated;
        // oldChild.OnVisibleChanged -= OnChildVisibleChanged;
        // oldChild.OnParentVisibleChanged -= OnChildParentVisibleChanged;
        // oldChild.SetDisplayed(this, true);
        oldChild.Displayed = true;

    }

    #endregion
    
    #region Virtual
    protected virtual void OnChildNavigated(ControlNode child, Direction dir)
    {
        if (NavigationStep == 0) return;

        if (Type == ContainerType.None) return;
        if (Type == ContainerType.Grid)
        {
            if (!Grid.IsValid) return;
            var index = DisplayedChildren.IndexOf(child);
            var coords = Grid.IndexToCoordinates(index);
            var coordsDir = Grid.GetDirection(coords);
            if ((coordsDir.Vertical != 0 && coordsDir.Vertical == dir.Vertical) || (coordsDir.Horizontal != 0 && coordsDir.Horizontal == dir.Horizontal))
            {
                if(dir.IsLeft || dir.IsUp || dir.IsUpLeft || dir.IsUpRight) DisplayIndex -= NavigationStep;
                else DisplayIndex += NavigationStep;
            }
        }
        else
        {
            if (IsFirst(child))
            {
                if(dir.IsLeft || dir.IsUp || dir.IsUpLeft || dir.IsUpRight)
                {
                    DisplayIndex -= NavigationStep;
                }
            }
            else if (IsLast(child))
            {
                if(dir.IsRight || dir.IsDown || dir.IsDownRight || dir.IsDownLeft)
                {
                    DisplayIndex += NavigationStep;
                }
            }
        }
    }

    protected virtual void FirstNodeWasSelected(ControlNode node) { }
    protected virtual void LastNodeWasSelected(ControlNode node) { }
    protected virtual void NodeWasSelected(ControlNode node) { }
    protected virtual bool IsFirst(ControlNode node)
    {
        if (DisplayedChildrenCount <= 0) return false;
        return DisplayedChildren[0] == node;
    }
    protected virtual bool IsLast(ControlNode node)
    {
        if (DisplayedChildrenCount <= 0) return false;
        return DisplayedChildren[^1] == node;
    }
    #endregion

    #region Public
    public void NextItem() => DisplayIndex += 1;
    public void PreviousItem() => DisplayIndex -= 1;

    public void NextPage()
    {
        if (displayCount <= 0) return;
        
        DisplayIndex += displayCount;
    }
    public void PrevPage()
    {
        if (displayCount <= 0) return;
        DisplayIndex -= displayCount;
    }
    public void MovePage(int pages)
    {
        if (pages == 0) return;
        if (displayCount <= 0) return;
        DisplayIndex += displayCount * pages;
    }

    public void FirstPage() => DisplayIndex = 0;
    public void LastPage() => DisplayIndex = ChildCount - displayCount;

    #endregion
    
    #region Private
    private void CompileDisplayedNodes()
    {
        dirty = false;
        DisplayedChildren ??= new();
        DisplayedChildren.Clear();

        var visibleChildren = GetChildren((node => node.Visible && node.ParentVisible && node.IsActiveInHierarchy));

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
            if (DisplayCount == 0) child.Displayed = false;
            else if (DisplayCount > 0)
            {
                if (i < DisplayIndex) child.Displayed = false;
                else if (i >= DisplayIndex + DisplayCount) child.Displayed = false;
                else
                {
                    DisplayedChildren.Add(child);
                    child.Displayed = true;
                }

            }
            else
            {
                child.Displayed = true;
                DisplayedChildren.Add(child);
            }
        }
    }
    // private void OnChildVisibleChanged(ControlNode child, bool visible)
    // {
    //     dirty = true;
    // }
    // private void OnChildParentVisibleChanged(ControlNode child, bool parentVisible)
    // {
    //     dirty = true;
    // }

    private void OnChildVisibleInHierarchyChanged(ControlNode node, bool value)
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

        
        // if (displayCount >= 0 && type is ContainerType.Horizontal or ContainerType.Vertical)
        // {
        //     DisplayIndex -= 1;
        // }
    }
    private void ResolveOnLastNodeSelected(ControlNode node)
    {
        LastNodeWasSelected(node);
        OnLastNodeSelected?.Invoke(this, node);

        // if (displayCount >= 0 && type is ContainerType.Horizontal or ContainerType.Vertical)
        // {
        //     DisplayIndex += 1;
        // }
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
