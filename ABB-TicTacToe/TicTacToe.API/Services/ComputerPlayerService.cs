namespace TicTacToe.API.Services
{
    public class ComputerPlayerService
    {
        private static readonly int[][] WinningCombinations =
        {
            new[] { 0, 1, 2 },
            new[] { 3, 4, 5 },
            new[] { 6, 7, 8 },
            new[] { 0, 3, 6 },
            new[] { 1, 4, 7 },
            new[] { 2, 5, 8 },
            new[] { 0, 4, 8 },
            new[] { 2, 4, 6 }
        };

        private static readonly int[] Corners = { 0, 2, 6, 8 };

        // TODO: Greedy heuristic meets the spec. Minimax would make the computer unbeatable — left as future improvement.
        public int GetBestMove(string[] board)
        {
            var winningMove = FindWinningMove(board, "O");
            if (winningMove.HasValue)
            {
                return winningMove.Value;
            }

            var blockingMove = FindWinningMove(board, "X");
            if (blockingMove.HasValue)
            {
                return blockingMove.Value;
            }

            if (string.IsNullOrEmpty(board[4]))
            {
                return 4;
            }

            foreach (var corner in Corners)
            {
                if (string.IsNullOrEmpty(board[corner]))
                {
                    return corner;
                }
            }

            for (var i = 0; i < board.Length; i++)
            {
                if (string.IsNullOrEmpty(board[i]))
                {
                    return i;
                }
            }

            throw new InvalidOperationException("No empty cells available for a move.");
        }

        // Finds a cell that completes a line for the given mark: used both to find the
        // computer's own winning move (mark = "O") and to find a move to block (mark = "X").
        private static int? FindWinningMove(string[] board, string mark)
        {
            foreach (var combination in WinningCombinations)
            {
                var a = combination[0];
                var b = combination[1];
                var c = combination[2];

                var cells = new[] { board[a], board[b], board[c] };
                var markCount = cells.Count(cell => cell == mark);
                var emptyIndex = Array.FindIndex(cells, string.IsNullOrEmpty);

                if (markCount == 2 && emptyIndex >= 0)
                {
                    return combination[emptyIndex];
                }
            }

            return null;
        }
    }
}
