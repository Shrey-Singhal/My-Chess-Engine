using Microsoft.AspNetCore.Mvc;
using ChessEngineAPI.Models;
using ChessEngineAPI.Engine;

namespace ChessEngineAPI.Controllers
{
    [ApiController]
    [Route("api/chess")]
    public class ChessController : ControllerBase
    {
        private readonly ChessEngineState _engine;

        public ChessController(ChessEngineState engine)
        {
            _engine = engine;
        }
        [HttpPost("setfen")]
        // [FromBody] FenRequest requestgets the json body from request and converts it to FenRequest object
        public IActionResult SetFEN([FromBody] FenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Fen))
            {
                return BadRequest("FEN string is required");
            }
            Console.WriteLine("Received FEN: " + request.Fen);
            _engine.SetPositionFromFEN(request.Fen);

            return Ok(new { message = "Fen received and applied", fen = request.Fen });
        }

        [HttpGet("getpieces")]
        public IActionResult GetPieces()
        {
            var pieces = _engine.GetGuiPieces();
            return Ok(pieces);
        }

    }
}