using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Pathfinding;

/// <summary>
/// A grid based pathfinder that uses A* to find paths for agents.
/// </summary>
public class Pathfinder
{
    /// <summary>
    /// Invoked when the bounds of the pathfinder are changed and the pathfinder needs to be regenerated.
    /// </summary>
    public event Action<Pathfinder>? OnRegenerationRequested;
    /// <summary>
    /// Invoked when the pathfinder is cleared.
    /// </summary>
    public event Action<Pathfinder>? OnCleared;

    #region Members

    #region Public
    /// <summary>
    /// The size of each cell in the grid.
    /// </summary>
    public Size CellSize { get; private set; }
    /// <summary>
    /// The grid used by the pathfinder.
    /// </summary>
    public readonly Grid Grid;
    #endregion

    #region Private
    private Rect bounds;
    private readonly List<GridNode> nodes;
    private HashSet<GridNode> nodeHelper1 = new(1024);
    private HashSet<GridNode> nodeHelper2 = new(1024);
    private HashSet<GridNode> resultSet = new(1024);
    
    /// <summary>
    /// How many agent requests are handled each frame.
    /// If the value is smaller or equal to zero, all incoming requests are handled.
    /// </summary>
    public int RequestsPerFrame = 25;
    private readonly List<PathRequest> pathRequests = new(256);
    private readonly Dictionary<IPathfinderAgent, PathRequest?> agents = new(1024);
    // private readonly Dictionary<IPathfinderObstacle, List<GridNode>> obstacles = new(256);
    
    #endregion

    #region Getters & Setters

    /// <summary>
    /// The bounds of the pathfinder.
    /// If the bounds are changed, the pathfinder is regenerated.
    /// </summary>
    public Rect Bounds
    {
        get => bounds;
        set
        {
            if (value.Size.Area <= 0) return;
            if (value == bounds) return;
            bounds = value;
            CellSize = Grid.GetCellSize(Bounds);
            ResolveRegenerationRequested();
        }
    }

    #endregion
    
    #endregion

    /// <summary>
    /// Creates a new pathfinder.
    /// </summary>
    /// <param name="bounds">The bounds of the pathfinder.</param>
    /// <param name="cols">The amount of columns in the grid.</param>
    /// <param name="rows">The amount of rows in the grid.</param>
    public Pathfinder(Rect bounds, int cols, int rows)
    {
        this.bounds = bounds;
        Grid = new(cols, rows);
        CellSize = Grid.GetCellSize(bounds);
        nodes = new(Grid.Count + 1);
        for (var i = 0; i < Grid.Count; i++)
        {
            var coordinates = Grid.IndexToCoordinates(i);
            var node = new GridNode(coordinates, this);
            nodes.Add(node);
            // GScores.Add(0);
            // FScores.Add(0);
        }
        
        for (var i = 0; i < Grid.Count; i++)
        {
            var node = nodes[i];
            var neighbors = GetNeighbors(node);
            node.SetNeighbors(neighbors);
        }
        
    }

    
    #region Public

    /// <summary>
    /// Resizes the bounds of the pathfinder.
    /// </summary>
    /// <param name="newBounds">The new bounds.</param>
    public void ResizeBounds(Rect newBounds)
    {
        bounds = newBounds;
        CellSize = Grid.GetCellSize(bounds);
    }
    /// <summary>
    /// Updates the pathfinder.
    /// </summary>
    /// <param name="dt">The delta time.</param>
    public void Update(float dt)
    {
        // HandleObstacles();
        HandlePathRequests();
    }

    /// <summary>
    /// Clears the pathfinder.
    /// </summary>
    public void Clear()
    {
        // ClearObstacles();
        ClearAgents();
        pathRequests.Clear();
        ResetNodes();
        ResolveClear();
    }
    /// <summary>
    /// Resets all nodes in the pathfinder.
    /// </summary>
    public void ResetNodes()
    {
        foreach (var node in nodes)
        {
            node.Reset();
        }
    }
    /// <summary>
    /// Resets all nodes in the given rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to reset the nodes in.</param>
    public void ResetNodes(Rect rect)
    {
        nodeHelper1.Clear();
        var count = GetNodes(rect, ref nodeHelper1);
        if (count <= 0) return;
        foreach (var node in nodeHelper1)
        {
            node.Reset();
        }
    }

