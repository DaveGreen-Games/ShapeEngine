using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Pathfinding;

public class Pathfinder
{

    // private static readonly float DiagonalLength = MathF.Sqrt(2f);
    public event Action<Pathfinder>? OnRegenerationRequested;
    public event Action<Pathfinder>? OnCleared;
    // public event Action<Pathfinder>? OnCleared;

    #region Members

    #region Public

    // public float RelaxationPower = 1f;
    // public float KeepPathThreshold = 0f;

    // public int DEBUG_TOUCHED_COUNT { get; private set; } = 0;
    // public int DEBUG_TOUCHED_UNIQUE_COUNT => closedSet.Count;
    // public int DEBUG_MAX_OPEN_SET_COUNT { get; private set; } = 0;
    // public int DEBUG_PATH_REQUEST_COUNT => pathRequests.Count;
    public Size CellSize { get; private set; }
    public readonly Grid Grid;
    // public int NodeCount => cells.Count;
    #endregion

    #region Private
    private Rect bounds;
    private readonly List<GridNode> cells;
    private HashSet<GridNode> cellHelper1 = new(1024);
    private HashSet<GridNode> cellHelper2 = new(1024);
    private HashSet<GridNode> resultSet = new(1024);
    
    // private readonly NodeQueue openSet = new(1024);
    // private readonly HashSet<Node> openSetCells = new(1024);
    // private readonly HashSet<Node> closedSet = new(1024);
    // private readonly Dictionary<Node, Node> cellPath = new(1024);

    /// <summary>
    /// How many agent requests are handled each frame
    /// smaller or equal to zero handles all incoming requests
    /// </summary>
    public int RequestsPerFrame = 25;
    private readonly List<PathRequest> pathRequests = new(256);
    private readonly Dictionary<IPathfinderAgent, PathRequest?> agents = new(1024);

    private readonly Dictionary<IPathfinderObstacle, List<GridNode>> obstacles = new(256);
    // private readonly Dictionary<Cell, int> affectedCells = new(1024);
    
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
        cells = new(Grid.Count + 1);
        for (var i = 0; i < Grid.Count; i++)
        {
            var coordinates = Grid.IndexToCoordinates(i);
            var cell = new GridNode(coordinates, this);
            cells.Add(cell);
            // GScores.Add(0);
            // FScores.Add(0);
        }
        
