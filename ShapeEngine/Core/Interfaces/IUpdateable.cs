using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Interfaces;

public interface IUpdateable
{
    public void Update(GameTime time, ScreenInfo game, ScreenInfo ui);
}