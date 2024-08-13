using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Interfaces;

public interface IUpdateable
{
    public void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui);
    public void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui);
    public void InterpolateFixedUpdate(float f);
}