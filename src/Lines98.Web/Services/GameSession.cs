using Lines98.Core.Board;
using Lines98.Core.Game;

namespace Lines98.Web.Services;

public sealed class GameSession
{
    private readonly GameEngine engine;

    public GameSession(GameEngine engine)
    {
        ArgumentNullException.ThrowIfNull(engine);
        this.engine = engine;
        State = engine.NewGame();
    }

    public GameState State { get; private set; }
    public Position? SelectedPosition { get; private set; }
    public IReadOnlySet<Position> ReachablePositions { get; private set; } = new HashSet<Position>();
    public string Message { get; private set; } = "Выберите шарик, затем свободную клетку.";

    public void NewGame()
    {
        State = engine.NewGame();
        SelectedPosition = null;
        ReachablePositions = new HashSet<Position>();
        Message = "Новая игра. Выберите шарик.";
    }

    public void SelectCell(Position position)
    {
        if (State.IsGameOver)
        {
            Message = "Игра окончена. Начните новую партию.";
            return;
        }

        if (!position.IsWithinBoard)
        {
            Message = FailureMessage(MoveFailureReason.InvalidEnd);
            return;
        }

        if (!State.Board.GetCell(position).IsEmpty)
        {
            SelectedPosition = position;
            ReachablePositions = State.Board.EmptyPositions
                .Where(end => PathFinder.FindPath(State.Board, position, end) is not null).ToHashSet();
            Message = "Выберите свободную клетку для перемещения.";
            return;
        }

        if (SelectedPosition is not { } start)
        {
            Message = "Сначала выберите шарик.";
            return;
        }

        var previousScore = State.Score;
        var result = engine.TryMove(State, start, position);
        State = result.State;
        if (!result.IsSuccess)
        {
            Message = FailureMessage(result.FailureReason);
            return;
        }

        SelectedPosition = null;
        ReachablePositions = new HashSet<Position>();
        Message = State.IsGameOver ? "Игра окончена. Начните новую партию."
            : State.Score > previousScore ? $"Линия удалена! +{State.Score - previousScore} очков."
            : "Ход выполнен. Выберите следующий шарик.";
    }

    private static string FailureMessage(MoveFailureReason reason) => reason switch
    {
        MoveFailureReason.InvalidStart or MoveFailureReason.InvalidEnd => "Клетка находится за границами поля.",
        MoveFailureReason.SamePosition => "Выберите другую клетку.",
        MoveFailureReason.EmptyStart => "В начальной клетке нет шарика.",
        MoveFailureReason.OccupiedEnd => "Конечная клетка занята.",
        MoveFailureReason.NoPath => "Нет свободного пути. Выберите другую клетку или шарик.",
        MoveFailureReason.GameOver => "Игра окончена. Начните новую партию.",
        _ => "Не удалось выполнить ход."
    };
}
