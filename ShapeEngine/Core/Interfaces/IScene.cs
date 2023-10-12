using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
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

    public virtual void OnWindowSizeChanged(DimensionConversionFactors conversionFactors){}
    public virtual void OnWindowPositionChanged(Vector2 oldPos, Vector2 newPos){}
    public virtual void OnMonitorChanged(MonitorInfo newMonitor){}
    public virtual void OnGamepadConnected(Gamepad gamepad){}
    public virtual void OnGamepadDisconnected(Gamepad gamepad){}
    public virtual void OnInputDeviceChanged(InputDevice prevDevice, InputDevice curDevice){}
    public virtual void OnPausedChanged(bool newPaused) { }
    public virtual void OnCursorEnteredScreen() { }
    public virtual void OnCursorLeftScreen() { }
    public virtual void OnCursorHiddenChanged(bool hidden) { }
    public virtual void OnCursorLockChanged(bool locked) { }
    public virtual void OnWindowFocusChanged(bool focused) { }
    public virtual void OnWindowFullscreenChanged(bool fullscreen) { }
    public virtual void OnWindowMaximizeChanged(bool maximized) { }

}