using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Geometry;

public interface ICellCollider
{
    public int GetTypeId();
    public uint GetCollisionLayer();
    public BitFlag GetCollisionMask();
    public void Move(float dt, out Vector2 position);
    public Vector2 GetPosition();
    
    public bool CollidedWith(ICellCollider other);
    public void CellEnteredBy(ICellCollider other);
}


public class CellCollisionSystem(Size cellSize)
{
    private class ColliderContainer
    {
        private static readonly Queue<ColliderContainer> pool = [];
        public static ColliderContainer RentInstance(ICellCollider collider, Coordinates coordinates, CellLayer cellLayer)
        {
            if (pool.Count <= 0) return new ColliderContainer(collider, coordinates, cellLayer);
            
            var container = pool.Dequeue();
            container.Collider = collider;
            container.Coordinates = coordinates;
            container.CellLayer = cellLayer;
            return container;
        }
        public static void ReturnInstance(ColliderContainer container)
        {
            container.Clear();
            pool.Enqueue(container);
        }
        
        
        public ICellCollider? Collider;
        public Coordinates? Coordinates;
        public CellLayer? CellLayer;

        private ColliderContainer(ICellCollider collider, Coordinates coordinates, CellLayer cellLayer)
        {
            Collider = collider;
            Coordinates = coordinates;
            CellLayer = cellLayer;
        }
        
        public void Clear()
        {
            Collider = null;
            Coordinates = null;
            CellLayer = null;
        }
    }
    private class CellLayer
    {
        private static readonly Queue<CellLayer> pool = [];
        public static CellLayer RentInstance()
        {
            if (pool.Count <= 0) return new CellLayer(64);
            var layer = pool.Dequeue();
            return layer;
        }
        public static void ReturnInstance(CellLayer layer)
        {
            layer.Clear();
            pool.Enqueue(layer);
        }
        
        
        private readonly Dictionary<uint, Cell> cells;
        
        private CellLayer(int capacity)
        {
            cells = new Dictionary<uint, Cell>(capacity);
        }

        public bool Add(ICellCollider collider)
        {
            var layer = collider.GetCollisionLayer();
            if (cells.TryGetValue(layer, out var value))
            {
                if (value.Add(collider)) return true;
                return false;
            }

            var cell = Cell.RentInstance();
            cell.Add(collider);
            cells.Add(layer, cell);
            return true;
        }
        public bool Remove(ICellCollider collider)
        {
            var layer = collider.GetCollisionLayer();
            if(cells.TryGetValue(layer, out var cell))
            {
                var removed = cell.Remove(collider);
                if (removed && cell.IsEmpty)
                {
                    cells.Remove(layer);
                    Cell.ReturnInstance(cell);
                }
                return removed;
            }

            return false;
        }
        public bool IsOccupied(uint layer)
        {
            return cells.TryGetValue(layer, out var value) && !value.IsEmpty;
        }
        public bool Clear(uint layer)
        {
            return cells.Remove(layer);
        }
        public void Collide(ICellCollider collider)
        {
            uint layer = collider.GetCollisionLayer();
            var mask = collider.GetCollisionMask();
            foreach (var (curLayer, curCell) in cells)
            {
                if(curLayer == layer) continue;
                if(!mask.Has(curLayer)) continue;
                var occupants = curCell.Occupants;
                foreach (var other in occupants)
                {
                    var otherMask = other.GetCollisionMask();
                    if(!otherMask.Has(layer)) continue;
                    if (collider.CollidedWith(other))
                    {
                        other.CellEnteredBy(collider);
                    }
                }
            }
        }
        public bool Contains(ICellCollider collider)
        {
            var layer = collider.GetCollisionLayer();
            return cells.ContainsKey(layer) && cells[layer].Contains(collider);
        }
        
        public bool IsEmpty => cells.Count <= 0;

        public void Clear()
        {
            foreach (var cell in cells.Values)
            {
                Cell.ReturnInstance(cell);
            }
            cells.Clear();
        }
    }
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
        
        private readonly HashSet<ICellCollider> occupants;

