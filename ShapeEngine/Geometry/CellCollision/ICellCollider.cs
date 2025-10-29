using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Geometry.CellCollision;

/// <summary>
/// Represents an object that can participate in the cell-based collision system.
/// Implementers are queried for position, collision layer/mask and receive lifecycle callbacks
/// when they enter/exit cells or start/end collisions with other colliders.
/// </summary>
public interface ICellCollider
{
    /// <summary>
    /// Returns an integer identifier for the collider's concrete type.
    /// Used for type-discrimination or lookup by the collision system.
    /// </summary>
    public int GetTypeId();

    /// <summary>
    /// Returns the collision layer bit for this collider.
    /// Layers are represented as a <see cref="uint"/> bitmask where a single bit denotes the layer.
    /// </summary>
    public uint GetCollisionLayer();

    /// <summary>
    /// Returns the collision mask for this collider.
    /// The mask indicates which layers this collider should interact with.
    /// </summary>
    public BitFlag GetCollisionMask();

    /// <summary>
    /// Advance the collider's internal state by <paramref name="dt"/> seconds.
    /// The current world position must be written to <paramref name="position"/>.
    /// </summary>
    /// <param name="dt">Delta time in seconds since last update.</param>
    /// <param name="position">Output parameter that receives the collider's current position.</param>
    public void Update(float dt, out Vector2 position);

    /// <summary>
    /// Returns the collider's current position in world coordinates.
    /// </summary>
    public Vector2 GetPosition();

    /// <summary>
    /// Called when the collider has entered a new cell identified by <paramref name="coordinates"/>.
    /// </summary>
    /// <param name="coordinates">Cell coordinates that were entered.</param>
    public void OnEnterCell(Coordinates coordinates);

    /// <summary>
    /// Called when the collider has exited a cell identified by <paramref name="coordinates"/>.
    /// </summary>
    /// <param name="coordinates">Cell coordinates that were exited.</param>
    public void OnExitCell(Coordinates coordinates);
    
    /// <summary>
    /// Called when a collision with <paramref name="other"/> has started. Happens when this collider enters a new cell.
    /// </summary>
    /// <param name="other">The other collider involved in the collision.</param>
    public void OnCollisionStartedWith(ICellCollider other);

    /// <summary>
    /// Called when a collision with <paramref name="other"/> has ended. Happens when this collider exits the previous cell.
    /// </summary>
    /// <param name="other">The other collider involved in the collision.</param>
    public void OnCollisionEndedWith(ICellCollider other);
}