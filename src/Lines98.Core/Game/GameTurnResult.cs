namespace Lines98.Core.Game;

/// <summary>The outcome of a complete turn, including spawning and scoring.</summary>
public sealed class GameTurnResult
{
    internal GameTurnResult(GameState state, MoveFailureReason failureReason)
    {
        State = state;
        FailureReason = failureReason;
    }

    /// <summary>The resulting state, or the original instance when rejected.</summary>
    public GameState State { get; }

    public bool IsSuccess => FailureReason == MoveFailureReason.None;

    public MoveFailureReason FailureReason { get; }
}
