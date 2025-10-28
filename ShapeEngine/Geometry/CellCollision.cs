using System.Numerics;
using System.Runtime.Intrinsics.X86;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Random;

namespace ShapeEngine.Geometry;
//TODO: Docs
public interface ICellCollider
{
    public int GetTypeId();
    public uint GetCollisionLayer();
    public BitFlag GetCollisionMask();
    public void Update(float dt, out Vector2 position);
    public Vector2 GetPosition();


    public void OnEnterCell(Coordinates coordinates);
    // public void OnRemainInCell(Coordinates coordinates, float dt);
    public void OnExitCell(Coordinates coordinates);
    
    public void OnCollisionStartedWith(ICellCollider other);
    // public void OnCollisionOngoingWith(ICellCollider other, float dt);
    public void OnCollisionEndedWith(ICellCollider other);
}
public class CellCollisionSystem(Size cellSize)
{
    private class Cell
    {
        private static readonly Queue<Cell> pool = [];
        public static Cell RentInstance()
        {
            if (pool.Count <= 0) return new Cell();
            var cell = pool.Dequeue();
            return cell;
        }
        public static void ReturnInstance(Cell cell)
        {
            cell.Clear();
            pool.Enqueue(cell);
        }
        
        public readonly HashSet<ICellCollider> Occupants;

        private Cell()
        {
            Occupants = new HashSet<ICellCollider>(32);
        }

        public bool Add(ICellCollider c) => Occupants.Add(c);
        public bool Remove(ICellCollider c) => Occupants.Remove(c);
        public bool Contains(ICellCollider c) => Occupants.Contains(c);
        public bool IsEmpty => Occupants.Count == 0;

        public void Clear()
        {
            Occupants.Clear();
        }
    }
    
    private readonly Dictionary<Coordinates, Cell> cells = new();
    private readonly HashSet<ICellCollider> collidersToAdd = [];
    private readonly HashSet<ICellCollider> collidersToRemove = [];
    
    private readonly Dictionary<ICellCollider, Coordinates> colliders = [];
    private readonly Dictionary<ICellCollider, (Coordinates, Coordinates)> collisionRegister = new(500);

    public void Update(float dt)
    {
        ProcessPendingColliders();
        UpdateColliders(dt);
        ResolveColliderCollisions(dt);
    }
    public void Clear()
    {
        collidersToAdd.Clear();
        collidersToRemove.Clear();
        // foreach (var kvp in colliders)
        // {
        //     ColliderContainer.ReturnInstance(kvp.Value);
        // }
        colliders.Clear();
        
        foreach (var kvp in cells)
        {
            var cell = kvp.Value;
            cell.Clear();
            Cell.ReturnInstance(cell);
        }
        cells.Clear();
    }
    public void Close()
    {
        Clear();
    }
    
    public bool Add(ICellCollider collider)
    {
        if(collidersToRemove.Contains(collider)) return false;
        return collidersToAdd.Add(collider);
    }
    public bool Remove(ICellCollider collider)
    {
        if(collidersToAdd.Contains(collider))
        {
            collidersToAdd.Remove(collider);
        }
        return collidersToRemove.Add(collider);
    }

    public int GetAllColliders<T>(ref HashSet<T> result) where T : ICellCollider
    {
        var count = 0;
        foreach (var kvp in colliders)
        {
            if (kvp.Key is T t)
            {
                result.Add(t);
                count++;
            }
        }
        return count;
    }
    public void DrawDebug(ColorRgba lineColor, ColorRgba fillColor, ColorRgba borderColor)
    {
        float lt = cellSize.Max() * 0.01f;

        var minCoords = Coordinates.MaxValue;
        var maxCoords = Coordinates.MinValue;
        
        foreach (var kvp in cells)
        {
            var coords = kvp.Key;
            
            minCoords = minCoords.Min(coords);
            maxCoords = maxCoords.Max(coords);
            
            var cellLayer = kvp.Value;
            var filled = !cellLayer.IsEmpty;
            var rect = new Rect(
                coords.X * cellSize.Width,
                coords.Y * cellSize.Height,
                cellSize.Width,
                cellSize.Height);

            rect.DrawLines(lt, lineColor);
            if (filled)
            {
                rect.ScaleSize(0.75f, AnchorPoint.Center).Draw(fillColor);
            }
        }
        
        var borderRect = new Rect(
            minCoords.X * cellSize.Width,
            minCoords.Y * cellSize.Height,
            (maxCoords.X - minCoords.X + 1) * cellSize.Width,
            (maxCoords.Y - minCoords.Y + 1) * cellSize.Height);
        borderRect.DrawLines(lt * 2f, borderColor);
    }
    
    private void ProcessPendingColliders()
    {
        foreach (var collider in collidersToAdd)
        {
            AddCollider(collider);
        }
        collidersToAdd.Clear();
        
        foreach (var collider in collidersToRemove)
        {
            RemoveCollider(collider);
        }
        collidersToRemove.Clear();
    }
    
