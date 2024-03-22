using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Pathfinding;

public class Pathfinder
{

    public event Action<Pathfinder>? OnRegenerationRequested;
    public event Action<Pathfinder>? OnCleared;

    #region Members

    #region Public
    public Size CellSize { get; private set; }
    public readonly Grid Grid;
    #endregion

    #region Private
    private Rect bounds;
    private readonly List<GridNode> nodes;
    private HashSet<GridNode> nodeHelper1 = new(1024);
    private HashSet<GridNode> nodeHelper2 = new(1024);
    private HashSet<GridNode> resultSet = new(1024);
    
    /// <summary>
    /// How many agent requests are handled each frame
    /// smaller or equal to zero handles all incoming requests
    /// </summary>
    public int RequestsPerFrame = 25;
    private readonly List<PathRequest> pathRequests = new(256);
    private readonly Dictionary<IPathfinderAgent, PathRequest?> agents = new(1024);
    // private readonly Dictionary<IPathfinderObstacle, List<GridNode>> obstacles = new(256);
    
    #endregion

    #region Getters & Setters

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
            var neighbors = GetNeighbors(node, true);
            node.SetNeighbors(neighbors);
        }
        
    }

    
    #region Public

    public void Update(float dt)
    {
        // HandleObstacles();
        HandlePathRequests();
    }

    public void Clear()
    {
        // ClearObstacles();
        ClearAgents();
        pathRequests.Clear();
        ResetNodes();
        ResolveClear();
    }
    public void ResetNodes()
    {
        foreach (var node in nodes)
        {
            node.Reset();
        }
    }
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

    public bool IsInside(Vector2 position) => Bounds.ContainsPoint(position);
    public bool IsIndexValid(int index) => Grid.IsIndexInBounds(index);
    public int GetIndex(Vector2 position) => Grid.GetCellIndex(position, Bounds);

    public int GetIndexUnclamped(Vector2 position) => Grid.GetCellIndexUnclamped(position, bounds);
    
    public Rect GetRect(Vector2 position) => GetRect(GetIndex(position));
    public Rect GetRect(int index)
    {
        var coordinates = Grid.IndexToCoordinates(index);
        var pos = Bounds.TopLeft + CellSize * coordinates.ToVector2();
        return new Rect(pos, CellSize, new Vector2(0f));
    }

    
    public float GetWeight(int index) => GetNode(index)?.GetWeight() ?? 1;
    public float GetWeight(int index, uint layer) => GetNode(index)?.GetWeight(layer) ?? 1;
    public bool IsTraversable(int index) => GetNode(index)?.IsTraversable() ?? true;
    public bool IsTraversable(int index, uint layer) => GetNode(index)?.IsTraversable(layer) ?? true;
    
    #endregion
    
    //
    // #region Obstacles System
    //
    // public void ClearObstacles()
    // {
    //     foreach (var obstacle in obstacles)
    //     {
    //         foreach (var node in obstacle.Value)
    //         {
    //             node.Reset();
    //         }
    //         obstacle.Key.OnShapeChanged -= OnObstacleShapeChanged;
    //     }
    //     obstacles.Clear();
    // }
    // public bool AddObstacle(IPathfinderObstacle obstacle)
    // {
    //     List<GridNode>? nodeList = null;
    //     GenerateObstacleCells(obstacle, ref nodeList);
    //     
    //     if (nodeList == null) return false;
    //     obstacles.Add(obstacle, nodeList);
    //     
    //     var nodeValues = obstacle.GetNodeValues();
    //     foreach (var c in nodeList)
    //     {
    //         foreach (var v in nodeValues)
    //         {
    //             c.ApplyNodeValue(v);
    //         }
    //     }
    //
    //     obstacle.OnShapeChanged += OnObstacleShapeChanged;
    //     return true;
    // }
    // public bool RemoveObstacle(IPathfinderObstacle obstacle)
    // {
    //     obstacles.TryGetValue(obstacle, out var nodeList);
    //     var nodeValues = obstacle.GetNodeValues();
    //     if (nodeList != null)
    //     {
    //         foreach (var c in nodeList)
    //         {
    //             foreach (var v in nodeValues)
    //             {
    //                 c.RemoveNodeValue(v);
    //             }
    //         }
    //     }
    //     
    //     if (!obstacles.Remove(obstacle)) return false;
    //     
    //     obstacle.OnShapeChanged -= OnObstacleShapeChanged;
    //     return true;
    //
    // }
    // private void OnObstacleShapeChanged(IPathfinderObstacle obstacle)
    // {
    //     obstacles.TryGetValue(obstacle, out var cellList);
    //     var cellValues = obstacle.GetNodeValues();
    //     if (cellList != null && cellList.Count > 0)
    //     {
    //         foreach (var c in cellList)
    //         {
    //             foreach (var v in cellValues)
    //             {
    //                 c.RemoveNodeValue(v);
    //             }
    //         }
    //     }
    //     
    //     GenerateObstacleCells(obstacle, ref cellList);
    //     obstacles[obstacle] = cellList ?? new();
    //     if (cellList != null && cellList.Count > 0)
    //     {
    //         foreach (var c in cellList)
    //         {
    //             foreach (var v in cellValues)
    //             {
    //                 c.ApplyNodeValue(v);
    //             }
    //         }
    //     }
    //     
    //     // obstacles.TryGetValue(obstacle, out var value);
    //     // if (value != null)
    //     // {
    //     //     foreach (var node in value)
    //     //     {
    //     //         node.SetWeight(1);
    //     //     }
    //     //     GenerateObstacleCells(obstacle, ref value);
    //     //     
    //     // }
    // }
    //
    //
    // private void GenerateObstacleCells(IPathfinderObstacle obstacle, ref List<GridNode>? cellList)
    // {
    //     switch (obstacle.GetShapeType())
    //     {
    //         case ShapeType.None: return;
    //         case ShapeType.Circle:
    //             GenerateCellList(obstacle.GetCircleShape(), ref cellList);
    //             break;
    //         case ShapeType.Segment: 
    //             GenerateCellList(obstacle.GetSegmentShape(), ref cellList);
    //             break;
    //         case ShapeType.Triangle:
    //             GenerateCellList(obstacle.GetTriangleShape(), ref cellList);
    //             break;
    //         case ShapeType.Quad:
    //             GenerateCellList(obstacle.GetQuadShape(), ref cellList);
    //             break;
    //         case ShapeType.Rect:
    //             GenerateCellList(obstacle.GetRectShape(), ref cellList);
    //             break;
    //         case ShapeType.Poly:
    //             GenerateCellList(obstacle.GetPolygonShape(), ref cellList);
    //             break;
    //         case ShapeType.PolyLine:
    //             GenerateCellList(obstacle.GetPolylineShape(), ref cellList);
    //             break;
    //     }
    // }
    // private void GenerateCellList(Segment shape, ref List<GridNode>? cellList)
    // {
    //     var rect = shape.GetBoundingBox();
    //     var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
    //     var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
    //
    //     
    //     var dif = bottomRight - topLeft;
    //     if (cellList == null) cellList = new(dif.Count);
    //     else cellList.Clear();
    //     
    //     for (int j = topLeft.Row; j <= bottomRight.Row; j++)
    //     {
    //         for (int i = topLeft.Col; i <= bottomRight.Col; i++)
    //         {
    //             int index = Grid.CoordinatesToIndex(new(i, j));
    //             var node = GetNode(index);
    //             if(node == null) continue;
    //             if(node.Rect.OverlapShape(shape)) cellList.Add(node);
    //         }
    //     }
    // }
    // private void GenerateCellList(Circle shape, ref List<GridNode>? cellList)
    // {
    //     var rect = shape.GetBoundingBox();
    //     var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
    //     var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
    //
    //     
    //     var dif = bottomRight - topLeft;
    //     if (cellList == null) cellList = new(dif.Count);
    //     else cellList.Clear();
    //     
    //     for (int j = topLeft.Row; j <= bottomRight.Row; j++)
    //     {
    //         for (int i = topLeft.Col; i <= bottomRight.Col; i++)
    //         {
    //             int index = Grid.CoordinatesToIndex(new(i, j));
    //             var node = GetNode(index);
    //             if(node == null) continue;
    //             if(node.Rect.OverlapShape(shape)) cellList.Add(node);
    //         }
    //     }
    // }
    // private void GenerateCellList(Triangle shape, ref List<GridNode>? cellList)
    // {
    //     var rect = shape.GetBoundingBox();
    //     var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
    //     var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
    //
    //     
    //     var dif = bottomRight - topLeft;
    //     if (cellList == null) cellList = new(dif.Count);
    //     else cellList.Clear();
    //     
    //     for (int j = topLeft.Row; j <= bottomRight.Row; j++)
    //     {
    //         for (int i = topLeft.Col; i <= bottomRight.Col; i++)
    //         {
    //             int index = Grid.CoordinatesToIndex(new(i, j));
    //             var node = GetNode(index);
    //             if(node == null) continue;
    //             if(node.Rect.OverlapShape(shape)) cellList.Add(node);
    //         }
    //     }
    // }
    // private void GenerateCellList(Quad shape, ref List<GridNode>? cellList)
    // {
    //     var rect = shape.GetBoundingBox();
    //     var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
    //     var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
    //
    //     
    //     var dif = bottomRight - topLeft;
    //     if (cellList == null) cellList = new(dif.Count);
    //     else cellList.Clear();
    //     
    //     for (int j = topLeft.Row; j <= bottomRight.Row; j++)
    //     {
    //         for (int i = topLeft.Col; i <= bottomRight.Col; i++)
    //         {
    //             int index = Grid.CoordinatesToIndex(new(i, j));
    //             var node = GetNode(index);
    //             if(node == null) continue;
    //             if(node.Rect.OverlapShape(shape)) cellList.Add(node);
    //         }
    //     }
    // }
    // private void GenerateCellList(Rect rect, ref List<GridNode>? cellList)
    // {
    //     var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
    //     var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
    //
    //     
    //     var dif = bottomRight - topLeft;
    //     if (cellList == null) cellList = new(dif.Count);
    //     else cellList.Clear();
    //     
    //     for (int j = topLeft.Row; j <= bottomRight.Row; j++)
    //     {
    //         for (int i = topLeft.Col; i <= bottomRight.Col; i++)
    //         {
    //             int index = Grid.CoordinatesToIndex(new(i, j));
    //             var node = GetNode(index);
    //             if(node == null) continue;
    //             if(node.Rect.OverlapShape(rect)) cellList.Add(node);
    //         }
    //     }
    // }
    // private void GenerateCellList(Polygon shape, ref List<GridNode>? cellList)
    // {
    //     var rect = shape.GetBoundingBox();
    //     var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
    //     var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
    //
    //     
    //     var dif = bottomRight - topLeft;
    //     if (cellList == null) cellList = new(dif.Count);
    //     else cellList.Clear();
    //     
    //     for (int j = topLeft.Row; j <= bottomRight.Row; j++)
    //     {
    //         for (int i = topLeft.Col; i <= bottomRight.Col; i++)
    //         {
    //             int index = Grid.CoordinatesToIndex(new(i, j));
    //             var node = GetNode(index);
    //             if(node == null) continue;
    //             if(node.Rect.OverlapShape(shape)) cellList.Add(node);
    //         }
    //     }
    // }
    // private void GenerateCellList(Polyline shape, ref List<GridNode>? cellList)
    // {
    //     var rect = shape.GetBoundingBox();
    //     var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
    //     var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
    //
    //     
    //     var dif = bottomRight - topLeft;
    //     if (cellList == null) cellList = new(dif.Count);
    //     else cellList.Clear();
    //     
    //     for (int j = topLeft.Row; j <= bottomRight.Row; j++)
    //     {
    //         for (int i = topLeft.Col; i <= bottomRight.Col; i++)
    //         {
    //             int index = Grid.CoordinatesToIndex(new(i, j));
    //             var node = GetNode(index);
    //             if(node == null) continue;
    //             if(node.Rect.OverlapShape(shape)) cellList.Add(node);
    //         }
    //     }
    // }
    // #endregion
    //
    
    #region Agent System

    public bool AddAgent(IPathfinderAgent agent)
    {
        // if (!agents.Add(agent)) return false;
        if (!agents.TryAdd(agent, null)) return false;

        agent.OnRequestPath += OnAgentRequestedPath;
        agent.AddedToPathfinder(this);
        return true;
    }
    public bool RemoveAgent(IPathfinderAgent agent)
    {
        // if (!agents.Remove(agent)) return false;
        if (!agents.Remove(agent, out var request)) return false;
        if(request != null) pathRequests.Remove(request);
        
        agent.OnRequestPath -= OnAgentRequestedPath;
        agent.RemovedFromPathfinder();
        return true;
    }

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

    public bool AddConnections(Vector2 a, Vector2 b, bool oneWay)
    {
        var cellA = GetNode(a);
        if (cellA == null) return false;
        
        var cellB = GetNode(b);
        if (cellB == null) return false;
        
        return ConnectNodes(cellA, cellB, oneWay);
    }
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
    public bool RemoveConnections(Vector2 a, Vector2 b, bool oneWay)
    {
        var cellA = GetNode(a);
        if (cellA == null) return false;
        
        var cellB = GetNode(b);
        if (cellB == null) return false;
        
        return DisconnectNodes(cellA, cellB, oneWay);
    }
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
    
    public bool ApplyNodeValue(int index, NodeValue value)
    {
        var node = GetNode(index);
        if (node == null) return false;
        node.ApplyNodeValue(value);
        return true;
    }
    public bool RemoveNodeValue(int index, NodeValue value)
    {
        var node = GetNode(index);
        if (node == null) return false;
        node.RemoveNodeValue(value);
        return true;
    }
    public bool ApplyNodeValues(int index, params NodeValue[] values)
    {
        var node = GetNode(index);
        if (node == null) return false;
        node.ApplyNodeValues(values);
        return true;
    }
    public bool RemoveNodeValues(int index, params NodeValue[] values)
    {
        var node = GetNode(index);
        if (node == null) return false;
        node.RemoveNodeValues(values);
        return true;
    }
    
    #endregion
    
    #region Position
    
    public bool ApplyNodeValue(Vector2 position, NodeValue value)
    {
        var node = GetNode(position);
        if (node == null) return false;
        node.ApplyNodeValue(value);
        return true;
    }
    public bool RemoveNodeValue(Vector2 position, NodeValue value)
    {
        var node = GetNode(position);
        if (node == null) return false;
        node.RemoveNodeValue(value);
        return true;
    }
    public bool ApplyNodeValues(Vector2 position, IEnumerable<NodeValue> values)
    {
        var node = GetNode(position);
        if (node == null) return false;
        node.ApplyNodeValues(values);
        return true;
    }
    public bool RemoveNodeValues(Vector2 position, IEnumerable<NodeValue> values)
    {
        var node = GetNode(position);
        if (node == null) return false;
        node.RemoveNodeValues(values);
        return true;
    }

    #endregion
    
    #region Segment
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
    public int RemoveNodeValue(Segment shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValue(value);
        }
    
        return nodeCount;
    }
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
    public int RemoveNodeValues(Segment shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValues(values);
        }
    
        return nodeCount;
    }
    #endregion
    
    #region Circle
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
    public int RemoveNodeValue(Circle shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValue(value);
        }
    
        return nodeCount;
    }
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
    public int RemoveNodeValues(Circle shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValues(values);
        }
    
        return nodeCount;
    }
    #endregion
    
    #region Triangle
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
    public int RemoveNodeValue(Triangle shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValue(value);
        }
    
        return nodeCount;
    }
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
    public int RemoveNodeValues(Triangle shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValues(values);
        }
    
        return nodeCount;
    }
    #endregion
    
    #region Rect
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
    public int RemoveNodeValue(Rect shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValue(value);
        }
    
        return nodeCount;
    }
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
    public int RemoveNodeValues(Rect shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValues(values);
        }
    
        return nodeCount;
    }
    #endregion
    
    #region Quad
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
    public int RemoveNodeValue(Quad shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValue(value);
        }
    
        return nodeCount;
    }
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
    public int RemoveNodeValues(Quad shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValues(values);
        }
    
        return nodeCount;
    }
    #endregion
    
    #region Polygon
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
    public int RemoveNodeValue(Polygon shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValue(value);
        }
    
        return nodeCount;
    }
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
    public int RemoveNodeValues(Polygon shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValues(values);
        }
    
        return nodeCount;
    }
    #endregion
    
    #region Polyline
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
    public int RemoveNodeValue(Polyline shape, NodeValue value)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValue(value);
        }
    
        return nodeCount;
    }
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
    public int RemoveNodeValues(Polyline shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var nodeCount = GetNodes(shape, ref resultSet);
        if (nodeCount <= 0) return 0;
        foreach (var node in resultSet)
        {
            node.RemoveNodeValues(values);
        }
    
        return nodeCount;
    }
    #endregion
    #endregion
    
    #region Virtual
    protected virtual void WasCleared(){}
    protected virtual void RegenerationWasRequested() { }
    // protected virtual void WasCleared(){}
    #endregion

    #region Private
    
    
    #region GetCells

    private GridNode? GetNode(int index)
    {
        if (!Grid.IsIndexInBounds(index)) return null;
        return nodes[index];
    }
    private GridNode? GetNode(Grid.Coordinates coordinates)
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

        Node? neighbor = null;
        neighbor = GetNode(coordinates + Direction.Right);
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
    public void DrawDebug(ColorRgba bounds, ColorRgba standard, ColorRgba blocked, ColorRgba desirable, ColorRgba undesirable, uint layer)
    {
        Bounds.DrawLines(12f, bounds);
        foreach (var node in nodes)
        {
            var r = node.Rect;

            // if(closedSet.Contains(node)) //touched
            // {
            //     r.ScaleSize(0.9f, new Vector2(0.5f)).Draw(new ColorRgba(System.Drawing.Color.Bisque));
            // }
            //
            if(node.GetWeight(layer) == 0) r.ScaleSize(0.5f, new Vector2(0.5f)).Draw(blocked);
            else if(node.GetWeight(layer) > 1) r.ScaleSize(0.65f, new Vector2(0.5f)).Draw(undesirable);
            else if(node.GetWeight(layer) < 1) r.ScaleSize(0.65f, new Vector2(0.5f)).Draw(desirable);
            else r.ScaleSize(0.8f, new Vector2(0.5f)).Draw(standard);
        }
        
    }
    #endregion

    #region Review if needed

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


