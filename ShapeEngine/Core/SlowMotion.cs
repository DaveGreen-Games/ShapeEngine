using ShapeEngine.Lib;
using ShapeEngine.Timing;

namespace ShapeEngine.Core;

public sealed class SlowMotion
{
    public const uint TagDefault = 0;
    
    internal class Container : List<Item>
    {
        public uint Tag { get; private set; }
        public float TotalFactor { get; private set; } = 1f;

        public Container(uint tag)
        {
            Tag = tag;
        }
        
        public Container Copy()
        {
            var copy = new Container(Tag);

            foreach (var item in this)
            {
                copy.Add(item.Copy());
            }

            return copy;
        }
        public void Update(float dt)
        {
            float totalFactor = 1f;
            for (int i = Count - 1; i >= 0; i--)
            {
                var item = this[i];
                item.Update(dt);
                if (item.Finished) RemoveAt(i);
                else totalFactor *= item.Factor;

            }

            TotalFactor = totalFactor;
        }

        public bool RemoveID(uint id)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i].ID == id)
                {
                    RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
    }
    internal class Item
    {
        public uint ID { get; private set; }
        public uint Tag { get; private set; }
        public float Factor { get; private set; }

        private float timer = 0f;

        public Item(float factor, float duration, uint tag)
        {
            ID = ShapeID.NextID;
            Tag = tag;
            Factor = MathF.Max(0f, factor);
            if (duration > 0f)
            {
                // this.duration = duration;
                this.timer = duration;
            }
        }
        private Item(float factor, float duration, uint tag, uint id)
        {
            ID = id;
            Tag = tag;
            Factor = MathF.Max(0f, factor);
            if (duration > 0f)
            {
                // this.duration = duration;
                this.timer = duration;
            }
        }

        public Item Copy()
        {
            var copy = new Item(Factor, timer, Tag, ID);
            return copy;
        }

    
        public virtual bool Finished => timer < 0f;
        public virtual void Update(float dt)
        {
            if (timer <= 0f) return;

            timer -= dt;
            if (timer <= 0f) timer = -1f;
        }
    }

    
    private readonly Dictionary<uint, Container> slowItemContainers = new();

    public void Update(float dt)
    {
        foreach (var container in slowItemContainers.Values)
        {
            container.Update(dt);
        }
    }
    
    public uint Add(float factor, float duration, uint tag)
    {
        var slowItem = new Item(factor, duration, tag);
        
        if(slowItemContainers.ContainsKey(tag)) slowItemContainers[tag].Add(slowItem);
        else slowItemContainers.Add(tag, new Container(tag){slowItem});

        return slowItem.ID;
    }
    public uint Add(float factor, float duration) => Add(factor, duration, TagDefault);

    public SlowMotionState Clear()
    {
        var state = new SlowMotionState(slowItemContainers.Values);
        slowItemContainers.Clear();
        return state;
    }

    public SlowMotionState Clear(params uint[] tags)
    {
        List<Container> removed = new();
        foreach (uint tag in tags)
        {
            if(!slowItemContainers.ContainsKey(tag)) continue;
            var container = slowItemContainers[tag];
            slowItemContainers.Remove(tag);
            removed.Add(container);
        }
        return new SlowMotionState(removed);
    }
    public void ApplyState(SlowMotionState state)
    {
        foreach (var container in state.Containers)
        {
            if(!slowItemContainers.ContainsKey(container.Tag)) slowItemContainers.Add(container.Tag, container);
            else
            {
                slowItemContainers[container.Tag].AddRange(container);
            }
        }
    }

    public bool RemoveTag(uint tag) => slowItemContainers.Remove(tag);
    public void Remove(uint id, uint tag)
    {
        if (!slowItemContainers.ContainsKey(tag)) return;
    }
    public bool Remove(uint id)
    {
        foreach (var container in slowItemContainers.Values)
        {
            if (container.RemoveID(id)) return true;
        }

        return false;
    }
    public bool HasTag(uint tag) => slowItemContainers.ContainsKey(tag);
    public float GetFactor(uint tag)
    {
        if (!HasTag(tag)) return 1f;
        return slowItemContainers[tag].TotalFactor;
    }
    public float GetFactor(params uint[] tags)
    {
        float totalFactor = 1f;
        foreach (var tag in tags)
        {
            totalFactor *= GetFactor(tag);
        }

        return totalFactor;
    }
    
}



