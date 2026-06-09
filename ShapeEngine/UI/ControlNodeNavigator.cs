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

    private Direction currentRepeatDir = new();
    private float repeatTimer = 0f;

    #endregion

    #region Getter & Setter

    /// <summary>
    /// Gets or sets whether navigation repeat is enabled.
    /// </summary>
    public bool NavigationRepeatEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the delay before navigation starts repeating when a direction is held.
    /// </summary>
    public float NavigationRepeatDelay { get; set; } = 0.35f;

    /// <summary>
    /// Gets or sets the interval between navigation repeats when a direction is held.
    /// </summary>
    public float NavigationRepeatInterval { get; set; } = 0.1f;

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

        currentRepeatDir = new();
        repeatTimer = 0f;

        ResolveOnSelectedControlNodeChanged(prev, selectedNode);
    }

    /// <summary>
    /// Gets whether navigation is currently active.
    /// </summary>
    public bool IsNavigating { get; private set; }
    
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
        if (!CanNavigate() || !grid.IsValid) return;
        var dir = grid.GetNextDirection();
        if (grid.IsGrid)
        {
            var next = GetNextNode();
            if (next == null || next == selectedNode) return;
            nextNodeToSelect = next;
        }
        // selectedNode!.NavigatedTo(dir);
        navigationPending = true;
        prevDir = dir;
    }

    /// <summary>
    /// Selects the previous control node in the navigation order, using the provided grid.
    /// </summary>
    /// <param name="grid">The grid describing navigation layout and direction.</param>
    public void SelectPrevious(Grid grid)
    {
        if (!CanNavigate() || !grid.IsValid) return;
        var dir = grid.GetPreviousDirection();
        if (grid.IsGrid)
        {
            var prev = GetPrevNode();
            if (prev == null || prev == selectedNode) return;
            nextNodeToSelect = prev;
        }
        // selectedNode!.NavigatedTo(dir);
        navigationPending = true;
        prevDir = dir;
    }

    /// <summary>
    /// Updates the navigation state, handling pending navigation and selection changes.
    /// </summary>
    /// <remarks> This function is deprecated and will be removed in a future update!
    /// Use <see cref="Update(float)"/> instead.</remarks>
    public void Update() => Update(0f);

    /// <summary>
    /// Updates the navigation state, handling pending navigation and selection changes.
    /// </summary>
    /// <param name="dt">The delta time since the last update.</param>
    public void Update(float dt)
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
            var nextNode = nextNodeToSelect;
            nextNodeToSelect = null;

            if (TryNavigate(dir, nextNode))
            {
                currentRepeatDir = dir;
                repeatTimer = NavigationRepeatDelay;
            }
        }
        else
        {
            var dir = selectedNode.GetNavigationDirection();
            if (dir.IsValid)
            {
                if (NavigationRepeatEnabled)
                {
                    if (dir != currentRepeatDir)
                    {
                        if (TryNavigate(dir))
                        {
                            currentRepeatDir = dir;
                            repeatTimer = NavigationRepeatDelay;
                        }
                    }
                    else
                    {
                        repeatTimer -= dt;
                        if (repeatTimer <= 0)
                        {
                            if (TryNavigate(dir))
                            {
                                currentRepeatDir = dir;
                                repeatTimer = NavigationRepeatInterval;
                            }
                        }
                    }
                }
                else
                {
                    TryNavigate(dir);
                }
            }
            else
            {
                currentRepeatDir = new();
                repeatTimer = 0f;
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
    
    private bool TryNavigate(Direction dir, ControlNode? explicitNextNode = null)
    {
        if (!IsNavigating || selectedNode == null || !dir.IsValid) return false;

        var currentNode = selectedNode;
        
        // Always notify the current node of the navigation attempt.
        // This is required for container scrolling to work even if focus doesn't change.
        currentNode.NavigatedTo(dir);
        
        var nextNode = explicitNextNode ?? GetNextNode(dir);

        if (nextNode != null && nextNode != currentNode && CheckNextNode(nextNode, dir))
        {
            currentNode.NavigationDeselect();

            SetSelectedNode(nextNode);

            if (selectedNode == null || !selectedNode.NavigationSelect())
            {
                throw new WarningException(
                    "Control Node Navigation Selected return false when it should have returned true!");
            }
   
            // Successfully navigated to a new node
            ResolveOnNavigated(dir);
            return true;
        }

        // No navigation occurred
        return false;
    }
    
    private bool CanNavigate()
    {
        return IsNavigating && selectedNode != null;
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
        if (index >= navigable.Count)
        {
            index = 0;
        }
        return navigable[index];
    }
    
    private ControlNode? GetPrevNode()
    {
        if (selectedNode == null) return null;
        var navigable = GetNavigableNodes();
        var index = navigable.IndexOf(selectedNode);
        if (index < 0) return null;

        index -= 1;
        if (index < 0)
        {
            index = navigable.Count - 1;
        }
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
        node.OnPressedReleased += OnNodePressedReleased;
        
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
        node.OnPressedReleased -= OnNodePressedReleased;
        
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
    
    private void OnNodePressedReleased(ControlNode node)
    {
        // Only treat it as a "mouse click selects" when the mouse is actually over that node.
        // Keyboard/gamepad "accept" releases should keep normal navigation behavior.
        if (!node.MouseInside) return;

        // If the node can't be selected, do nothing.
        if (!node.IsActiveInHierarchy || !node.IsVisibleInHierarchy) return;
        if (node.SelectionFilter is SelectFilter.None or SelectFilter.Navigation) return;

        // Force transfer selection to the clicked/hovered node.
        // This intentionally overrides the "hover shouldn't steal navigation focus" rule in OnNodeSelectionChanged.
        if (selectedNode != null && selectedNode != node)
        {
            if (IsNavigating) selectedNode.NavigationDeselect();
            else selectedNode.Deselect();
        }

        SetSelectedNode(node);

        if (IsNavigating)
        {
            // Keep navigator state consistent: selected node should be navigation-selected.
            node.NavigationSelect();
        }
        else
        {
            node.Select();
        }
    }
    
    private void OnNodeSelectionChanged(ControlNode node, bool value)
    {
        if (!value) return;
        
        // Only react to nodes that are currently eligible for navigator focus
        if (!node.Navigable) return;
        
        // If we're not in navigation mode, don't enforce any "hover can't steal focus" rules.
        // (Mouse click selection is handled elsewhere via OnPressedReleased.)
        if (!IsNavigating) return;
        
        if (selectedNode == null)
        {
            SetSelectedNode(node);
            
            if (selectedNode == null || !selectedNode.NavigationSelect())
            {
                throw new WarningException(
                    "Control Node Navigation Selected returned false when it should have returned true!");
            }
            
            // node.NavigationSelect();
            return;
        }
        
        // If the currently navigation-selected node is also selected, do nothing.
        if (node == selectedNode) return;
        
        // Hover-selection should not steal navigation focus within the same container
        if (node.MouseInside && node.Parent == selectedNode.Parent)
        {
            node.Deselect();               // undo hover-based selection
            selectedNode.NavigationSelect(); // re-assert navigation selection state
            return;
        }
        
        // Otherwise, treat it as a legitimate navigation selection change
        selectedNode.NavigationDeselect();
        SetSelectedNode(node);
        
        if (selectedNode == null || !selectedNode.NavigationSelect())
        {
            throw new WarningException(
                "Control Node Navigation Selected returned false when it should have returned true!");
        }
        
        // if (node != selectedNode)
        // {
        //     //if navigation selection changed and a node is currently hovered by the mouse,
        //     //the mouse selected node will be ignored in favor of the current selected node (if both have the same parent)
        //     if (node.MouseInside && node.Parent == selectedNode.Parent)
        //     {
        //         node.Deselect();
        //         // SetSelectedNode(selectedNode);
        //         selectedNode.NavigationSelect();
        //         return;
        //     }
        //     
        //     selectedNode.Deselect();
        //     SetSelectedNode(node);
        //     selectedNode.NavigationSelect();
        // }
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