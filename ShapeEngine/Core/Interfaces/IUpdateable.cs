using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Interfaces;

public interface IUpdateable
{
    
    /// <summary>
    /// Only called when fixed framerate is disabled. Called every frame.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="game"></param>
    /// <param name="gameUi"></param>
    /// <param name="ui"></param>
    public void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui);
    
    /// <summary>
    /// Only called when fixed framerate is enabled. Called every frame before fixed update.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="game"></param>
    /// <param name="gameUi"></param>
    /// <param name="ui"></param>
    public void HandleInput(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui);
    
    /// <summary>
    /// Only called when fixed framerate is enabled. Called in fixed interval.
    /// </summary>
    /// <param name="fixedTime"></param>
    /// <param name="game"></param>
    /// <param name="gameUi"></param>
    /// <param name="ui"></param>
    public void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui);
    
    /// <summary>
    /// Only called when fixed framerate is enabled. Called every frame after fixed update calls.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="game"></param>
    /// <param name="gameUi"></param>
    /// <param name="ui"></param>
    /// <param name="f"></param>
    public void InterpolateFixedUpdate(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui, float f);

}