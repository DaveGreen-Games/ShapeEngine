using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Pathfinding;

public interface IPathfinderObstacle : IShape
{
    public enum CellValueType
    {
        None = 0,
        Flat = 1,
        Bonus = 2,
        Set = 3,
        SetMin = 4,
        SetMax = 5,
        Block = 6,
        Clear = 7
        
    }
    public readonly struct CellValue
    {
        /// <summary>
        /// Higher numbers mean the cell is more favorable
        /// Smaller numbers mean the cell is less favorable
        /// BaseValue/Flat/Bonus of 0 is default
        /// A value of 5 makes the cell distance 5 timer shorter, a value -5 make the cell distance 5 timer longer
        /// </summary>
        public readonly float Value;
        public readonly CellValueType Type;
        public readonly uint Layer;

        public bool Valid => Type != CellValueType.None;

        public CellValue()
        {
            Value = 0;
            Type = CellValueType.None;
            Layer = 0;
        }
        public CellValue(float value, CellValueType type)
        {
            Value = value;
            Type = type;
            Layer = 0;
        }
        public CellValue(float value, CellValueType type, uint layer)
        {
            Value = value;
            Type = type;
            Layer = layer;
        }
    }

    
    /// <summary>
    /// Invoke this event the shape of the obstacle has changed
    /// </summary>
    public event Action<IPathfinderObstacle>? OnShapeChanged;
    
    /// <summary>
    /// Those are the values that will change the pathfinder grid. Do not change them once the obstacle was added.
    /// If the cell values have to be changed the obstacle has to be removed with the orginial values, and can then be
    /// added again with the new values
    /// </summary>
    public CellValue[] GetCellValues();
}

public interface IPathfinderAgent
{
    public class PathRequest // : IEquatable<PathRequest>, IEqualityComparer<PathRequest>
    {
        public readonly IPathfinderAgent? Agent;
        public readonly Vector2 Start;
        public readonly Vector2 End;
        /// <summary>
        /// The higher the priority the sooner path requests are handled.
        /// </summary>
        public readonly int Priority;

        public bool Valid => Agent != null;
        public PathRequest()
        {
            Agent = null;
            Start = new();
            End = new();
            Priority = 0;
        }
        public PathRequest(IPathfinderAgent agent, Vector2 start, Vector2 end, int priority)
        {
            this.Agent = agent;
            this.Start = start;
            this.End = end;
            this.Priority = priority;
        }

        // public bool Equals(PathRequest other)
        // {
        //     if (Agent == null || other.Agent == null) return false;
        //     return Agent == other.Agent;
        // }
        //
        // public bool Equals(PathRequest x, PathRequest y)
        // {
        //     return x.Equals(y);
        // }
        //
        // public int GetHashCode(PathRequest obj)
        // {
        //     return HashCode.Combine(Agent, Start, End, Priority);
        // }
    }
    public event Action<PathRequest> OnRequestPath;

    public void ReceiveRequestedPath(Pathfinder.Path? path, PathRequest request);
    public uint GetLayer();
    public void AddedToPathfinder(Pathfinder pathfinder);
    public void RemovedFromPathfinder();

}

public class Pathfinder
{

    // public static readonly float Blocked = 0;
    // public static readonly float Default = 1;
    private static readonly float DiagonalLength = MathF.Sqrt(2f);
    
    private class CellQueue
    {
        private readonly List<Cell> cells;

        public CellQueue(int capacity)
        {
            cells = new(capacity);
        }
        public int Count => cells.Count;
        public void Clear() => cells.Clear();
        public void Enqueue(Cell cell)
        {
            cells.Add(cell);
        }

        public Cell? Dequeue()
        {
            if (Count <= 0) return null;
            if (Count == 1)
            {
                var cell = cells[0];
                cells.RemoveAt(0);
                return cell;
            }
            var minIndex = 0;
            var current = cells[0];
            for (var i = 1; i < cells.Count; i++)
            {
                var cell = cells[i];
                // if (cell.FScore < current.FScore || (Math.Abs(cell.FScore - current.FScore) < 0.0001f && cell.H < current.H))
                if(cell.CompareTo(current) < 0)
                {
                    minIndex = i;
                    current = cell;
                }
            }
            
            cells.RemoveAt(minIndex);
            
            return current;
        }

