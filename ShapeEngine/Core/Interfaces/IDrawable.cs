using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Interfaces;

/// <summary>
/// Represents an object that can be drawn in the game and UI layers.
/// </summary>
/// <remarks>
/// Implement this interface to provide custom drawing logic for game objects. 
/// The <see cref="DrawGame"/> method is affected by the camera and screen shaders, while <see cref="DrawGameUI"/> is only affected by screen shaders.
/// </remarks>
public interface IDrawable
{
    /// <summary>
    /// Draws the object to the game world. This drawing is affected by screen shaders and the camera.
    /// </summary>
    /// <param name="game">The <see cref="ScreenInfo"/> context for the game world drawing.</param>
    void DrawGame(ScreenInfo game);

    /// <summary>
    /// Draws the object to the game UI. This drawing is affected by screen shaders but not by the camera.
    /// </summary>
    /// <param name="gameUi">The <see cref="ScreenInfo"/> context for the UI drawing.</param>
    void DrawGameUI(ScreenInfo gameUi);
}