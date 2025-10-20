using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class Bomb : ExplosivePayload
{
    public Bomb(CollisionHandler collisionHandler, BitFlag mask) : 
        base(new(2f, 1f, 600, 1500, 2500), collisionHandler, mask)
    {
    }
}