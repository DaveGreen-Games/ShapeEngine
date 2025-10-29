using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Pathfinding;

/// <summary>
/// Implements the A* pathfinding algorithm for nodes in a graph.
/// </summary>
internal class AStar
{
    private readonly NodeQueue openSet;
    private readonly HashSet<Node> openSetCells;
    private readonly HashSet<Node> closedSet;
    private readonly Dictionary<Node, Node> cellPath;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AStar"/> class.
    /// </summary>
    /// <param name="capacity">Estimated number of nodes the algorithm will handle.
    /// Used to size internal collections to reduce allocations and improve performance.</param>
    public AStar(int capacity)
    {
        openSet = new NodeQueue(capacity);
        openSetCells = new HashSet<Node>(capacity);
        closedSet = new HashSet<Node>(capacity);
        cellPath = new Dictionary<Node, Node>(capacity);
    }
    
    /// <summary>
    /// Finds a path between two nodes using the A* algorithm for a given layer.
    /// </summary>
    /// <param name="startNode">The starting node.</param>
    /// <param name="endNode">The ending node.</param>
    /// <param name="layer">The layer to consider for traversability and weights.</param>
    /// <returns>A <see cref="Path"/> if a path is found; otherwise, null.</returns>
    public  Path? GetPath(Node startNode, Node endNode, uint layer)
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
    private List<Rect> ReconstructPath(Node from, int capacityEstimate)
    {
        //TODO: Optimize allocations here (DPA issue)
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
    /// <summary>
    /// Finds a path between two nodes using the A* algorithm for a given layer asynchronously.
    /// </summary>
    /// <param name="startNode">The starting node.</param>
    /// <param name="endNode">The ending node.</param>
    /// <param name="layer">The layer to consider for traversability and weights.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="Path"/> if a path is found; otherwise, null.</returns>
    public async Task<Path?> GetPathAsync(Node startNode, Node endNode, uint layer, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            cellPath.Clear();
            closedSet.Clear();
            openSetCells.Clear();
            openSet.Clear();

            startNode.GScore = 0;
            startNode.H = startNode.DistanceToTarget(endNode);
            startNode.FScore = startNode.H;
            openSet.Enqueue(startNode);
            openSetCells.Add(startNode);

            while (openSet.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

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
                            openSet.Enqueue(connection);
                            openSetCells.Add(connection);
                            cellPath[connection] = current;
                        }
                    }
                }
            }

            return null;
        }, cancellationToken);
    }
    
}