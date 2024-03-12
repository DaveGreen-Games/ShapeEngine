namespace ShapeEngine.Pathfinding;

internal class NodeQueue
{
    private readonly List<Node> nodes;

    public NodeQueue(int capacity)
    {
        nodes = new(capacity);
    }
    public int Count => nodes.Count;
    public void Clear() => nodes.Clear();
    public void Enqueue(Node node)
    {
        nodes.Add(node);
    }

    public Node? Dequeue()
    {
        if (Count <= 0) return null;
        if (Count == 1)
        {
            var cell = nodes[0];
            nodes.RemoveAt(0);
            return cell;
        }
        var minIndex = 0;
        var current = nodes[0];
        for (var i = 1; i < nodes.Count; i++)
        {
            var cell = nodes[i];
            // if (cell.FScore < current.FScore || (Math.Abs(cell.FScore - current.FScore) < 0.0001f && cell.H < current.H))
            if(cell.CompareTo(current) < 0)
            {
                minIndex = i;
                current = cell;
            }
        }
            
        nodes.RemoveAt(minIndex);
            
        return current;
    }

}