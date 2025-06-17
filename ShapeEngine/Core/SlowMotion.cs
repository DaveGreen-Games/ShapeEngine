using ShapeEngine.StaticLib;
using ShapeEngine.Timing;

namespace ShapeEngine.Core;

/// <summary>
/// Manages and applies slow motion effects using time scaling factors, organized by tags.
/// </summary>
/// <remarks>
/// Allows stacking and combining multiple slow motion effects, each with its own tag and duration.
/// Useful for gameplay mechanics such as bullet time, slow zones, or temporary slowdowns.
/// </remarks>
public sealed class SlowMotion
{
    /// <summary>
    /// The default tag value for slow motion effects.
    /// </summary>
    public const uint TagDefault = 0;
    
    internal class Container : List<Item>
    {
        /// <summary>
        /// The tag associated with this container.
        /// </summary>
        public uint Tag { get; private set; }
        /// <summary>
        /// The total time scaling factor for all active items in this container.
        /// </summary>
        public float TotalFactor { get; private set; } = 1f;

        public Container(uint tag)
        {
            Tag = tag;
        }
        
        /// <summary>
        /// Creates a copy of the container, including all its items.
        /// </summary>
        /// <returns>A new <see cref="Container"/> instance with the same items.</returns>
        public Container Copy()
        {
            var copy = new Container(Tag);

            foreach (var item in this)
            {
                copy.Add(item.Copy());
            }

            return copy;
        }
        /// <summary>
        /// Updates all items in the container, removing expired ones and recalculating the total factor.
        /// </summary>
        /// <param name="dt">The time delta in seconds since the last update.</param>
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

        /// <summary>
        /// Removes an item with the specified ID from the container.
        /// </summary>
        /// <param name="id">The ID of the item to remove.</param>
        /// <returns>True if the item was found and removed; otherwise, false.</returns>
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
        /// <summary>
        /// The unique ID of this slow motion item.
        /// </summary>
        public uint ID { get; private set; }
        /// <summary>
        /// The tag associated with this slow motion item.
        /// </summary>
        public uint Tag { get; private set; }
        /// <summary>
        /// The time scaling factor of this slow motion item.
        /// </summary>
        public float Factor { get; private set; }

        private float timer = 0f;

        /// <summary>
        /// Creates a new slow motion item.
        /// </summary>
        /// <param name="factor">The time scaling factor (0 = stopped, 1 = normal speed).</param>
        /// <param name="duration">The duration in seconds for the effect. If 0, the effect is permanent until removed.</param>
        /// <param name="tag">The tag to group this effect under. Effects with the same tag are combined.</param>
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

        /// <summary>
        /// Creates a copy of the slow motion item.
        /// </summary>
        /// <returns>A new <see cref="Item"/> instance with the same properties.</returns>
        public Item Copy()
        {
            var copy = new Item(Factor, timer, Tag, ID);
            return copy;
        }

    
        /// <summary>
        /// Indicates if the slow motion effect has finished (i.e., the timer has expired).
        /// </summary>
        public bool Finished => timer < 0f;
        /// <summary>
        /// Updates the item's timer, marking it as finished if the timer expires.
        /// </summary>
        /// <param name="dt">The time delta in seconds since the last update.</param>
        public void Update(float dt)
        {
            if (timer <= 0f) return;

            timer -= dt;
            if (timer <= 0f) timer = -1f;
        }
    }

    
    private readonly Dictionary<uint, Container> slowItemContainers = new();

    /// <summary>
    /// Updates all active slow motion effects, removing expired ones and recalculating total factors.
    /// </summary>
    /// <param name="dt">The time delta in seconds since the last update.</param>
    public void Update(float dt)
    {
        foreach (var container in slowItemContainers.Values)
        {
            container.Update(dt);
        }
    }
    
