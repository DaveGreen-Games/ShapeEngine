using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Screen;

namespace ShapeEngine.Core;

public abstract class Scene : IUpdateable, IDrawable
{
    public SpawnArea? SpawnArea { get; private set; } = null;

    protected bool InitSpawnArea(Rect bounds)
    {
        if (SpawnArea != null) return false;

        SpawnArea = new(bounds);
            
        return true;
    }
    protected bool InitSpawnArea(float x, float y, float w, float h)
    {
        if (SpawnArea != null) return false;

        SpawnArea = new(x, y, w, h);
            
        return true;
    }
    protected bool InitSpawnArea(SpawnArea area)
    {
        if (SpawnArea != null) return false;
        if (SpawnArea == area) return false;

        SpawnArea = area;
            
        return true;
    }

    protected bool RemoveSpawnArea()
    {
        if (SpawnArea == null) return false;

        SpawnArea.Close();
        SpawnArea = null;
            
        return true;
    }
        
        
    public abstract void Activate(Scene oldScene);
    public abstract void Deactivate();
    public virtual void OnPauseChanged(bool paused) { }

    /// <summary>
    /// Used for cleanup. Should be called once right before the scene gets deleted.
    /// </summary>
    public virtual void Close()
    {
        RemoveSpawnArea();
    }

        
    public void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
    {
        SpawnArea?.Update(time, game, ui);
        OnUpdateGame(time, game, ui);
    }
    public void DrawGame(ScreenInfo game)
    {
        OnPreDrawGame(game);
        SpawnArea?.DrawGame(game);
        OnDrawGame(game);
    }
    public void DrawGameUI(ScreenInfo ui)
    {
        OnPreDrawGameUI(ui);
        SpawnArea?.DrawGameUI(ui);
        OnDrawGameUI(ui);
    }
    public void DrawUI(ScreenInfo ui)
    {
        OnDrawUI(ui);
    }
        
        
    protected virtual void OnUpdateGame(GameTime time, ScreenInfo game, ScreenInfo ui) { }
        
    /// <summary>
    /// Called before SpawnArea DrawGame is called.
    /// </summary>
    /// <param name="game"></param>
    protected virtual void OnPreDrawGame(ScreenInfo game) { }
    /// <summary>
    /// Draw your game. Is affected by screen shaders and the camera.
    /// </summary>
    /// <param name="game"></param>
    protected virtual void OnDrawGame(ScreenInfo game) { }
        
    /// <summary>
    /// Called before SpawnArea DrawGameUI is called
    /// </summary>
    /// <param name="ui"></param>
    protected virtual void OnPreDrawGameUI(ScreenInfo ui) { }
    /// <summary>
    /// Draw your game ui. Is affected by screen shaders but not by the camera.
    /// </summary>
    /// <param name="ui"></param>
    protected virtual void OnDrawGameUI(ScreenInfo ui) { }
        
    /// <summary>
    /// Draw your main ui. Is NOT affected by screen shaders and NOT affected by the camera.
    /// </summary>
    protected virtual void OnDrawUI(ScreenInfo ui) { }

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