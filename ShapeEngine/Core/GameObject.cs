using System.ComponentModel;
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
    #region Events
    
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
    
    #endregion
    
    #region Properties
    
    /// <summary>
    /// Gets or sets the transform (position, rotation, scale) of this object.
    /// </summary>
    public Transform2D Transform { get; set; }
    
    /// <summary>
    /// Gets whether this object is dead (killed).
    /// </summary>
    public bool IsDead { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether this object is currently managed by a <see cref="SpawnArea"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the object has been added to and is actively managed by a <see cref="SpawnArea"/>; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// <b>Deferred Processing:</b> Because <see cref="SpawnArea"/> processes additions and removals using deferred logic,
    /// this state may not change immediately upon calling <see cref="SpawnArea.AddGameObject(GameObject)"/> or <see cref="SpawnArea.RemoveGameObject(GameObject)"/>.
    /// Instead, the value is updated once the object is actually processed (which can be delayed until the end of the current frame).
    /// </para>
    /// </remarks>
    public bool IsSpawned { get; private set; }
    
    private uint currentLayer;
    
    /// <summary>
    /// Gets or sets the area layer in which this object is stored and drawn. Higher layers are rendered on top of lower layers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Restriction:</b> The layer <b>cannot</b> be changed while the object is spawned (i.e., when <see cref="IsSpawned"/> is <see langword="true"/>).
    /// </para>
    /// <para>
    /// You must check that <see cref="IsSpawned"/> is <see langword="false"/> (or remove the object from its <see cref="SpawnArea"/>)
    /// before setting this value. Attempting to modify the layer while spawned throws an <see cref="InvalidOperationException"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when attempting to set the layer while <see cref="IsSpawned"/> is <see langword="true"/>.</exception>
    public uint Layer
    {
        get => currentLayer;
        set
        {
            if (IsSpawned)
            {
                throw new InvalidOperationException(
                    "Layer cannot be changed while the GameObject is spawned. Remove it from the SpawnArea first.");
            }
            currentLayer = value;
        }
    }
    
    #endregion
    
    #region Public Methods
    
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

    #endregion

    #region Protected Methods
    
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

    #endregion
    
    #region Internal

    internal void ResolveOnSpawned(SpawnArea spawnArea)
    {
        IsSpawned = true;
        OnSpawned(spawnArea);
    }
    
    internal void ResolveOnDespawned(SpawnArea spawnArea)
    {
        IsSpawned = false;
        OnDespawned(spawnArea);
    }

    #endregion
}