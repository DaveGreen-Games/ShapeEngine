using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core
{
    public abstract class Scene : IScene
    {
        public virtual void Close() { }

        public virtual void Activate(IScene oldScene) { }
        public virtual void Deactivate() { }
        public virtual GameObjectHandler? GetGameObjectHandler() { return null; }

        public virtual void OnWindowSizeChanged(DimensionConversionFactors conversionFactors) { }
        public void OnWindowPositionChanged(Vector2 oldPos, Vector2 newPos){}
        public void OnMonitorChanged(MonitorInfo newMonitor){}

        public virtual void Update(float dt, ScreenInfo game, ScreenInfo ui) { }
        public virtual void DrawGame(ScreenInfo game) { }
        public virtual void DrawUI(ScreenInfo ui) { }
    }
   
    public sealed class SceneEmpty : Scene
    {
        public SceneEmpty() { }
    }

}
