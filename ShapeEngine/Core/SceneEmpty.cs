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
        public virtual void OnGamepadConnected(Gamepad gamepad){}

        public virtual void OnGamepadDisconnected(Gamepad gamepad){}

        public virtual void OnInputDeviceChanged(InputDevice prevDevice, InputDevice curDevice){}

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
