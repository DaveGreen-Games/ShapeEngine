

namespace ShapeEase
{
    public class EaseChain
    {
        List<EaseOrder> chain = new();
        bool finished = false;
        dynamic start;
        dynamic cur;
        public EaseChain(dynamic start, EaseOrder change, params EaseOrder[] changes)
        {
            if (changes.Length > 0)
            {
                Array.Reverse(changes);
                chain = changes.ToList();
            }
            chain.Add(change);
            this.start = start;
            cur = start;
        }
        public bool IsFinished() { return finished; }
        public dynamic GetValue() { return cur; }
        public void Update(float dt)
        {
            if (finished) return;
            int last = chain.Count - 1;
            var order = chain[last];
            order.Update(dt);
            cur = chain[chain.Count - 1].GetValue(start);
            if (order.IsFinished())
            {
                if (chain.Count <= 1)
                {
                    finished = true;
                }
                else
                {
                    chain.RemoveAt(last);
                    start = cur;
                }
            }

        }
    }

}
