using ShapeEngine.Screen;
using System.Numerics;

namespace ShapeEngine.Core
{
    public abstract class Scene : IScene
    {
        public virtual void Close() { }

        public virtual void Activate(IScene oldScene) { }
        public virtual void Deactivate() { }
        public virtual Area? GetCurArea() { return null; }


        public void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui) { }
        public virtual void DrawGame(Vector2 gameSIze, Vector2 mousePosGame) { }
        public virtual void DrawUI(Vector2 uiSize, Vector2 mousePosUI) { }
        public void DrawToTexture(ScreenTexture texture) { }
        public virtual void DrawToScreen(Vector2 screenSize, Vector2 mousePos) { }
    }
   
    public sealed class SceneEmpty : Scene
    {
        public SceneEmpty() { }
    }

    /*
    public class Scene<TArea> : Scene where TArea : IArea
    {
        public TArea Area { get; protected set; }

        public Scene(TArea area)
        {
            Area = area;
        }

        public override IArea GetCurArea() { return Area; }
    }
    */
}