        // public void UpdatePriority(Cell cell)
        // {
        //     if (cells is not { Count: > 0 }) return;
        //     if (!cells.Remove(cell)) return;
        //     
        //     int index = cells.BinarySearch(cell, comparer);
        //     if (index < 0) index = ~index;
        //     cells.Insert(index, cell);
        //
        // }

    }
    private class Cell : IComparable<Cell>
    {
        private class CellWeight
        {
            // public bool Blocked = false;
            private int blockCount = 0;
            private float baseValue = 0;
            private float flat = 0;
            private float bonus = 0;
            public bool Blocked => blockCount > 0;

            public void Apply(IPathfinderObstacle.CellValue value)
            {
                if (!value.Valid) return;
                switch (value.Type)
                {
                    case IPathfinderObstacle.CellValueType.Flat:
                        flat += value.Value;
                        break;
                    case IPathfinderObstacle.CellValueType.Bonus:
                        bonus += value.Value;
                        break;
                    case IPathfinderObstacle.CellValueType.Set:
                        baseValue = value.Value;
                        break;
                    case IPathfinderObstacle.CellValueType.SetMin:
                        if (baseValue > value.Value) baseValue = value.Value;
                        break;
                    case IPathfinderObstacle.CellValueType.SetMax:
                        if (baseValue < value.Value) baseValue = value.Value;
                        break;
                    case IPathfinderObstacle.CellValueType.Block:
                        blockCount++;
                        break;
                    case IPathfinderObstacle.CellValueType.Clear:
                        Reset();
                        break;
                }
            }

            public void Remove(IPathfinderObstacle.CellValue value)
            {
                if (!value.Valid) return;
                switch (value.Type)
                {
                    case IPathfinderObstacle.CellValueType.Flat:
                        flat -= value.Value;
                        break;
                    case IPathfinderObstacle.CellValueType.Bonus:
                        bonus -= value.Value;
                        break;
                    case IPathfinderObstacle.CellValueType.Set:
                        if (Math.Abs(baseValue - value.Value) < 0.0001f) baseValue = 0;
                        break;
                    case IPathfinderObstacle.CellValueType.SetMin:
                        if (Math.Abs(baseValue - value.Value) < 0.0001f) baseValue = 0;
                        break;
                    case IPathfinderObstacle.CellValueType.SetMax:
                        if (Math.Abs(baseValue - value.Value) < 0.0001f) baseValue = 0;
                        break;
                    case IPathfinderObstacle.CellValueType.Block:
                        blockCount--;
                        break;
                }
            }
            public float Cur => blockCount > 0 ? 0 : GetBaseValueFactor() * GetBonusFactor();
            // public float Cur => blockCount > 0 ? 0 : MathF.Max(baseValue + flat, 0) * GetBonusFactor();

            private float GetBaseValueFactor()
            {
                var v = baseValue + flat;
                if (v > 0) return 1f / v; //more favorable
                if (v < 0) return MathF.Abs(v); //less favorable
                return 1f; //normal
            } 
            private float GetBonusFactor()
            {
                if (bonus > 0) return 1f / bonus; //more favorable
                if (bonus < 0) return MathF.Abs(bonus); //less favorable
                return 1f; //normal
            } 
            public void Reset()
            {
                baseValue = 0;
                flat = 0;
                bonus = 0;
                blockCount = 0;
            }
        }

        #region Members

        #region Public

        public Rect Rect
        {
            get
            {
                var pos = parent.Bounds.TopLeft + parent.CellSize * Coordinates.ToVector2();
                return new Rect(pos, parent.CellSize, new Vector2(0f));
            }
        }
        public readonly Grid.Coordinates Coordinates;

        #endregion

        #region Internal

        internal HashSet<Cell>? Neighbors = null;
        internal HashSet<Cell>? Connections = null;
        internal float GScore = float.PositiveInfinity;
        internal float FScore = float.PositiveInfinity;
        internal float H = float.PositiveInfinity;

        #endregion

