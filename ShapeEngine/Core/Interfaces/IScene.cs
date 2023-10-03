using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Screen;

namespace ShapeEngine.Core.Interfaces;

public interface IScene : IUpdateable, IDrawable
{

    public GameObjectHandler? GetGameObjectHandler();


    public void Activate(IScene oldScene);// { }
    public void Deactivate();

        
    /// <summary>
    /// Used for cleanup. Should be called once right before the scene gets deleted.
    /// </summary>
    public void Close();

    public void OnWindowSizeChanged(DimensionConversionFactors conversionFactors);

    public void OnWindowPositionChanged(Vector2 oldPos, Vector2 newPos);

    public void OnMonitorChanged(MonitorInfo newMonitor);
    //public void DrawToTexture(ScreenTexture texture);
    //public void DrawToScreen(Vector2 size, Vector2 mousePos);

}