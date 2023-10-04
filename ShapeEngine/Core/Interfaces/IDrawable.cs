using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Interfaces;

public interface IDrawable
{
    /// <summary>
    /// Draw the game here. Is affected by screen shaders and the camera.
    /// </summary>
    /// <param name="game"></param>
    public void DrawGame(ScreenInfo game);
    /// <summary>
    /// Draw to the game ui here. Is affected by screen shaders but NOT by the camera.
    /// </summary>
    /// <param name="game"></param>
    public void DrawGameUI(ScreenInfo ui);

    
    //public void DrawToTexture(ScreenTexture texture);
    //public void DrawToScreen(Vector2 size, Vector2 mousePos);
}