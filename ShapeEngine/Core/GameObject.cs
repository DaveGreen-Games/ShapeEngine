using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Core;

/// <summary>
/// Represents a base class for all game objects in the engine.
/// Provides lifecycle management, update/draw hooks, and layer/bounds logic.
/// </summary>
/// <remarks>
/// GameObject is intended to be subclassed for specific game logic. It supports kill/revive semantics, event hooks, and parallax/bounds management.
/// </remarks>
public abstract class GameObject : IUpdateable, IDrawable
{
    /// <summary>
    /// Occurs when this object is killed.
    /// <list type="bullet">
    /// <item><description>GameObject: The object being killed.</description></item>
    /// <item><description>string?: Optional kill message.</description></item>
    /// <item><description>GameObject?: The killer object, if any.</description></item>
    /// </list>
    /// </summary>
    public event Action<GameObject, string?, GameObject?>? OnKilled;
    /// <summary>
    /// Occurs when this object is revived.
    /// <list type="bullet">
    /// <item><description>GameObject: The object being revived.</description></item>
    /// <item><description>string?: Optional revive message.</description></item>
    /// <item><description>GameObject?: The reviver object, if any.</description></item>
    /// </list>
    /// </summary>
    public event Action<GameObject, string?, GameObject?>? OnRevived;
    
    /// <summary>
    /// Gets or sets the transform (position, rotation, scale) of this object.
    /// </summary>
    public Transform2D Transform { get; set; }
    /// <summary>
    /// Gets whether this object is dead (killed).
    /// </summary>
    public bool IsDead { get; private set; } = false;
    
    /// <summary>
    /// Gets the bounding box of this object in world space.
    /// </summary>
    /// <returns>The bounding rectangle.</returns>
    public abstract Rect GetBoundingBox();

    /// <summary>
    /// Updates this object. Called every frame.
    /// </summary>
    /// <param name="time">The current game time.</param>
    /// <param name="game">Game screen info.</param>
    /// <param name="gameUi">Game UI screen info.</param>
    /// <param name="ui">UI screen info.</param>
    public abstract void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui);
    /// <summary>
    /// Updates this object at a fixed timestep. Override for physics or logic that needs fixed updates.
    /// </summary>
    /// <param name="fixedTime">The fixed game time.</param>
    /// <param name="game">Game screen info.</param>
    /// <param name="gameUi">Game UI screen info.</param>
    /// <param name="ui">UI screen info.</param>
    public virtual void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui) { }
    /// <summary>
    /// Interpolates this object between fixed updates for smooth rendering.
    /// </summary>
    /// <param name="time">The current game time.</param>
    /// <param name="game">Game screen info.</param>
    /// <param name="gameUi">Game UI screen info.</param>
    /// <param name="ui">UI screen info.</param>
    /// <param name="f">Interpolation factor (0-1).</param>
    public virtual void InterpolateFixedUpdate(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui, float f) { }

    /// <summary>
    /// Draws this object to the game world.
    /// </summary>
    /// <param name="game">Game screen info.</param>
    public abstract void DrawGame(ScreenInfo game);

    /// <summary>
    /// Draws this object to the game UI.
    /// </summary>
    /// <param name="gameUi">Game UI screen info.</param>
    public abstract void DrawGameUI(ScreenInfo gameUi);

    /// <summary>
    /// Determines if this object should be drawn to the game world. (default = true)
    /// </summary>
    /// <param name="gameArea">The area of the game world.</param>
    /// <returns>True if drawing to game, otherwise false.</returns>
    public virtual bool IsDrawingToGame(Rect gameArea) => true;
    /// <summary>
    /// Determines if this object should be drawn to the game UI. (default = false)
    /// </summary>
    /// <param name="gameUiArea">The area of the game UI.</param>
    /// <returns>True if drawing to game UI, otherwise false.</returns>
    public virtual bool IsDrawingToGameUI(Rect gameUiArea) => false;
        
    /// <summary>
    /// Gets or sets the area layer this object is stored in. Higher layers are drawn on top of lower layers.
    /// </summary>
    public uint Layer { get; set; }
    /// <summary>
    /// Called by the area to update the object's position based on the new parallax position.
    /// </summary>
    /// <param name="newParallaxPosition">The new parallax position from the layer the object is in.</param>
    public virtual void UpdateParallaxe(Vector2 newParallaxPosition) { }

    /// <summary>
    /// Checks if the object is in a specific layer.
    /// </summary>
    /// <param name="layer">The layer to check.</param>
    /// <returns>True if in the specified layer.</returns>
    public bool IsInLayer(uint layer) { return this.Layer == layer; }

    /// <summary>
    /// Called when the game object is added to an area.
    /// </summary>
    /// <param name="spawnArea">The spawn area this object is added to.</param>
    public virtual void OnSpawned(SpawnArea spawnArea){}
    /// <summary>
    /// Called by the area once a game object is removed or dead.
    /// </summary>
    /// <param name="spawnArea">The spawn area this object is removed from.</param>
    public virtual void OnDespawned(SpawnArea spawnArea){}
    
    /// <summary>
    /// Checks if this object should be removed from the spawn area based on bounds.
    /// </summary>
    /// <param name="bounds">The rect bounds of the spawn area.</param>
    /// <returns>True if the object should be removed from the spawn area.</returns>
    public virtual bool HasLeftBounds(Rect bounds) => false;
    
    /// <summary>
    /// Tries to kill this game object.
    /// </summary>
    /// <param name="killMessage">Optional message for the kill event.</param>
    /// <param name="killer">Optional killer object.</param>
    /// <returns>True if kill was successful.</returns>
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
    /// <summary>
    /// Called after the object is killed. Override for custom logic.
    /// </summary>
    /// <param name="killMessage">Optional message for the kill event.</param>
    /// <param name="killer">Optional killer object.</param>
    protected virtual void WasKilled(string? killMessage = null, GameObject? killer = null) { }
    /// <summary>
    /// Called before the object is killed. Override to prevent kill by returning false.
    /// </summary>
    /// <param name="killMessage">Optional message for the kill event.</param>
    /// <param name="killer">Optional killer object.</param>
    /// <returns>True to allow kill, false to prevent.</returns>
    protected virtual bool TryKill(string? killMessage = null, GameObject? killer = null) => true;

    /// <summary>
    /// Tries to revive this game object.
    /// </summary>
    /// <param name="reviveMessage">Optional message for the revive event.</param>
    /// <param name="reviver">Optional reviver object.</param>
    /// <returns>True if revive was successful.</returns>
    public bool Revive(string? reviveMessage = null, GameObject? reviver = null)
    {
        if (!IsDead) return false;

        if (TryRevive(reviveMessage, reviver))
        {
            IsDead = false;
            WasRevived(reviveMessage, reviver);
            OnRevived?.Invoke(this, reviveMessage, reviver);
            return true;
        }

        return false;
    }
    /// <summary>
    /// Called after the object is revived. Override for custom logic.
    /// </summary>
    /// <param name="reviveMessage">Optional message for the revive event.</param>
    /// <param name="reviver">Optional reviver object.</param>
    protected virtual void WasRevived(string? reviveMessage = null, GameObject? reviver = null) { }
    /// <summary>
    /// Called before the object is revived. Override to prevent revive by returning false.
    /// </summary>
    /// <param name="reviveMessage">Optional message for the revive event.</param>
    /// <param name="reviver">Optional reviver object.</param>
    /// <returns>True to allow revive, false to prevent.</returns>
    protected virtual bool TryRevive(string? reviveMessage = null, GameObject? reviver = null) => true;
}