namespace ShapeEngine.Pathfinding;

internal abstract partial class Node
{
    private class NodeWeight
    {
        private int blockCount;
        private float baseValue;
        private float flat;
        private float bonus;
        public bool Blocked => blockCount > 0;

        public void Apply(NodeCost cost)
        {
            if (!cost.Valid) return;
            switch (cost.Type)
            {
                case NodeCostType.Reset:
                    Reset();
                    break;

                case NodeCostType.SetBaseValue:
                    baseValue = cost.Value;
                    break;
                case NodeCostType.ResetThenSetBaseValue:
                    Reset();
                    baseValue = cost.Value;
                    break;
                case NodeCostType.Block:
                    blockCount++;
                    break;
                case NodeCostType.Unblock:
                    blockCount--;
                    if (blockCount < 0) blockCount = 0;
                    break;
                case NodeCostType.ResetThenBlock:
                    Reset();
                    blockCount++;
                    break;
                case NodeCostType.AddFlat:
                    flat += cost.Value;
                    break;
                case NodeCostType.RemoveFlat:
                    flat -= cost.Value;
                    break;
                case NodeCostType.ResetFlat:
                    flat = 0;
                    break;
                case NodeCostType.AddBonus:
                    bonus += cost.Value;
                    break;
                case NodeCostType.RemoveBonus:
                    bonus -= cost.Value;
                    break;
                case NodeCostType.ResetBonus:
                    bonus = 0;
                    break;
            }
        }

        public float Cur => blockCount > 0 ? 0 : CalculateCostMultiplier();

        private float GetBaseValueFactor()
        {
            var v = baseValue + flat;
            if (v > 0) return v; //less favorable
            if (v < 0) return 1f / (1f - v); //more favorable
            return 1f; //normal
        }

        private float GetBonusFactor()
        {
            if (bonus > 0) return bonus; //more favorable
            if (bonus < 0) return 1f / (1f - bonus); //less favorable
            return 1f; //normal
        }

        private float CalculateCostMultiplier()
        {
            return GetBaseValueFactor() * GetBonusFactor();
        }

        //Alternative Calculation Methods
        // private float CalculateCostMultiplier()
        // {
        //     const float minThreshold = 0.0001f;
        //     // Step 1: Start with base multiplier (1.0 = normal cost)
        //     // baseValue shifts the starting point (e.g., terrain type differences)
        //     float multiplier = 1f + baseValue;
        //
        //     // Step 2: Apply flat adjustment (additive modification)
        //     // This is useful for fixed cost changes like "add 2 cost for climbing stairs"
        //     // Multiple entities can stack their flat bonuses/penalties independently
        //     multiplier += flat;
        //     
        //     if (bonus == 0f)//no bonus to apply
        //     {
        //         return multiplier > 0f ? MathF.Max(minThreshold, multiplier) : MathF.Max(minThreshold, 1f / (1f - multiplier));
        //     }
        //     
        //     
        //     if (multiplier >= 0f)
        //     {
        //         // Step 3: Apply bonus as percentage modifier (multiplicative modification)
        //         // This is useful for percentage-based effects like "50% faster movement"
        //         
        //         if (bonus > 0)
        //         {
        //             // Positive bonus = less favorable = increase cost
        //             // +1 bonus = 2x cost (twice as slow)
        //             // +2 bonus = 3x cost (three times as slow)
        //             // Formula: current_multiplier * (1 + bonus)
        //             multiplier *= (1f + bonus);
        //         }
        //         else
        //         {
        //             // Negative bonus = more favorable = reduce cost
        //             // -1 bonus = 0.5x cost (twice as fast)
        //             // -2 bonus = 0.33x cost (three times as fast)
        //             // Formula: current_multiplier / (1 - bonus)
        //             // Since bonus is negative, (1 - bonus) becomes (1 + |bonus|)
        //             multiplier /= (1f - bonus);
        //         }
        //         return Math.Max(minThreshold, multiplier); 
        //     }
        //     
        //     
        //     //when multiplier is negative before bonus application, bonus logic is inverted
        //     if (bonus > 0)
        //     {
        //             
        //         multiplier /= (1f + bonus);
        //     }
        //     else
        //     {
        //             
        //         multiplier *= (1f - bonus);
        //     }
        //     
        //     // multiplier is still negative here
        //     // Return the inverse to represent "better than free" traversal
        //     return MathF.Max(minThreshold, 1f / (1f - multiplier));
        // }
        //

        // private float CalculateCostMultiplier()
        // {
        //     // Step 1: Start with base multiplier (1.0 = normal cost)
        //     // baseValue shifts the starting point (e.g., terrain type differences)
        //     float multiplier = 1f + baseValue;
        //
        //     // Step 2: Apply flat adjustment (additive modification)
        //     // This is useful for fixed cost changes like "add 2 cost for climbing stairs"
        //     // Multiple entities can stack their flat bonuses/penalties independently
        //     multiplier += flat;
        //
        //     // Step 3: Apply bonus as percentage modifier (multiplicative modification)
        //     // This is useful for percentage-based effects like "50% faster movement"
        //     if (bonus != 0)
        //     {
        //         if (bonus > 0)
        //         {
        //             // Positive bonus = less favorable = increase cost
        //             // +1 bonus = 2x cost (twice as slow)
        //             // +2 bonus = 3x cost (three times as slow)
        //             // Formula: current_multiplier * (1 + bonus)
        //             multiplier *= (1f + bonus);
        //         }
        //         else
        //         {
        //             // Negative bonus = more favorable = reduce cost
        //             // -1 bonus = 0.5x cost (twice as fast)
        //             // -2 bonus = 0.33x cost (three times as fast)
        //             // Formula: current_multiplier / (1 - bonus)
        //             // Since bonus is negative, (1 - bonus) becomes (1 + |bonus|)
        //             multiplier /= (1f - bonus);
        //         }
        //     }
        //
        //     // Step 4: Clamp to minimum threshold to avoid division by zero or negative costs
        //     // Ensures the node always has some traversal cost
        //     return Math.Max(0.001f, multiplier);
        // }

        public void Reset()
        {
            baseValue = 0;
            flat = 0;
            bonus = 0;
            blockCount = 0;
        }
    }
}