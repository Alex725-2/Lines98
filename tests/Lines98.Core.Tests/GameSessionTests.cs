using Lines98.Core.Board;
using Lines98.Core.Game;
using Lines98.Core.Randomness;
using Lines98.Web.Services;

namespace Lines98.Core.Tests;

public class GameSessionTests
{
    [Fact]
    public void Constructor_StartsPlayableGame()
    {
        var session = CreateSession();

        Assert.Equal(76, session.State.Board.EmptyCount);
        Assert.Equal(0, session.State.Score);
        Assert.Equal(3, session.State.NextColors.Count);
        Assert.Null(session.SelectedPosition);
        Assert.False(session.State.IsGameOver);
    }

    [Fact]
    public void SelectCell_SelectsAndReselectsBallWithoutChangingGameState()
    {
        var session = CreateSession();
        var original = session.State;

        session.SelectCell(new Position(0, 0));
        Assert.Equal(new Position(0, 0), session.SelectedPosition);
        session.SelectCell(new Position(0, 1));

        Assert.Equal(new Position(0, 1), session.SelectedPosition);
        Assert.Same(original, session.State);
    }

    [Fact]
    public void SelectCell_EmptyCellWithoutSelection_DoesNotMove()
    {
        var session = CreateSession();
        var original = session.State;

        session.SelectCell(new Position(8, 8));

        Assert.Same(original, session.State);
        Assert.Null(session.SelectedPosition);
        Assert.Contains("Сначала", session.Message);
    }

    [Fact]
    public void SelectCell_ValidMove_UpdatesStateAndClearsSelection()
    {
        var session = CreateSession();
        var original = session.State;
        session.SelectCell(new Position(0, 4));

        session.SelectCell(new Position(1, 4));

        Assert.NotSame(original, session.State);
        Assert.Equal(BallColor.Blue, session.State.Board.GetCell(new Position(1, 4)).Color);
        Assert.Null(session.SelectedPosition);
        // Constant randomness spawns three reds beside the four remaining reds.
        Assert.Equal(80, session.State.Board.EmptyCount);
        Assert.Equal(7, session.State.Score);
        Assert.Equal(76, original.Board.EmptyCount);
    }

    [Fact]
    public void SelectCell_NoPath_KeepsSelectionAndShowsReason()
    {
        // Put red at the corner, surrounded by the next two balls.
        var session = new GameSession(new GameEngine(new InitialRandom(0, 0, 0, 1, 7, 2, 10, 3, 20, 4, 0, 1, 2)));
        var original = session.State;
        session.SelectCell(new Position(0, 0));

        session.SelectCell(new Position(8, 8));

        Assert.Same(original, session.State);
        Assert.Equal(new Position(0, 0), session.SelectedPosition);
        Assert.Contains("Нет свободного пути", session.Message);
    }

    [Fact]
    public void NewGame_ResetsSelectionAndMessage()
    {
        var session = CreateSession();
        var original = session.State;
        session.SelectCell(new Position(0, 0));

        session.NewGame();

        Assert.NotSame(original, session.State);
        Assert.Null(session.SelectedPosition);
        Assert.Equal(0, session.State.Score);
        Assert.Equal(76, session.State.Board.EmptyCount);
        Assert.Contains("Новая игра", session.Message);
    }

    [Fact]
    public void SelectCell_OutOfBounds_DoesNotChangeState()
    {
        var session = CreateSession();
        var original = session.State;

        session.SelectCell(new Position(-1, 0));

        Assert.Same(original, session.State);
        Assert.Contains("за границами", session.Message);
    }

    private static GameSession CreateSession() => new(new GameEngine(new InitialRandom()));

    [Fact]
    public void ReachablePositions_MatchPathFinderAndResetAfterMoveOrRestart()
    {
        var session = CreateSession();
        var start = new Position(0, 4);
        session.SelectCell(start);
        Assert.NotEmpty(session.ReachablePositions);
        foreach (var position in session.State.Board.EmptyPositions)
        {
            Assert.Equal(PathFinder.FindPath(session.State.Board, start, position) is not null,
                session.ReachablePositions.Contains(position));
        }
        session.SelectCell(new Position(1, 4));
        Assert.Empty(session.ReachablePositions);
        session.SelectCell(new Position(1, 4));
        Assert.NotEmpty(session.ReachablePositions);
        session.NewGame();
        Assert.Empty(session.ReachablePositions);
    }

    [Fact]
    public void ReachablePositions_ReselectUpdatesAndRejectedMoveKeepsHighlights()
    {
        var session = new GameSession(new GameEngine(new InitialRandom(0, 0, 0, 1, 7, 2, 10, 3, 20, 4, 0, 1, 2)));
        session.SelectCell(new Position(0, 0));
        Assert.Empty(session.ReachablePositions);
        session.SelectCell(new Position(8, 8));
        Assert.Empty(session.ReachablePositions);
        session.SelectCell(new Position(0, 1));
        Assert.Contains(new Position(8, 8), session.ReachablePositions);
    }

    private sealed class InitialRandom(params int[] values) : IRandomSource
    {
        private int index;
        public int Next(int exclusiveMax) => index < values.Length ? values[index++] : 0;
    }
}
