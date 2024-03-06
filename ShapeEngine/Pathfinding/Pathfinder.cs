using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Pathfinding;




// public interface INavigationObject : IShape
// {
//     public event Action<INavigationObject>? OnShapeChanged;
//     public event Action<INavigationObject, float>? OnValueChanged;
//
//
//     public float? GetOverrideValue() => null;
//     public float? GetBonusValue() => null;
//     public float? GetFlatValue() => null;
// }
// public class CellValue
// {
//     // public event Action<CellValue, float>? OnChanged;
//     
//     private float baseValue;
//     private float totalBonus = 0f;
//     private float totalFlat = 0f;
//     private float cur;
//     private bool dirty = false;
//
//     public float Override = -1f;
//     public float Base 
//     {
//         get => baseValue;
//         set
//         {
//             if (Math.Abs(baseValue - value) < 0.0001f) return;
//             baseValue = value;
//             dirty = true;
//         }
//     }
//     public float Cur
//     {
//         get
//         {
//             if(dirty)Recalculate();
//             return Override >= 0 ? Override : cur;
//         }
//         private set => cur = value;
//     }
//     public float TotalBonus
//     {
//         get => totalBonus;
//         set
//         {
//             if (Math.Abs(totalBonus - value) < 0.0001f) return;
//             totalBonus = value;
//             dirty = true;
//         }
//     }
//     public float TotalFlat 
//     {
//         get => totalFlat;
//         set
//         {
//             if (Math.Abs(totalFlat - value) < 0.0001f) return;
//             totalFlat = value;
//             dirty = true;
//         }
//     }
//         
//
//     public CellValue(float baseValue)
//     {
//         this.baseValue = baseValue;
//         cur = baseValue;
//     }
//
//
//     public void Reset()
//     {
//         totalBonus = 0;
//         totalFlat = 0;
//         Override = -1;
//         Recalculate();
//     }
//     private void Recalculate()
//     {
//         dirty = false;
//         float old = Cur;
//         if (TotalBonus >= 0f)
//         {
//             Cur = (Base + TotalFlat) * (1f + TotalBonus);
//         }
//         else
//         {
//             Cur = (Base + TotalFlat) / (1f + MathF.Abs(TotalBonus));
//         }
//
//         // if (Math.Abs(Cur - old) > 0.0001f) OnChanged?.Invoke(this, old);
//     }
// }

public interface IPathfinderObstacle : IShape
{
    
}

public interface IPathfinderAgent
{
    public class PathRequest
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
    }
    public event Action<PathRequest> OnRequestPath;

    public bool ReceiveRequestedPath(Pathfinder.Path? path);
    public uint GetLayer();
    public void AddedToPathfinder(Pathfinder pathfinder);
    public void RemovedFromPathfinder(Pathfinder pathfinder);

}
public class Pathfinder
{

    public static readonly float Blocked = 0;
    public static readonly float Default = 1;
    public static readonly float DiagonalLength = MathF.Sqrt(2f);
    
    private class CellQueue
    {
        
        private readonly List<Cell> cells = new();
        // private static readonly Comparer<Cell> comparer = Comparer<Cell>.Create((x, y) => (int)(x.FScore - y.FScore));

        public int Count => cells.Count;
        public void Enqueue(Cell cell)
        {
            // if (cells == null)
            // {
            //     cells = new() { cell };
            //     return;
            // }
            cells.Add(cell);
            // if(cells.Count <= 0) cells.Add(cell);
            
            // int index = cells.BinarySearch(cell, comparer);
            // if (index < 0) index = ~index;
            // cells.Insert(index, cell);
        }

        public Cell Dequeue()
        {
            int minIndex = 0;
            float minScore = float.PositiveInfinity;
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell.FScore < minScore)
                {
                    minScore = cell.FScore;
                    minIndex = i;
                }
            }

            var c = cells[minIndex];
            cells.RemoveAt(minIndex);
            
            return c;
            // if (cells is not { Count: > 0 }) return null;
            // int lastIndex = cells.Count - 1;
            // var cell = cells[lastIndex];
            // cells.RemoveAt(lastIndex);
            // return cell;
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
    private class Cell
    {
        private readonly Pathfinder parent;

