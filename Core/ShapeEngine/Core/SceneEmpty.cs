using System.Numerics;

namespace ShapeEngine.Core
{
    public abstract class Scene : IScene
    {
        public bool CallUpdate { get; set; } = true;
        public bool CallHandleInput { get; set; } = true;
        public bool CallDraw { get; set; } = true;
        public virtual void Close() { }

        public virtual void Activate(IScene oldScene) { }
        public virtual void Deactivate() { }
        //public virtual void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI) { }
        public virtual void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI) { }
        public virtual void Draw(Vector2 gameSIze, Vector2 mousePosGame) { }
        public virtual void DrawUI(Vector2 uiSize, Vector2 mousePosUI) { }
        public virtual void DrawToScreen(Vector2 screenSize, Vector2 mousePos) { }
        public virtual IArea? GetCurArea() { return null; }
    }
    public class Scene<TArea> : Scene where TArea : IArea
    {
        public TArea Area { get; protected set; }

        public Scene(TArea area)
        {
            Area = area;
        }

        public override IArea GetCurArea() { return Area; }
    }
    public sealed class SceneEmpty : Scene
    {
        public SceneEmpty() { }
    }

}
