namespace ShapeEngine.Geometry.CollisionSystem;


//NOTE: 
// - coordinates struct -> needs to be hashable for dict (IEquatable?)
// - store buckets in Dict<Coordinates, Bucket>
// - store added colliders in register Dict<Collider, HashSet<Bucket>>
// - when adding a collider, get bounding box and use the 4 corners to calculate coordinate range -> add all buckets in that range to the collider's register entry and create buckets for unused coordinates
// - when querying, do the same (bounding box corners -> corrdinates -> coordinate range -> get all buckets in that range and return them)

//Q: Is there any way to set this up in a way that doesn´t require recalculating everything every frame? Just update colliders that are already in the register, and remove colliders that are no longer there?

//NOTE: 
// - set max amount of buckets to avoid memory issues
// - once max is reached, start reusing old buckets (either oldest, or empty ones, or least used ones?)
// - every frame all buckets are cleared but the could stay in the dict -> there needs to be a way to track which buckets are in used, which are empty, and which are old
// - the other way would be a simple pooling system where all buckets are cleared and removed from the dict and put into bucket pool.
// whenever a bucket is needed, one is taken from the pool or a new one is created if the pool is empty and is put into the inuse set

public class BroadphaseDynamicSpatialHash
{
    
}