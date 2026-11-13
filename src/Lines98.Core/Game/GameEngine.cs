using Lines98.Core.Board;
using Lines98.Core.Randomness;
using Lines98.Core.Scoring;
using GameBoard = Lines98.Core.Board.Board;

namespace Lines98.Core.Game;

public sealed class GameEngine
{
    private readonly IRandomSource random;
    private readonly IScoreCalculator scoreCalculator;
    private readonly MoveService moveService;
    private static readonly BallColor[] Colors = Enum.GetValues<BallColor>();

    public GameEngine() : this(new SystemRandomSource())
    {
    }

    public GameEngine(IRandomSource random) : this(random, new ScorePerBallCalculator())
    {
    }

    public GameEngine(IRandomSource random, IScoreCalculator scoreCalculator)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(scoreCalculator);
        this.random = random;
        this.scoreCalculator = scoreCalculator;
        moveService = new MoveService(scoreCalculator);
    }

    public GameState NewGame()
    {
        var board = new GameBoard();
        for (var index = 0; index < 5; index++)
        {
            var position = board.EmptyPositions[random.Next(board.EmptyCount)];
            var colorIndex = random.Next(Colors.Length);
            var color = Colors[colorIndex];
            var candidate = board.WithCell(position, Cell.WithBall(color));
            if (LineFinder.FindLines(candidate).Count > 0)
            {
                var allowedColors = Colors.Where(value => LineFinder.FindLines(
                    board.WithCell(position, Cell.WithBall(value))).Count == 0).ToArray();
                color = allowedColors[random.Next(allowedColors.Length)];
                candidate = board.WithCell(position, Cell.WithBall(color));
            }

            board = candidate;
        }

        return new GameState(board, 0, GenerateColors());
    }

    /// <summary>
    /// Returns the original state for a rejected move or a finished game.
    /// </summary>
    public GameState Move(GameState state, Position start, Position end) => TryMove(state, start, end).State;

    /// <summary>
    /// Executes a complete turn. Rejection preserves the original state and consumes no randomness.
    /// The total score saturates at int.MaxValue.
    /// </summary>
    public GameTurnResult TryMove(GameState state, Position start, Position end)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.IsGameOver)
        {
            return new GameTurnResult(state, MoveFailureReason.GameOver);
        }

        var move = moveService.Move(state.Board, start, end);
        if (!move.IsSuccess)
        {
            return new GameTurnResult(state, move.FailureReason);
        }

        if (move.RemovedPositions.Count > 0)
        {
            return new GameTurnResult(new GameState(move.Board, AddScore(state.Score, move.ScoreEarned),
                state.NextColors), MoveFailureReason.None);
        }

        var board = move.Board;
        foreach (var color in state.NextColors)
        {
            if (board.IsFull)
            {
                break;
            }

            var position = board.EmptyPositions[random.Next(board.EmptyCount)];
            board = board.WithCell(position, Cell.WithBall(color));
        }

        var removed = LineFinder.FindLines(board);
        foreach (var position in removed)
        {
            board = board.WithCell(position, Cell.Empty);
        }

        var score = AddScore(state.Score, scoreCalculator.CalculateScore(removed.Count));
        return new GameTurnResult(new GameState(board, score, GenerateColors()), MoveFailureReason.None);
    }

    private static int AddScore(int score, int earned) => (int)Math.Min(int.MaxValue, (long)score + earned);

    private BallColor[] GenerateColors() =>
        Enumerable.Range(0, 3).Select(_ => Colors[random.Next(Colors.Length)]).ToArray();
}