    /// <summary>
    /// Checks if the given position is inside the pathfinder bounds.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>True if the position is inside the bounds, false otherwise.</returns>
    public bool IsInside(Vector2 position) => Bounds.ContainsPoint(position);
    /// <summary>
    /// Checks if the given index is valid.
    /// </summary>
    /// <param name="index">The index to check.</param>
    /// <returns>True if the index is valid, false otherwise.</returns>
    public bool IsIndexValid(int index) => Grid.IsIndexInBounds(index);
    /// <summary>
    /// Gets the index of the cell at the given position.
    /// </summary>
    /// <param name="position">The position to get the index for.</param>
    /// <returns>The index of the cell at the given position.</returns>
    public int GetIndex(Vector2 position) => Grid.GetCellIndex(position, Bounds);

    /// <summary>
    /// Gets the index of the cell at the given position without clamping the position to the bounds.
    /// </summary>
    /// <param name="position">The position to get the index for.</param>
    /// <returns>The index of the cell at the given position.</returns>
    public int GetIndexUnclamped(Vector2 position) => Grid.GetCellIndexUnclamped(position, bounds);
    
    /// <summary>
    /// Gets the rectangle of the cell at the given position.
    /// </summary>
    /// <param name="position">The position to get the rectangle for.</param>
    /// <returns>The rectangle of the cell at the given position.</returns>
    public Rect GetRect(Vector2 position) => GetRect(GetIndex(position));
    /// <summary>
    /// Gets the rectangle of the cell at the given index.
    /// </summary>
    /// <param name="index">The index to get the rectangle for.</param>
    /// <returns>The rectangle of the cell at the given index.</returns>
    public Rect GetRect(int index)
    {
        var coordinates = Grid.IndexToCoordinates(index);
        var pos = Bounds.TopLeft + CellSize * coordinates.ToVector2();
        return new Rect(pos, CellSize, new (0f));
    }

    
    /// <summary>
    /// Gets the weight of the cell at the given index.
    /// </summary>
    /// <param name="index">The index of the cell.</param>
    /// <returns>The weight of the cell.</returns>
    public float GetWeight(int index) => GetNode(index)?.GetWeight() ?? 1;
    /// <summary>
    /// Gets the weight of the cell at the given index for the given layer.
    /// </summary>
    /// <param name="index">The index of the cell.</param>
    /// <param name="layer">The layer to get the weight for.</param>
    /// <returns>The weight of the cell for the given layer.</returns>
    public float GetWeight(int index, uint layer) => GetNode(index)?.GetWeight(layer) ?? 1;
    /// <summary>
    /// Checks if the cell at the given index is traversable.
    /// </summary>
    /// <param name="index">The index of the cell.</param>
    /// <returns>True if the cell is traversable, false otherwise.</returns>
    public bool IsTraversable(int index) => GetNode(index)?.IsTraversable() ?? true;
    /// <summary>
    /// Checks if the cell at the given index is traversable for the given layer.
    /// </summary>
    /// <param name="index">The index of the cell.</param>
    /// <param name="layer">The layer to check.</param>
    /// <returns>True if the cell is traversable, false otherwise.</returns>
    public bool IsTraversable(int index, uint layer) => GetNode(index)?.IsTraversable(layer) ?? true;
    
    #endregion
    
    #region Agent System

    /// <summary>
    /// Adds an agent to the pathfinder.
    /// </summary>
    /// <param name="agent">The agent to add.</param>
    /// <returns>True if the agent was added, false otherwise.</returns>
    public bool AddAgent(IPathfinderAgent agent)
    {
        // if (!agents.Add(agent)) return false;
        if (!agents.TryAdd(agent, null)) return false;

        agent.OnRequestPath += OnAgentRequestedPath;
        agent.AddedToPathfinder(this);
        return true;
    }
    /// <summary>
    /// Removes an agent from the pathfinder.
    /// </summary>
    /// <param name="agent">The agent to remove.</param>
    /// <returns>True if the agent was removed, false otherwise.</returns>
    public bool RemoveAgent(IPathfinderAgent agent)
    {
        // if (!agents.Remove(agent)) return false;
        if (!agents.Remove(agent, out var request)) return false;
        if(request != null) pathRequests.Remove(request);
        
        agent.OnRequestPath -= OnAgentRequestedPath;
        agent.RemovedFromPathfinder();
        return true;
    }