        #region Private

        private readonly CellWeight weight = new();
        private Dictionary<uint, CellWeight>? weights = null;
        private readonly Pathfinder parent;

        #endregion
        

        #endregion

        #region Public

        public Cell(Grid.Coordinates coordinates, Pathfinder parent)
        {
            Coordinates = coordinates;
            this.parent = parent;
            // Value = new(1f);
        }
        public void Reset()
        {
            Connections?.Clear();
            
            weight.Reset();
            ResetWeights();
            // weights?.Clear();
        }


        #endregion
        
        #region Neighbors & Connections
        public bool HasNeighbors => Neighbors is { Count: > 0 };
        public bool HasConnections => Connections is { Count: > 0 };
        
        public bool AddNeighbor(Cell cell)
        {
            Neighbors ??= new();
            return Neighbors.Add(cell);
        }

        public bool RemoveNeighbor(Cell cell)
        {
            if (Neighbors == null) return false;
            return Neighbors.Remove(cell);
        }
        public void SetNeighbors(HashSet<Cell>? neighbors)
        {
            Neighbors = neighbors;
        }
        public bool Connect(Cell other)
        {
            if (other == this) return false;
            Connections ??= new();
            return Connections.Add(other);
        }
        public bool Disconnect(Cell other)
        {
            if (other == this) return false;
            if (Connections == null) return false;
            return Connections.Remove(other);
        }
        #endregion

        #region CellValues

        public void ApplyCellValue(IPathfinderObstacle.CellValue value)
        {
            if (value.Layer > 0)
            {
                if (weights == null) weights = new();
                if (!weights.ContainsKey(value.Layer))
                {
                    weights.Add(value.Layer, new());
                }
            }
            else weight.Apply(value);
        }
        public void RemoveCellValue(IPathfinderObstacle.CellValue value)
        {
            if (value.Layer > 0)
            {
                if (weights == null) return;
                if (!weights.ContainsKey(value.Layer)) return;
                weights[value.Layer].Remove(value);
            }
            else weight.Remove(value);
        }
        
        public void ApplyCellValues(IEnumerable<IPathfinderObstacle.CellValue> values)
        {
            foreach (var value in values)
            {
                ApplyCellValue(value);
            }
        }
        public void RemoveCellValues(IEnumerable<IPathfinderObstacle.CellValue> values)
        {
            foreach (var value in values)
            {
                RemoveCellValue(value);
            }
        }


        #endregion
        
        #region Weight

        public bool IsTraversable() => !weight.Blocked; // weight > Blocked || weight < Blocked;
        public bool IsTraversable(uint layer)
        {
            if (weights == null) return !weight.Blocked; // IsTraversable();
            if (weights.TryGetValue(layer, out var value)) return !value.Blocked; // value > Blocked || value < Blocked;
            return !weight.Blocked; // weight > Blocked || weight < Blocked;
        }
        public float GetWeight() => weight.Cur;
        public float GetWeight(uint layer)
        {
            if (weights == null) return weight.Cur;
            return weights.TryGetValue(layer, out var w) ? w.Cur : weight.Cur;
        }

        public bool HasWeight(uint layer) => weights?.ContainsKey(layer) ?? false;
        public bool ResetWeight(uint layer)
        {
            if (layer <= 0)
            {
                weight.Reset();
                return true;
            }
            
            if (weights == null) return false;
            if (!weights.TryGetValue(layer, out var cell)) return false;
            cell.Reset();
            return true;

        }
        public void ResetWeights()
        {
            weight.Reset();
            if (weights == null || weights.Count <= 0) return;
            foreach (var key in weights.Keys)
            {
                weights[key].Reset();
            }
        }
        #endregion

        public int CompareTo(Cell? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            
            int fScoreComparison = FScore.CompareTo(other.FScore);
            if (fScoreComparison != 0) return fScoreComparison;
            return H.CompareTo(other.H);
        }
    }
    public class Path
    {
        public readonly Vector2 Start;
        public readonly Vector2 End;
        public readonly List<Rect> Rects;
        