/*
 //alternative obstacle shape changed system
 public bool AddObstacle(IPathfinderObstacle obstacle)
      {
          List<GridNode>? cellList = null;
          GenerateObstacleCells(obstacle, ref cellList);
          
          if (cellList == null) return false;
          obstacles.Add(obstacle, cellList);
          
          var cellValues = obstacle.GetNodeValues();
          foreach (var c in cellList)
          {
              foreach (var v in cellValues)
              {
                  c.ApplyCellValue(v);
              }
          }

          obstacle.OnShapeChanged += OnObstacleShapeChanged;
          return true;
      }
      public bool RemoveObstacle(IPathfinderObstacle obstacle)
      {
          obstacles.TryGetValue(obstacle, out var cellList);
          var cellValues = obstacle.GetNodeValues();
          if (cellList != null)
          {
              foreach (var c in cellList)
              {
                  foreach (var v in cellValues)
                  {
                      c.RemoveCellValue(v);
                  }
              }
          }
          
          if (!obstacles.Remove(obstacle)) return false;
          
          obstacle.OnShapeChanged -= OnObstacleShapeChanged;
          return true;

      }

      private bool DoesObstacleOverlapCellRect(IPathfinderObstacle obstacle, Rect rect)
      {
          switch (obstacle.GetShapeType())
          {
              case ShapeType.Circle: return obstacle.GetCircleShape().OverlapShape(rect);
              case ShapeType.Segment: return obstacle.GetSegmentShape().OverlapShape(rect);
              case ShapeType.Triangle: return obstacle.GetTriangleShape().OverlapShape(rect);
              case ShapeType.Quad: return obstacle.GetQuadShape().OverlapShape(rect);
              case ShapeType.Rect: return obstacle.GetRectShape().OverlapShape(rect);
              case ShapeType.Poly: return obstacle.GetPolygonShape().OverlapShape(rect);
              case ShapeType.PolyLine: return obstacle.GetPolylineShape().OverlapShape(rect);
          }

          return false;
      }
      private void OnObstacleShapeChanged(IPathfinderObstacle obstacle, Rect area)
      {
          cellHelper1.Clear();
          var nodeCount = GetCells(area, ref cellHelper1);
          if (nodeCount <= 0) return;
          
          
          obstacles.TryGetValue(obstacle, out var cellList);
          bool cellListIsEmpty = false;
          if (cellList == null)
          {
              cellList = new(nodeCount);
              obstacles[obstacle] = cellList;
              cellListIsEmpty = true;
          }
          
          var cellValues = obstacle.GetNodeValues();
          
          foreach (var node in cellHelper1)
          {
              var cr = node.GetRect();
              var overlaps = DoesObstacleOverlapCellRect(obstacle, cr);
                  
              if (!cellListIsEmpty && cellList.Contains(node))
              {
                  if (!overlaps)
                  {
                      foreach (var v in cellValues)
                      {
                          node.RemoveCellValue(v);
                      }

                      cellList.Remove(node);
                  }
              }
              else
              {
                  if (overlaps)
                  {
                      foreach (var v in cellValues)
                      {
                          node.ApplyCellValue(v);
                      }

                      cellList.Add(node);
                  }
              }
                  
          }



          // obstacles.TryGetValue(obstacle, out var cellList);
          // var cellValues = obstacle.GetNodeValues();
          // if (cellList != null && cellList.Count > 0)
          // {
          //     foreach (var c in cellList)
          //     {
          //         foreach (var v in cellValues)
          //         {
          //             c.RemoveCellValue(v);
          //         }
          //     }
          // }
          
          // GenerateObstacleCells(obstacle, ref cellList);
          // obstacles[obstacle] = cellList ?? new();
          // if (cellList != null && cellList.Count > 0)
          // {
          //     foreach (var c in cellList)
          //     {
          //         foreach (var v in cellValues)
          //         {
          //             c.ApplyCellValue(v);
          //         }
          //     }
          // }
      }
      
 */