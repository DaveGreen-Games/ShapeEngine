using System.IO.Pipes;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Stats;

namespace ShapeEngine.Pathfinding;

public interface INavigationObject : IShape
{
    public event Action<INavigationObject>? OnShapeChanged;
    public event Action<INavigationObject, float>? OnValueChanged;


    public float? GetOverrideValue() => null;
    public float? GetBonusValue() => null;
    public float? GetFlatValue() => null;
}

public class Path
{
    
}
public class CellValue
{
    // public event Action<CellValue, float>? OnChanged;
    
    private float baseValue;
    private float totalBonus = 0f;
    private float totalFlat = 0f;
    private float cur;
    private bool dirty = false;

    public float Override = -1f;
    public float Base 
    {
        get => baseValue;
        set
        {
            if (Math.Abs(baseValue - value) < 0.0001f) return;
            baseValue = value;
            dirty = true;
        }
    }
    public float Cur
    {
        get
        {
            if(dirty)Recalculate();
            return Override >= 0 ? Override : cur;
        }
        private set => cur = value;
    }
    public float TotalBonus
    {
        get => totalBonus;
        set
        {
            if (Math.Abs(totalBonus - value) < 0.0001f) return;
            totalBonus = value;
            dirty = true;
        }
    }
    public float TotalFlat 
    {
        get => totalFlat;
        set
        {
            if (Math.Abs(totalFlat - value) < 0.0001f) return;
            totalFlat = value;
            dirty = true;
        }
    }
        

    public CellValue(float baseValue)
    {
        this.baseValue = baseValue;
        cur = baseValue;
    }


    public void Reset()
    {
        totalBonus = 0;
        totalFlat = 0;
        Override = -1;
        Recalculate();
    }
    private void Recalculate()
    {
        dirty = false;
        float old = Cur;
        if (TotalBonus >= 0f)
        {
            Cur = (Base + TotalFlat) * (1f + TotalBonus);
        }
        else
        {
            Cur = (Base + TotalFlat) / (1f + MathF.Abs(TotalBonus));
        }

        // if (Math.Abs(Cur - old) > 0.0001f) OnChanged?.Invoke(this, old);
    }
}

public class Cell
{
    private readonly Pathfinder parent;

    public Rect Rect
    {
        get
        {
            var pos = parent.Bounds.TopLeft + parent.CellSize * Coordinates.ToVector2();
            return new Rect(pos, parent.CellSize, new Vector2(0f));
        }
    }
    public Grid.Coordinates Coordinates;
    public HashSet<Cell>? Neighbors = null;
    public HashSet<Cell>? Connections = null;
    public bool Traversable => Value.Cur > 0;

    public bool HasNeighbors => Neighbors is { Count: > 0 };
    public bool HasConnections => Connections is { Count: > 0 };
    
    /// <summary>
    /// 0 = Blocked/Not Traversable, smaller than 1 -> decrease worth, bigger than 1 -> increase worth
    /// </summary>
    public CellValue Value;

    public Cell(Grid.Coordinates coordinates, Pathfinder parent)
    {
        Coordinates = coordinates;
        this.parent = parent;
        Value = new(1f);
    }
    
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

    public void Reset()
    {
        Connections?.Clear();
        Value.Reset();
    }
}

//make abstract and just handle cells and their values
public abstract class Pathfinder
{

    public event Action<Pathfinder>? OnRegenerationRequested;
    public event Action<Pathfinder>? OnResetRequested;

    #region Members

    #region Public
    public Size CellSize { get; private set; }
    public readonly Grid Grid;

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
    public abstract bool GetPath(Vector2 start, Vector2 end, ref Path? result);
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

    private void ResolveReset()
    {
        ResetWasRequested();
        OnResetRequested?.Invoke(this);
    }
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
    // private void Regenerate()
    // {
    //     CellSize = Grid.GetCellSize(Bounds);// CalculateCellSize();
    //     // RecalculateCellRects();
    // }
    // // private void RecalculateCellRects()
    // // {
    // //     foreach (var cell in cells)
    // //     {
    // //         var coordinates = cell.Coordinates;
    // //         cell.Rect = GetRect(coordinates);
    // //     }
    // // }
    
    
    private void ResolveRegenerationRequested()
    {
        RegenerationWasRequested();
        OnRegenerationRequested?.Invoke(this);
    }

    #endregion

    public void DrawDebug(ColorRgba bounds, ColorRgba standard, ColorRgba blocked, ColorRgba changed)
    {
        Bounds.DrawLines(12f, bounds);
        foreach (var cell in cells)
        {
            var r = cell.Rect;
            if(cell.Value.Cur <= 0) r.ScaleSize(0.5f, new Vector2(0.5f)).Draw(blocked);
            else if(Math.Abs(cell.Value.Cur - 1f) > 0.0001f) r.ScaleSize(0.65f, new Vector2(0.5f)).Draw(changed);
            else r.ScaleSize(0.8f, new Vector2(0.5f)).Draw(standard);
        }
        
    }
}


public class PathfinderStatic : Pathfinder
{
    public PathfinderStatic(Rect bounds, int cols, int rows) : base(bounds, cols, rows)
    {
    }

    public override bool GetPath(Vector2 start, Vector2 end, ref Path? result)
    {
        throw new NotImplementedException();
    }

    public void AddBonus(Vector2 pos, float bonus)
    {
        var cell = GetCell(pos);
        cell.Value.TotalBonus += bonus;
    }
    public void AddFlat(Vector2 pos, float flat)
    {
        var cell = GetCell(pos);
        cell.Value.TotalFlat += flat;
    }
    public int AddFlat(Rect rect, float flat)
    {
        HashSet<Cell> result = new();
        var cellCount = GetCells(rect, ref result);
        if (cellCount <= 0) return 0;
        foreach (var cell in result)
        {
            cell.Value.TotalFlat += flat;
        }

        return cellCount;
    }
    public int SetValue(Rect rect, float value)
    {
        HashSet<Cell> result = new();
        var cellCount = GetCells(rect, ref result);
        if (cellCount <= 0) return 0;
        foreach (var cell in result)
        {
            cell.Value.Override = value;
        }

        return cellCount;
    }
    //add functions to just change values of cells
    //only those functions change values of cells and nothing else
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

    public override bool GetPath(Vector2 start, Vector2 end, ref Path? result)
    {
        throw new NotImplementedException();
    }
}