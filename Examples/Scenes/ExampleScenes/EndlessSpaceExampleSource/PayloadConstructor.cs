using Examples.PayloadSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class PayloadConstructor : IPayloadConstructor
{
    private CollisionHandler collisionHandler;
    private BitFlag castMask;
    public PayloadConstructor(CollisionHandler collisionHandler, BitFlag mask)
    {
        this.collisionHandler = collisionHandler;
        this.castMask = mask;
    }
    
    public enum PayloadIds
    {
        Bomb = 1,
        Grenade350mm = 2,
        Grenade100mm = 3,
        HyperBullet = 4,
        Turret = 5,
        Penetrator = 6
    }
    
    
    public IPayload? Create(uint payloadId)
    {
        var id = (PayloadIds)payloadId;

        switch (id)
        {
            case PayloadIds.Bomb: return new Bomb(collisionHandler, castMask);
            case PayloadIds.Grenade350mm: return new Grenade350mm(collisionHandler, castMask);
            case PayloadIds.Grenade100mm: return new Grenade100mm(collisionHandler, castMask);
            case PayloadIds.HyperBullet: return new HyperBullet(collisionHandler, castMask);
            case PayloadIds.Turret: return new TurretPayload(collisionHandler, castMask);
            case PayloadIds.Penetrator: return new Penetrator(collisionHandler, castMask);
        }
        
        return null;
    }
}