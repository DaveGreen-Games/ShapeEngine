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
    /// Called every frame before Draw.
    /// </summary>
    /// <param name="time">The current game time for this frame.</param>
    /// <param name="game">Screen information for the main game view.</param>
    /// <param name="gameUi">Screen information for the game UI overlay.</param>
    /// <param name="ui">Screen information for the global UI.</param>
    /// <remarks>
    /// Use this method to update logic that should run every frame, such as input handling, animations, or non-physics updates.
    /// </remarks>
    public void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui);

}