    /// <summary>
    /// Clears all agents from the pathfinder.
    /// </summary>
    public void ClearAgents()
    {
        pathRequests.Clear();
        var allAgents = this.agents.Keys;
        foreach (var agent in allAgents)
        {
            agent.OnRequestPath -= OnAgentRequestedPath;
            agent.RemovedFromPathfinder();
        }
        agents.Clear();
    }
    private void OnAgentRequestedPath(PathRequest request)
    {
        if (request.Agent == null) return;
        
        var prevRequest = agents[request.Agent];
        if(prevRequest != null) pathRequests.Remove(prevRequest);
        agents[request.Agent] = request;
        pathRequests.Add(request);
    }

    private void HandlePathRequests()
    {
        if (pathRequests.Count <= 0) return;
        
        if (RequestsPerFrame > 0 && pathRequests.Count > RequestsPerFrame)
        {
            //sorted from highest priority to lowest in ascending order (highest is at the end for easier removing)
            pathRequests.Sort((a, b) => a.Priority - b.Priority); 
        }

        int requests = 0;
        while (pathRequests.Count > 0 && (RequestsPerFrame <= 0 || requests <= RequestsPerFrame))
        {
            var lastIndex = pathRequests.Count - 1;
            var request = pathRequests[lastIndex];
            pathRequests.RemoveAt(lastIndex);
            if (request.Agent == null) continue; //should not happen
            agents[request.Agent] = null; //handled and therefore cleared
            requests ++;
            var path = GetPath(request.Start, request.End, request.Agent.GetLayer()); //GetPath(request.Start, request.End, request.Agent.GetLayer());
            if(path == null) continue;
            request.Agent.ReceiveRequestedPath(path, request);
        }
        pathRequests.Clear();
        
        // int endIndex = RequestsPerFrame > 0 ? ShapeMath.MaxInt(pathRequests.Count - RequestsPerFrame, 0) : 0;
        //
        // for (int i = pathRequests.Count - 1; i >= endIndex; i--)
        // {
        //     var request = pathRequests[i];
        //     // pathRequests.RemoveAt(i);
        //     if (request.Agent == null) continue; //should not happen
        //     agents[request.Agent] = null; //handled and therefor cleared
        //     var path = GetPath(request.Start, request.End, request.Agent.GetLayer());
        //     if(path == null) continue;
        //     request.Agent.ReceiveRequestedPath(path, request);
        // }
        // pathRequests.Clear();
        
        
        
    }

    /// <summary>
    /// Gets a path from the start to the end position for the given layer.
    /// </summary>
    /// <param name="start">The start position.</param>
    /// <param name="end">The end position.</param>
    /// <param name="layer">The layer to get the path for.</param>
    /// <returns>The path from start to end, or null if no path was found.</returns>
    public Path? GetPath(Vector2 start, Vector2 end, uint layer)
    {
        var startNode = GetNodeClamped(start);
        
        if (!startNode.IsTraversable())
        {
            var newStartNode = startNode.GetClosestTraversableCell();
            if (newStartNode is GridNode rn) startNode = rn;
            else return null;
        }
        
        var endNode = GetNodeClamped(end);
        if (!endNode.IsTraversable())
        {
            var newEndNode = endNode.GetClosestTraversableCell();
            if (newEndNode is GridNode rn) endNode = rn;
            else return null;
        }

        return AStar.GetPath(startNode, endNode, layer);
    }
    
    #endregion
    
    #region Connections

