using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Geometry.CellCollision;

/// <summary>
/// Cell-based collision system that partitions world space into a grid of cells of the specified size.
/// Manages registration of <see cref="ICellCollider"/> instances, updates their positions each frame,
/// moves them between cells, and invokes enter/exit and collision start/end callbacks accordingly.
/// </summary>
/// <param name="cellSize">Size of each grid cell in world units (width and height).</param>
public class CellCollisionSystem(Size cellSize)
{
    /// <summary>
    /// Represents a single grid cell that holds a set of colliders (occupants).
    /// Instances are pooled via <see cref="RentInstance"/> / <see cref="ReturnInstance"/>
    /// to reduce allocations during frequent cell creation/destruction.
    /// </summary>
    private class Cell : HashSet<ICellCollider>
    {
        /// <summary>
        /// Pool of reusable <see cref="Cell"/> instances to reduce allocations during frequent
        /// cell creation and destruction. Instances are rented via <see cref="RentInstance"/>
        /// and returned via <see cref="ReturnInstance"/>.
        /// </summary>
        private static readonly Queue<Cell> pool = [];
       
        /// <summary>
        /// Rent a pooled <see cref="Cell"/> instance. If the pool is empty a new instance is created.
        /// Callers must return instances via <see cref="ReturnInstance(Cell)"/> when finished.
        /// </summary>
        /// <returns>A <see cref="Cell"/> instance from the pool or a newly created one.</returns>
        public static Cell RentInstance()
        {
            if (pool.Count <= 0) return new Cell(32);
            var cell = pool.Dequeue();
            return cell;
        }
        
        /// <summary>
        /// Returns a <see cref="Cell"/> instance to the internal pool for reuse.
        /// The cell is cleared by the caller before being enqueued to avoid retaining references
        /// to previous occupants. After calling this method the returned <see cref="Cell"/>
        /// must not be used by the caller.
        /// </summary>
        /// <param name="cell">The <see cref="Cell"/> instance being returned to the pool.</param>
        public static void ReturnInstance(Cell cell)
        {
            cell.Clear();
            pool.Enqueue(cell);
        }
        
        /// <summary>
        /// Initializes a new pooled <see cref="Cell"/> with the specified initial capacity.
        /// This constructor is private because instances are created and reused via
        /// <see cref="RentInstance"/> / <see cref="ReturnInstance"/> to reduce allocations.
        /// </summary>
        /// <param name="capacity">Initial internal capacity for the HashSet to avoid reallocations.</param>
        private Cell(int capacity) : base(capacity) { }
        
        /// <summary>
        /// Gets a value indicating whether this cell contains no occupants.
        /// </summary>
        public bool IsEmpty => Count == 0;
        
    }
    
    /// <summary>
    /// Maps occupied cell coordinates to their pooled <see cref="Cell"/> instance.
    /// Cells are created on demand and returned to the pool when emptied.
    /// </summary>
    private readonly Dictionary<Coordinates, Cell> cells = new();
    
    /// <summary>
    /// Colliders that have been requested to be added this frame.
    /// Processed by <see cref="ProcessPendingColliders"/> before updates.
    /// </summary>
    private readonly HashSet<ICellCollider> collidersToAdd = [];
    
    /// <summary>
    /// Colliders that have been requested to be removed this frame.
    /// Processed by <see cref="ProcessPendingColliders"/> before updates.
    /// </summary>
    private readonly HashSet<ICellCollider> collidersToRemove = [];
    
    /// <summary>
    /// Maps an active collider to the cell coordinates it currently occupies.
    /// Used for quick lookup during movement/removal operations.
    /// </summary>
    private readonly Dictionary<ICellCollider, Coordinates> colliders = [];
    
    /// <summary>
    /// Temporary register of colliders that changed cells during the current update.
    /// Each entry maps a collider to a tuple of (previousCoordinates, currentCoordinates).
    /// Cleared at the end of collision resolution. Initialized with an expected capacity.
    /// </summary>
    private readonly Dictionary<ICellCollider, (Coordinates, Coordinates)> collisionRegister = new(64);

    /// <summary>
    /// Advance the collision system by a single frame.
    /// This will process any pending additions/removals, update all registered colliders'
    /// positions by <paramref name="dt"/>, and resolve collisions for colliders that moved
    /// between cells during this update.
    /// </summary>
    /// <param name="dt">Delta time in seconds since the last update.</param>
    public void Update(float dt)
    {
        ProcessPendingColliders();
        UpdateColliders(dt);
        ResolveColliderCollisions(dt);
    }
    
