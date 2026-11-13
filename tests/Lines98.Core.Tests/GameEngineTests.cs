using Lines98.Core.Board;
using Lines98.Core.Game;
using Lines98.Core.Randomness;
using Lines98.Core.Scoring;
using GameBoard = Lines98.Core.Board.Board;

namespace Lines98.Core.Tests;

public class GameEngineTests
{
    private static readonly BallColor[] Preview = [BallColor.Red, BallColor.Blue, BallColor.Green];

    [Fact]
    public void NewGame_HasFiveBallsNoLinesZeroScoreAndThreeNextColors()
    {
        var random = new ScriptedRandom();

        var state = new GameEngine(random).NewGame();

        Assert.Equal(76, state.Board.EmptyCount);
        Assert.Equal(0, state.Score);
        Assert.False(state.IsGameOver);
        Assert.Empty(LineFinder.FindLines(state.Board));
        Assert.Equal(3, state.NextColors.Count);
        Assert.All(state.NextColors, color => Assert.True(Enum.IsDefined(color)));
        Assert.Equal(14, random.Calls);
        // A constant source would otherwise create five red balls in the first row.
        Assert.Equal(BallColor.Blue, state.Board.GetCell(new Position(0, 4)).Color);
    }

    [Fact]
    public void NewGame_CanGenerateAllSevenColors()
    {
        var random = new ScriptedRandom(0, 0, 0, 1, 0, 2, 0, 3, 0, 4, 5, 6, 0);

        var state = new GameEngine(random).NewGame();

        Assert.Equal(new[] { BallColor.Cyan, BallColor.Orange, BallColor.Red }, state.NextColors);
        for (var column = 0; column < 5; column++)
        {
            Assert.Equal((BallColor)column, state.Board.GetCell(new Position(0, column)).Color);
        }
    }

    [Fact]
    public void Move_WithLine_ScoresWithoutSpawningOrChangingPreview()
    {
        var board = new GameBoard();
        for (var column = 0; column < 4; column++)
        {
            board = board.WithCell(new Position(0, column), Cell.WithBall(BallColor.Red));
        }

        var start = new Position(1, 4);
        board = board.WithCell(start, Cell.WithBall(BallColor.Red));
        var state = new GameState(board, 10, Preview);
        var random = new ScriptedRandom();

        var result = new GameEngine(random).Move(state, start, new Position(0, 4));

        Assert.Equal(15, result.Score);
        Assert.Equal(81, result.Board.EmptyCount);
        Assert.Equal(state.NextColors, result.NextColors);
        Assert.Equal(0, random.Calls);
        Assert.Equal(76, state.Board.EmptyCount);
        Assert.Equal(10, state.Score);
    }

    [Fact]
    public void Move_WithoutLine_SpawnsPreviewThenGeneratesNewPreview()
    {
        var start = new Position(8, 8);
        var state = new GameState(new GameBoard().WithCell(start, Cell.WithBall(BallColor.Orange)), 7, Preview);
        var random = new ScriptedRandom(0, 0, 0, 4, 5, 6);

        var result = new GameEngine(random).Move(state, start, new Position(8, 7));

        Assert.Equal(77, result.Board.EmptyCount);
        for (var column = 0; column < 3; column++)
        {
            Assert.Equal(Preview[column], result.Board.GetCell(new Position(0, column)).Color);
        }

        Assert.Equal(new[] { BallColor.Purple, BallColor.Cyan, BallColor.Orange }, result.NextColors);
        Assert.Equal(7, result.Score);
        Assert.True(result.Board.GetCell(start).IsEmpty);
        Assert.Equal(BallColor.Orange, state.Board.GetCell(start).Color);
        Assert.Equal(80, state.Board.EmptyCount);
        Assert.Equal(Preview, state.NextColors);
    }