// public interface ISlowMotionTween : ISequenceable
// {
//     public float GetFactor();
// }
// public class SlowMotionTween : ISlowMotionTween
// {
//     private float factorCur;
//     private float factorStart;
//     private float factorChange = 0f;
//     private float duration = 0f;
//     private float timer = 0f;
//     private TweenType tweenType = TweenType.LINEAR;
//
//     public SlowMotionTween(float factor)
//     {
//         factorStart = ShapeMath.Clamp(factor, 0f, 1f);
//         factorCur = factorStart;
//     }
//     public SlowMotionTween(float factor, float duration)
//     {
//         factorStart = ShapeMath.Clamp(factor, 0f, 1f);
//         factorCur = factorStart;
//         if (duration > 0f)
//         {
//             this.duration = duration;
//             this.timer = duration;
//         } 
//     }
//     public SlowMotionTween(float factorStart, float factorEnd, float duration)
//     {
//         this.factorStart = ShapeMath.Clamp(factorStart, 0f, 1f);
//         this.factorCur = this.factorStart;
//         if (duration > 0f)
//         {
//             this.duration = duration;
//             this.timer = duration;
//             this.factorChange = ShapeMath.Clamp(factorEnd, 0f, 1f) - this.factorStart;
//         } 
//     }
//
//     private SlowMotionTween(SlowMotionTween tween)
//     {
//         factorCur = tween.factorCur;
//         factorStart = tween.factorStart;
//         factorChange = tween.factorChange;
//         duration = tween.duration;
//         timer = tween.timer;
//         tweenType = tween.tweenType;
//     }
//
//     public ISequenceable Copy() => new SlowMotionTween(this);
//     
//     public bool Update(float dt)
//     {
//         if (duration <= 0f) return false;
//         if (timer <= 0f) return true;
//         
//         timer -= dt;
//         if (timer < 0f) timer = 0f;
//
//         if (factorChange != 0f)
//         {
//             float t = 1f - (timer / duration);
//             factorCur = ShapeTween.Tween(factorStart, factorStart + factorChange, t, tweenType);
//         }
//         return timer <= 0f;
//     }
//
//     public float GetFactor() => factorCur;
// }

