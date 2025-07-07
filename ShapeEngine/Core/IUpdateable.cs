using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core;

/// <summary>
/// Interface for objects that require update logic in the game loop.
/// </summary>
/// <remarks>
/// Implement this interface to receive update, fixed update, and interpolation calls from the engine.
/// Useful for game objects, systems, or managers that need to update their state every frame or at fixed intervals.
/// </remarks>
public interface IUpdateable
{
    /// <summary>
    /// Called every frame before <see cref="FixedUpdate"/>.
    /// </summary>
    /// <param name="time">The current game time for this frame.</param>
    /// <param name="game">Screen information for the main game view.</param>
    /// <param name="gameUi">Screen information for the game UI overlay.</param>
    /// <param name="ui">Screen information for the global UI.</param>
    /// <remarks>
    /// Use this method to update logic that should run every frame, such as input handling, animations, or non-physics updates.
    /// </remarks>
    public void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui);
    
    /// <summary>
    /// Called at a fixed interval when <see cref="GameDef.Game.FixedPhysicsFramerate"/> is enabled.
    /// </summary>
    /// <param name="fixedTime">The fixed time step for this update.</param>
    /// <param name="game">Screen information for the main game view.</param>
    /// <param name="gameUi">Screen information for the game UI overlay.</param>
    /// <param name="ui">Screen information for the global UI.</param>
    /// <remarks>
    /// Use this method for physics or other time-sensitive logic that requires consistent updates regardless of frame rate.
    /// </remarks>
    public void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui);
    
    /// <summary>
    /// Called every frame after <see cref="FixedUpdate"/> when <see cref="GameDef.Game.FixedPhysicsFramerate"/> is enabled,
    /// to interpolate between fixed updates.
    /// </summary>
    /// <param name="time">The current game time for this frame.</param>
    /// <param name="game">Screen information for the main game view.</param>
    /// <param name="gameUi">Screen information for the game UI overlay.</param>
    /// <param name="ui">Screen information for the global UI.</param>
    /// <param name="f">The interpolation factor between the last and next fixed update (0.0 to 1.0).</param>
    /// <remarks>
    /// Use this method to interpolate visual state for smooth rendering between fixed updates.
    /// </remarks>
    public void InterpolateFixedUpdate(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui, float f);

}