using System.ComponentModel;
using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.UI;

/// <summary>
/// Provides navigation and selection logic for a set of <see cref="ControlNode"/> instances, supporting keyboard/gamepad navigation and selection events.
/// </summary>
/// <remarks>
/// ControlNodeNavigator manages a set of control nodes, tracks the currently selected node, and handles navigation events and selection changes.
/// </remarks>
public class ControlNodeNavigator
{
    #region Events
    /// <summary>
    /// Occurs when navigation starts.
    /// </summary>
    public event Action<ControlNodeNavigator>? OnNavigationStarted;
    /// <summary>
    /// Occurs when navigation ends.
    /// </summary>
    public event Action<ControlNodeNavigator>? OnNavigationEnded;
    /// <summary>
    /// Occurs when a control node is added to the navigator.
    /// </summary>
    public event Action<ControlNodeNavigator, ControlNode>? OnControlNodeAdded;
    /// <summary>
    /// Occurs when a control node is removed from the navigator.
    /// </summary>
    public event Action<ControlNodeNavigator, ControlNode>? OnControlNodeRemoved;
    /// <summary>
    /// Occurs when the selected control node changes.
    /// </summary>
    public event Action<ControlNodeNavigator, ControlNode?, ControlNode?>? OnSelectedControlNodeChanged;
    /// <summary>
    /// Occurs when navigation happens in a direction.
    /// </summary>
    public event Action<ControlNodeNavigator, Direction>? OnNavigated;
    #endregion

    #region Private Members

    private readonly HashSet<ControlNode> nodes = new();
    private readonly List<ControlNode> navigableNodes = new();
    private bool dirty;
    private ControlNode? selectedNode;

    #endregion

    #region Getter & Setter

    /// <summary>
    /// Gets the currently selected control node.
    /// </summary>
    public ControlNode? SelectedNode => selectedNode;

    /// <summary>
    /// Sets the selected control node and triggers selection change events.
    /// </summary>
    /// <param name="newNode">The new node to select.</param>
    private void SetSelectedNode(ControlNode? newNode)
    {
        if (newNode == null && selectedNode == null) return;
        if (selectedNode == newNode) return;
        var prev = selectedNode;
        selectedNode = newNode;
        ResolveOnSelectedControlNodeChanged(prev, selectedNode);
    }

    /// <summary>
    /// Gets whether navigation is currently active.
    /// </summary>
    public bool IsNavigating { get; private set; } = false;

    #endregion

    #region Public
    /// <summary>
    /// Starts navigation mode, enabling selection and navigation of control nodes.
    /// </summary>
    public void StartNavigation()
    {
        if (IsNavigating) return;
        IsNavigating = true;
        selectedNode?.NavigationSelect();
        ResolveOnNavigationStarted();
    }
    /// <summary>
    /// Ends navigation mode, disabling selection and navigation of control nodes.
    /// </summary>
    public void EndNavigation()
    {
        if (!IsNavigating) return;
        IsNavigating = false;
        selectedNode?.NavigationDeselect();
        ResolveOnNavigationEnded();
    }

    /// <summary>
    /// Removes all control nodes from the navigator.
    /// </summary>
    public void Clear()
    {
        var nodesToRemove = nodes.ToList();
        foreach (var node in nodesToRemove)
        {
            RemoveNode(node);
        }
    }
    /// <summary>
    /// Adds a control node to the navigator.
    /// </summary>
    /// <param name="node">The control node to add.</param>
    /// <returns>True if the node was added; otherwise, false.</returns>
    public bool AddNode(ControlNode node)
    {
        if (!nodes.Add(node)) return false;
        dirty = true;
        HandleNodeAddition(node);
        return true;
    }
    /// <summary>
    /// Removes a control node from the navigator.
    /// </summary>
    /// <param name="node">The control node to remove.</param>
    /// <returns>True if the node was removed; otherwise, false.</returns>
    public bool RemoveNode(ControlNode node)
    {
        if (!nodes.Remove(node)) return false;
        dirty = true;
        HandleNodeRemoval(node);
        return true;
    }

    private bool navigationPending;
    private Direction prevDir = new();
    private ControlNode? nextNodeToSelect;

    /// <summary>
    /// Selects the next control node in the navigation order, using the provided grid.
    /// </summary>
    /// <param name="grid">The grid describing navigation layout and direction.</param>
    public void SelectNext(Grid grid)
    {
        if (!IsNavigating || selectedNode == null || !grid.IsValid) return;
        var dir = grid.GetNextDirection();
        if (grid.IsGrid)
        {
            var next = GetNextNode();
            if (next == null || next == selectedNode) return;
            nextNodeToSelect = next;
        }
        selectedNode.NavigatedTo(dir);
        navigationPending = true;
        prevDir = dir;
    }

