using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Screen;

namespace ShapeEngine.Core.Interfaces;


public interface IScene : IUpdateable, IDrawable
{

    public GameObjectHandler? GetGameObjectHandler();

    //public void SetInput(ShapeInput input);
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

    public virtual void OnWindowSizeChanged(DimensionConversionFactors conversionFactors){}
    public virtual void OnWindowPositionChanged(Vector2 oldPos, Vector2 newPos){}
    public virtual void OnMonitorChanged(MonitorInfo newMonitor){}
    public virtual void OnGamepadConnected(ShapeGamepadDevice gamepad){}
    public virtual void OnGamepadDisconnected(ShapeGamepadDevice gamepad){}
    public virtual void OnInputDeviceChanged(InputDeviceType prevDeviceType, InputDeviceType curDeviceType){}
    public virtual void OnPausedChanged(bool newPaused) { }
    public virtual void OnMouseEnteredScreen() { }
    public virtual void OnMouseLeftScreen() { }
    public virtual void OnMouseVisibilityChanged(bool visible) { }
    public virtual void OnMouseEnabledChanged(bool enabled) { }
    public virtual void OnWindowFocusChanged(bool focused) { }
    public virtual void OnWindowFullscreenChanged(bool fullscreen) { }
    public virtual void OnWindowMaximizeChanged(bool maximized) { }

}