        private Cell()
        {
            occupants = [];
        }
        
        public IReadOnlyCollection<ICellCollider> Occupants => occupants;

        public bool Add(ICellCollider c) => occupants.Add(c);
        public bool Remove(ICellCollider c) => occupants.Remove(c);
        public bool Contains(ICellCollider c) => occupants.Contains(c);
        public bool IsEmpty => occupants.Count == 0;

        public void Clear()
        {
            occupants.Clear();
        }
    }


    private readonly Dictionary<Coordinates, CellLayer> cellLayers = new();
    private readonly Dictionary<ICellCollider, ColliderContainer> occupants = [];
    private readonly HashSet<ICellCollider> occupantsToAdd = [];
    private readonly HashSet<ICellCollider> occupantsToRemove = [];

    public void Update(float dt)
    {
        ProcessPendingOccupants();
        ProcessOccupants(dt);
    }
    public void Clear()
    {
        occupantsToAdd.Clear();
        occupantsToRemove.Clear();
        foreach (var kvp in occupants)
        {
            kvp.Value.Clear();
            ColliderContainer.ReturnInstance(kvp.Value);
        }
        occupants.Clear();
        
        foreach (var cellLayer in cellLayers.Values)
        {
            cellLayer.Clear();
            CellLayer.ReturnInstance(cellLayer);
        }
        cellLayers.Clear();
    }
    public void Close()
    {
        Clear();
    }
    
    public bool Add(ICellCollider collider)
    {
        if(occupantsToRemove.Contains(collider)) return false;
        return occupantsToAdd.Add(collider);
    }
    public bool Remove(ICellCollider collider)
    {
        if(occupantsToAdd.Contains(collider))
        {
            occupantsToAdd.Remove(collider);
        }
        return occupantsToRemove.Add(collider);
    }

    private void ProcessOccupants(float dt)
    {
        foreach (var kvp in occupants)
        {
            ProcessOccupant(kvp.Key, kvp.Value, dt);
        }
    }
    private void ProcessOccupant(ICellCollider collider, ColliderContainer container, float dt)
    {
        collider.Move(dt, out var newPosition);
        var newCoords = GetCoordinates(newPosition);
        if (newCoords != container.Coordinates) //collider moved to a new cell
        { 
            if(container.Coordinates != null && cellLayers.TryGetValue((Coordinates)container.Coordinates, out var oldLayer))
            {
                oldLayer.Remove(collider);
            }
            var cellLayer = GetCellLayer(newCoords);
            bool added = cellLayer.Add(collider);
            if (added)
            {
                container.Coordinates = newCoords;
                container.CellLayer = cellLayer;
                cellLayer.Collide(collider);
            }
        }
    }
    private void ProcessPendingOccupants()
    {
        foreach (var collider in occupantsToAdd)
        {
            AddCollider(collider);
        }
        occupantsToAdd.Clear();
        
        foreach (var collider in occupantsToRemove)
        {
            RemoveCollider(collider);
        }
        occupantsToRemove.Clear();
    }
    private bool AddCollider(ICellCollider collider)
    {
        var coords = GetCoordinates(collider.GetPosition());
        var cellLayer = GetCellLayer(coords);
        var added = cellLayer.Add(collider);
        if (!added) return false;
        
        var container = ColliderContainer.RentInstance(collider, coords, cellLayer);
        occupants.Add(collider, container);
        return true;
    }
    private bool RemoveCollider(ICellCollider collider)
    {
        if (occupants.TryGetValue(collider, out var container))
        {
            container.CellLayer?.Remove(collider);
            occupants.Remove(collider);
            ColliderContainer.ReturnInstance(container);
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
    private CellLayer GetCellLayer(Coordinates coords)
    {
        CellLayer cellLayer;
        if (cellLayers.TryGetValue(coords, out var cellLayer1))
        {
            cellLayer = cellLayer1;
        }
        else
        {
            cellLayer = CellLayer.RentInstance();
            cellLayers.Add(coords, cellLayer);
        }

        return cellLayer;
    }
    
}