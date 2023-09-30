using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Interfaces;

public interface IDrawable
{
    public void DrawGame(ScreenInfo game);
    public void DrawUI(ScreenInfo ui);
    //public void DrawToTexture(ScreenTexture texture);
    //public void DrawToScreen(Vector2 size, Vector2 mousePos);
}