/*
public sealed class SlowMotionState
   {
   internal readonly List<SlowMotion.Container> Containers;
   internal SlowMotionState(IEnumerable<SlowMotion.Container> containers)
   {
   Containers = containers.ToList();
   
   }
   }
   public sealed class SlowMotion
   {
   public const uint TagDefault = 0;
   
   internal class Container : List<Item>
   {
   public uint Tag { get; private set; }
   public float TotalFactor { get; private set; } = 1f;
   
   public Container(uint tag)
   {
   Tag = tag;
   }
   
   public Container Copy()
   {
   var copy = new Container(Tag);
   
   foreach (var item in this)
   {
   copy.Add(item.Copy());
   }
   
   return copy;
   }
   public void Update(float dt)
   {
   float totalFactor = 1f;
   for (int i = Count - 1; i >= 0; i--)
   {
   var item = this[i];
   item.Update(dt);
   if (item.Finished) RemoveAt(i);
   else totalFactor *= item.Factor;
   
   }
   
   TotalFactor = totalFactor;
   }
   
   public bool RemoveID(uint id)
   {
   for (int i = Count - 1; i >= 0; i--)
   {
   if (this[i].ID == id)
   {
   RemoveAt(i);
   return true;
   }
   }
   
   return false;
   }
   }
   internal class Item
   {
   public uint ID { get; private set; }
   public uint Tag { get; private set; }
   public float Factor { get; private set; }
   
   private float timer = 0f;
   
   public Item(float factor, float duration, uint tag)
   {
   ID = ShapeID.NextID;
   Tag = tag;
   Factor = MathF.Max(0f, factor);
   if (duration > 0f)
   {
   // this.duration = duration;
   this.timer = duration;
   }
   }
   private Item(float factor, float duration, uint tag, uint id)
   {
   ID = id;
   Tag = tag;
   Factor = MathF.Max(0f, factor);
   if (duration > 0f)
   {
   // this.duration = duration;
   this.timer = duration;
   }
   }
   
   public Item Copy()
   {
   var copy = new Item(Factor, timer, Tag, ID);
   return copy;
   }
   
   
   public virtual bool Finished => timer < 0f;
   public virtual void Update(float dt)
   {
   if (timer <= 0f) return;
   
   timer -= dt;
   if (timer <= 0f) timer = -1f;
   }
   }
   
   
   private readonly Dictionary<uint, Container> slowItemContainers = new();
   
   public void Update(float dt)
   {
   foreach (var container in slowItemContainers.Values)
   {
   container.Update(dt);
   }
   }
   
   public uint Add(float factor, float duration, uint tag)
   {
   var slowItem = new Item(factor, duration, tag);
   
   if(slowItemContainers.ContainsKey(tag)) slowItemContainers[tag].Add(slowItem);
   else slowItemContainers.Add(tag, new Container(tag){slowItem});
   
   return slowItem.ID;
   }
   public uint Add(float factor, float duration) => Add(factor, duration, TagDefault);
   
   public SlowMotionState Clear()
   {
   var state = new SlowMotionState(slowItemContainers.Values);
   slowItemContainers.Clear();
   return state;
   }
   
   public SlowMotionState Clear(params uint[] tags)
   {
   List<Container> removed = new();
   foreach (uint tag in tags)
   {
   if(!slowItemContainers.ContainsKey(tag)) continue;
   var container = slowItemContainers[tag];
   slowItemContainers.Remove(tag);
   removed.Add(container);
   }
   return new SlowMotionState(removed);
   }
   public void ApplyState(SlowMotionState state)
   {
   foreach (var container in state.Containers)
   {
   if(!slowItemContainers.ContainsKey(container.Tag)) slowItemContainers.Add(container.Tag, container);
   else
   {
   slowItemContainers[container.Tag].AddRange(container);
   }
   }
   }
   
   public bool RemoveTag(uint tag) => slowItemContainers.Remove(tag);
   public void Remove(uint id, uint tag)
   {
   if (!slowItemContainers.ContainsKey(tag)) return;
   }
   public bool Remove(uint id)
   {
   foreach (var container in slowItemContainers.Values)
   {
   if (container.RemoveID(id)) return true;
   }
   
   return false;
   }
   public bool HasTag(uint tag) => slowItemContainers.ContainsKey(tag);
   public float GetFactor(uint tag)
   {
   if (!HasTag(tag)) return 1f;
   return slowItemContainers[tag].TotalFactor;
   }
   public float GetFactor(params uint[] tags)
   {
   float totalFactor = 1f;
   foreach (var tag in tags)
   {
   totalFactor *= GetFactor(tag);
   }
   
   return totalFactor;
   }
   
   }
*/

