using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class HyperBullet : ExplosivePayload
{
    public HyperBullet(CollisionHandler collisionHandler, BitFlag mask) : 
        base(new(0.5f, 0.2f, 50, 100, 100), collisionHandler, mask)
    {
    }
}