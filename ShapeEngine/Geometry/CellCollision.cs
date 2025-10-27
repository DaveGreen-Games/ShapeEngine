using System.Numerics;
using System.Runtime.Intrinsics.X86;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Random;

namespace ShapeEngine.Geometry;

public interface ICellCollider
{
    public int GetTypeId();
    public uint GetCollisionLayer();
    public BitFlag GetCollisionMask();
    public void Update(float dt, out Vector2 position);
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

    public int GetAllOccupants<T>(ref HashSet<T> result) where T : ICellCollider
    {
        var count = 0;
        foreach (var kvp in occupants)
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
        
        foreach (var kvp in cellLayers)
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
    
    private void ProcessOccupants(float dt)
    {
        foreach (var kvp in occupants)
        {
            ProcessOccupant(kvp.Key, kvp.Value, dt);
        }
    }
    private void ProcessOccupant(ICellCollider collider, ColliderContainer container, float dt)
    {
        collider.Update(dt, out var newPosition);
        var newCoords = GetCoordinates(newPosition);
        var oldCoords = container.Coordinates;
        if (oldCoords == null || newCoords != oldCoords) //collider moved to a new cell
        {
            if (oldCoords != null)
            {
                var validOldCoordinates = (Coordinates)oldCoords;
                if(cellLayers.TryGetValue(validOldCoordinates, out var oldLayer))
                {
                    if (oldLayer.Remove(collider))
                    {
                        if (oldLayer.IsEmpty)
                        {
                            cellLayers.Remove(validOldCoordinates);
                            CellLayer.ReturnInstance(oldLayer);
                        }
                    }
                }
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
    private Rect GetCellRect(Coordinates coords)
    {
        return new Rect(
            coords.X * cellSize.Width,
            coords.Y * cellSize.Height,
            cellSize.Width,
            cellSize.Height);
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




// Simple example collider implementing ICellCollider
public class SimpleCollider : ICellCollider
{
    private const int BlueId = 1;
    private const uint BlueLayer = 1;
    private static readonly BitFlag BlueMask = new(RedLayer);
    private static readonly ColorRgba BlueColor = new ColorRgba(System.Drawing.Color.Blue);
    
    private const int RedId = 2;
    private const uint RedLayer = 2;
    private static readonly BitFlag RedMask = new(BlueLayer);
    private static readonly ColorRgba RedColor = new ColorRgba(System.Drawing.Color.Red);
    private static readonly ColorRgba DamageColor = new ColorRgba(System.Drawing.Color.NavajoWhite);
    private const float MinSize = 1f;
    private const float MaxSize = 10f;
    private const float DamageValue = 0.2f;
    private const float FoodValue = 0.1f;
    
    private Vector2 position;
    private Vector2 velocity;
    private float radius;
    private uint layer;
    private BitFlag mask;
    private int typeId;
    private ColorRgba color;
    private readonly Rect bounds;
    private float damageTimer = 0f;
    private const float damagedDuration = 0.2f;
    
    public SimpleCollider(bool redTeam, Rect bounds)
    { 
        this.bounds = bounds;
        if (redTeam)
        {
            typeId = RedId;
            layer = RedLayer;
            mask = RedMask;
            color = RedColor;
        }
        else
        {
            typeId = BlueId;
            layer = BlueLayer;
            mask = BlueMask;
            color = BlueColor;
        }
        
        var randPos = bounds.GetRandomPointInside();
        var randSpeed = 100f; //Rng.Instance.RandF(75, 125);
        var randVel = Rng.Instance.RandVec2() * randSpeed;
        var randRadius = Rng.Instance.RandF(MinSize, MaxSize);
        
        position = randPos;
        velocity = randVel;
        radius = randRadius;
    }

    
    
    public int GetTypeId() => typeId;
    public uint GetCollisionLayer() => layer;
    public BitFlag GetCollisionMask() => mask;

    // Move advances position by velocity * dt and returns new position
    public void Update(float dt, out Vector2 newPosition)
    {
        if (damageTimer > 0)
        {
            damageTimer -= dt;
            if (damageTimer < 0f) damageTimer = 0f;
        }
        
        position += velocity * dt;
        
        // Bounce off bounds
        if (position.X - radius < bounds.Left)
        {
            position.X = bounds.Left + radius;
            velocity.X = -velocity.X;
        }
        else if (position.X + radius > bounds.Right)
        {
            position.X = bounds.Right - radius;
            velocity.X = -velocity.X;
        }
        if (position.Y - radius < bounds.Top)
        {
            position.Y = bounds.Top + radius;
            velocity.Y = -velocity.Y;
        }
        else if (position.Y + radius > bounds.Bottom)
        {
            position.Y = bounds.Bottom - radius;
            velocity.Y = -velocity.Y;
        }
        
        newPosition = position;
    }

    public Vector2 GetPosition() => position;

    public void Grow()
    {
        radius += FoodValue;
        if (radius > MaxSize)
        {
            Die();
        }
    }
    public void Damage()
    {
        radius -= DamageValue;
        if (radius < MinSize)
        {
            Die();
        }
        else
        {
            damageTimer = damagedDuration;
        }
    }

    private void Die()
    {
        damageTimer = 0f;
        var randPos = bounds.GetRandomPointInside();
        var randSpeed = 100f; //Rng.Instance.RandF(75, 125);
        var randVel = Rng.Instance.RandVec2() * randSpeed;
        var randRadius = Rng.Instance.RandF(MinSize, MaxSize);
        
        position = randPos;
        velocity = randVel;
        radius = randRadius;

        if (typeId == RedId)
        {
            typeId = BlueId;
            layer = BlueLayer;
            mask = BlueMask;
            color = BlueColor;
        }
        else
        {
            typeId = RedId;
            layer = RedLayer;
            mask = RedMask;
            color = RedColor;
        }
    }
    // Simple circle-vs-circle collision test
    public bool CollidedWith(ICellCollider other)
    {
        if (other is not SimpleCollider sc) return false;

        if (radius > sc.radius)
        {
            Grow();
            sc.Damage();
        }
        // if (sc.radius > radius)
        // {
        //     Damage();
        // }
        // else
        // {
        //     sc.Damage();
        // }

        return false;
    }

    public void CellEnteredBy(ICellCollider other)
    {
        // Console.WriteLine($"Collider (type {typeId}, layer {layer}) entered by other on layer {other.GetCollisionLayer()} at {position}");
    }

    public void Draw()
    {
        CircleDrawing.DrawCircleFast(position, radius, damageTimer > 0f ? DamageColor : color);
    }
}
public class CellCollisionDemo
{
    private CellCollisionSystem cellCollisionSystem;
    private HashSet<SimpleCollider> drawableColliders;

    public CellCollisionDemo(Rect bounds, Size cellSize, int amount)
    {
        cellCollisionSystem = new CellCollisionSystem(cellSize);
        drawableColliders = new HashSet<SimpleCollider>(amount);
        
        for (var i = 0; i < amount; i++)
        {
            if(i % 2 == 0)
            {
                var col = new SimpleCollider(redTeam:false, bounds:bounds);
                cellCollisionSystem.Add(col);
            }
            else
            {
                var col = new SimpleCollider(redTeam:true,  bounds:bounds);
                cellCollisionSystem.Add(col);
            }
        }
        
        
    }

    public void Update(float dt)
    {
        cellCollisionSystem.Update(dt);
    }

    public void Draw()
    {
        // cellCollisionSystem.DrawDebug(new ColorRgba(System.Drawing.Color.DarkOliveGreen), new ColorRgba(System.Drawing.Color.DarkCyan), new ColorRgba(System.Drawing.Color.Aquamarine));
        
        drawableColliders.Clear();
        int count = cellCollisionSystem.GetAllOccupants(ref drawableColliders);
        foreach (var col in drawableColliders)
        {
            col.Draw();
        }
    }
}