        public Path(Vector2 start, Vector2 end, List<Rect> rects)
    {
        Start = start;
        End = end;
        Rects = rects;
    }
    }

    
    public event Action<Pathfinder>? OnRegenerationRequested;
    public event Action<Pathfinder>? OnReset;
    public event Action<Pathfinder>? OnCleared;

    #region Members

    #region Public

    // public float RelaxationPower = 1f;
    // public float KeepPathThreshold = 0f;

    // public int DEBUG_TOUCHED_COUNT { get; private set; } = 0;
    public int DEBUG_TOUCHED_UNIQUE_COUNT => closedSet.Count;
    public int DEBUG_MAX_OPEN_SET_COUNT { get; private set; } = 0;
    public int DEBUG_PATH_REQUEST_COUNT => pathRequests.Count;
    public Size CellSize { get; private set; }
    public readonly Grid Grid;
    
    #endregion

    #region Private
    private Rect bounds;
    private readonly List<Cell> cells;
    private HashSet<Cell> cellHelper1 = new(1024);
    private HashSet<Cell> cellHelper2 = new(1024);
    private HashSet<Cell> resultSet = new(1024);
    
    private readonly CellQueue openSet = new(1024);
    private readonly HashSet<Cell> openSetCells = new(1024);
    private readonly HashSet<Cell> closedSet = new(1024);
    private readonly Dictionary<Cell, Cell> cellPath = new(1024);

    /// <summary>
    /// How many agent requests are handled each frame
    /// smaller or equal to zero handles all incoming requests
    /// </summary>
    public int RequestsPerFrame = 25;
    private readonly List<IPathfinderAgent.PathRequest> pathRequests = new(256);
    private readonly Dictionary<IPathfinderAgent, IPathfinderAgent.PathRequest?> agents = new(1024);

    private readonly Dictionary<IPathfinderObstacle, List<Cell>> obstacles = new(256);
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
            var cell = new Cell(coordinates, this);
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

    public void Close()
    {
        ClearAgents();
        pathRequests.Clear();
        
    }
    public void Reset()
    {
        foreach (var cell in cells)
        {
            cell.Reset();
        }
        ResolveReset();
    }
    // public void Clear()
    // {
    //     foreach (var cell in cells)
    //     {
    //         cell.Clear();
    //     }
    // }
    public void Reset(Rect rect)
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

    #region Path
    