        // internal bool DEBUG_Touched = false;
        
        public Rect Rect
        {
            get
            {
                var pos = parent.Bounds.TopLeft + parent.CellSize * Coordinates.ToVector2();
                return new Rect(pos, parent.CellSize, new Vector2(0f));
            }
        }
        public readonly Grid.Coordinates Coordinates;
        
        private float weight = 1;
        private Dictionary<uint, float>? weights = null;

        internal HashSet<Cell>? Neighbors = null;
        internal HashSet<Cell>? Connections = null;
        internal float GScore = float.PositiveInfinity;
        internal float FScore = float.PositiveInfinity;
        internal float H = float.PositiveInfinity;
        
        public Cell(Grid.Coordinates coordinates, Pathfinder parent)
        {
            Coordinates = coordinates;
            this.parent = parent;
            // Value = new(1f);
        }
        public void Reset()
        {
            Connections?.Clear();
            weight = Default;
            weights?.Clear();
            // DEBUG_Touched = false;
        }

        public void Clear()
        {
            weight = Default;
            weights?.Clear();
        }
        
        public bool HasNeighbors => Neighbors is { Count: > 0 };
        public bool HasConnections => Connections is { Count: > 0 };
        
        #region Neighbors & Connections
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
        
        #region Weight
        private float CalculateWeightFactor(float w)
        {
            if (w == 0) return 0f;
            if (w < 0) return w * -1;
            return 1f / w;
        }
        public bool IsTraversable() => weight > Blocked || weight < Blocked;
        public bool IsTraversable(uint layer)
        {
            if (weights == null) return IsTraversable();
            if (weights.TryGetValue(layer, out float value)) return value > Blocked || value < Blocked;
            return weight > Blocked || weight < Blocked;
        }
        public void SetWeight(float value)
        {
            weight = value;
        }
        public void SetWeight(float value, uint layer)
        {
            weights ??= new();
            weights[layer] = value;
        }
        public void ChangeWeight(float factor)
        {
            if (weight == 0) return;
            if (factor == 0f) weight = 0;
            if (weight < 0)weight /= factor;
            else weight *= factor;
        }
        public void ChangeWeight(float factor, uint layer)
        {
            if (weights == null) return;
            if (!weights.TryGetValue(layer, out float w)) return;
            if (w == 0) return;
            
            if (factor == 0f) w = 0;
            if (weight < 0) w /= factor;
            else w *= factor;
            
            weights[layer] = w;
        }

        public float GetWeight() => weight;
        public float GetWeight(uint layer) => weights?.GetValueOrDefault(layer, weight) ?? weight;
        public float GetWeightFactor() => CalculateWeightFactor(weight);
        public float GetWeightFactor(uint layer) => CalculateWeightFactor(GetWeight(layer));

        public bool ClearWeight(uint layer) => weights?.Remove(layer) ?? false;
        public bool HasWeight(uint layer) => weights?.ContainsKey(layer) ?? false;
        public int ClearLayerWeights()
        {
            if (weights == null) return 0;
            int count = weights.Count;
            weights.Clear();
            return count;
        }
        #endregion
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
    public Size CellSize { get; private set; }
    public readonly Grid Grid;
    
    #endregion

    #region Private
    private Rect bounds;
    private readonly List<Cell> cells;
    private HashSet<Cell> cellHelper1 = new();
    private HashSet<Cell> cellHelper2 = new();
    private HashSet<Cell> resultSet = new();
    private readonly HashSet<Cell> closedSet = new();
    private readonly Dictionary<Cell, Cell> cellPath = new();

    /// <summary>
    /// How many agent requests are handled each frame
    /// smaller or equal to zero handles all incoming requests
    /// </summary>
    public int RequestsPerFrame = 0;
    private readonly List<IPathfinderAgent.PathRequest> pathRequests = new();
    private readonly Dictionary<IPathfinderAgent, IPathfinderAgent.PathRequest> agents = new();
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
        cells = new();
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

