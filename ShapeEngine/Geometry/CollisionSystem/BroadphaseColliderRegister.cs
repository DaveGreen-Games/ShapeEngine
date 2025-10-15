namespace ShapeEngine.Geometry.CollisionSystem;

//TODO: Docs
public class BroadphaseColliderRegister<T>
{
    private readonly Dictionary<Collider, HashSet<T>> register = new();
    private readonly HashSet<Collider> unusedRegisterColliders = [];

    public HashSet<T>? AddEntry(Collider collider, int capacity)
    {
        HashSet<T> registerSet;
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
            registerSet = new HashSet<T>(capacity);
            register[collider] = registerSet;
        }
        return registerSet;
    }
    // public HashSet<T>? GetEntry(Collider collider)
    // {
    //     return register.TryGetValue(collider, out var registerBuckets) ? registerBuckets : null;
    // }

    public bool TryGetEntry(Collider collider, out HashSet<T>? value)
    {
        return register.TryGetValue(collider, out value);
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