    public Path? GetPath(Vector2 start, Vector2 end, uint layer)
    {
        // GScore is the cost of the cheapest path from start to n currently known.
        // FScore represents our current best guess as to how cheap a path could be from start to finish if it goes through n.
        var startCell = GetCellClamped(start);
        
        if (!startCell.IsTraversable())
        {
            var newStartCell = GetClosestTraversableCell(startCell);
            if (newStartCell == null) return null;
            startCell = newStartCell;
        }
        
        var targetCell = GetCellClamped(end);
        if (!targetCell.IsTraversable())
        {
            var newTargetCell = GetClosestTraversableCell(targetCell);
            if (newTargetCell == null) return null;
            targetCell = newTargetCell;
        }
        
        cellPath.Clear();

        DEBUG_MAX_OPEN_SET_COUNT = 0;
        
        closedSet.Clear();
        openSetCells.Clear();
        openSet.Clear();

        startCell.GScore = 0;
        startCell.H = DistanceToTarget(startCell, targetCell);
        startCell.FScore = startCell.H;
        // openSet.Enqueue(startCell, startCell.FScore);
        openSet.Enqueue(startCell);
        openSetCells.Add(startCell);


        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            if(current == null) continue;
            
            if (openSetCells.Count > DEBUG_MAX_OPEN_SET_COUNT)
            {
                DEBUG_MAX_OPEN_SET_COUNT = openSetCells.Count;
            }
            
            if (current == targetCell)
            {
                var startCoordinates = startCell.Coordinates;
                var targetCoordinates = targetCell.Coordinates;
                var dif = startCoordinates - targetCoordinates;
                var rects = ReconstructPath(current, dif.Distance);
                
                // if(openSet.Count > 1024) Console.WriteLine($"Open Set Count: {openSet.Count}, Open Set Cells Count: {openSetCells}");
                // if(closedSet.Count > 1024) Console.WriteLine($"Closed Set Count: {closedSet.Count}");
                // if(cellPath.Count > 1024) Console.WriteLine($"Cell Path Count: {cellPath.Count}");
    
                return new Path(start, end, rects);
            }

            openSetCells.Remove(current);
            closedSet.Add(current);
            
            if (current.Neighbors != null)
            {
                foreach (var neighbor in current.Neighbors)
                {
                    if(closedSet.Contains(neighbor) || !neighbor.IsTraversable(layer)) continue;

                    float tentativeGScore = current.GScore + WeightedDistanceToNeighbor(current, neighbor, layer);

                    if (openSetCells.Contains(neighbor))
                    {
                        if (tentativeGScore < neighbor.GScore)
                        {
                            neighbor.GScore = tentativeGScore;
                            neighbor.FScore = neighbor.GScore + neighbor.H;
                            cellPath[neighbor] = current;
                        }
                    }
                    else
                    {
                        neighbor.GScore = tentativeGScore;
                        neighbor.H = DistanceToTarget(neighbor, targetCell);
                        neighbor.FScore = neighbor.GScore + neighbor.H;
                        // openSet.Enqueue(neighbor, neighbor.FScore);
                        openSet.Enqueue(neighbor);
                        openSetCells.Add(neighbor);
                        cellPath[neighbor] = current;
                    }
                    // if (tentativeGScore < neighbor.GScore)
                    // {
                    //     neighbor.DEBUG_Touched = true;
                    //     DEBUG_TOUCHED_COUNT++;
                    //     
                    //     CellPath[neighbor] = current;
                    //     neighbor.GScore = tentativeGScore;
                    //     neighbor.FScore = tentativeGScore + DistanceToTarget(neighbor, targetCell);
                    //
                    //     if (!openSetCells.Contains(neighbor))
                    //     {
                    //         openSet.Enqueue(neighbor, neighbor.FScore);
                    //         openSetCells.Add(neighbor);
                    //     }
                    // }
                }
            }
            
            if (current.Connections != null)
            {
                foreach (var connection in current.Connections)
                {
                    if(closedSet.Contains(connection) || !connection.IsTraversable(layer)) continue;

                    // connection.DEBUG_Touched = true;
                    // DEBUG_TOUCHED_COUNT++;
                    
                    float tentativeGScore = current.GScore + WeightedDistanceToNeighbor(current, connection, layer);

                    if (openSetCells.Contains(connection))
                    {
                        if (tentativeGScore < connection.GScore)
                        {
                            connection.GScore = tentativeGScore;
                            connection.FScore = connection.GScore + connection.H;
                            cellPath[connection] = current;
                        }
                    }
                    else
                    {
                        connection.GScore = tentativeGScore;
                        connection.H = DistanceToTarget(connection, targetCell);
                        connection.FScore = connection.GScore + connection.H;
                        // openSet.Enqueue(neighbor, neighbor.FScore);
                        openSet.Enqueue(connection);
                        openSetCells.Add(connection);
                        cellPath[connection] = current;
                    }
                }
            }
            
            // if (current.Connections != null)
            // {
            //     foreach (var connection in current.Connections)
            //     {
            //         if(closedSet.Contains(connection) || !connection.IsTraversable(layer)) continue;
            //         
            //         float tentativeGScore = current.GScore + WeightedDistanceToNeighbor(current, connection, layer);
            //         if (tentativeGScore < connection.GScore)
            //         {
            //             CellPath[connection] = current;
            //             connection.GScore = tentativeGScore;
            //             connection.FScore = tentativeGScore + DistanceToTarget(connection, targetCell);
            //
            //             if (!openSetCells.Contains(connection))
            //             {
            //                 openSet.Enqueue(connection, connection.FScore);
            //             }
            //         }
            //     }
            // }

        }
        
