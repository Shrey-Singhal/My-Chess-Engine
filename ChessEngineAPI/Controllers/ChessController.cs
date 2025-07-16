using Microsoft.AspNetCore.Mvc;
using ChessEngineAPI.Models;
using ChessEngineAPI.Engine;

namespace ChessEngineAPI.Controllers
{
    [ApiController]
    [Route("api/chess")]
    public class ChessController(ChessEngineState engine) : ControllerBase
    {
        private readonly ChessEngineState _engine = engine;

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

        [HttpGet("fr2sq")]
        public IActionResult FileRankToSq(int file, int rank)
        {
            int sq = Defs.GetSquareIndex(file, rank); // returns 0–119
            string prSq = Defs.FileChar[Defs.FilesBrd[sq]] + Defs.RankChar[Defs.RanksBrd[sq]].ToString(); // convert to "e4", "d5", etc.
            return Ok(new { prSq, sq });
        }

    }
}