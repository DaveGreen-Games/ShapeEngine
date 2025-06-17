using ShapeEngine.StaticLib;
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