        return null;
    }

    

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
        List<Cell>? cellList = null;
        GenerateObstacleCells(obstacle, ref cellList);
        
        if (cellList == null) return false;
        obstacles.Add(obstacle, cellList);
        
        var cellValues = obstacle.GetCellValues();
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
        var cellValues = obstacle.GetCellValues();
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
        var cellValues = obstacle.GetCellValues();
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
    
    
    private void GenerateObstacleCells(IPathfinderObstacle obstacle, ref List<Cell>? cellList)
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
    private void GenerateCellList(Segment shape, ref List<Cell>? cellList)
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
    private void GenerateCellList(Circle shape, ref List<Cell>? cellList)
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
    private void GenerateCellList(Triangle shape, ref List<Cell>? cellList)
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
    private void GenerateCellList(Quad shape, ref List<Cell>? cellList)
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
    private void GenerateCellList(Rect rect, ref List<Cell>? cellList)
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
    private void GenerateCellList(Polygon shape, ref List<Cell>? cellList)
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
    private void GenerateCellList(Polyline shape, ref List<Cell>? cellList)
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
    private void OnAgentRequestedPath(IPathfinderAgent.PathRequest request)
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
            var path = GetPath(request.Start, request.End, request.Agent.GetLayer());
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
    
    public bool ApplyCellValue(int index, IPathfinderObstacle.CellValue value)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        cell.ApplyCellValue(value);
        return true;
    }
    public bool RemoveCellValue(int index, IPathfinderObstacle.CellValue value)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        cell.RemoveCellValue(value);
        return true;
    }
    public bool ApplyCellValues(int index, params IPathfinderObstacle.CellValue[] values)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        cell.ApplyCellValues(values);
        return true;
    }
    public bool RemoveCellValues(int index, params IPathfinderObstacle.CellValue[] values)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        cell.RemoveCellValues(values);
        return true;
    }
    
    #endregion
    
    #region Position
    
    public bool ApplyCellValue(Vector2 position, IPathfinderObstacle.CellValue value)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        cell.ApplyCellValue(value);
        return true;
    }
    public bool RemoveCellValue(Vector2 position, IPathfinderObstacle.CellValue value)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        cell.RemoveCellValue(value);
        return true;
    }
    public bool ApplyCellValues(Vector2 position, IEnumerable<IPathfinderObstacle.CellValue> values)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        cell.ApplyCellValues(values);
        return true;
    }
    public bool RemoveCellValues(Vector2 position, IEnumerable<IPathfinderObstacle.CellValue> values)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        cell.RemoveCellValues(values);
        return true;
    }

    #endregion
    
    #region Segment
    public int ApplyCellValue(Segment shape, IPathfinderObstacle.CellValue value)
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
    public int RemoveCellValue(Segment shape, IPathfinderObstacle.CellValue value)
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
    public int ApplyCellValues(Segment shape, params IPathfinderObstacle.CellValue[] values)
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
    public int RemoveCellValues(Segment shape, params IPathfinderObstacle.CellValue[] values)
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
    public int ApplyCellValue(Circle shape, IPathfinderObstacle.CellValue value)
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
    public int RemoveCellValue(Circle shape, IPathfinderObstacle.CellValue value)
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
    public int ApplyCellValues(Circle shape, params IPathfinderObstacle.CellValue[] values)
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
    public int RemoveCellValues(Circle shape, params IPathfinderObstacle.CellValue[] values)
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
    public int ApplyCellValue(Triangle shape, IPathfinderObstacle.CellValue value)
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
    public int RemoveCellValue(Triangle shape, IPathfinderObstacle.CellValue value)
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
    public int ApplyCellValues(Triangle shape, params IPathfinderObstacle.CellValue[] values)
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
    public int RemoveCellValues(Triangle shape, params IPathfinderObstacle.CellValue[] values)
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
    public int ApplyCellValue(Rect shape, IPathfinderObstacle.CellValue value)
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
    public int RemoveCellValue(Rect shape, IPathfinderObstacle.CellValue value)
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
    public int ApplyCellValues(Rect shape, params IPathfinderObstacle.CellValue[] values)
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
    public int RemoveCellValues(Rect shape, params IPathfinderObstacle.CellValue[] values)
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
    public int ApplyCellValue(Quad shape, IPathfinderObstacle.CellValue value)
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
    public int RemoveCellValue(Quad shape, IPathfinderObstacle.CellValue value)
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
    public int ApplyCellValues(Quad shape, params IPathfinderObstacle.CellValue[] values)
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
    public int RemoveCellValues(Quad shape, params IPathfinderObstacle.CellValue[] values)
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
    public int ApplyCellValue(Polygon shape, IPathfinderObstacle.CellValue value)
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
    public int RemoveCellValue(Polygon shape, IPathfinderObstacle.CellValue value)
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
    public int ApplyCellValues(Polygon shape, params IPathfinderObstacle.CellValue[] values)
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
    public int RemoveCellValues(Polygon shape, params IPathfinderObstacle.CellValue[] values)
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
    public int ApplyCellValue(Polyline shape, IPathfinderObstacle.CellValue value)
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
    public int RemoveCellValue(Polyline shape, IPathfinderObstacle.CellValue value)
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
    public int ApplyCellValues(Polyline shape, params IPathfinderObstacle.CellValue[] values)
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
    public int RemoveCellValues(Polyline shape, params IPathfinderObstacle.CellValue[] values)
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
    protected virtual void WasReset(){}
    protected virtual void RegenerationWasRequested() { }
    protected virtual void WasCleared(){}
    #endregion

    #region Private
    
    
    #region GetCells

    private Cell? GetCell(int index)
    {
        if (!Grid.IsIndexInBounds(index)) return null;
        return cells[index];
    }
    private Cell? GetCell(Grid.Coordinates coordinates)
    {
        if (!Grid.AreCoordinatesInside(coordinates)) return null;
        var index = Grid.CoordinatesToIndex(coordinates);
        if (index >= 0 && index < cells.Count) return cells[index];
        return null;
    }
    private Cell? GetCell(Vector2 position)
    {
        var index = Grid.GetCellIndexUnclamped(position, bounds);
        if (!Grid.IsIndexInBounds(index)) return null;
        return cells[index];
    }
    private Cell GetCellClamped(Vector2 position)
    {
        return cells[Grid.GetCellIndex(position, bounds)];
    }
    private int GetCells(Segment segment, ref HashSet<Cell> result)
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
    private int GetCells(Circle circle, ref HashSet<Cell> result)
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
    private int GetCells(Triangle triangle, ref HashSet<Cell> result)
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
    private int GetCells(Quad quad, ref HashSet<Cell> result)
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
    private int GetCells(Rect rect, ref HashSet<Cell> result)
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
    private int GetCells(Polygon polygon, ref HashSet<Cell> result)
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
    private int GetCells(Polyline polyline, ref HashSet<Cell> result)
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
    private float DistanceToTarget(Cell current, Cell target)
    {
        // return (current.Rect.Center - target.Rect.Center).Length();
        
        var cc = current.Coordinates;
        var nc = target.Coordinates;

        var c = nc - cc;
        return c.Distance;

        // if (RelaxationPower <= 1f) return c.Distance;
        
        // return MathF.Pow(c.Distance, RelaxationPower);
        // return MathF.Pow(c.Distance, 4);

        // return c.Col > c.Row ? c.Col : c.Row;
        // const float relaxationValue = 1;
        // return c.Distance * relaxationValue;

        // if (factor <= 0f) return c.Distance;
        // var dis = c.Distance;
        // return dis * (dis * factor);
        
    }
    private float WeightedDistanceToNeighbor(Cell current, Cell neighbor, uint layer)
    {
        
        // return (current.Rect.Center - neighbor.Rect.Center).Length() * neighbor.GetWeightFactor(layer);
        var cc = current.Coordinates;
        var nc = neighbor.Coordinates;

        var c = nc - cc;
        if (c.Col != 0 && c.Row != 0) return DiagonalLength * neighbor.GetWeight(layer);//diagonal
        return 1f * neighbor.GetWeight(layer);
        // return c.Distance * neighbor.GetWeightFactor(layer);
        
        
        // var dis = (current.Rect.Center - neighbor.Rect.Center).Length();
        // return dis * neighbor.Weight;
    }
    private List<Rect> ReconstructPath(Cell from, int capacityEstimate)
    {
        List<Rect> rects = new(capacityEstimate) { from.Rect };

        var current = from;

        do
        {
            if (cellPath.ContainsKey(current))
            {
                current = cellPath[current];
                rects.Add(current.Rect);
            }
            else current = null;

        } while (current != null);

        // if(cellPath.Count > 250) Console.WriteLine($"Cell Path Count: {cellPath.Count}, Capacity Estimate: {capacityEstimate}, Rect Count: {rects.Count}");

        rects.Reverse();
        return rects;
    }
    private Cell? GetClosestTraversableCell(Cell cell)
    {

        if (cell.Neighbors == null || cell.Neighbors.Count <= 0) return null;

        HashSet<Cell> lookedAt = new();
        List<Cell> nextNeighbors = new();
        List<Cell> currentNeighbors = new();
        foreach (var neighbor in cell.Neighbors)
        {
            currentNeighbors.Add(neighbor);
            lookedAt.Add(neighbor);
        }
        lookedAt.Add(cell);
        
        var minDisSq = float.PositiveInfinity;
        Cell? closestNeighbor = null;
        while (currentNeighbors.Count > 0)
        {
            foreach (var neighbor in currentNeighbors)
            {
                if (neighbor.IsTraversable())
                {
                    var disSq = (cell.Rect.Center - neighbor.Rect.Center).LengthSquared();
                    if (disSq < minDisSq)
                    {
                        minDisSq = disSq;
                        closestNeighbor = neighbor;
                    }
                }
                else nextNeighbors.Add(neighbor);
            }

            if (closestNeighbor != null) return closestNeighbor;
            currentNeighbors.Clear();
            GetNewNeighbors(nextNeighbors, ref lookedAt, ref currentNeighbors);
        }

        return null;
    }
    
    private void GetNewNeighbors(List<Cell> collection, ref HashSet<Cell> lookedAt, ref List<Cell> newNeighbors)
    {
        foreach (var cell in collection)
        {
            if(cell.Neighbors == null || cell.Neighbors.Count <= 0) continue;
            foreach (var neighbor in cell.Neighbors)
            {
                if(lookedAt.Contains(neighbor)) continue;
                lookedAt.Add(neighbor);
                newNeighbors.Add(neighbor);
            }
        }
    }
    
    private Cell? GetNeighborCell(Cell cell, Direction dir) => GetCell(cell.Coordinates + dir);
    private HashSet<Cell>? GetNeighbors(Cell cell, bool diagonal = true)
    {
        HashSet<Cell>? neighbors = null;
        var coordinates = cell.Coordinates;

        Cell? neighbor = null;
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
    private bool ConnectCells(Cell a, Cell b, bool oneWay)
    {
        if (a == b) return false;

        a.Connect(b);
        if (!oneWay) b.Connect(a);
        return true;
    }
    private bool DisconnectCells(Cell a, Cell b, bool oneWay)
    {
        if (a == b) return false;
        a.Disconnect(b);
        if (oneWay) b.Disconnect(a);
        return true;
    }
    #endregion
    
    private Rect GetRect(Grid.Coordinates coordinates)
    {
        var pos = Bounds.TopLeft + CellSize * coordinates.ToVector2();
        return new Rect(pos, CellSize, new Vector2(0f));
    }

    private void ResolveRegenerationRequested()
    {
        RegenerationWasRequested();
        OnRegenerationRequested?.Invoke(this);
    }
    private void ResolveReset()
    {
        WasReset();
        OnReset?.Invoke(this);
    }

    private void ResolveCleared()
    {
        WasCleared();
        OnCleared?.Invoke(this);
    }
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

    public List<Rect> GetTraversableRects(int layer)
    {
        var rects = new List<Rect>();
        foreach (var cell in cells)
        {
            if (cell.IsTraversable())
            {
                rects.Add(cell.Rect);
            }
        }

        return rects;
    }
    public List<Rect> GetRects(int minWeight, int maxWeight, int layer)
    {
        var rects = new List<Rect>();
        foreach (var cell in cells)
        {
            var w = cell.GetWeight();
            if(w >= minWeight && w <= maxWeight) rects.Add(cell.Rect);
        }

        return rects;
    }


    #endregion
}