    /// <summary>
    /// Selects the previous control node in the navigation order, using the provided grid.
    /// </summary>
    /// <param name="grid">The grid describing navigation layout and direction.</param>
    public void SelectPrevious(Grid grid)
    {
        if (!IsNavigating || selectedNode == null || !grid.IsValid) return;
        var dir = grid.GetPreviousDirection();
        if (grid.IsGrid)
        {
            var prev = GetPrevNode();
            if (prev == null || prev == selectedNode) return;
            nextNodeToSelect = prev;
        }
        selectedNode.NavigatedTo(dir);
        navigationPending = true;
        prevDir = dir;
    }

    /// <summary>
    /// Updates the navigation state, handling pending navigation and selection changes.
    /// </summary>
    public void Update()
    {
        if (!IsNavigating) return;
        if (selectedNode == null)
        {
            var navigable = GetNavigableNodes();
            if (navigable.Count > 0)
            {
                SetSelectedNode(navigable[0]);
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
            var nextNode = nextNodeToSelect ?? GetNextNode(dir);
            nextNodeToSelect = null;
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
    private static Direction GetDirection(ControlNode from, ControlNode to)
    {
        var dir = to.Rect.TopLeft - from.Rect.TopLeft;
    
        if (dir.X == 0)
        {
            return new Direction(0, MathF.Sign(dir.Y));
        }
        else if (dir.Y == 0)
        {
            return new Direction(MathF.Sign(dir.X),0);
        }
        else
        {
            return MathF.Abs(dir.X) < MathF.Abs(dir.Y) ? new Direction(MathF.Sign(dir.X), 0) : new Direction(0, MathF.Sign(dir.Y));
        }
    }
    
    
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
    private ControlNode? GetNextNode()
    {
        if (selectedNode == null) return null;
        var navigable = GetNavigableNodes();
        var index = navigable.IndexOf(selectedNode);
        if (index < 0) return null;

        index += 1;
        if (index >= navigable.Count) index = 0;
        return navigable[index];
    }
    private ControlNode? GetPrevNode()
    {
        if (selectedNode == null) return null;
        var navigable = GetNavigableNodes();
        var index = navigable.IndexOf(selectedNode);
        if (index < 0) return null;

        index -= 1;
        if (index < 0) index = navigable.Count - 1;
        return navigable[index];
    }
    private ControlNode? GetNextNode(Direction dir)
    {
        if (!dir.IsValid) return null;
        if (selectedNode == null) return null;
        var navigable = GetNavigableNodes();
        if (navigable.Count <= 0) return null;

        var minDisSq = float.MaxValue;
        var origin = selectedNode.GetNavigationOrigin(dir.Invert());
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

    /// <summary>
    /// Determines whether the next node can be navigated to in the given direction.
    /// </summary>
    /// <remarks>
    /// Override this method to implement custom logic.
    /// </remarks>
    /// <param name="nextNode">The next <see cref="ControlNode"/> to navigate to.</param>
    /// <param name="dir">The navigation <see cref="Direction"/>.</param>
    /// <returns>True if navigation is allowed; otherwise, false.</returns>
    protected virtual bool CheckNextNode(ControlNode nextNode, Direction dir) => true;

    /// <summary>
    /// Called after navigation occurs in a given direction.
    /// </summary>
    /// <remarks>
    /// Override this method to implement custom logic.
    /// </remarks>
    /// <param name="dir">The navigation <see cref="Direction"/>.</param>
    protected virtual void WasNavigated(Direction dir) { }

    /// <summary>
    /// Called when navigation mode is started.
    /// </summary>
    /// <remarks>
    /// Override this method to implement custom logic.
    /// </remarks>
    protected virtual void NavigationWasStarted() { }

    /// <summary>
    /// Called when navigation mode is ended.
    /// </summary>
    /// <remarks>
    /// Override this method to implement custom logic.
    /// </remarks>
    protected virtual void NavigationWasEnded() { }

    /// <summary>
    /// Called when a control node is added to the navigator.
    /// </summary>
    /// <remarks>
    /// Override this method to implement custom logic.
    /// </remarks>
    /// <param name="node">The <see cref="ControlNode"/> that was added.</param>
    protected virtual void ControlNodeWasAdded(ControlNode node) { }

    /// <summary>
    /// Called when a control node is removed from the navigator.
    /// </summary>
    /// <remarks>
    /// Override this method to implement custom logic.
    /// </remarks>
    /// <param name="node">The <see cref="ControlNode"/> that was removed.</param>
    protected virtual void ControlNodeWasRemoved(ControlNode node) { }

    /// <summary>
    /// Called when the selected control node changes.
    /// </summary>
    /// <remarks>
    /// Override this method to implement custom logic.
    /// </remarks>
    /// <param name="prev">The previously selected <see cref="ControlNode"/>.</param>
    /// <param name="cur">The currently selected <see cref="ControlNode"/>.</param>
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