    public void Reset()
    {
        foreach (var cell in cells)
        {
            cell.Reset();
        }
        ResolveReset();
    }
    public void Clear()
    {
        foreach (var cell in cells)
        {
            cell.Clear();
        }
    }
    public void Clear(Rect rect)
    {
        cellHelper1.Clear();
        var count = GetCells(rect, ref cellHelper1);
        if (count <= 0) return;
        foreach (var cell in cellHelper1)
        {
            cell.Clear();
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
    public bool IsTraversable(int index) => GetCell(index)?.IsTraversable() ?? false;
    public bool IsTraversable(int index, uint layer) => GetCell(index)?.IsTraversable(layer) ?? false;
    
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
        
        
        //todo make members to save memory ?
        // PriorityQueue<Cell, float> openSet = new();
        CellQueue openSet = new();
        HashSet<Cell> openSetCells = new();
        closedSet.Clear();
        // HashSet<Cell> closedSet = new();

        // var startCell = GetCell(start);
        // var targetCell = GetCell(end);

        startCell.GScore = 0;
        startCell.H = DistanceToTarget(startCell, targetCell);
        startCell.FScore = startCell.H;
        // openSet.Enqueue(startCell, startCell.FScore);
        openSet.Enqueue(startCell);
        openSetCells.Add(startCell);


        Cell current;
        while (openSet.Count > 0)
        {
            current = openSet.Dequeue();
            
            if (openSetCells.Count > DEBUG_MAX_OPEN_SET_COUNT)
            {
                DEBUG_MAX_OPEN_SET_COUNT = openSetCells.Count;
            }
            
            if (current == targetCell)
            {
                var rects = ReconstructPath(current, cellPath);
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
    
    #region Agent System

    public bool AddAgent(IPathfinderAgent agent)
    {
        if (!agents.TryAdd(agent, new())) return false;

        agent.OnRequestPath += OnAgentRequestedPath;
        agent.AddedToPathfinder(this);
        return true;
    }
    public bool RemoveAgent(IPathfinderAgent agent)
    {
        if (!agents.Remove(agent, out var request)) return false;
        pathRequests.Remove(request);
        
        agent.OnRequestPath -= OnAgentRequestedPath;
        return true;
    }
    private void OnAgentRequestedPath(IPathfinderAgent.PathRequest request)
    {
        if (request.Agent == null) return;
        
        var prevRequest = agents[request.Agent];
        pathRequests.Remove(prevRequest);
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

        int endIndex = RequestsPerFrame > 0 ? ShapeMath.MaxInt(pathRequests.Count - RequestsPerFrame, 0) : 0;
        
        for (int i = pathRequests.Count - 1; i >= endIndex; i--)
        {
            var request = pathRequests[i];
            pathRequests.RemoveAt(i);
            if (request.Agent == null) continue;
            var path = GetPath(request.Start, request.End, request.Agent.GetLayer());
            if(path == null) continue;
            request.Agent.ReceiveRequestedPath(path);
        }
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

    public bool SetCellValues(int index, float value)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        cell.SetWeight(value);
        return true;
    }
    public bool ChangeCellValues(int index, float factor)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        cell.ChangeWeight(factor);
        return true;
    }
    
    public bool SetCellValues(int index, float value, uint layer)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        cell.SetWeight(value, layer);
        return true;
    }
    public bool ChangeCellValues(int index, float factor, uint layer)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        cell.ChangeWeight(factor, layer);
        return true;
    }

    public bool SetCellValues(int index, float value, uint[] layers)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        foreach (var layer in layers)
        {
            cell.SetWeight(value, layer);
        }
        return true;
    }
    public bool ChangeCellValues(int index, float factor, uint[] layers)
    {
        var cell = GetCell(index);
        if (cell == null) return false;
        foreach (var layer in layers)
        {
            cell.ChangeWeight(factor, layer);
        }
        return true;
    }
    

    #endregion
    
    #region Position

