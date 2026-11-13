namespace Lines98.Core.Board;

public static class LineFinder
{
    public static IReadOnlyList<Position> FindLines(Board board)
    {
        ArgumentNullException.ThrowIfNull(board);
        var matches = new HashSet<Position>();
        (int Row, int Column)[] directions = [(0, 1), (1, 0), (1, 1), (1, -1)];

        for (var row = 0; row < Board.Size; row++)
        {
            for (var column = 0; column < Board.Size; column++)
            {
                var start = new Position(row, column);
                var color = board.GetCell(start).Color;
                if (color is null)
                {
                    continue;
                }

                foreach (var direction in directions)
                {
                    var previous = new Position(row - direction.Row, column - direction.Column);
                    if (previous.IsWithinBoard && board.GetCell(previous).Color == color)
                    {
                        continue;
                    }

                    var line = new List<Position>();
                    var current = start;
                    while (current.IsWithinBoard && board.GetCell(current).Color == color)
                    {
                        line.Add(current);
                        current = new Position(current.Row + direction.Row, current.Column + direction.Column);
                    }

                    if (line.Count >= 5)
                    {
                        matches.UnionWith(line);
                    }
                }
            }
        }

        return matches.OrderBy(position => position.Row).ThenBy(position => position.Column)
            .ToList().AsReadOnly();
    }
}
