using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core;

public abstract class GameObject : IUpdateable, IDrawable
{
    public event Action<GameObject, string?, GameObject?>? OnKilled;
    // public event Action<GameObject, SpawnArea>? OnSpawned;
    // public event Action<GameObject, SpawnArea>? OnDespawned;
    
    
    public Transform2D Transform { get; set; }
    public bool IsDead { get; private set; } = false;
    

    public abstract Rect GetBoundingBox();

    public abstract void Update(GameTime time, ScreenInfo game, ScreenInfo ui);

    public abstract void DrawGame(ScreenInfo game);

    public abstract void DrawGameUI(ScreenInfo ui);

    public virtual bool IsDrawingToGame(Rect gameArea) => true;
    public virtual bool IsDrawingToGameUI(Rect screenArea) => false;
        
    /// <summary>
    /// The area layer the object is stored in. Higher layers are draw on top of lower layers.
    /// </summary>
    public uint Layer { get; set; }
    /// <summary>
    /// Is called by the area. Can be used to update the objects position based on the new parallax position.
    /// </summary>
    /// <param name="newParallaxPosition">The new parallax position from the layer the object is in.</param>
    public virtual void UpdateParallaxe(Vector2 newParallaxPosition) { }

    /// <summary>
    /// Check if the object is in a layer.
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    public bool IsInLayer(uint layer) { return this.Layer == layer; }

    /// <summary>
    /// Is called when gameobject is added to an area.
    /// </summary>
    public virtual void OnSpawned(SpawnArea spawnArea){}
    /// <summary>
    /// Is called by the area once a game object is dead.
    /// </summary>
    public virtual void OnDespawned(SpawnArea spawnArea){}

    // /// <summary>
    // /// Should this object be checked for leaving the bounds of the area?
    // /// </summary>
    // /// <returns></returns>
    // public virtual bool IsCheckingHandlerBounds() => false;
    // /// <summary>
    // /// Will be called if the object left the bounds of the area. The BoundingCircle is used for this check.
    // /// </summary>
    // /// <param name="info">The info about where the object left the bounds.</param>
    // public virtual void OnLeftHandlerBounds(BoundsCollisionInfo info){}

    
    /// <summary>
    /// Get the bounds of the spawn area this object is in and return if this object should be removed from the spawn area.
    /// </summary>
    /// <param name="bounds">The rect bounds of the spawn area.</param>
    /// <returns>If true is return object is removed from the spawn area</returns>
    public virtual bool HasLeftBounds(Rect bounds) => false;
    
    /// <summary>
    /// Tries to kill this game object.
    /// </summary>
    /// <returns>Returns if kill was successful.</returns>
    public bool Kill(string? killMessage = null, GameObject? killer = null)
    {
        if (IsDead) return false;

        if (TryKill(killMessage, killer))
        {
            IsDead = true;
            WasKilled(killMessage, killer);
            OnKilled?.Invoke(this, killMessage, killer);
            return true;
        }

        return false;
    }
    protected virtual void WasKilled(string? killMessage = null, GameObject? killer = null) { }
    protected virtual bool TryKill(string? killMessage = null, GameObject? killer = null) => true;
}