using TicTacToe.API.Models;

namespace TicTacToe.API.Services
{
    public class GameService
    {
        private readonly Dictionary<Guid, Game> _games = new();
        private readonly Dictionary<Guid, ScoreboardDto> _scoreboards = new();
        private readonly Random _random = new();

        public GameStateResponse CreateGame(GameMode mode)
        {
            var game = new Game
            {
                Id = Guid.NewGuid(),
                Board = new string[9],
                CurrentTurn = Player.X,
                Mode = mode,
                Status = GameStatus.InProgress,
                Winner = null,
                WinningCells = Array.Empty<int>(),
                MoveHistory = new List<int>()
            };

            _games[game.Id] = game;
            _scoreboards[game.Id] = new ScoreboardDto();

            return ToResponse(game);
        }

        public GameStateResponse GetGame(Guid id)
        {
            var game = GetGameOrThrow(id);
            return ToResponse(game);
        }

        public ScoreboardDto GetScoreboard(Guid id)
        {
            GetGameOrThrow(id);
            return _scoreboards[id];
        }

        public bool DeleteGame(Guid id)
        {
            _scoreboards.Remove(id);
            return _games.Remove(id);
        }

        public GameStateResponse MakeMove(Guid id, int cellIndex, Player player)
        {
            var game = GetGameOrThrow(id);

            if (game.Status != GameStatus.InProgress)
            {
                throw new InvalidOperationException("Game is already over.");
            }

            if (cellIndex < 0 || cellIndex > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(cellIndex), "Cell index must be between 0 and 8.");
            }

            if (!string.IsNullOrEmpty(game.Board[cellIndex]))
            {
                throw new InvalidOperationException("Cell is already occupied.");
            }

            if (player != game.CurrentTurn)
            {
                throw new InvalidOperationException("It is not this player's turn.");
            }

            ApplyMove(game, cellIndex, player);

            if (game.Status == GameStatus.InProgress && game.Mode == GameMode.VsComputer && game.CurrentTurn != player)
            {
                var computerMove = PickComputerMove(game);
                if (computerMove.HasValue)
                {
                    ApplyMove(game, computerMove.Value, game.CurrentTurn);
                }
            }

            return ToResponse(game);
        }

        public GameStateResponse UndoMove(Guid id)
        {
            var game = GetGameOrThrow(id);

            if (game.Status != GameStatus.InProgress)
            {
                throw new InvalidOperationException("Cannot undo — game is already over.");
            }

            if (game.MoveHistory.Count == 0)
            {
                throw new InvalidOperationException("No moves to undo.");
            }

            var movesToUndo = game.Mode == GameMode.VsComputer ? 2 : 1;

            for (var i = 0; i < movesToUndo && game.MoveHistory.Count > 0; i++)
            {
                var lastIndex = game.MoveHistory[^1];
                game.MoveHistory.RemoveAt(game.MoveHistory.Count - 1);
                game.Board[lastIndex] = string.Empty;
                game.CurrentTurn = game.CurrentTurn == Player.X ? Player.O : Player.X;
            }

            return ToResponse(game);
        }

        public GameStateResponse ResetGame(Guid id)
        {
            var game = GetGameOrThrow(id);

            game.Board = new string[9];
            game.CurrentTurn = Player.X;
            game.Status = GameStatus.InProgress;
            game.Winner = null;
            game.WinningCells = Array.Empty<int>();
            game.MoveHistory = new List<int>();

            return ToResponse(game);
        }

        private void ApplyMove(Game game, int cellIndex, Player player)
        {
            game.Board[cellIndex] = player.ToString();
            game.MoveHistory.Add(cellIndex);

            var winningCells = FindWinningCells(game.Board, player);
            if (winningCells != null)
            {
                game.Status = GameStatus.Won;
                game.Winner = player;
                game.WinningCells = winningCells;
                UpdateScoreboard(game.Id, player);
                return;
            }

            if (game.Board.All(cell => !string.IsNullOrEmpty(cell)))
            {
                game.Status = GameStatus.Draw;
                UpdateScoreboard(game.Id, null);
                return;
            }

            game.CurrentTurn = player == Player.X ? Player.O : Player.X;
        }

        private int? PickComputerMove(Game game)
        {
            var emptyCells = Enumerable.Range(0, 9)
                .Where(i => string.IsNullOrEmpty(game.Board[i]))
                .ToList();

            if (emptyCells.Count == 0)
            {
                return null;
            }

            return emptyCells[_random.Next(emptyCells.Count)];
        }

        private void UpdateScoreboard(Guid gameId, Player? winner)
        {
            if (!_scoreboards.TryGetValue(gameId, out var scoreboard))
            {
                scoreboard = new ScoreboardDto();
                _scoreboards[gameId] = scoreboard;
            }

            if (winner == Player.X)
            {
                scoreboard.XWins++;
            }
            else if (winner == Player.O)
            {
                scoreboard.OWins++;
            }
            else
            {
                scoreboard.Draws++;
            }
        }

        private static int[]? FindWinningCells(string[] board, Player player)
        {
            var mark = player.ToString();

            // TODO: Could refactor — define winning combinations as static array and loop. Kept explicit for readability during review.
            if (board[0] == mark && board[1] == mark && board[2] == mark) return new[] { 0, 1, 2 };
            if (board[3] == mark && board[4] == mark && board[5] == mark) return new[] { 3, 4, 5 };
            if (board[6] == mark && board[7] == mark && board[8] == mark) return new[] { 6, 7, 8 };

            if (board[0] == mark && board[3] == mark && board[6] == mark) return new[] { 0, 3, 6 };
            if (board[1] == mark && board[4] == mark && board[7] == mark) return new[] { 1, 4, 7 };
            if (board[2] == mark && board[5] == mark && board[8] == mark) return new[] { 2, 5, 8 };

            if (board[0] == mark && board[4] == mark && board[8] == mark) return new[] { 0, 4, 8 };
            if (board[2] == mark && board[4] == mark && board[6] == mark) return new[] { 2, 4, 6 };

            return null;
        }

        private Game GetGameOrThrow(Guid id)
        {
            if (!_games.TryGetValue(id, out var game))
            {
                throw new KeyNotFoundException($"Game with id '{id}' was not found.");
            }

            return game;
        }

        private GameStateResponse ToResponse(Game game)
        {
            _scoreboards.TryGetValue(game.Id, out var scoreboard);

            return new GameStateResponse
            {
                Id = game.Id,
                Board = game.Board,
                CurrentTurn = game.CurrentTurn,
                Mode = game.Mode,
                Status = game.Status,
                Winner = game.Winner,
                WinningCells = game.WinningCells,
                MoveHistory = game.MoveHistory,
                CanUndo = game.Status == GameStatus.InProgress && game.MoveHistory.Count > 0,
                Scoreboard = scoreboard ?? new ScoreboardDto()
            };
        }
    }
}
