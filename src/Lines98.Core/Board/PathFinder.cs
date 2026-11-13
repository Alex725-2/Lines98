namespace Lines98.Core.Board;

public static class PathFinder
{
    /// <summary>
    /// Finds a shortest orthogonal path, including both endpoints.
    /// Returns null when no path exists or the start is empty or the end is occupied.
    /// </summary>
    public static IReadOnlyList<Position>? FindPath(Board board, Position start, Position end)
    {
        ArgumentNullException.ThrowIfNull(board);
        if (!start.IsWithinBoard)
        {
            throw new ArgumentOutOfRangeException(nameof(start), start, "Position must be within the board.");
        }

        if (!end.IsWithinBoard)
        {
            throw new ArgumentOutOfRangeException(nameof(end), end, "Position must be within the board.");
        }

        if (board.GetCell(start).IsEmpty || !board.GetCell(end).IsEmpty)
        {
            return null;
        }

        var queue = new Queue<Position>();
        var predecessors = new Dictionary<Position, Position>();
        var visited = new HashSet<Position> { start };
        queue.Enqueue(start);

        while (queue.TryDequeue(out var current))
        {
            if (current == end)
            {
                var path = new List<Position> { end };
                while (current != start)
                {
                    current = predecessors[current];
                    path.Add(current);
                }

                path.Reverse();
                return path.AsReadOnly();
            }

            Position[] neighbors =
            [
                new(current.Row - 1, current.Column),
                new(current.Row + 1, current.Column),
                new(current.Row, current.Column - 1),
                new(current.Row, current.Column + 1)
            ];

            foreach (var neighbor in neighbors)
            {
                if (neighbor.IsWithinBoard && board.GetCell(neighbor).IsEmpty && visited.Add(neighbor))
                {
                    predecessors.Add(neighbor, current);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return null;
    }
}
