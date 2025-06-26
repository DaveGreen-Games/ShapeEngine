using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Pathfinding;

/// <summary>
/// Implements the A* pathfinding algorithm for nodes in a graph.
/// </summary>
internal static class AStar
{
    private static readonly NodeQueue openSet = new(1024);
    private static readonly HashSet<Node> openSetCells = new(1024);
    private static readonly HashSet<Node> closedSet = new(1024);
    private static readonly Dictionary<Node, Node> cellPath = new(1024);
    
    
    /// <summary>
    /// Finds a path between two nodes using the A* algorithm for a given layer.
    /// </summary>
    /// <param name="startNode">The starting node.</param>
    /// <param name="endNode">The ending node.</param>
    /// <param name="layer">The layer to consider for traversability and weights.</param>
    /// <returns>A <see cref="Path"/> if a path is found; otherwise, null.</returns>
    public static Path? GetPath(Node startNode, Node endNode, uint layer)
    {
        cellPath.Clear();
        closedSet.Clear();
        openSetCells.Clear();
        openSet.Clear();

        startNode.GScore = 0;
        startNode.H = startNode.DistanceToTarget(endNode);
        startNode.FScore = startNode.H;
        // openSet.Enqueue(startCell, startCell.FScore);
        openSet.Enqueue(startNode);
        openSetCells.Add(startNode);


        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            if(current == null) continue;
            
            if (current == endNode)
            {
                var countEstimate = startNode.EstimateCellCount(endNode);
                var pathPoints = ReconstructPath(current, countEstimate);
    
                return new Path(startNode.GetPosition(), endNode.GetPosition(), pathPoints);
                
            }

            openSetCells.Remove(current);
            closedSet.Add(current);
            
            if (current.Neighbors != null)
            {
                foreach (var neighbor in current.Neighbors)
                {
                    if(closedSet.Contains(neighbor) || !neighbor.IsTraversable(layer)) continue;

                    float tentativeGScore = current.GScore + current.WeightedDistanceToNeighbor(neighbor, layer);

                    if (openSetCells.Contains(neighbor))
                    {
                        if (tentativeGScore < neighbor.GScore)
                        {
                            neighbor.GScore = tentativeGScore;
                            neighbor.FScore = neighbor.GScore + neighbor.H;
                            cellPath[neighbor] = current;
                        }
                    }
                    else
                    {
                        neighbor.GScore = tentativeGScore;
                        neighbor.H = neighbor.DistanceToTarget(endNode);
                        neighbor.FScore = neighbor.GScore + neighbor.H;
                        // openSet.Enqueue(neighbor, neighbor.FScore);
                        openSet.Enqueue(neighbor);
                        openSetCells.Add(neighbor);
                        cellPath[neighbor] = current;
                    }
                }
            }
            
            if (current.Connections != null)
            {
                foreach (var connection in current.Connections)
                {
                    if(closedSet.Contains(connection) || !connection.IsTraversable(layer)) continue;

                    // connection.DEBUG_Touched = true;
                    // DEBUG_TOUCHED_COUNT++;
                    
                    float tentativeGScore = current.GScore + current.WeightedDistanceToNeighbor(connection, layer);

                    if (openSetCells.Contains(connection))
                    {
                        if (tentativeGScore < connection.GScore)
                        {
                            connection.GScore = tentativeGScore;
                            connection.FScore = connection.GScore + connection.H;
                            cellPath[connection] = current;
                        }
                    }
                    else
                    {
                        connection.GScore = tentativeGScore;
                        connection.H = connection.DistanceToTarget(endNode);
                        connection.FScore = connection.GScore + connection.H;
                        // openSet.Enqueue(neighbor, neighbor.FScore);
                        openSet.Enqueue(connection);
                        openSetCells.Add(connection);
                        cellPath[connection] = current;
                    }
                }
            }

        }
        
        return null;
    }
    /// <summary>
    /// Reconstructs the path from the end node to the start node.
    /// </summary>
    /// <param name="from">The end node.</param>
    /// <param name="capacityEstimate">Estimated capacity for the path list.</param>
    /// <returns>A list of rectangles representing the path.</returns>
    private static List<Rect> ReconstructPath(Node from, int capacityEstimate)
    {
        
        List<Rect> nodes = new(capacityEstimate) { from.GetRect() };

        var current = from;

        do
        {
            if (cellPath.ContainsKey(current))
            {
                current = cellPath[current];
                nodes.Add(current.GetRect());
            }
            else current = null;

        } while (current != null);

        nodes.Reverse();
        return nodes;
    }
  
}