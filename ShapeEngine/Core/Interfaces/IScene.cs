using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Screen;

namespace ShapeEngine.Core.Interfaces;

public interface IScene : IUpdateable, IDrawable
{

    public GameObjectHandler? GetGameObjectHandler();


    public void Activate(IScene oldScene);// { }
    public void Deactivate();
    public void OnPauseChanged(bool paused);
        
    /// <summary>
    /// Used for cleanup. Should be called once right before the scene gets deleted.
    /// </summary>
    public void Close();

    /// <summary>
    /// Draw your main ui. Is NOT affected by screen shaders and NOT affected by the camera.
    /// </summary>
    /// <param name="ui"></param>
    public void DrawUI(ScreenInfo ui);

    public void OnWindowSizeChanged(DimensionConversionFactors conversionFactors);

    public void OnWindowPositionChanged(Vector2 oldPos, Vector2 newPos);

    public void OnMonitorChanged(MonitorInfo newMonitor);
    //public void DrawToTexture(ScreenTexture texture);
    //public void DrawToScreen(Vector2 size, Vector2 mousePos);

}