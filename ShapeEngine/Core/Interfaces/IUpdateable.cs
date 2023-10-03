using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Interfaces;

public interface IUpdateable
{
    public void Update(float dt, float deltaSow, ScreenInfo game, ScreenInfo ui);
}