    /// <summary>
    /// Adds a connection between two nodes.
    /// </summary>
    /// <param name="a">The position of the first node.</param>
    /// <param name="b">The position of the second node.</param>
    /// <param name="oneWay">If the connection is one way.</param>
    /// <returns>True if the connection was added, false otherwise.</returns>
    public bool AddConnections(Vector2 a, Vector2 b, bool oneWay)
    {
        var cellA = GetNode(a);
        if (cellA == null) return false;
        
        var cellB = GetNode(b);
        if (cellB == null) return false;
        
        return ConnectNodes(cellA, cellB, oneWay);
    }
    /// <summary>
    /// Adds connections between a node and all nodes in a rectangle.
    /// </summary>
    /// <param name="a">The position of the node.</param>
    /// <param name="b">The rectangle.</param>
    /// <param name="oneWay">If the connections are one way.</param>
    public void AddConnections(Vector2 a, Rect b, bool oneWay)
    {
        var cellA = GetNode(a);
        if (cellA == null) return;
        
        nodeHelper1.Clear();
        if (GetNodes(b, ref nodeHelper1) <= 0) return;
        
        foreach (var node in nodeHelper1)
        {
            ConnectNodes(cellA, node, oneWay);
        }

    }
    /// <summary>
    /// Adds connections between all nodes in a rectangle and a node.
    /// </summary>
    /// <param name="a">The rectangle.</param>
    /// <param name="b">The position of the node.</param>
    /// <param name="oneWay">If the connections are one way.</param>
    public void AddConnections(Rect a, Vector2 b, bool oneWay)
    {
        nodeHelper1.Clear();
        if (GetNodes(a, ref nodeHelper2) <= 0) return;
        var cellB = GetNode(b);
        if (cellB == null) return;
        foreach (var node in nodeHelper1)
        {
            ConnectNodes(node, cellB, oneWay);
        }
    }
    /// <summary>
    /// Adds connections between all nodes in two rectangles.
    /// </summary>
    /// <param name="a">The first rectangle.</param>
    /// <param name="b">The second rectangle.</param>
    /// <param name="oneWay">If the connections are one way.</param>
    public void AddConnections(Rect a, Rect b, bool oneWay)
    {
        nodeHelper1.Clear();
        if (GetNodes(a, ref nodeHelper1) <= 0) return;
        
        nodeHelper2.Clear();
        if (GetNodes(b, ref nodeHelper2) <= 0) return;
        
        foreach (var cellA in nodeHelper1)
        {
            foreach (var cellB in nodeHelper2)
            {
                ConnectNodes(cellA, cellB, oneWay);
            }
        }
    }
    /// <summary>
    /// Removes a connection between two nodes.
    /// </summary>
    /// <param name="a">The position of the first node.</param>
    /// <param name="b">The position of the second node.</param>
    /// <param name="oneWay">If the connection is one way.</param>
    /// <returns>True if the connection was removed, false otherwise.</returns>
    public bool RemoveConnections(Vector2 a, Vector2 b, bool oneWay)
    {
        var cellA = GetNode(a);
        if (cellA == null) return false;
        
        var cellB = GetNode(b);
        if (cellB == null) return false;
        
        return DisconnectNodes(cellA, cellB, oneWay);
    }
    /// <summary>
    /// Removes connections between a node and all nodes in a rectangle.
    /// </summary>
    /// <param name="a">The position of the node.</param>
    /// <param name="b">The rectangle.</param>
    /// <param name="oneWay">If the connections are one way.</param>
    public void RemoveConnections(Vector2 a, Rect b, bool oneWay)
    {
        var cellA = GetNode(a);
        if (cellA == null) return;
        
        nodeHelper1.Clear();
        if (GetNodes(b, ref nodeHelper1) <= 0) return;
        
        foreach (var node in nodeHelper1)
        {
            DisconnectNodes(cellA, node, oneWay);
        }

    }
    /// <summary>
    /// Removes connections between all nodes in a rectangle and a node.
    /// </summary>
    /// <param name="a">The rectangle.</param>
    /// <param name="b">The position of the node.</param>
    /// <param name="oneWay">If the connections are one way.</param>
    public void RemoveConnections(Rect a, Vector2 b, bool oneWay)
    {
        var cellB = GetNode(b);
        if (cellB == null) return;
        
        nodeHelper1.Clear();
        if (GetNodes(a, ref nodeHelper2) <= 0) return;
        
        foreach (var node in nodeHelper1)
        {
            DisconnectNodes(node, cellB, oneWay);
        }
    }
    /// <summary>
    /// Removes connections between all nodes in two rectangles.
    /// </summary>
    /// <param name="a">The first rectangle.</param>
    /// <param name="b">The second rectangle.</param>
    /// <param name="oneWay">If the connections are one way.</param>
    public void RemoveConnections(Rect a, Rect b, bool oneWay)
    {
        nodeHelper1.Clear();
        if (GetNodes(a, ref nodeHelper1) <= 0) return;
        
        nodeHelper2.Clear();
        if (GetNodes(b, ref nodeHelper2) <= 0) return;
        
        foreach (var cellA in nodeHelper1)
        {
            foreach (var cellB in nodeHelper2)
            {
                DisconnectNodes(cellA, cellB, oneWay);
            }
        }
    }
    #endregion
    
