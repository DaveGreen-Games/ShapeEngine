namespace ShapeEngine.Geometry.CollisionSystem;

//TODO: Docs
public class BroadphaseColliderRegister
{
    private readonly Dictionary<Collider, HashSet<BroadphaseBucket>> register = new();
    private readonly HashSet<Collider> unusedRegisterColliders = [];

    public HashSet<BroadphaseBucket>? AddEntry(Collider collider, int capacity)
    {
        HashSet<BroadphaseBucket> registerSet;
        if (register.TryGetValue(collider, out var value))
        {
            //already added this frame
            if (!unusedRegisterColliders.Contains(collider))
            {
                return null;
            }
            registerSet = value;
            registerSet.Clear();//clean up from last frame
            unusedRegisterColliders.Remove(collider);
        }
        else
        {
            registerSet = new HashSet<BroadphaseBucket>(capacity);
            register[collider] = registerSet;
        }
        return registerSet;
    }
    public HashSet<BroadphaseBucket>? GetEntry(Collider collider)
    {
        return register.TryGetValue(collider, out var registerBuckets) ? registerBuckets : null;
    }
    public void Clean()
    {
        //remaining colliders that were not used this frame. Remove them from the register.
        foreach (var collider in unusedRegisterColliders)
        {
            register.Remove(collider);
        }
        //set up for next frame
        unusedRegisterColliders.Clear();
        //all keys in register are now candidates for removal next frame if not used again
        unusedRegisterColliders.UnionWith(register.Keys);
    }
    public void Close()
    {
        register.Clear();
        unusedRegisterColliders.Clear();
    }
}