        for (var i = 0; i < Grid.Count; i++)
        {
            var cell = cells[i];
            var neighbors = GetNeighbors(cell, true);
            cell.SetNeighbors(neighbors);
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
        ClearObstacles();
        ClearAgents();
        pathRequests.Clear();
        ResetCells();
        ResolveClear();
    }
    public void ResetCells()
    {
        foreach (var cell in cells)
        {
            cell.Reset();
        }
    }
    public void ResetCells(Rect rect)
    {
        cellHelper1.Clear();
        var count = GetCells(rect, ref cellHelper1);
        if (count <= 0) return;
        foreach (var cell in cellHelper1)
        {
            cell.Reset();
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

    
    public float GetWeight(int index) => GetCell(index)?.GetWeight() ?? 1;
    public float GetWeight(int index, uint layer) => GetCell(index)?.GetWeight(layer) ?? 1;
    public bool IsTraversable(int index) => GetCell(index)?.IsTraversable() ?? true;
    public bool IsTraversable(int index, uint layer) => GetCell(index)?.IsTraversable(layer) ?? true;
    
    #endregion

    #region Obstacles System

    public void ClearObstacles()
    {
        foreach (var obstacle in obstacles)
        {
            foreach (var cell in obstacle.Value)
            {
                cell.Reset();
            }
            obstacle.Key.OnShapeChanged -= OnObstacleShapeChanged;
        }
        obstacles.Clear();
    }
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
    private void OnObstacleShapeChanged(IPathfinderObstacle obstacle)
    {
        obstacles.TryGetValue(obstacle, out var cellList);
        var cellValues = obstacle.GetNodeValues();
        if (cellList != null && cellList.Count > 0)
        {
            foreach (var c in cellList)
            {
                foreach (var v in cellValues)
                {
                    c.RemoveCellValue(v);
                }
            }
        }
        
        GenerateObstacleCells(obstacle, ref cellList);
        obstacles[obstacle] = cellList ?? new();
        if (cellList != null && cellList.Count > 0)
        {
            foreach (var c in cellList)
            {
                foreach (var v in cellValues)
                {
                    c.ApplyCellValue(v);
                }
            }
        }
        
        // obstacles.TryGetValue(obstacle, out var value);
        // if (value != null)
        // {
        //     foreach (var cell in value)
        //     {
        //         cell.SetWeight(1);
        //     }
        //     GenerateObstacleCells(obstacle, ref value);
        //     
        // }
    }
    
    
    private void GenerateObstacleCells(IPathfinderObstacle obstacle, ref List<GridNode>? cellList)
    {
        switch (obstacle.GetShapeType())
        {
            case ShapeType.None: return;
            case ShapeType.Circle:
                GenerateCellList(obstacle.GetCircleShape(), ref cellList);
                break;
            case ShapeType.Segment: 
                GenerateCellList(obstacle.GetSegmentShape(), ref cellList);
                break;
            case ShapeType.Triangle:
                GenerateCellList(obstacle.GetTriangleShape(), ref cellList);
                break;
            case ShapeType.Quad:
                GenerateCellList(obstacle.GetQuadShape(), ref cellList);
                break;
            case ShapeType.Rect:
                GenerateCellList(obstacle.GetRectShape(), ref cellList);
                break;
            case ShapeType.Poly:
                GenerateCellList(obstacle.GetPolygonShape(), ref cellList);
                break;
            case ShapeType.PolyLine:
                GenerateCellList(obstacle.GetPolylineShape(), ref cellList);
                break;
        }
    }
    private void GenerateCellList(Segment shape, ref List<GridNode>? cellList)
    {
        var rect = shape.GetBoundingBox();
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);

        
        var dif = bottomRight - topLeft;
        if (cellList == null) cellList = new(dif.Count);
        else cellList.Clear();
        
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                var cell = GetCell(index);
                if(cell == null) continue;
                if(cell.Rect.OverlapShape(shape)) cellList.Add(cell);
            }
        }
    }
    private void GenerateCellList(Circle shape, ref List<GridNode>? cellList)
    {
        var rect = shape.GetBoundingBox();
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);

        
        var dif = bottomRight - topLeft;
        if (cellList == null) cellList = new(dif.Count);
        else cellList.Clear();
        
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                var cell = GetCell(index);
                if(cell == null) continue;
                if(cell.Rect.OverlapShape(shape)) cellList.Add(cell);
            }
        }
    }
    private void GenerateCellList(Triangle shape, ref List<GridNode>? cellList)
    {
        var rect = shape.GetBoundingBox();
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);

        
        var dif = bottomRight - topLeft;
        if (cellList == null) cellList = new(dif.Count);
        else cellList.Clear();
        
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                var cell = GetCell(index);
                if(cell == null) continue;
                if(cell.Rect.OverlapShape(shape)) cellList.Add(cell);
            }
        }
    }
    private void GenerateCellList(Quad shape, ref List<GridNode>? cellList)
    {
        var rect = shape.GetBoundingBox();
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);

        
        var dif = bottomRight - topLeft;
        if (cellList == null) cellList = new(dif.Count);
        else cellList.Clear();
        
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                var cell = GetCell(index);
                if(cell == null) continue;
                if(cell.Rect.OverlapShape(shape)) cellList.Add(cell);
            }
        }
    }
    private void GenerateCellList(Rect rect, ref List<GridNode>? cellList)
    {
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);

        
        var dif = bottomRight - topLeft;
        if (cellList == null) cellList = new(dif.Count);
        else cellList.Clear();
        
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                var cell = GetCell(index);
                if(cell == null) continue;
                if(cell.Rect.OverlapShape(rect)) cellList.Add(cell);
            }
        }
    }
    private void GenerateCellList(Polygon shape, ref List<GridNode>? cellList)
    {
        var rect = shape.GetBoundingBox();
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);

        
        var dif = bottomRight - topLeft;
        if (cellList == null) cellList = new(dif.Count);
        else cellList.Clear();
        
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                var cell = GetCell(index);
                if(cell == null) continue;
                if(cell.Rect.OverlapShape(shape)) cellList.Add(cell);
            }
        }
    }
    private void GenerateCellList(Polyline shape, ref List<GridNode>? cellList)
    {
        var rect = shape.GetBoundingBox();
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);

        
        var dif = bottomRight - topLeft;
        if (cellList == null) cellList = new(dif.Count);
        else cellList.Clear();
        
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                var cell = GetCell(index);
                if(cell == null) continue;
                if(cell.Rect.OverlapShape(shape)) cellList.Add(cell);
            }
        }
    }
    #endregion
    
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
            var path = GetRectPath(request.Start, request.End, request.Agent.GetLayer()); //GetPath(request.Start, request.End, request.Agent.GetLayer());
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

    public Path? GetRectPath(Vector2 start, Vector2 end, uint layer)
    {
        var startNode = GetCellClamped(start);
        
        if (!startNode.IsTraversable())
        {
            var newStartNode = startNode.GetClosestTraversableCell();
            if (newStartNode is GridNode rn) startNode = rn;
            else return null;
        }
        
        var endNode = GetCellClamped(end);
        if (!endNode.IsTraversable())
        {
            var newEndNode = endNode.GetClosestTraversableCell();
            if (newEndNode is GridNode rn) endNode = rn;
            else return null;
        }

        return AStar.GetRectPath(startNode, endNode, layer);
    }
    
    #endregion
    
    #region Connections

    public bool AddConnections(Vector2 a, Vector2 b, bool oneWay)
    {
        var cellA = GetCell(a);
        if (cellA == null) return false;
        
        var cellB = GetCell(b);
        if (cellB == null) return false;
        
        return ConnectCells(cellA, cellB, oneWay);
    }
    public void AddConnections(Vector2 a, Rect b, bool oneWay)
    {
        var cellA = GetCell(a);
        if (cellA == null) return;
        
        cellHelper1.Clear();
        if (GetCells(b, ref cellHelper1) <= 0) return;
        
        foreach (var cell in cellHelper1)
        {
            ConnectCells(cellA, cell, oneWay);
        }

    }
    public void AddConnections(Rect a, Vector2 b, bool oneWay)
    {
        cellHelper1.Clear();
        if (GetCells(a, ref cellHelper2) <= 0) return;
        var cellB = GetCell(b);
        if (cellB == null) return;
        foreach (var cell in cellHelper1)
        {
            ConnectCells(cell, cellB, oneWay);
        }
    }
    public void AddConnections(Rect a, Rect b, bool oneWay)
    {
        cellHelper1.Clear();
        if (GetCells(a, ref cellHelper1) <= 0) return;
        
        cellHelper2.Clear();
        if (GetCells(b, ref cellHelper2) <= 0) return;
        
        foreach (var cellA in cellHelper1)
        {
            foreach (var cellB in cellHelper2)
            {
                ConnectCells(cellA, cellB, oneWay);
            }
        }
    }
    public bool RemoveConnections(Vector2 a, Vector2 b, bool oneWay)
    {
        var cellA = GetCell(a);
        if (cellA == null) return false;
        
        var cellB = GetCell(b);
        if (cellB == null) return false;
        
        return DisconnectCells(cellA, cellB, oneWay);
    }
    public void RemoveConnections(Vector2 a, Rect b, bool oneWay)
    {
        var cellA = GetCell(a);
        if (cellA == null) return;
        
        cellHelper1.Clear();
        if (GetCells(b, ref cellHelper1) <= 0) return;
        
        foreach (var cell in cellHelper1)
        {
            DisconnectCells(cellA, cell, oneWay);
        }

    }
    public void RemoveConnections(Rect a, Vector2 b, bool oneWay)
    {
        var cellB = GetCell(b);
        if (cellB == null) return;
        
        cellHelper1.Clear();
        if (GetCells(a, ref cellHelper2) <= 0) return;
        
        foreach (var cell in cellHelper1)
        {
            DisconnectCells(cell, cellB, oneWay);
        }
    }
    public void RemoveConnections(Rect a, Rect b, bool oneWay)
    {
        cellHelper1.Clear();
        if (GetCells(a, ref cellHelper1) <= 0) return;
        
        cellHelper2.Clear();
        if (GetCells(b, ref cellHelper2) <= 0) return;
        
        foreach (var cellA in cellHelper1)
        {
            foreach (var cellB in cellHelper2)
            {
                DisconnectCells(cellA, cellB, oneWay);
            }
        }
    }
    #endregion
    
    #region Set Cell Values
    
    #region Index
    
    public bool ApplyCellValue(int index, NodeValue value)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        cell.ApplyCellValue(value);
        return true;
    }
    public bool RemoveCellValue(int index, NodeValue value)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        cell.RemoveCellValue(value);
        return true;
    }
    public bool ApplyCellValues(int index, params NodeValue[] values)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        cell.ApplyCellValues(values);
        return true;
    }
    public bool RemoveCellValues(int index, params NodeValue[] values)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        cell.RemoveCellValues(values);
        return true;
    }
    
    #endregion
    
    #region Position
    
    public bool ApplyCellValue(Vector2 position, NodeValue value)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        cell.ApplyCellValue(value);
        return true;
    }
    public bool RemoveCellValue(Vector2 position, NodeValue value)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        cell.RemoveCellValue(value);
        return true;
    }
    public bool ApplyCellValues(Vector2 position, IEnumerable<NodeValue> values)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        cell.ApplyCellValues(values);
        return true;
    }
    public bool RemoveCellValues(Vector2 position, IEnumerable<NodeValue> values)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        cell.RemoveCellValues(values);
        return true;
    }

    #endregion
    
    #region Segment
    public int ApplyCellValue(Segment shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValue(value);
        }
    
        return cellCount;
    }
    public int RemoveCellValue(Segment shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValue(value);
        }
    
        return cellCount;
    }
    public int ApplyCellValues(Segment shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValues(values);
        }
    
        return cellCount;
    }
    public int RemoveCellValues(Segment shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValues(values);
        }
    
        return cellCount;
    }
    #endregion
    
    #region Circle
    public int ApplyCellValue(Circle shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValue(value);
        }
    
        return cellCount;
    }
    public int RemoveCellValue(Circle shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValue(value);
        }
    
        return cellCount;
    }
    public int ApplyCellValues(Circle shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValues(values);
        }
    
        return cellCount;
    }
    public int RemoveCellValues(Circle shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValues(values);
        }
    
        return cellCount;
    }
    #endregion
    
    #region Triangle
    public int ApplyCellValue(Triangle shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValue(value);
        }
    
        return cellCount;
    }
    public int RemoveCellValue(Triangle shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValue(value);
        }
    
        return cellCount;
    }
    public int ApplyCellValues(Triangle shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValues(values);
        }
    
        return cellCount;
    }
    public int RemoveCellValues(Triangle shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValues(values);
        }
    
        return cellCount;
    }
    #endregion
    
    #region Rect
    public int ApplyCellValue(Rect shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValue(value);
        }
    
        return cellCount;
    }
    public int RemoveCellValue(Rect shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValue(value);
        }
    
        return cellCount;
    }
    public int ApplyCellValues(Rect shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValues(values);
        }
    
        return cellCount;
    }
    public int RemoveCellValues(Rect shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValues(values);
        }
    
        return cellCount;
    }
    #endregion
    
    #region Quad
    public int ApplyCellValue(Quad shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValue(value);
        }
    
        return cellCount;
    }
    public int RemoveCellValue(Quad shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValue(value);
        }
    
        return cellCount;
    }
    public int ApplyCellValues(Quad shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValues(values);
        }
    
        return cellCount;
    }
    public int RemoveCellValues(Quad shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValues(values);
        }
    
        return cellCount;
    }
    #endregion
    
    #region Polygon
    public int ApplyCellValue(Polygon shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValue(value);
        }
    
        return cellCount;
    }
    public int RemoveCellValue(Polygon shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValue(value);
        }
    
        return cellCount;
    }
    public int ApplyCellValues(Polygon shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValues(values);
        }
    
        return cellCount;
    }
    public int RemoveCellValues(Polygon shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValues(values);
        }
    
        return cellCount;
    }
    #endregion
    
    #region Polyline
    public int ApplyCellValue(Polyline shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValue(value);
        }
    
        return cellCount;
    }
    public int RemoveCellValue(Polyline shape, NodeValue value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValue(value);
        }
    
        return cellCount;
    }
    public int ApplyCellValues(Polyline shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ApplyCellValues(values);
        }
    
        return cellCount;
    }
    public int RemoveCellValues(Polyline shape, params NodeValue[] values)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.RemoveCellValues(values);
        }
    
        return cellCount;
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

    private GridNode? GetCell(int index)
    {
        if (!Grid.IsIndexInBounds(index)) return null;
        return cells[index];
    }
    private GridNode? GetCell(Grid.Coordinates coordinates)
    {
        if (!Grid.AreCoordinatesInside(coordinates)) return null;
        var index = Grid.CoordinatesToIndex(coordinates);
        if (index >= 0 && index < cells.Count) return cells[index];
        return null;
    }
    private GridNode? GetCell(Vector2 position)
    {
        var index = Grid.GetCellIndexUnclamped(position, bounds);
        if (!Grid.IsIndexInBounds(index)) return null;
        return cells[index];
    }
    private GridNode GetCellClamped(Vector2 position)
    {
        return cells[Grid.GetCellIndex(position, bounds)];
    }
    private int GetCells(Segment segment, ref HashSet<GridNode> result)
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
                var cell = GetCell(index);
                if(cell == null) continue;
                if(cell.Rect.OverlapShape(segment)) result.Add(cell);
            }
        }

        return result.Count - count;
    }
    private int GetCells(Circle circle, ref HashSet<GridNode> result)
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
                var cell = GetCell(index);
                if(cell == null) continue;
                if(cell.Rect.OverlapShape(circle)) result.Add(cell);
            }
        }

        return result.Count - count;
    }
    private int GetCells(Triangle triangle, ref HashSet<GridNode> result)
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
                var cell = GetCell(index);
                if(cell == null) continue;
                if(cell.Rect.OverlapShape(triangle)) result.Add(cell);
            }
        }

        return result.Count - count;
    }
    private int GetCells(Quad quad, ref HashSet<GridNode> result)
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
                var cell = GetCell(index);
                if(cell == null) continue;
                if(cell.Rect.OverlapShape(quad)) result.Add(cell);
            }
        }

        return result.Count - count;
    }
    private int GetCells(Rect rect, ref HashSet<GridNode> result)
    {
        var topLeft = Grid.GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = Grid.GetCellCoordinate(rect.BottomRight, bounds);
        var count = result.Count;
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int index = Grid.CoordinatesToIndex(new(i, j));
                result.Add(cells[index]);
            }
        }

        return result.Count - count;
    }
    private int GetCells(Polygon polygon, ref HashSet<GridNode> result)
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
                var cell = GetCell(index);
                if(cell == null) continue;
                if(cell.Rect.OverlapShape(polygon)) result.Add(cell);
            }
        }

        return result.Count - count;
    }
    private int GetCells(Polyline polyline, ref HashSet<GridNode> result)
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
                var cell = GetCell(index);
                if(cell == null) continue;
                if(cell.Rect.OverlapShape(polyline)) result.Add(cell);
            }
        }

        return result.Count - count;
    }
    #endregion

    #region Path Helper
    private Node? GetNeighborCell(GridNode node, Direction dir) => GetCell(node.Coordinates + dir);
    private HashSet<Node>? GetNeighbors(GridNode node, bool diagonal = true)
    {
        HashSet<Node>? neighbors = null;
        var coordinates = node.Coordinates;

        Node? neighbor = null;
        neighbor = GetCell(coordinates + Direction.Right);
        if (neighbor != null)
        {
            neighbors ??= new();
            neighbors.Add(neighbor);
        }
        neighbor = GetCell(coordinates + Direction.Left);
        if (neighbor != null)
        {
            neighbors ??= new();
            neighbors.Add(neighbor);
        }
        neighbor = GetCell(coordinates + Direction.Up);
        if (neighbor != null)
        {
            neighbors ??= new();
            neighbors.Add(neighbor);
        }
        neighbor = GetCell(coordinates + Direction.Down);
        if (neighbor != null)
        {
            neighbors ??= new();
            neighbors.Add(neighbor);
        }

        if (diagonal)
        {
            neighbor = GetCell(coordinates + Direction.UpLeft);
            if (neighbor != null)
            {
                neighbors ??= new();
                neighbors.Add(neighbor);
            }
            neighbor = GetCell(coordinates + Direction.UpRight);
            if (neighbor != null)
            {
                neighbors ??= new();
                neighbors.Add(neighbor);
            }
            neighbor = GetCell(coordinates + Direction.DownLeft);
            if (neighbor != null)
            {
                neighbors ??= new();
                neighbors.Add(neighbor);
            }
            neighbor = GetCell(coordinates + Direction.DownRight);
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
    private bool ConnectCells(Node a, Node b, bool oneWay)
    {
        if (a == b) return false;

        a.Connect(b);
        if (!oneWay) b.Connect(a);
        return true;
    }
    private bool DisconnectCells(Node a, Node b, bool oneWay)
    {
        if (a == b) return false;
        a.Disconnect(b);
        if (oneWay) b.Disconnect(a);
        return true;
    }
    #endregion
    
    // private Rect GetRect(Grid.Coordinates coordinates)
    // {
    //     var pos = Bounds.TopLeft + CellSize * coordinates.ToVector2();
    //     return new Rect(pos, CellSize, new Vector2(0f));
    // }
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

    // private void ResolveCleared()
    // {
    //     WasCleared();
    //     OnCleared?.Invoke(this);
    // }
    #endregion

    #region Debug
    public void DrawDebug(ColorRgba bounds, ColorRgba standard, ColorRgba blocked, ColorRgba desirable, ColorRgba undesirable, uint layer)
    {
        Bounds.DrawLines(12f, bounds);
        foreach (var cell in cells)
        {
            var r = cell.Rect;

            // if(closedSet.Contains(cell)) //touched
            // {
            //     r.ScaleSize(0.9f, new Vector2(0.5f)).Draw(new ColorRgba(System.Drawing.Color.Bisque));
            // }
            //
            if(cell.GetWeight(layer) == 0) r.ScaleSize(0.5f, new Vector2(0.5f)).Draw(blocked);
            else if(cell.GetWeight(layer) > 1) r.ScaleSize(0.65f, new Vector2(0.5f)).Draw(undesirable);
            else if(cell.GetWeight(layer) < 1) r.ScaleSize(0.65f, new Vector2(0.5f)).Draw(desirable);
            else r.ScaleSize(0.8f, new Vector2(0.5f)).Draw(standard);
        }
        
    }
    #endregion

    #region Review if needed

    public List<Rect> GetRects(bool traversable = true)
    {
        var rects = new List<Rect>();
        foreach (var cell in cells)
        {
            if (!traversable || cell.IsTraversable())
            {
                rects.Add(cell.Rect);
            }
        }

        return rects;
    }
    public List<Rect> GetRects(uint layer, int minWeight, int maxWeight)
    {
        var rects = new List<Rect>();
        foreach (var cell in cells)
        {
            var w = cell.GetWeight(layer);
            if(w >= minWeight && w <= maxWeight) rects.Add(cell.Rect);
        }

        return rects;
    }
    public List<Rect> GetRects(Rect area, bool traversable = true)
    {
        cellHelper1.Clear();
        GetCells(area, ref cellHelper1);
        var result = new List<Rect>();
        foreach (var node in cellHelper1)
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
        cellHelper1.Clear();
        GetCells(area, ref cellHelper1);
        var result = new List<Rect>();
        foreach (var node in cellHelper1)
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
          var cellCount = GetCells(area, ref cellHelper1);
          if (cellCount <= 0) return;
          
          
          obstacles.TryGetValue(obstacle, out var cellList);
          bool cellListIsEmpty = false;
          if (cellList == null)
          {
              cellList = new(cellCount);
              obstacles[obstacle] = cellList;
              cellListIsEmpty = true;
          }
          
          var cellValues = obstacle.GetNodeValues();
          
          foreach (var cell in cellHelper1)
          {
              var cr = cell.GetRect();
              var overlaps = DoesObstacleOverlapCellRect(obstacle, cr);
                  
              if (!cellListIsEmpty && cellList.Contains(cell))
              {
                  if (!overlaps)
                  {
                      foreach (var v in cellValues)
                      {
                          cell.RemoveCellValue(v);
                      }

                      cellList.Remove(cell);
                  }
              }
              else
              {
                  if (overlaps)
                  {
                      foreach (var v in cellValues)
                      {
                          cell.ApplyCellValue(v);
                      }

                      cellList.Add(cell);
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