    /// <summary>
    /// Clears the collision system state and returns all pooled resources.
    /// </summary>
    /// <remarks>
    /// This method clears the pending add/remove queues, unregisters all colliders,
    /// and returns any allocated pooled <see cref="Cell"/> instances back to the pool.
    /// After calling <see cref="Clear"/>, the system will be in an empty state with
    /// no registered colliders or active cells.
    /// </remarks>
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
    
    /// <summary>
    /// Closes the collision system and releases any managed resources.
    /// </summary>
    /// <remarks>
    /// This currently delegates to <see cref="Clear"/> to unregister colliders and
    /// return pooled cell instances.
    /// </remarks>
    public void Close()
    {
        Clear();
    }
    
    /// <summary>
    /// Request registration of a collider with the cell collision system.
    /// If the collider is currently scheduled for removal this frame, the add will be rejected.
    /// The collider is enqueued and will be processed by <see cref="ProcessPendingColliders"/> on the next update.
    /// </summary>
    /// <param name="collider">The <see cref="ICellCollider"/> to add.</param>
    /// <returns>
    /// True if the collider was successfully queued for addition; false if it was already scheduled for removal
    /// or already present in the add queue.
    /// </returns>
    public bool Add(ICellCollider collider)
    {
        if(collidersToRemove.Contains(collider)) return false;
        return collidersToAdd.Add(collider);
    }
    
    /// <summary>
    /// Schedule a collider to be unregistered from the collision system.
    /// If the collider is currently queued for addition this frame, it will be removed from the add queue.
    /// The actual removal will be processed by <see cref="ProcessPendingColliders"/> on the next update,
    /// which will call <see cref="RemoveCollider(ICellCollider)"/> to perform the removal and trigger
    /// any necessary exit/collision end callbacks.
    /// </summary>
    /// <param name="collider">The <see cref="ICellCollider"/> instance to remove.</param>
    /// <returns>
    /// True if the collider was successfully queued for removal (i.e. added to the internal removal set);
    /// false if it was already present in the removal queue.</returns>
    public bool Remove(ICellCollider collider)
    {
        if(collidersToAdd.Contains(collider))
        {
            collidersToAdd.Remove(collider);
        }
        return collidersToRemove.Add(collider);
    }

    /// <summary>
    /// Copies all registered colliders of type <typeparamref name="T"/> into the provided result set.
    /// Iterates the internal registry and adds any collider that can be cast to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The collider concrete type to filter for. Must implement <see cref="ICellCollider"/>.</typeparam>
    /// <param name="result">A reference to a <see cref="HashSet{T}"/> that will be populated with matching colliders.
    /// The set is modified in-place; duplicates are ignored.</param>
    /// <returns>The number of colliders added to <paramref name="result"/> by this call.</returns>
    public int GetAllColliders<T>(ref HashSet<T> result) where T : ICellCollider
    {
        var count = 0;
        foreach (var kvp in colliders)
        {
            if (kvp.Key is not T t) continue;
            if(result.Add(t)) count++;
        }
        return count;
    }
    
    /// <summary>
    /// Draws a debug visualization of the collision grid.
    /// Renders grid lines for all tracked cells, optionally fills occupied cells,
    /// and draws an outer bounding rectangle around the active cell area.
    /// </summary>
    /// <param name="lineColor">Color used to draw the grid lines for each cell.</param>
    /// <param name="fillColor">Color used to fill cells that contain occupants.</param>
    /// <param name="borderColor">Color used to draw the bounding rectangle around all active cells.</param>
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
    
    /// <summary>
    /// Processes pending collider registration and removal requests queued for this frame.
    /// </summary>
    /// <remarks>
    /// - Iterates <see cref="collidersToAdd"/> and attempts to register each collider using <see cref="AddCollider(ICellCollider)"/>.
    /// - Iterates <see cref="collidersToRemove"/> and attempts to unregister each collider using <see cref="RemoveCollider(ICellCollider)"/>.
    /// - Clears both pending sets after processing.
    /// This defers modifications to the active collider set so updates can iterate the current
    /// colliders without concurrent modification issues. Additions are processed before removals.
    /// </remarks>
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
   
