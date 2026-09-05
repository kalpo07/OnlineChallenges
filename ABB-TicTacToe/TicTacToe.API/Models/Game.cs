namespace TicTacToe.API.Models
{
    public enum Player
    {
        X,
        O
    }

    public enum GameMode
    {
        TwoPlayer,
        VsComputer
    }

    public enum GameStatus
    {
        InProgress,
        Won,
        Draw
    }

    public class Game
    {
        public Guid Id { get; set; }
        public string[] Board { get; set; } = new string[9];
        public Player CurrentTurn { get; set; }
        public GameMode Mode { get; set; }
        public GameStatus Status { get; set; }
        public Player? Winner { get; set; }
        public int[] WinningCells { get; set; } = Array.Empty<int>();
        public List<int> MoveHistory { get; set; } = new List<int>();
    }
}
