using System.ComponentModel;
using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.UI;

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
    private ControlNode? nextNodeToSelect = null;

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