    #region Set Node Values
    
    #region Index
    
    /// <summary>
    /// Applies a node value to the node at the given index.
    /// </summary>
    /// <param name="index">The index of the node.</param>
    /// <param name="value">The node value to apply.</param>
    /// <returns>True if the node value was applied, false otherwise.</returns>
    public bool ApplyNodeValue(int index, NodeValue value)
    {
        var node = GetNode(index);
        if (node == null) return false;
        node.ApplyNodeValue(value);
        return true;
    }
    /// <summary>
    /// Applies multiple node values to the node at the given index.
    /// </summary>
    /// <param name="index">The index of the node.</param>
    /// <param name="values">The node values to apply.</param>
    /// <returns>True if the node values were applied, false otherwise.</returns>
    public bool ApplyNodeValues(int index, params NodeValue[] values)
    {
        var node = GetNode(index);
        if (node == null) return false;
        node.ApplyNodeValues(values);
        return true;
    }
    #endregion
    
    #region Position
    
    /// <summary>
    /// Applies a node value to the node at the given position.
    /// </summary>
    /// <param name="position">The position of the node.</param>
    /// <param name="value">The node value to apply.</param>
    /// <returns>True if the node value was applied, false otherwise.</returns>
    public bool ApplyNodeValue(Vector2 position, NodeValue value)
    {
        var node = GetNode(position);
        if (node == null) return false;
        node.ApplyNodeValue(value);
        return true;
    }
    /// <summary>
    /// Applies multiple node values to the node at the given position.
    /// </summary>
    /// <param name="position">The position of the node.</param>
    /// <param name="values">The node values to apply.</param>
    /// <returns>True if the node values were applied, false otherwise.</returns>
    public bool ApplyNodeValues(Vector2 position, IEnumerable<NodeValue> values)
    {
        var node = GetNode(position);
        if (node == null) return false;
        node.ApplyNodeValues(values);
        return true;
    }
    
    #endregion
    
