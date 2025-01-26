using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class Grenade350mm : ExplosivePayload
{
    public Grenade350mm(CollisionHandler collisionHandler, BitFlag mask) : 
        base(new(0.75f, 0.75f, 250, 800, 1500), collisionHandler, mask)
    {
    }
}