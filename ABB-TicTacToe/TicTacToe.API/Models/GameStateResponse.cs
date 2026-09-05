namespace TicTacToe.API.Models
{
    public class GameStateResponse
    {
        public Guid Id { get; set; }
        public string[] Board { get; set; } = new string[9];
        public Player CurrentTurn { get; set; }
        public GameMode Mode { get; set; }
        public GameStatus Status { get; set; }
        public Player? Winner { get; set; }
        public int[] WinningCells { get; set; } = Array.Empty<int>();
        public List<int> MoveHistory { get; set; } = new List<int>();
        public bool CanUndo { get; set; }
        public ScoreboardDto Scoreboard { get; set; } = new ScoreboardDto();
    }

    public class ScoreboardDto
    {
        public int XWins { get; set; }
        public int OWins { get; set; }
        public int Draws { get; set; }
    }

    public class StartGameRequest
    {
        public GameMode Mode { get; set; }
        public Player StartingPlayer { get; set; }
    }

    public class MakeMoveRequest
    {
        public Guid GameId { get; set; }
        public int CellIndex { get; set; }
        public Player Player { get; set; }
    }
}
