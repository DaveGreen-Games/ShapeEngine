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

public abstract class Pathfinder
{

    public static readonly float Blocked = 0;
    public static readonly float Default = 1;
    
    
    public class Cell
    {
        private readonly Pathfinder parent;

        public bool DEBUG_Touched = false;
        
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
        internal float GScore = 0f;
        internal float FScore = 0f;
        
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
            DEBUG_Touched = false;
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
        public bool IsTraversable() => weight > Blocked;
        public bool IsTraversable(uint layer)
        {
            if (weights == null) return weight > Blocked;
            if (weights.TryGetValue(layer, out float value)) return value > Blocked;
            return weight > Blocked;
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
    public event Action<Pathfinder>? OnResetRequested;

    #region Members

    #region Public
    public Size CellSize { get; private set; }
    public readonly Grid Grid;
    protected Dictionary<Cell, Cell> CellPath = new();
    // protected List<float> GScores = new();
    // protected List<float> FScores = new();

    #endregion

    #region Private

    private Rect bounds;
    private readonly List<Cell> cells;
    private HashSet<Cell> cellHelper1 = new();
    private HashSet<Cell> cellHelper2 = new();
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

    private float DistanceToTarget(Cell current, Cell target)
    {
        var cc = current.Coordinates;
        var nc = target.Coordinates;

        var c = nc - cc;

        const float relaxationValue = 1;
        return c.Distance * relaxationValue;
        
        // return (current.Rect.Center - target.Rect.Center).Length();
    }

    private float WeightedDistanceToNeighbor(Cell current, Cell neighbor, uint layer)
    {
        var cc = current.Coordinates;
        var nc = neighbor.Coordinates;

        var c = nc - cc;

        return c.Distance * neighbor.GetWeightFactor(layer);
        
        
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
    public Path? GetPath(Vector2 start, Vector2 end, uint layer)
    {
        // GScore is the cost of the cheapest path from start to n currently known.
        // FScore represents our current best guess as to how cheap a path could be from start to finish if it goes through n.
        
        CellPath.Clear();
        foreach (var cell in cells)
        {
            cell.GScore = float.PositiveInfinity;
            cell.FScore = float.PositiveInfinity;
            // cell.DEBUG_Touched = false;
        }
        
        PriorityQueue<Cell, float> openSet = new();
        HashSet<Cell> openSetCells = new();
        HashSet<Cell> closedSet = new();

        var startCell = GetCell(start);
        var targetCell = GetCell(end);

        openSet.Enqueue(startCell, startCell.FScore);
        openSetCells.Add(startCell);

        startCell.GScore = 0;
        startCell.FScore = DistanceToTarget(startCell, targetCell);

        Cell current;
        while (openSet.Count > 0)
        {
            current = openSet.Dequeue();

            if (current == targetCell)
            {
                var rects = ReconstructPath(current, CellPath);
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
                    if (tentativeGScore < neighbor.GScore)
                    {
                        CellPath[neighbor] = current;
                        neighbor.GScore = tentativeGScore;
                        neighbor.FScore = tentativeGScore + DistanceToTarget(neighbor, targetCell);

                        if (!openSetCells.Contains(neighbor))
                        {
                            openSet.Enqueue(neighbor, neighbor.FScore);
                            neighbor.DEBUG_Touched = true;
                        }
                    }
                }
            }
            
            if (current.Connections != null)
            {
                foreach (var connection in current.Connections)
                {
                    if(closedSet.Contains(connection) || !connection.IsTraversable(layer)) continue;
                    
                    float tentativeGScore = current.GScore + WeightedDistanceToNeighbor(current, connection, layer);
                    if (tentativeGScore < connection.GScore)
                    {
                        CellPath[connection] = current;
                        connection.GScore = tentativeGScore;
                        connection.FScore = tentativeGScore + DistanceToTarget(connection, targetCell);

                        if (!openSetCells.Contains(connection))
                        {
                            openSet.Enqueue(connection, connection.FScore);
                        }
                    }
                }
            }

        }
        
        return null;
    }

    
    #endregion
    
    #region Connections

    public bool AddConnections(Vector2 a, Vector2 b, bool oneWay)
    {
        var cellA = GetCell(a);
        var cellB = GetCell(b);
        return ConnectCells(cellA, cellB, oneWay);
    }
    public void AddConnections(Vector2 a, Rect b, bool oneWay)
    {
        cellHelper1.Clear();
        if (GetCells(b, ref cellHelper1) <= 0) return;
        var cellA = GetCell(a);
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
        var cellB = GetCell(b);
        return DisconnectCells(cellA, cellB, oneWay);
    }
    public void RemoveConnections(Vector2 a, Rect b, bool oneWay)
    {
        cellHelper1.Clear();
        if (GetCells(b, ref cellHelper1) <= 0) return;
        var cellA = GetCell(a);
        foreach (var cell in cellHelper1)
        {
            DisconnectCells(cellA, cell, oneWay);
        }

    }
    public void RemoveConnections(Rect a, Vector2 b, bool oneWay)
    {
        cellHelper1.Clear();
        if (GetCells(a, ref cellHelper2) <= 0) return;
        var cellB = GetCell(b);
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
    
    #region Virtual

    protected virtual void ResetWasRequested(){}
    protected virtual void RegenerationWasRequested() { }

    #endregion

    #region Protected

    protected Rect GetRect(int index)
    {
        var coordinates = Grid.IndexToCoordinates(index);
        var pos = Bounds.TopLeft + CellSize * coordinates.ToVector2();
        return new Rect(pos, CellSize, new Vector2(0f));
    }
    protected Rect GetRect(Grid.Coordinates coordinates)
    {
        var pos = Bounds.TopLeft + CellSize * coordinates.ToVector2();
        return new Rect(pos, CellSize, new Vector2(0f));
    }

    protected Cell? GetCell(int index)
    {
        if (!Grid.IsIndexInBounds(index)) return null;
        return cells[index];
    }
    protected Cell? GetCell(Grid.Coordinates coordinates)
    {
        if (!Grid.AreCoordinatesInside(coordinates)) return null;
        var index = Grid.CoordinatesToIndex(coordinates);
        if (index >= 0 && index < cells.Count) return cells[index];
        return null;
    }

    protected Cell GetCell(Vector2 pos)
    {
        var index = Grid.GetCellIndex(pos, Bounds);
        return cells[index];
    }
    
    protected int GetCells(Segment segment, ref HashSet<Cell> result)
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
    protected int GetCells(Circle circle, ref HashSet<Cell> result)
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
    protected int GetCells(Triangle triangle, ref HashSet<Cell> result)
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
    protected int GetCells(Quad quad, ref HashSet<Cell> result)
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
    protected int GetCells(Rect rect, ref HashSet<Cell> result)
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
    protected int GetCells(Polygon polygon, ref HashSet<Cell> result)
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
    protected int GetCells(Polyline polyline, ref HashSet<Cell> result)
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
   
    #region Private

    
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
    private void ResolveRegenerationRequested()
    {
        RegenerationWasRequested();
        OnRegenerationRequested?.Invoke(this);
    }
    private void ResolveReset()
    {
        ResetWasRequested();
        OnResetRequested?.Invoke(this);
    }

    #endregion

    public void DrawDebug(ColorRgba bounds, ColorRgba standard, ColorRgba blocked, ColorRgba desirable, ColorRgba undesirable, uint layer)
    {
        Bounds.DrawLines(12f, bounds);
        foreach (var cell in cells)
        {
            var r = cell.Rect;

            if (cell.DEBUG_Touched)
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
}


public class PathfinderStatic : Pathfinder
{
    private HashSet<Cell> resultSet = new();
    public PathfinderStatic(Rect bounds, int cols, int rows) : base(bounds, cols, rows)
    {
    }

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
    
    
    // public void SetCellValues(Rect rect, Func<int, int> setCellValue)
    // {
    //     resultSet.Clear();
    //     var cellCount = GetCells(rect, ref resultSet);
    //     if (cellCount <= 0) return;
    //     foreach (var cell in resultSet)
    //     {
    //         cell.SetWeight(setCellValue(cell.GetWeight()));
    //     }
    // }
    // public void SetCellValues(Rect rect, Func<int, int> setCellValue, uint layer)
    // {
    //     resultSet.Clear();
    //     var cellCount = GetCells(rect, ref resultSet);
    //     if (cellCount <= 0) return;
    //     foreach (var cell in resultSet)
    //     {
    //         cell.SetWeight(setCellValue(cell.GetWeight(layer)), layer);
    //     }
    // }
    // public void SetCellValues(Rect rect, Func<int, int> setCellValue, uint[] layers)
    // {
    //     resultSet.Clear();
    //     var cellCount = GetCells(rect, ref resultSet);
    //     if (cellCount <= 0) return;
    //     foreach (var cell in resultSet)
    //     {
    //         foreach (var layer in layers)
    //         {
    //             cell.SetWeight(setCellValue(cell.GetWeight(layer)), layer);
    //         }
    //     }
    // }
    //

}


//works like spatial hash
//has an internal set of objects
//clears all cells and goes through all objects and fills cells based on their shape with the specified value
public class PathfinderDynamic : Pathfinder
{
    //has a list of objects that set values of cells
    //grid is cleared and recalculated each interval (every frame or every x seconds)
    
    public PathfinderDynamic(Rect bounds, int cols, int rows) : base(bounds, cols, rows)
    {
    }

    // public override bool GetPath(Vector2 start, Vector2 end, ref Path? result)
    // {
    //     throw new NotImplementedException();
    // }
}