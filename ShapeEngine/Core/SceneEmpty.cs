using ShapeEngine.Core.Interfaces;

namespace ShapeEngine.Core
{
    public sealed class SceneEmpty : Scene
    {
        public SceneEmpty() { }

        protected override void OnActivate(Scene oldScene)
        {
        }

        protected override void OnDeactivate()
        {
        }
    }

}
