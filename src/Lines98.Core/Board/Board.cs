namespace Lines98.Core.Board;

public sealed class Board
{
    public const int Size = 9;

    private readonly Cell[,] cells;

    public Board() : this(new Cell[Size, Size])
    {
    }

    private Board(Cell[,] cells)
    {
        this.cells = cells;

        var emptyPositions = new List<Position>();
        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                if (cells[row, column].IsEmpty)
                {
                    emptyPositions.Add(new Position(row, column));
                }
            }
        }

        EmptyPositions = emptyPositions.AsReadOnly();
    }

    public IReadOnlyList<Position> EmptyPositions { get; }

    public int EmptyCount => EmptyPositions.Count;

    public bool IsFull => EmptyCount == 0;

    public Cell GetCell(Position position)
    {
        ValidatePosition(position);
        return cells[position.Row, position.Column];
    }

    public Board WithCell(Position position, Cell cell)
    {
        ValidatePosition(position);
        var updatedCells = (Cell[,])cells.Clone();
        updatedCells[position.Row, position.Column] = cell;
        return new Board(updatedCells);
    }

    private static void ValidatePosition(Position position)
    {
        if (!position.IsWithinBoard)
        {
            throw new ArgumentOutOfRangeException(nameof(position), position, "Position must be within the board.");
        }
    }
}