    /// <summary>
    /// Adds a new slow motion effect.
    /// </summary>
    /// <param name="factor">The time scaling factor <c>(0 = stopped, 1 = normal speed)</c>.</param>
    /// <param name="duration">The duration in seconds for the effect.
    /// If 0, the effect is permanent until removed.</param>
    /// <param name="tag">The tag to group this effect under. Effects with the same tag are combined.</param>
    /// <returns>The unique ID of the created slow motion effect.</returns>
    /// <remarks>
    /// Use tags to independently control different slow motion sources.
    /// </remarks>
    public uint Add(float factor, float duration, uint tag)
    {
        var slowItem = new Item(factor, duration, tag);
        
        if(slowItemContainers.ContainsKey(tag)) slowItemContainers[tag].Add(slowItem);
        else slowItemContainers.Add(tag, new Container(tag){slowItem});

        return slowItem.ID;
    }
    /// <summary>
    /// Adds a new slow motion effect with the default tag.
    /// </summary>
    /// <param name="factor">The time scaling factor <c>(0 = stopped, 1 = normal speed)</c>.</param>
    /// <param name="duration">The duration in seconds for the effect.
    /// If 0, the effect is permanent until removed.</param>
    /// <returns>The unique ID of the created slow motion effect.</returns>
    public uint Add(float factor, float duration) => Add(factor, duration, TagDefault);

    /// <summary>
    /// Clears all slow motion effects and returns their previous state.
    /// </summary>
    /// <returns>A <see cref="SlowMotionState"/> representing the cleared effects.</returns>
    /// <remarks>
    /// Use <see cref="ApplyState"/> to restore the cleared effects later.
    /// </remarks>
    public SlowMotionState Clear()
    {
        var state = new SlowMotionState(slowItemContainers.Values);
        slowItemContainers.Clear();
        return state;
    }

    /// <summary>
    /// Clears slow motion effects for the specified tags and returns their previous state.
    /// </summary>
    /// <param name="tags">The tags of the effects to clear.</param>
    /// <returns>A <see cref="SlowMotionState"/> representing the cleared effects.</returns>
    /// <remarks>
    /// Use <see cref="ApplyState"/> to restore the cleared effects later.
    /// </remarks>
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
    /// <summary>
    /// Applies a previously saved <see cref="SlowMotionState"/>, restoring its effects.
    /// </summary>
    /// <param name="state">The state to apply.</param>
    /// <remarks>
    /// Useful for temporarily disabling slow motion and restoring it later.
    /// </remarks>
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

    /// <summary>
    /// Removes all slow motion effects with the specified tag.
    /// </summary>
    /// <param name="tag">The tag of the effects to remove.</param>
    /// <returns>True if effects were removed; otherwise, false.</returns>
    public bool RemoveTag(uint tag) => slowItemContainers.Remove(tag);

    /// <summary>
    /// Removes a slow motion effect by its unique ID and tag.
    /// </summary>
    /// <param name="id">The unique ID of the effect to remove.</param>
    /// <param name="tag">The tag of the effect to remove.</param>
    public void Remove(uint id, uint tag)
    {
        if (!slowItemContainers.ContainsKey(tag)) return;
    }
    /// <summary>
    /// Removes a slow motion effect by its unique ID, searching all tags.
    /// </summary>
    /// <param name="id">The unique ID of the effect to remove.</param>
    /// <returns>True if the effect was found and removed; otherwise, false.</returns>
    public bool Remove(uint id)
    {
        foreach (var container in slowItemContainers.Values)
        {
            if (container.RemoveID(id)) return true;
        }

        return false;
    }
    /// <summary>
    /// Checks if there are any slow motion effects with the specified tag.
    /// </summary>
    /// <param name="tag">The tag to check.</param>
    /// <returns>True if the tag exists; otherwise, false.</returns>
    public bool HasTag(uint tag) => slowItemContainers.ContainsKey(tag);

    /// <summary>
    /// Gets the combined time scaling factor for a specific tag.
    /// </summary>
    /// <param name="tag">The tag to query.</param>
    /// <returns>The combined factor <c>(1 = normal speed, &lt;1 = slowed)</c>.</returns>
    public float GetFactor(uint tag)
    {
        if (!HasTag(tag)) return 1f;
        return slowItemContainers[tag].TotalFactor;
    }
    /// <summary>
    /// Gets the combined time scaling factor for multiple tags.
    /// </summary>
    /// <param name="tags">The tags to query.</param>
    /// <returns>The product of all tag factors <c>(1 = normal speed, &lt;1 = slowed)</c>.</returns>
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
