using ShapeEngine.Core.Interfaces;

namespace ShapeEngine.Core
{
    public sealed class SceneEmpty : Scene
    {
        public SceneEmpty() { }
        public override void Activate(Scene oldScene)
        {
        }

        public override void Deactivate()
        {
        }
    }

}
