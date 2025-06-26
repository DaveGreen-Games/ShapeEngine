using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class HyperBullet : ExplosivePayload
{
    public HyperBullet(CollisionHandler collisionHandler, BitFlag mask) : 
        base(new(0.5f, 0.2f, 50, 100, 100), collisionHandler, mask)
    {
    }
}