    [Fact]
    public void Move_SpawnCompletesIntersectingLines_RemovesUniqueBallsAndScores()
    {
        var board = new GameBoard();
        for (var index = 0; index < 4; index++)
        {
            board = board.WithCell(new Position(4, index), Cell.WithBall(BallColor.Red))
                .WithCell(new Position(index, 4), Cell.WithBall(BallColor.Red));
        }

        var start = new Position(8, 8);
        var end = new Position(8, 7);
        board = board.WithCell(start, Cell.WithBall(BallColor.Orange));
        var moved = board.WithCell(start, Cell.Empty).WithCell(end, board.GetCell(start));
        var targetIndex = moved.EmptyPositions.ToList().IndexOf(new Position(4, 4));
        var state = new GameState(board, 2, Preview);

        var result = new GameEngine(new ScriptedRandom(targetIndex, 0, 0, 0, 0, 0))
            .Move(state, start, end);

        Assert.Equal(11, result.Score);
        Assert.Equal(78, result.Board.EmptyCount);
        Assert.Empty(LineFinder.FindLines(result.Board));
        Assert.True(result.Board.GetCell(new Position(4, 4)).IsEmpty);
        Assert.Equal(72, state.Board.EmptyCount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Move_FewerThanThreeEmptyCells_FillsBoardAndEndsGame(int emptyCount)
    {
        var board = PatternBoard().WithCell(new Position(0, 0), Cell.Empty);
        if (emptyCount == 2)
        {
            board = board.WithCell(new Position(8, 8), Cell.Empty);
        }

        Assert.Empty(LineFinder.FindLines(board));
        var random = new ScriptedRandom();
        var state = new GameState(board, 4, Preview);

        var result = new GameEngine(random).Move(state, new Position(0, 1), new Position(0, 0));

        Assert.True(result.IsGameOver);
        Assert.Equal(0, result.Board.EmptyCount);
        Assert.Equal(4, result.Score);
        Assert.Equal(emptyCount + 3, random.Calls);
        Assert.False(state.IsGameOver);
    }

    [Fact]
    public void Move_FullAfterSpawningButLineRemoved_DoesNotEndGame()
    {
        var board = PatternBoard();
        for (var column = 0; column < 4; column++)
        {
            board = board.WithCell(new Position(0, column), Cell.WithBall(BallColor.Red));
        }

        board = board.WithCell(new Position(0, 4), Cell.Empty)
            .WithCell(new Position(8, 8), Cell.Empty);
        var state = new GameState(board, 3, Preview);
        Assert.Empty(LineFinder.FindLines(board));

        var result = new GameEngine(new ScriptedRandom()).Move(state, new Position(8, 7), new Position(8, 8));

        Assert.False(result.IsGameOver);
        Assert.Equal(5, result.Board.EmptyCount);
        Assert.Equal(8, result.Score);
        Assert.Empty(LineFinder.FindLines(result.Board));
        Assert.Equal(2, state.Board.EmptyCount);
    }

    [Fact]
    public void Move_RejectedOrGameOver_ReturnsSameStateWithoutRandomness()
    {
        var random = new ScriptedRandom();
        var engine = new GameEngine(random);
        var state = new GameState(new GameBoard(), 0, Preview);
        Assert.Same(state, engine.Move(state, new Position(0, 0), new Position(0, 1)));
        Assert.Same(state, engine.Move(state, new Position(-1, 0), new Position(0, 1)));
        var finished = new GameState(PatternBoard(), 9, Preview);
        Assert.Same(finished, engine.Move(finished, new Position(0, 0), new Position(0, 1)));
        Assert.Equal(0, random.Calls);
    }

    [Fact]
    public void FixedSeed_ReproducesNewGameAndTurn()
    {
        var firstEngine = new GameEngine(new SystemRandomSource(123));
        var secondEngine = new GameEngine(new SystemRandomSource(123));
        var first = firstEngine.NewGame();
        var second = secondEngine.NewGame();
        AssertStatesEqual(first, second);
        var start = Enumerable.Range(0, 81).Select(index => new Position(index / 9, index % 9))
            .First(position => !first.Board.GetCell(position).IsEmpty);
        var end = first.Board.EmptyPositions.First(position => PathFinder.FindPath(first.Board, start, position) is not null);

        AssertStatesEqual(firstEngine.Move(first, start, end), secondEngine.Move(second, start, end));
    }

    [Fact]
    public void CustomScoreCalculator_IsUsedAfterSpawning()
    {
        var board = new GameBoard();
        for (var column = 1; column < 5; column++)
        {
            board = board.WithCell(new Position(0, column), Cell.WithBall(BallColor.Red));
        }

        var start = new Position(8, 8);
        board = board.WithCell(start, Cell.WithBall(BallColor.Orange));
        var result = new GameEngine(new ScriptedRandom(), new DoubleScore())
            .Move(new GameState(board, 1, Preview), start, new Position(8, 7));

        Assert.Equal(11, result.Score);
    }

    [Theory]
    [InlineData(MoveFailureReason.InvalidStart)]
    [InlineData(MoveFailureReason.InvalidEnd)]
    [InlineData(MoveFailureReason.SamePosition)]
    [InlineData(MoveFailureReason.EmptyStart)]
    [InlineData(MoveFailureReason.OccupiedEnd)]
    [InlineData(MoveFailureReason.NoPath)]
    [InlineData(MoveFailureReason.GameOver)]
    public void TryMove_Rejected_PreservesStateAndRandomness(MoveFailureReason reason)
    {
        var start = new Position(0, 0);
        var end = new Position(1, 1);
        var board = new GameBoard().WithCell(start, Cell.WithBall(BallColor.Red));
        switch (reason)
        {
            case MoveFailureReason.InvalidStart: start = new Position(-1, 0); break;
            case MoveFailureReason.InvalidEnd: end = new Position(9, 0); break;
            case MoveFailureReason.SamePosition: end = start; break;
            case MoveFailureReason.EmptyStart: board = new GameBoard(); break;
            case MoveFailureReason.OccupiedEnd:
                board = board.WithCell(end, Cell.WithBall(BallColor.Blue)); break;
            case MoveFailureReason.NoPath:
                board = board.WithCell(new Position(0, 1), Cell.WithBall(BallColor.Blue))
                    .WithCell(new Position(1, 0), Cell.WithBall(BallColor.Blue)); break;
            case MoveFailureReason.GameOver: board = PatternBoard(); break;
        }

        var state = new GameState(board, 42, Preview);
        var random = new ScriptedRandom();
        var engine = new GameEngine(random);
        var result = engine.TryMove(state, start, end);

        Assert.False(result.IsSuccess);
        Assert.Equal(reason, result.FailureReason);
        Assert.Same(state, result.State);
        Assert.Same(board, result.State.Board);
        Assert.Equal(42, result.State.Score);
        Assert.Equal(Preview, result.State.NextColors);
        Assert.Same(state, engine.Move(state, start, end));
        Assert.Equal(0, random.Calls);
    }

    [Theory]
    [InlineData(false, int.MaxValue - 2)]
    [InlineData(true, int.MaxValue - 2)]
    [InlineData(false, int.MaxValue)]
    [InlineData(true, int.MaxValue)]
    public void TryMove_ScoreSaturatesAfterMoveOrSpawning(bool spawning, int score)
    {
        var board = new GameBoard();
        for (var column = 0; column < 4; column++)
        {
            board = board.WithCell(new Position(0, column), Cell.WithBall(BallColor.Red));
        }

        var start = spawning ? new Position(8, 8) : new Position(1, 4);
        var end = spawning ? new Position(8, 7) : new Position(0, 4);
        board = board.WithCell(start, Cell.WithBall(spawning ? BallColor.Orange : BallColor.Red));
        var state = new GameState(board, score, Preview);

        var result = new GameEngine(new ScriptedRandom()).TryMove(state, start, end);

        Assert.True(result.IsSuccess);
        Assert.Equal(MoveFailureReason.None, result.FailureReason);
        Assert.Equal(int.MaxValue, result.State.Score);
        Assert.Empty(LineFinder.FindLines(result.State.Board));
        Assert.Equal(score, state.Score);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void NewGame_ForbiddenColor_SelectsAnyOfSixAllowedColors(int choice)
    {
        var random = new ScriptedRandom(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, choice, 0, 0, 0);

        var state = new GameEngine(random).NewGame();

        Assert.Equal((BallColor)(choice + 1), state.Board.GetCell(new Position(0, 4)).Color);
        Assert.Equal(76, state.Board.EmptyCount);
        Assert.Empty(LineFinder.FindLines(state.Board));
        Assert.Equal(14, random.Calls);
    }

    [Fact]
    public void TryMove_SpawnsEntirePreviewBeforeRemovingLines()
    {
        var board = new GameBoard();
        for (var column = 1; column < 5; column++)
        {
            board = board.WithCell(new Position(0, column), Cell.WithBall(BallColor.Red));
        }

        var start = new Position(8, 8);
        board = board.WithCell(start, Cell.WithBall(BallColor.Orange));
        var state = new GameState(board, 0, Preview);

        var result = new GameEngine(new ScriptedRandom()).TryMove(state, start, new Position(8, 7));

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.State.Score);
        // The first spawn completes row 0; the next two must still see it occupied.
        Assert.Equal(BallColor.Blue, result.State.Board.GetCell(new Position(0, 5)).Color);
        Assert.Equal(BallColor.Green, result.State.Board.GetCell(new Position(0, 6)).Color);
        Assert.Equal(78, result.State.Board.EmptyCount);
    }

    [Fact]
    public void TryMove_ConsecutiveTurns_UseAndRefreshPreview()
    {
        var start = new Position(8, 8);
        var initial = new GameState(new GameBoard().WithCell(start, Cell.WithBall(BallColor.Orange)), 0, Preview);
        var random = new ScriptedRandom(0, 0, 0, 4, 5, 6, 0, 0, 0, 1, 2, 3);
        var engine = new GameEngine(random);

        var first = engine.TryMove(initial, start, new Position(8, 7));
        var second = engine.TryMove(first.State, new Position(8, 7), start);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(new[] { BallColor.Purple, BallColor.Cyan, BallColor.Orange }, first.State.NextColors);
        Assert.Equal(new[] { BallColor.Blue, BallColor.Green, BallColor.Yellow }, second.State.NextColors);
        for (var column = 0; column < 3; column++)
        {
            Assert.Equal(Preview[column], second.State.Board.GetCell(new Position(0, column)).Color);
            Assert.Equal(first.State.NextColors[column], second.State.Board.GetCell(new Position(0, column + 3)).Color);
        }

        Assert.Equal(74, second.State.Board.EmptyCount);
        Assert.Empty(LineFinder.FindLines(first.State.Board));
        Assert.Empty(LineFinder.FindLines(second.State.Board));
        Assert.Equal(Preview, initial.NextColors);
        Assert.Equal(12, random.Calls);
    }

    [Fact]
    public void TryMove_CompletesTwoDiagonals_RemovesIntersectionOnce()
    {
        var board = new GameBoard();
        for (var offset = 1; offset <= 4; offset++)
        {
            board = board.WithCell(new Position(4 - offset, 4 - offset), Cell.WithBall(BallColor.Red))
                .WithCell(new Position(4 - offset, 4 + offset), Cell.WithBall(BallColor.Red));
        }

        var start = new Position(5, 4);
        board = board.WithCell(start, Cell.WithBall(BallColor.Red));
        var state = new GameState(board, 3, Preview);
        var random = new ScriptedRandom();

        var result = new GameEngine(random).TryMove(state, start, new Position(4, 4));

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.State.Score);
        Assert.Equal(81, result.State.Board.EmptyCount);
        Assert.Equal(72, state.Board.EmptyCount);
        Assert.Equal(0, random.Calls);
    }

    private sealed class DoubleScore : IScoreCalculator
    {
        public int CalculateScore(int removedBallCount) => removedBallCount * 2;
    }

    private static GameBoard PatternBoard()
    {
        var board = new GameBoard();
        for (var row = 0; row < 9; row++)
        {
            for (var column = 0; column < 9; column++)
            {
                board = board.WithCell(new Position(row, column), Cell.WithBall((BallColor)((row + 2 * column) % 7)));
            }
        }

        return board;
    }

    private static void AssertStatesEqual(GameState first, GameState second)
    {
        Assert.Equal(first.Score, second.Score);
        Assert.Equal(first.NextColors, second.NextColors);
        Assert.Equal(first.IsGameOver, second.IsGameOver);
        for (var row = 0; row < 9; row++)
        {
            for (var column = 0; column < 9; column++)
            {
                Assert.Equal(first.Board.GetCell(new Position(row, column)), second.Board.GetCell(new Position(row, column)));
            }
        }
    }

    private sealed class ScriptedRandom(params int[] values) : IRandomSource
    {
        public int Calls { get; private set; }

        public int Next(int exclusiveMax)
        {
            var value = Calls < values.Length ? values[Calls] : 0;
            Calls++;
            Assert.InRange(value, 0, exclusiveMax - 1);
            return value;
        }
    }
}