    /// <summary>
    /// Update all registered colliders for this frame.
    /// Iterates the internal collider registry and advances each collider's state by <paramref name="dt"/> seconds.
    /// For each collider this calls <see cref="UpdateCollider(ICellCollider, Coordinates, float)"/>,
    /// which handles moving the collider between cells and registering any cell changes for later collision resolution.
    /// </summary>
    /// <param name="dt">Delta time in seconds since the last update.</param>
    private void UpdateColliders(float dt)
    {
        foreach (var kvp in colliders)
        {
            UpdateCollider(kvp.Key, kvp.Value, dt);
        }
    }
    
    /// <summary>
    /// Updates a single collider by advancing its internal state and handling cell transitions.
    /// Calls <c>collider.Update(dt, out var newPosition)</c> and computes the new cell coordinates.
    /// If the collider moved to a different cell this method:
    /// - removes it from the old cell (returning the pooled cell when emptied),
    /// - adds it to the new cell,
    /// - records the move in <c>collisionRegister</c> for later collision resolution.
    /// </summary>
    /// <param name="collider">The collider to update.</param>
    /// <param name="oldCoords">The collider's previous cell coordinates.</param>
    /// <param name="dt">Delta time in seconds since the last update.</param>
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
   
    /// <summary>
    /// Resolves enter/exit cell events and collision start/end callbacks for colliders
    /// that moved during the current update (stored in <c>collisionRegister</c>).
    /// For each moved collider this method:
    /// - updates the collider's recorded cell in <c>colliders</c>,
    /// - invokes <c>OnExitCell</c> for the previous cell and notifies occupants in that cell
    ///   about ended collisions (respecting layers and masks),
    /// - invokes <c>OnEnterCell</c> for the new cell and notifies occupants in that cell
    ///   about started collisions (respecting layers and masks).
    /// The method clears <c>collisionRegister</c> after processing.
    /// </summary>
    /// <param name="dt">Delta time in seconds since the last update (present for symmetry with Update,
    /// but not currently used by this method).</param>
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
                foreach (var c in oldCell)
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
                foreach (var c in newCell)
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
    
    /// <summary>
    /// Attempts to add the given <see cref="ICellCollider"/> to the cell corresponding to its current position.
    /// If the target cell already exists the collider is added to it; otherwise a pooled <see cref="Cell"/> is rented
    /// and inserted into the cell map. On success the collider is recorded in the internal <c>colliders</c> map.
    /// </summary>
    /// <param name="collider">The collider to register with the collision system.</param>
    /// <returns>
    /// <c>true</c> if the collider was successfully added and registered; <c>false</c> if the collider could not be added
    /// (for example it was already present in the target cell or adding failed).</returns>
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
    
    /// <summary>
    /// Unregisters the specified <see cref="ICellCollider"/> from the collision system.
    /// Removes the collider from its current cell (returning the pooled cell if empty),
    /// invokes exit and collision end callbacks for other occupants in the cell,
    /// and removes the collider from the internal registry.
    /// </summary>
    /// <param name="collider">The collider to remove from the system.</param>
    /// <returns>True if the collider was present and removed; otherwise false.</returns>
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
                    
                    foreach (var c in cell)
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
    
    /// <summary>
    /// Converts a world-space point to discrete grid cell coordinates.
    /// Divides the point by the configured <c>cellSize</c> and uses <see cref="MathF.Floor"/>
    /// to produce consistent integer cell indices (handles negative positions correctly).
    /// </summary>
    /// <param name="point">World-space position to convert.</param>
    /// <returns>Corresponding <see cref="Coordinates"/> for the grid cell containing the point.</returns>
    private Coordinates GetCoordinates(Vector2 point)
    {
        var x = (int)MathF.Floor(point.X / cellSize.Width);
        var y = (int)MathF.Floor(point.Y / cellSize.Height);
        return new Coordinates(x, y);
    }
    
    /// <summary>
    /// Retrieve or create the pooled <see cref="Cell"/> for the specified grid coordinates.
    /// If a <see cref="Cell"/> already exists for <paramref name="coords"/> it is returned;
    /// otherwise a new instance is rented from the internal pool and inserted into the map.
    /// </summary>
    /// <param name="coords">The discrete grid coordinates identifying the target cell.</param>
    /// <returns>The <see cref="Cell"/> instance corresponding to <paramref name="coords"/>.</returns>
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