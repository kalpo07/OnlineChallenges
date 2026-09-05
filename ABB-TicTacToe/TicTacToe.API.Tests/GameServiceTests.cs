using TicTacToe.API.Models;
using TicTacToe.API.Services;

namespace TicTacToe.API.Tests;

public class GameServiceTests
{
    private static GameService CreateService() => new(new ComputerPlayerService());

    // A valid move should place the player's mark on the correct cell
    [Fact]
    public void Should_PlaceMarkOnBoard_When_ValidMoveIsSubmitted()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        var result = service.MakeMove(game.Id, 0, Player.X);

        Assert.Equal("X", result.Board[0]);
    }

    // Playing on a cell that is already occupied should be rejected
    [Fact]
    public void Should_ThrowException_When_MoveOnOccupiedCell()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(game.Id, 0, Player.X);

        Assert.Throws<InvalidOperationException>(() => service.MakeMove(game.Id, 0, Player.O));
    }

    // After a valid move, turn should pass to the other player
    [Fact]
    public void Should_SwitchTurn_After_ValidMove()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        var result = service.MakeMove(game.Id, 0, Player.X);

        Assert.Equal(Player.O, result.CurrentTurn);
    }

    // Three marks in a row should be recognized as a win
    [Fact]
    public void Should_DetectWin_When_RowIsCompleted()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, 0, Player.X);
        service.MakeMove(game.Id, 3, Player.O);
        service.MakeMove(game.Id, 1, Player.X);
        service.MakeMove(game.Id, 4, Player.O);
        var result = service.MakeMove(game.Id, 2, Player.X);

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new[] { 0, 1, 2 }, result.WinningCells);
    }

    // Three marks in a column should be recognized as a win
    [Fact]
    public void Should_DetectWin_When_ColumnIsCompleted()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, 0, Player.X);
        service.MakeMove(game.Id, 1, Player.O);
        service.MakeMove(game.Id, 3, Player.X);
        service.MakeMove(game.Id, 2, Player.O);
        var result = service.MakeMove(game.Id, 6, Player.X);

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new[] { 0, 3, 6 }, result.WinningCells);
    }

    // Three marks along a diagonal should be recognized as a win
    [Fact]
    public void Should_DetectWin_When_DiagonalIsCompleted()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, 0, Player.X);
        service.MakeMove(game.Id, 1, Player.O);
        service.MakeMove(game.Id, 4, Player.X);
        service.MakeMove(game.Id, 2, Player.O);
        var result = service.MakeMove(game.Id, 8, Player.X);

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new[] { 0, 4, 8 }, result.WinningCells);
    }

    // A full board with no winning line should be scored as a draw
    [Fact]
    public void Should_DetectDraw_When_BoardIsFullWithNoWinner()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        // Final board:
        // X O X
        // X O O
        // O X X
        service.MakeMove(game.Id, 0, Player.X);
        service.MakeMove(game.Id, 1, Player.O);
        service.MakeMove(game.Id, 2, Player.X);
        service.MakeMove(game.Id, 4, Player.O);
        service.MakeMove(game.Id, 3, Player.X);
        service.MakeMove(game.Id, 5, Player.O);
        service.MakeMove(game.Id, 7, Player.X);
        service.MakeMove(game.Id, 6, Player.O);
        var result = service.MakeMove(game.Id, 8, Player.X);

        Assert.Equal(GameStatus.Draw, result.Status);
        Assert.Null(result.Winner);
        Assert.All(result.Board, cell => Assert.False(string.IsNullOrEmpty(cell)));
    }

    // Resetting a game should clear the board and turn order but leave the scoreboard untouched
    [Fact]
    public void Should_ClearBoardAndResetTurnButKeepScoreboard_When_GameIsReset()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, 0, Player.X);
        service.MakeMove(game.Id, 3, Player.O);
        service.MakeMove(game.Id, 1, Player.X);
        service.MakeMove(game.Id, 4, Player.O);
        service.MakeMove(game.Id, 2, Player.X); // X wins, scoreboard.XWins == 1

        var result = service.ResetGame(game.Id);

        Assert.All(result.Board, cell => Assert.True(string.IsNullOrEmpty(cell)));
        Assert.Equal(Player.X, result.CurrentTurn);
        Assert.Equal(GameStatus.InProgress, result.Status);
        Assert.Empty(result.MoveHistory);
        Assert.Equal(1, result.Scoreboard.XWins);
    }

    // Undoing in two-player mode should remove only the single last move and restore whose turn it was
    [Fact]
    public void Should_RemoveLastMoveAndRestoreTurn_When_UndoInTwoPlayerMode()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, 0, Player.X);
        service.MakeMove(game.Id, 1, Player.O);

        var result = service.UndoMove(game.Id);

        Assert.True(string.IsNullOrEmpty(result.Board[1]));
        Assert.Equal("X", result.Board[0]);
        Assert.Single(result.MoveHistory);
        Assert.Equal(Player.O, result.CurrentTurn);
    }

    // Undoing in vs-computer mode should remove the human/computer move pair and restore the human's turn
    [Fact]
    public void Should_RemoveLastTwoMovesAndRestoreTurn_When_UndoInComputerMode()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.VsComputer);

        var afterMove = service.MakeMove(game.Id, 0, Player.X);
        Assert.Equal(2, afterMove.MoveHistory.Count); // human move + computer's auto-response
        Assert.Equal(Player.X, afterMove.CurrentTurn);

        var result = service.UndoMove(game.Id);

        Assert.Empty(result.MoveHistory);
        Assert.All(result.Board, cell => Assert.True(string.IsNullOrEmpty(cell)));
        Assert.Equal(Player.X, result.CurrentTurn);
    }

    // Winning as X should increment the scoreboard's X win count only
    [Fact]
    public void Should_IncrementXWins_When_XWinsGame()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, 0, Player.X);
        service.MakeMove(game.Id, 3, Player.O);
        service.MakeMove(game.Id, 1, Player.X);
        service.MakeMove(game.Id, 4, Player.O);
        service.MakeMove(game.Id, 2, Player.X);

        var scoreboard = service.GetScoreboard(game.Id);

        Assert.Equal(1, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }

    // Winning as O should increment the scoreboard's O win count only
    [Fact]
    public void Should_IncrementOWins_When_OWinsGame()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, 3, Player.X);
        service.MakeMove(game.Id, 0, Player.O);
        service.MakeMove(game.Id, 4, Player.X);
        service.MakeMove(game.Id, 1, Player.O);
        service.MakeMove(game.Id, 8, Player.X);
        service.MakeMove(game.Id, 2, Player.O);

        var scoreboard = service.GetScoreboard(game.Id);

        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(1, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }

    // A drawn game should increment the scoreboard's draw count only
    [Fact]
    public void Should_IncrementDraws_When_GameEndsInDraw()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, 0, Player.X);
        service.MakeMove(game.Id, 1, Player.O);
        service.MakeMove(game.Id, 2, Player.X);
        service.MakeMove(game.Id, 4, Player.O);
        service.MakeMove(game.Id, 3, Player.X);
        service.MakeMove(game.Id, 5, Player.O);
        service.MakeMove(game.Id, 7, Player.X);
        service.MakeMove(game.Id, 6, Player.O);
        service.MakeMove(game.Id, 8, Player.X);

        var scoreboard = service.GetScoreboard(game.Id);

        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(1, scoreboard.Draws);
    }

    // Once a game has finished, no further moves should be accepted
    [Fact]
    public void Should_ThrowException_When_MoveIsMadeAfterGameCompletion()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(game.Id, 0, Player.X);
        service.MakeMove(game.Id, 3, Player.O);
        service.MakeMove(game.Id, 1, Player.X);
        service.MakeMove(game.Id, 4, Player.O);
        service.MakeMove(game.Id, 2, Player.X); // X wins here

        Assert.Throws<InvalidOperationException>(() => service.MakeMove(game.Id, 5, Player.O));
    }
}
