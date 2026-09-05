using Microsoft.AspNetCore.Mvc;
using TicTacToe.API.Models;
using TicTacToe.API.Services;

namespace TicTacToe.API.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameController : ControllerBase
    {
        private readonly GameService _gameService;

        public GameController(GameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("start")]
        public ActionResult<GameStateResponse> StartGame([FromBody] StartGameRequest request)
        {
            try
            {
                var response = _gameService.CreateGame(request.Mode);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:guid}")]
        public ActionResult<GameStateResponse> GetGame(Guid id)
        {
            try
            {
                var response = _gameService.GetGame(id);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id:guid}/move")]
        public ActionResult<GameStateResponse> MakeMove(Guid id, [FromBody] MakeMoveRequest request)
        {
            try
            {
                var response = _gameService.MakeMove(id, request.CellIndex, request.Player);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id:guid}/undo")]
        public ActionResult<GameStateResponse> UndoMove(Guid id)
        {
            try
            {
                var response = _gameService.UndoMove(id);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id:guid}/reset")]
        public ActionResult<GameStateResponse> ResetGame(Guid id)
        {
            try
            {
                var response = _gameService.ResetGame(id);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id:guid}/scoreboard")]
        public ActionResult<ScoreboardDto> GetScoreboard(Guid id)
        {
            try
            {
                var scoreboard = _gameService.GetScoreboard(id);
                return Ok(scoreboard);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteGame(Guid id)
        {
            _gameService.DeleteGame(id);
            return NoContent();
        }
    }
}
