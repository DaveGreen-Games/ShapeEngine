using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class Bomb : ExplosivePayload
{
    public Bomb(CollisionHandler collisionHandler, BitFlag mask) : 
        base(new(2f, 1f, 600, 1500, 2500), collisionHandler, mask)
    {
    }
}