    #region Segment
    /// <summary>
    /// Applies a node value to all nodes that intersect the given segment.
    /// </summary>
    /// <param name="shape">The segment.</param>
    /// <param name="value">The node value to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValue(Segment shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValue(value);
        }
    
        return nodeCount;
    }
    /// <summary>
    /// Applies multiple node values to all nodes that intersect the given segment.
    /// </summary>
    /// <param name="shape">The segment.</param>
    /// <param name="values">The node values to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValues(Segment shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValues(values);
        }
    
        return nodeCount;
    }
    
    #endregion
    
    #region Circle
    /// <summary>
    /// Applies a node value to all nodes that intersect the given circle.
    /// </summary>
    /// <param name="shape">The circle.</param>
    /// <param name="value">The node value to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValue(Circle shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValue(value);
        }
    
        return nodeCount;
    }
    /// <summary>
    /// Applies multiple node values to all nodes that intersect the given circle.
    /// </summary>
    /// <param name="shape">The circle.</param>
    /// <param name="values">The node values to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValues(Circle shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValues(values);
        }
    
        return nodeCount;
    }
    
    #endregion
    
    #region Triangle
    /// <summary>
    /// Applies a node value to all nodes that intersect the given triangle.
    /// </summary>
    /// <param name="shape">The triangle.</param>
    /// <param name="value">The node value to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValue(Triangle shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValue(value);
        }
    
        return nodeCount;
    }
    /// <summary>
    /// Applies multiple node values to all nodes that intersect the given triangle.
    /// </summary>
    /// <param name="shape">The triangle.</param>
    /// <param name="values">The node values to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValues(Triangle shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValues(values);
        }
    
        return nodeCount;
    }
    
    #endregion
    
    #region Rect
    /// <summary>
    /// Applies a node value to all nodes that intersect the given rectangle.
    /// </summary>
    /// <param name="shape">The rectangle.</param>
    /// <param name="value">The node value to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValue(Rect shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValue(value);
        }
    
        return nodeCount;
    }
    
    /// <summary>
    /// Applies multiple node values to all nodes that intersect the given rectangle.
    /// </summary>
    /// <param name="shape">The rectangle.</param>
    /// <param name="values">The node values to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValues(Rect shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValues(values);
        }
    
        return nodeCount;
    }
    
    #endregion
    
    #region Quad
    /// <summary>
    /// Applies a node value to all nodes that intersect the given quad.
    /// </summary>
    /// <param name="shape">The quad.</param>
    /// <param name="value">The node value to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValue(Quad shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValue(value);
        }
    
        return nodeCount;
    }
    
    /// <summary>
    /// Applies multiple node values to all nodes that intersect the given quad.
    /// </summary>
    /// <param name="shape">The quad.</param>
    /// <param name="values">The node values to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValues(Quad shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValues(values);
        }
    
        return nodeCount;
    }
    
    #endregion
    
    #region Polygon
    /// <summary>
    /// Applies a node value to all nodes that intersect the given polygon.
    /// </summary>
    /// <param name="shape">The polygon.</param>
    /// <param name="value">The node value to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValue(Polygon shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValue(value);
        }
    
        return nodeCount;
    }
    
    /// <summary>
    /// Applies multiple node values to all nodes that intersect the given polygon.
    /// </summary>
    /// <param name="shape">The polygon.</param>
    /// <param name="values">The node values to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValues(Polygon shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValues(values);
        }
    
        return nodeCount;
    }
    
    #endregion
    
    #region Polyline
    /// <summary>
    /// Applies a node value to all nodes that intersect the given polyline.
    /// </summary>
    /// <param name="shape">The polyline.</param>
    /// <param name="value">The node value to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValue(Polyline shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValue(value);
        }
    
        return nodeCount;
    }
    
    /// <summary>
    /// Applies multiple node values to all nodes that intersect the given polyline.
    /// </summary>
    /// <param name="shape">The polyline.</param>
    /// <param name="values">The node values to apply.</param>
    /// <returns>The amount of nodes that were changed.</returns>
    public int ApplyNodeValues(Polyline shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.ApplyNodeValues(values);
        }
    
        return nodeCount;
    }
    
    #endregion
    #endregion
    
    #region Virtual
    /// <summary>
    /// Called when the pathfinder has been cleared. Override to implement custom behavior on clear.
    /// </summary>
    protected virtual void WasCleared(){ }
    /// <summary>
    /// Called when the pathfinder requests regeneration. Override to implement custom behavior on regeneration request.
    /// </summary>
    protected virtual void RegenerationWasRequested() { }
    #endregion

    #region Private
    
    
    #region GetCells

    private GridNode? GetNode(int index)
    {
        if (!Grid.IsIndexInBounds(index)) return null;
        return nodes[index];
    }
    private GridNode? GetNode(Coordinates coordinates)
    {
        if (!Grid.AreCoordinatesInside(coordinates)) return null;
        var index = Grid.CoordinatesToIndex(coordinates);
        if (index >= 0 && index < nodes.Count) return nodes[index];
        return null;
    }
    private GridNode? GetNode(Vector2 position)
    {
        var index = Grid.GetCellIndexUnclamped(position, bounds);
        if (!Grid.IsIndexInBounds(index)) return null;
        return nodes[index];
    }
    private GridNode GetNodeClamped(Vector2 position)
    {
        return nodes[Grid.GetCellIndex(position, bounds)];
    }
    private int GetNodes(Segment segment, ref HashSet<GridNode> result)
    {
        var rect = segment.GetBoundingBox();
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
        var count = result.Count;
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                var node = GetNode(index);
                if(node == null) continue;
                if(node.Rect.OverlapShape(segment)) result.Add(node);
            }
        }

        return result.Count - count;
    }
    private int GetNodes(Circle circle, ref HashSet<GridNode> result)
    {
        var rect = circle.GetBoundingBox();
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
        var count = result.Count;
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                var node = GetNode(index);
                if(node == null) continue;
                if(node.Rect.OverlapShape(circle)) result.Add(node);
            }
        }

        return result.Count - count;
    }
    private int GetNodes(Triangle triangle, ref HashSet<GridNode> result)
    {
        var rect = triangle.GetBoundingBox();
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
        var count = result.Count;
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                var node = GetNode(index);
                if(node == null) continue;
                if(node.Rect.OverlapShape(triangle)) result.Add(node);
            }
        }

        return result.Count - count;
    }
    private int GetNodes(Quad quad, ref HashSet<GridNode> result)
    {
        var rect = quad.GetBoundingBox();
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
        var count = result.Count;
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                var node = GetNode(index);
                if(node == null) continue;
                if(node.Rect.OverlapShape(quad)) result.Add(node);
            }
        }

        return result.Count - count;
    }
    private int GetNodes(Rect rect, ref HashSet<GridNode> result)
    {
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
        var count = result.Count;
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                result.Add(nodes[index]);
            }
        }

        return result.Count - count;
    }
    private int GetNodes(Polygon polygon, ref HashSet<GridNode> result)
    {
        var rect = polygon.GetBoundingBox();
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
        var count = result.Count;
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                var node = GetNode(index);
                if(node == null) continue;
                if(node.Rect.OverlapShape(polygon)) result.Add(node);
            }
        }

        return result.Count - count;
    }
    private int GetNodes(Polyline polyline, ref HashSet<GridNode> result)
    {
        var rect = polyline.GetBoundingBox();
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
        var count = result.Count;
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                var node = GetNode(index);
                if(node == null) continue;
                if(node.Rect.OverlapShape(polyline)) result.Add(node);
            }
        }

        return result.Count - count;
    }
    #endregion

    #region Path Helper
    private Node? GetNeighborNode(GridNode node, Direction dir) => GetNode(node.Coordinates + dir);
    private HashSet<Node>? GetNeighbors(GridNode node, bool diagonal = true)
    {
        HashSet<Node>? neighbors = null;
        var coordinates = node.Coordinates;

        Node? neighbor = GetNode(coordinates + Direction.Right);
        if (neighbor != null)
        {
            neighbors ??= new();
            neighbors.Add(neighbor);
        }
        neighbor = GetNode(coordinates + Direction.Left);
        if (neighbor != null)
        {
            neighbors ??= new();
            neighbors.Add(neighbor);
        }
        neighbor = GetNode(coordinates + Direction.Up);
        if (neighbor != null)
        {
            neighbors ??= new();
            neighbors.Add(neighbor);
        }
        neighbor = GetNode(coordinates + Direction.Down);
        if (neighbor != null)
        {
            neighbors ??= new();
            neighbors.Add(neighbor);
        }

        if (diagonal)
        {
            neighbor = GetNode(coordinates + Direction.UpLeft);
            if (neighbor != null)
            {
                neighbors ??= new();
                neighbors.Add(neighbor);
            }
            neighbor = GetNode(coordinates + Direction.UpRight);
            if (neighbor != null)
            {
                neighbors ??= new();
                neighbors.Add(neighbor);
            }
            neighbor = GetNode(coordinates + Direction.DownLeft);
            if (neighbor != null)
            {
                neighbors ??= new();
                neighbors.Add(neighbor);
            }
            neighbor = GetNode(coordinates + Direction.DownRight);
            if (neighbor != null)
            {
                neighbors ??= new();
                neighbors.Add(neighbor);
            }
        }
        
        return neighbors;
    }

    #endregion

    #region Connection Helper
    private bool ConnectNodes(Node a, Node b, bool oneWay)
    {
        if (a == b) return false;

        a.Connect(b);
        if (!oneWay) b.Connect(a);
        return true;
    }
    private bool DisconnectNodes(Node a, Node b, bool oneWay)
    {
        if (a == b) return false;
        a.Disconnect(b);
        if (oneWay) b.Disconnect(a);
        return true;
    }
    #endregion
    
    private void ResolveRegenerationRequested()
    {
        RegenerationWasRequested();
        OnRegenerationRequested?.Invoke(this);
    }
    private void ResolveClear()
    {
        WasCleared();
        OnCleared?.Invoke(this);
    }
    #endregion

    #region Debug
    /// <summary>
    /// Draws a debug view of the pathfinder.
    /// </summary>
    /// <param name="boundary">The color of the boundary.</param>
    /// <param name="standard">The color of standard nodes.</param>
    /// <param name="blocked">The color of blocked nodes.</param>
    /// <param name="desirable">The color of desirable nodes.</param>
    /// <param name="undesirable">The color of undesirable nodes.</param>
    /// <param name="layer">The layer to draw.</param>
    public void DrawDebug(ColorRgba boundary, ColorRgba standard, ColorRgba blocked, ColorRgba desirable, ColorRgba undesirable, uint layer)
    {
        Bounds.DrawLines(12f, boundary);
        foreach (var node in nodes)
        {
            var r = node.Rect;

            // if(closedSet.Contains(node)) //touched
            // {
            //     r.ScaleSize(0.9f, new Vector2(0.5f)).Draw(new ColorRgba(System.Drawing.Color.Bisque));
            // }
            //
            if(node.GetWeight(layer) == 0) r.ScaleSize(0.5f, new AnchorPoint(0.5f)).Draw(blocked);
            else if(node.GetWeight(layer) > 1) r.ScaleSize(0.65f, new AnchorPoint(0.5f)).Draw(undesirable);
            else if(node.GetWeight(layer) < 1) r.ScaleSize(0.65f, new AnchorPoint(0.5f)).Draw(desirable);
            else r.ScaleSize(0.8f, new AnchorPoint(0.5f)).Draw(standard);
        }
        
    }
    #endregion

    #region Review if needed

    /// <summary>
    /// Gets a list of all rectangles in the pathfinder.
    /// </summary>
    /// <param name="traversable">If only traversable nodes should be included.</param>
    /// <returns>A list of all rectangles in the pathfinder.</returns>
    public List<Rect> GetRects(bool traversable = true)
    {
        var rects = new List<Rect>();
        foreach (var node in nodes)
        {
            if (!traversable || node.IsTraversable())
            {
                rects.Add(node.Rect);
            }
        }

        return rects;
    }
    /// <summary>
    /// Gets a list of all rectangles in the pathfinder with a weight between the given values.
    /// </summary>
    /// <param name="layer">The layer to get the rectangles for.</param>
    /// <param name="minWeight">The minimum weight.</param>
    /// <param name="maxWeight">The maximum weight.</param>
    /// <returns>A list of all rectangles in the pathfinder with a weight between the given values.</returns>
    public List<Rect> GetRects(uint layer, int minWeight, int maxWeight)
    {
        var rects = new List<Rect>();
        foreach (var node in nodes)
        {
            var w = node.GetWeight(layer);
            if(w >= minWeight && w <= maxWeight) rects.Add(node.Rect);
        }

        return rects;
    }
    /// <summary>
    /// Gets a list of all rectangles in the given area.
    /// </summary>
    /// <param name="area">The area to get the rectangles from.</param>
    /// <param name="traversable">If only traversable nodes should be included.</param>
    /// <returns>A list of all rectangles in the given area.</returns>
    public List<Rect> GetRects(Rect area, bool traversable = true)
    {
        nodeHelper1.Clear();
        GetNodes(area, ref nodeHelper1);
        var result = new List<Rect>();
        foreach (var node in nodeHelper1)
        {
            if (!traversable || node.IsTraversable())
            {
                result.Add(node.GetRect());
            }
        }

        return result;
    }
    /// <summary>
    /// Gets a list of all rectangles in the given area with a weight between the given values.
    /// </summary>
    /// <param name="area">The area to get the rectangles from.</param>
    /// <param name="layer">The layer to get the rectangles for.</param>
    /// <param name="minWeight">The minimum weight.</param>
    /// <param name="maxWeight">The maximum weight.</param>
    /// <returns>A list of all rectangles in the given area with a weight between the given values.</returns>
    public List<Rect> GetRects(Rect area, uint layer, int minWeight, int maxWeight)
    {
        nodeHelper1.Clear();
        GetNodes(area, ref nodeHelper1);
        var result = new List<Rect>();
        foreach (var node in nodeHelper1)
        {
            var w = node.GetWeight(layer);
            if (w >= minWeight && w <= maxWeight)
            {
                result.Add(node.GetRect());
            }
        }

        return result;
    }

    #endregion
}