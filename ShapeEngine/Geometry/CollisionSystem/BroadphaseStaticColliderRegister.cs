using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Geometry.CollisionSystem;

//TODO: Docs
public class BroadphaseStaticColliderRegister<T>
{
    private readonly Dictionary<Collider, Rect> registerRects = new();
    private readonly Dictionary<Collider, HashSet<T>> register = new();
    private readonly HashSet<Collider> unusedRegisterColliders = [];

    public bool AddEntry(Collider collider, int capacity, out HashSet<T> coordinates, out Rect rect)
    {
        if (register.TryGetValue(collider, out var value))
        {
            unusedRegisterColliders.Remove(collider);
            coordinates =  value;
            rect = registerRects[collider];
            return true;
        }
        coordinates = new HashSet<T>(capacity);
        register[collider] = coordinates;
        registerRects[collider] = new Rect();
        rect = new();
        return false;
    }

    public bool UpdateRect(Collider collider, Rect rect)
    {
        if (!registerRects.ContainsKey(collider)) return false;
        registerRects[collider] = rect;
        return true;

    }
    public void Clean()
    {
        //remaining colliders that were not used this frame. Remove them from the register.
        foreach (var collider in unusedRegisterColliders)
        {
            register.Remove(collider);
            registerRects.Remove(collider);
        }
        //set up for next frame
        unusedRegisterColliders.Clear();
        //all keys in register are now candidates for removal next frame if not used again
        unusedRegisterColliders.UnionWith(register.Keys);
    }
    public void Close()
    {
        register.Clear();
        registerRects.Clear();
        unusedRegisterColliders.Clear();
    }
}