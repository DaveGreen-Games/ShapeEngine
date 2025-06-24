namespace ShapeEngine.Pathfinding;

/// <summary>
/// Represents a priority queue for nodes, used in pathfinding algorithms.
/// </summary>
internal class NodeQueue
{
    private readonly List<Node> nodes;

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeQueue"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the queue.</param>
    public NodeQueue(int capacity)
    {
        nodes = new(capacity);
    }
    /// <summary>
    /// Gets the number of nodes in the queue.
    /// </summary>
    public int Count => nodes.Count;
    /// <summary>
    /// Removes all nodes from the queue.
    /// </summary>
    public void Clear() => nodes.Clear();
    /// <summary>
    /// Adds a node to the queue.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void Enqueue(Node node)
    {
        nodes.Add(node);
    }
    /// <summary>
    /// Removes and returns the node with the highest priority (lowest FScore and H).
    /// </summary>
    /// <returns>The node with the highest priority, or null if the queue is empty.</returns>
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