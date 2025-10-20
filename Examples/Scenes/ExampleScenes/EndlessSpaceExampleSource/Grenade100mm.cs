using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class Grenade100mm : ExplosivePayload
{
    public Grenade100mm(CollisionHandler collisionHandler, BitFlag mask) : 
        base(new(0.25f, 0.25f, 80, 400, 500), collisionHandler, mask)
    {
    }
}