    private void UpdateColliders(float dt)
    {
        foreach (var kvp in colliders)
        {
            UpdateCollider(kvp.Key, kvp.Value, dt);
        }
    }
    private void UpdateCollider(ICellCollider collider, Coordinates oldCoords, float dt)
    {
        collider.Update(dt, out var newPosition);
        var newCoords = GetCoordinates(newPosition);
        // var oldCoords = container.Coordinates;

        if (oldCoords == newCoords) return;

        if (cells.TryGetValue(oldCoords, out var oldCell))
        {
            if (oldCell.Remove(collider))
            {
                if (oldCell.IsEmpty)
                {
                    cells.Remove(oldCoords);
                    Cell.ReturnInstance(oldCell);
                }
            }
        }
        
        var newCell = GetCell(newCoords);
        if (newCell.Add(collider))
        {
            collisionRegister.Add(collider, (oldCoords, newCoords));
        }
    }
    
    private void ResolveColliderCollisions(float dt)
    {
        foreach (var (collider, (previousCoords, currentCoords)) in collisionRegister)
        {
            colliders[collider] = currentCoords;
            
            var layer = collider.GetCollisionLayer();
            var mask = collider.GetCollisionMask();
            
            collider.OnExitCell(previousCoords);
            if (cells.TryGetValue(previousCoords, out var oldCell))
            {
                foreach (var c in oldCell.Occupants)
                {
                    var otherLayer = c.GetCollisionLayer();
                    var otherMask = c.GetCollisionMask();
                    if(otherLayer == layer) continue;
                    if (mask.Has(otherLayer))
                    {
                        collider.OnCollisionEndedWith(c);
                    }
                
                    if (otherMask.Has(layer))
                    {
                        c.OnCollisionEndedWith(collider);
                    }
                }
            }

            collider.OnEnterCell(currentCoords);
            if (cells.TryGetValue(currentCoords, out var newCell))
            {
                foreach (var c in newCell.Occupants)
                {
                    var otherLayer = c.GetCollisionLayer();
                    var otherMask = c.GetCollisionMask();
                    if(otherLayer == layer) continue;
                    if (mask.Has(otherLayer))
                    {
                        collider.OnCollisionStartedWith(c);
                    }

                    if (otherMask.Has(layer))
                    {
                        c.OnCollisionStartedWith(collider);
                    }
                }
            }
        }
        collisionRegister.Clear();
    }
    
    private bool AddCollider(ICellCollider collider)
    {
        var coords = GetCoordinates(collider.GetPosition());
        if (cells.TryGetValue(coords, out var cell))
        {
            if (!cell.Add(collider)) return false;
        }
        else
        {
            var newCell = Cell.RentInstance();
            if (newCell.Add(collider))
            {
                cells.Add(coords, newCell);
            }
            else
            {
                Cell.ReturnInstance(newCell);
                return false;
            }
        }
        colliders.Add(collider, coords);
        return true;
    }
    private bool RemoveCollider(ICellCollider collider)
    {
        if (colliders.TryGetValue(collider, out var coords))
        {
            if (cells.TryGetValue(coords, out var cell))
            {
                cell.Remove(collider);
                collider.OnExitCell(coords);
                if (cell.IsEmpty)
                {
                    cells.Remove(coords);
                    Cell.ReturnInstance(cell);
                }
                else
                {
                    var layer = collider.GetCollisionLayer();
                    var mask = collider.GetCollisionMask();
                    
                    foreach (var c in cell.Occupants)
                    {
                        var otherLayer = c.GetCollisionLayer();
                        var otherMask = c.GetCollisionMask();
                        if(otherLayer == layer) continue;
                        if (mask.Has(otherLayer))
                        {
                            collider.OnCollisionEndedWith(c);
                        }
                
                        if (otherMask.Has(layer))
                        {
                            c.OnCollisionEndedWith(collider);
                        }
                    }
                }
            }
            colliders.Remove(collider);
            return true;
        }
        return false;
    }
    private Coordinates GetCoordinates(Vector2 point)
    {
        var x = (int)Math.Floor(point.X / cellSize.Width);
        var y = (int)Math.Floor(point.Y / cellSize.Height);
        return new Coordinates(x, y);
    }
    private Rect GetCellRect(Coordinates coords)
    {
        return new Rect(
            coords.X * cellSize.Width,
            coords.Y * cellSize.Height,
            cellSize.Width,
            cellSize.Height);
    }
    private Cell GetCell(Coordinates coords)
    {
        Cell cell;
        if (cells.TryGetValue(coords, out var cellLayer1))
        {
            cell = cellLayer1;
        }
        else
        {
            cell = Cell.RentInstance();
            cells.Add(coords, cell);
        }

        return cell;
    }
    
}