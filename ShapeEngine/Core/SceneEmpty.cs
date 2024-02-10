using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;

namespace ShapeEngine.Core
{
    public abstract class Scene : IScene
    {
        public virtual void OnPauseChanged(bool paused){}

        public virtual void Close() { }


        public virtual void Activate(IScene oldScene) { }
        public virtual void Deactivate() { }
        public virtual GameObjectHandler? GetGameObjectHandler() { return null; }

        public virtual void OnWindowSizeChanged(DimensionConversionFactors conversionFactors) { }
        public virtual void OnWindowPositionChanged(Vector2 oldPos, Vector2 newPos){}
        public virtual void OnMonitorChanged(MonitorInfo newMonitor){}
        public virtual void OnGamepadConnected(ShapeGamepadDevice gamepad){}
        public virtual void OnGamepadDisconnected(ShapeGamepadDevice gamepad){}
        public virtual void OnInputDeviceChanged(InputDeviceType prevDeviceType, InputDeviceType curDeviceType){}
        public virtual void OnWindowMaximizeChanged(bool maximized){}
        public virtual void OnWindowFullscreenChanged(bool fullscreen){}
        public virtual void OnPausedChanged(bool newPaused){}
        public virtual void OnMouseEnteredScreen(){}
        public virtual void OnMouseVisibilityChanged(bool hidden){}
        public virtual void OnMouseLeftScreen(){}
        public virtual void OnMouseEnabledChanged(bool locked){}
        public virtual void OnWindowFocusChanged(bool focused){}

        public virtual void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui) { }
        public virtual void DrawGame(ScreenInfo game) { }
        public virtual void DrawGameUI(ScreenInfo ui) { }
        public virtual void DrawUI(ScreenInfo ui) { }
    }
   
    public sealed class SceneEmpty : Scene
    {
        public SceneEmpty() { }
    }

}