    public bool SetCellValues(Vector2 position, float value)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        cell.SetWeight(value);
        return true;
    }
    public bool ChangeCellValues(Vector2 position, float factor)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        cell.ChangeWeight(factor);
        return true;
    }
    
    public bool SetCellValues(Vector2 position, float value, uint layer)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        cell.SetWeight(value, layer);
        return true;
    }
    public bool ChangeCellValues(Vector2 position, float factor, uint layer)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        cell.ChangeWeight(factor, layer);
        return true;
    }

    public bool SetCellValues(Vector2 position, float value, uint[] layers)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        foreach (var layer in layers)
        {
            cell.SetWeight(value, layer);
        }
        return true;
    }
    public bool ChangeCellValues(Vector2 position, float factor, uint[] layers)
    {
        var cell = GetCell(position);
        if (cell == null) return false;
        foreach (var layer in layers)
        {
            cell.ChangeWeight(factor, layer);
        }
        return true;
    }

    #endregion
    
    #region Segment
    public int SetCellValues(Segment shape, float value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.SetWeight(value);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Segment shape, float factor)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor);
        }
    
        return cellCount;
    }
    
    public int SetCellValues(Segment shape, float value, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.SetWeight(value, layer);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Segment shape, float factor, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor, layer);
        }
    
        return cellCount;
    }

    public int SetCellValues(Segment shape, float value, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.SetWeight(value);
                continue;
            }
            foreach (var layer in layers)
            {
                cell.SetWeight(value, layer);
            }
            
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Segment shape, float factor, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.ChangeWeight(factor);
                continue;
            }
            
            foreach (var layer in layers)
            {
                cell.ChangeWeight(factor, layer);
            }
            
        }
    
        return cellCount;
    }
    
    #endregion
    
    #region Circle
    public int SetCellValues(Circle shape, float value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.SetWeight(value);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Circle shape, float factor)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor);
        }
    
        return cellCount;
    }
    
    public int SetCellValues(Circle shape, float value, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.SetWeight(value, layer);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Circle shape, float factor, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor, layer);
        }
    
        return cellCount;
    }

    public int SetCellValues(Circle shape, float value, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.SetWeight(value);
                continue;
            }
            foreach (var layer in layers)
            {
                cell.SetWeight(value, layer);
            }
            
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Circle shape, float factor, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.ChangeWeight(factor);
                continue;
            }
            
            foreach (var layer in layers)
            {
                cell.ChangeWeight(factor, layer);
            }
            
        }
    
        return cellCount;
    }
    #endregion
    
    #region Triangle
    public int SetCellValues(Triangle shape, float value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.SetWeight(value);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Triangle shape, float factor)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor);
        }
    
        return cellCount;
    }
    
    public int SetCellValues(Triangle shape, float value, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.SetWeight(value, layer);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Triangle shape, float factor, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor, layer);
        }
    
        return cellCount;
    }

    public int SetCellValues(Triangle shape, float value, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.SetWeight(value);
                continue;
            }
            foreach (var layer in layers)
            {
                cell.SetWeight(value, layer);
            }
            
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Triangle shape, float factor, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.ChangeWeight(factor);
                continue;
            }
            
            foreach (var layer in layers)
            {
                cell.ChangeWeight(factor, layer);
            }
            
        }
    
        return cellCount;
    }
    #endregion
    
    #region Quad
    public int SetCellValues(Quad shape, float value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.SetWeight(value);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Quad shape, float factor)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor);
        }
    
        return cellCount;
    }
    
    public int SetCellValues(Quad shape, float value, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.SetWeight(value, layer);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Quad shape, float factor, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor, layer);
        }
    
        return cellCount;
    }

    public int SetCellValues(Quad shape, float value, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.SetWeight(value);
                continue;
            }
            foreach (var layer in layers)
            {
                cell.SetWeight(value, layer);
            }
            
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Quad shape, float factor, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.ChangeWeight(factor);
                continue;
            }
            
            foreach (var layer in layers)
            {
                cell.ChangeWeight(factor, layer);
            }
            
        }
    
        return cellCount;
    }
    #endregion
    
    #region Rect
    public int SetCellValues(Rect shape, float value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.SetWeight(value);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Rect shape, float factor)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor);
        }
    
        return cellCount;
    }
    
    public int SetCellValues(Rect shape, float value, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.SetWeight(value, layer);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Rect shape, float factor, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor, layer);
        }
    
        return cellCount;
    }

    public int SetCellValues(Rect shape, float value, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.SetWeight(value);
                continue;
            }
            foreach (var layer in layers)
            {
                cell.SetWeight(value, layer);
            }
            
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Rect shape, float factor, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.ChangeWeight(factor);
                continue;
            }
            
            foreach (var layer in layers)
            {
                cell.ChangeWeight(factor, layer);
            }
            
        }
    
        return cellCount;
    }
    #endregion
    
    #region Polygon
    public int SetCellValues(Polygon shape, float value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            // if(shape.ContainsShape(cell.Rect)) cell.SetWeight(0);
            // else cell.SetWeight(value);
            cell.SetWeight(value);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Polygon shape, float factor)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor);
        }
    
        return cellCount;
    }
    
    public int SetCellValues(Polygon shape, float value, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.SetWeight(value, layer);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Polygon shape, float factor, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor, layer);
        }
    
        return cellCount;
    }

    public int SetCellValues(Polygon shape, float value, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.SetWeight(value);
                continue;
            }
            foreach (var layer in layers)
            {
                cell.SetWeight(value, layer);
            }
            
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Polygon shape, float factor, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.ChangeWeight(factor);
                continue;
            }
            
            foreach (var layer in layers)
            {
                cell.ChangeWeight(factor, layer);
            }
            
        }
    
        return cellCount;
    }
    #endregion

    #region Polyline
    public int SetCellValues(Polyline shape, float value)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.SetWeight(value);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Polyline shape, float factor)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor);
        }
    
        return cellCount;
    }
    
    public int SetCellValues(Polyline shape, float value, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.SetWeight(value, layer);
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Polyline shape, float factor, uint layer)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            cell.ChangeWeight(factor, layer);
        }
    
        return cellCount;
    }

    public int SetCellValues(Polyline shape, float value, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.SetWeight(value);
                continue;
            }
            foreach (var layer in layers)
            {
                cell.SetWeight(value, layer);
            }
            
        }
    
        return cellCount;
    }
    public int ChangeCellValues(Polyline shape, float factor, uint[] layers)
    {
        resultSet.Clear();
        var cellCount = GetCells(shape, ref resultSet);
        if (cellCount <= 0) return 0;
        foreach (var cell in resultSet)
        {
            if (layers.Length <= 0)
            {
                cell.ChangeWeight(factor);
                continue;
            }
            
            foreach (var layer in layers)
            {
                cell.ChangeWeight(factor, layer);
            }
            
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
        if (c.Col != 0 && c.Row != 0) return DiagonalLength * neighbor.GetWeightFactor(layer);//diagonal
        return 1f *neighbor.GetWeightFactor(layer);
        // return c.Distance * neighbor.GetWeightFactor(layer);
        
        
        // var dis = (current.Rect.Center - neighbor.Rect.Center).Length();
        // return dis * neighbor.Weight;
    }
    private List<Rect> ReconstructPath(Cell from, Dictionary<Cell, Cell> cellPath)
    {
        List<Rect> rects = new() { from.Rect };

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

            if(closedSet.Contains(cell)) //touched
            {
                // r.DrawLines(4f, new ColorRgba(System.Drawing.Color.Azure));
                r.ScaleSize(0.9f, new Vector2(0.5f)).Draw(new ColorRgba(System.Drawing.Color.Bisque));
                // r.ScaleSize(0.5f, new Vector2(0.9f)).Draw(new ColorRgba(System.Drawing.Color.Red));
            }
            
            if(cell.GetWeight(layer) == 0) r.ScaleSize(0.5f, new Vector2(0.5f)).Draw(blocked);
            else if(cell.GetWeight(layer) < 1) r.ScaleSize(0.65f, new Vector2(0.5f)).Draw(undesirable);
            else if(cell.GetWeight(layer) > 1) r.ScaleSize(0.65f, new Vector2(0.5f)).Draw(desirable);
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