/*public sealed class SlowMotion
{
    public const uint TagDefault = 0;
    
    internal class Container : List<Item>
    {
        public uint Tag { get; private set; }
        public float TotalFactor { get; private set; } = 1f;

        public Container(uint tag)
        {
            Tag = tag;
        }
        
        public Container Copy()
        {
            var copy = new Container(Tag);

            foreach (var item in this)
            {
                copy.Add(item.Copy());
            }

            return copy;
        }
        public void Update(float dt)
        {
            float totalFactor = 1f;
            for (int i = Count - 1; i >= 0; i--)
            {
                var item = this[i];
                item.Update(dt);
                if (item.Finished) RemoveAt(i);
                else totalFactor *= item.Factor;

            }

            TotalFactor = totalFactor;
        }

        public bool RemoveID(uint id)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i].ID == id)
                {
                    RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
    }
    internal class Item
    {
        public uint ID { get; private set; }
        public uint Tag { get; private set; }

        public float Factor { get; private set; }
        private float factorStart;

        private float timer = 0f;
        private float duration = 0f;
        private float factorChange = 0f;
        private TweenType tweenType = TweenType.LINEAR;

        public Item(float factor, uint tag)
        {
            ID = ShapeID.NextID;
            Tag = tag;
            factorStart = ShapeMath.Clamp(factor, 0f, 1f);
            Factor = factorStart;
        }
        public Item(float factor, float duration, uint tag)
        {
            ID = ShapeID.NextID;
            Tag = tag;
            factorStart = ShapeMath.Clamp(factor, 0f, 1f);
            Factor = factorStart;
            if (duration > 0f)
            {
                this.duration = duration;
                this.timer = duration;
            }
        }
        public Item(float factorStart, float factorEnd, float duration, uint tag, TweenType tweenType = TweenType.LINEAR)
        {
            ID = ShapeID.NextID;
            Tag = tag;
            this.factorStart = ShapeMath.Clamp(factorStart, 0f, 1f);
            Factor = this.factorStart;
            if (duration > 0f)
            {
                this.duration = duration;
                this.timer = duration;
                factorChange = ShapeMath.Clamp(factorEnd, 0f, 1f) - this.factorStart;
                this.tweenType = tweenType;
            }
        }
        
        private Item(Item item)
        {
            ID = item.ID;
            Tag = item.Tag;
            Factor = item.Factor;
            factorStart = item.Factor;
            timer = item.timer;
            duration = item.duration;
            factorChange = item.factorChange;
            tweenType = item.tweenType;
        }

        public Item Copy() => new(this);


        public virtual bool Finished => timer <= 0f && duration > 0f;
        public virtual void Update(float dt)
        {
            if (timer <= 0f) return;

            timer -= dt;
            if (timer <= 0f) timer = 0f;

            if (factorChange != 0f && duration > 0f)
            {
                float f = 1f - (timer / duration);
                Factor = ShapeTween.Tween(factorStart, factorStart + factorChange, f, tweenType);
            }
            
            
        }
    }

    
    private readonly Dictionary<uint, Container> slowItemContainers = new();

    public void Update(float dt)
    {
        foreach (var container in slowItemContainers.Values)
        {
            container.Update(dt);
        }
    }

    public uint Add(float factor) => Add(factor, 0f);
    public uint Add(float factor, uint tag) => Add(factor, 0f, tag);
    public uint Add(float factor, float duration, uint tag)
    {
        var slowItem = new Item(factor, duration, tag);

        Add(slowItem);

        return slowItem.ID;
    }
    public uint Add(float factor, float duration) => Add(factor, duration, TagDefault);

    public uint Add(float factorStart, float factorEnd, float duration, uint tag, TweenType tweenType = TweenType.LINEAR)
    {
        var slowItem = new Item(factorStart, factorEnd, duration, tag, tweenType);

        Add(slowItem);

        return slowItem.ID;
    }
    private void Add(Item slowItem)
    {
        var tag = slowItem.Tag;
        if(slowItemContainers.ContainsKey(tag)) slowItemContainers[tag].Add(slowItem);
        else slowItemContainers.Add(tag, new Container(tag){slowItem});
    }
    
    public SlowMotionState Clear()
    {
        var state = new SlowMotionState(slowItemContainers.Values);
        slowItemContainers.Clear();
        return state;
    }

    public SlowMotionState Clear(params uint[] tags)
    {
        List<Container> removed = new();
        foreach (uint tag in tags)
        {
            if(!slowItemContainers.ContainsKey(tag)) continue;
            var container = slowItemContainers[tag];
            slowItemContainers.Remove(tag);
            removed.Add(container);
        }
        return new SlowMotionState(removed);
    }
    public void ApplyState(SlowMotionState state)
    {
        foreach (var container in state.Containers)
        {
            if(!slowItemContainers.ContainsKey(container.Tag)) slowItemContainers.Add(container.Tag, container);
            else
            {
                slowItemContainers[container.Tag].AddRange(container);
            }
        }
    }

    public bool RemoveTag(uint tag) => slowItemContainers.Remove(tag);
    public void Remove(uint id, uint tag)
    {
        if (!slowItemContainers.ContainsKey(tag)) return;
    }
    public bool Remove(uint id)
    {
        foreach (var container in slowItemContainers.Values)
        {
            if (container.RemoveID(id)) return true;
        }

        return false;
    }
    public bool HasTag(uint tag) => slowItemContainers.ContainsKey(tag);
    public float GetFactor(uint tag)
    {
        if (!HasTag(tag)) return 1f;
        return slowItemContainers[tag].TotalFactor;
    }
    public float GetFactor(params uint[] tags)
    {
        float totalFactor = 1f;
        foreach (var tag in tags)
        {
            totalFactor *= GetFactor(tag);
        }

        return totalFactor;
    }
    
}*/