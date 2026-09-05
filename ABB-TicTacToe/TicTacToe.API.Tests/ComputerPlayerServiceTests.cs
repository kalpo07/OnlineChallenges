using TicTacToe.API.Services;

namespace TicTacToe.API.Tests;

public class ComputerPlayerServiceTests
{
    private static ComputerPlayerService CreateService() => new();

    // When the computer can win immediately, it should take that move over anything else
    [Fact]
    public void Should_TakeWinningMove_When_WinIsAvailable()
    {
        var service = CreateService();
        // O has two in a row (0,1) with 2 open -> should take the win over any other option.
        var board = new[] { "O", "O", "", "X", "X", "", "", "", "" };

        var move = service.GetBestMove(board);

        Assert.Equal(2, move);
    }

    // With no win available, the computer should block the opponent's imminent win
    [Fact]
    public void Should_BlockOpponent_When_NoWinIsAvailable()
    {
        var service = CreateService();
        // X has two in a column (0,3) with 6 open; O has no winning move available.
        var board = new[] { "X", "O", "", "X", "", "", "", "", "" };

        var move = service.GetBestMove(board);

        Assert.Equal(6, move);
    }

    // With no win or block available, the computer should prefer the center cell
    [Fact]
    public void Should_TakeCenter_When_NoWinOrBlockIsAvailable()
    {
        var service = CreateService();
        var board = new[] { "X", "", "", "", "", "", "", "", "" };

        var move = service.GetBestMove(board);

        Assert.Equal(4, move);
    }

    // With the center taken and no win or block available, the computer should take a corner
    [Fact]
    public void Should_TakeCorner_When_NoWinBlockOrCenterIsAvailable()
    {
        var service = CreateService();
        // Center already taken, no win/block available.
        var board = new[] { "", "", "", "", "X", "", "", "", "" };

        var move = service.GetBestMove(board);

        Assert.Equal(0, move);
    }

    // With the center and all corners taken and no win or block available, the computer should take any remaining cell
    [Fact]
    public void Should_TakeAnyRemainingCell_When_NoWinBlockCenterOrCornerIsAvailable()
    {
        var service = CreateService();
        // Center and all corners taken (X O / O X pattern on corners, no line has two
        // matching marks with the third cell empty), so neither a win nor a block
        // exists; only edge cells (1, 3, 5, 7) remain and none of them is a corner.
        var board = new[] { "X", "", "O", "", "X", "", "O", "", "X" };

        var move = service.GetBestMove(board);

        Assert.Equal(1, move);
    }
}
