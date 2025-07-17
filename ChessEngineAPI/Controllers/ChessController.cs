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

        [HttpPost("setusermove")]
        public IActionResult SetUserMove([FromBody] int sq)
        {
            if (Defs.UserMove.from == Defs.Squares.NO_SQ)
            {
                Defs.UserMove.from = sq;
                return Ok(new
                {
                    message = "from set",
                    fromSq = Defs.SqToPrSq(Defs.UserMove.from)
                });
            }
            else
            {
                Defs.UserMove.to = sq;

                var fromPrSq = Defs.SqToPrSq(Defs.UserMove.from);
                var toPrSq = Defs.SqToPrSq(Defs.UserMove.to);

                return Ok(new
                {
                    message = "move completed",
                    fromSq = fromPrSq,
                    toSq = toPrSq
                });
            }
        }

        [HttpGet("getusermove")]
        public IActionResult GetUserMove()
        {
            return Ok(new
            {
                from = Defs.UserMove.from,
                to = Defs.UserMove.to,
                fromSq = Defs.UserMove.from == Defs.Squares.NO_SQ ? null : Defs.SqToPrSq(Defs.UserMove.from),
                toSq = Defs.UserMove.to == Defs.Squares.NO_SQ ? null : Defs.SqToPrSq(Defs.UserMove.to)
            });
        }

        [HttpPost("resetusermove")]
        public IActionResult ResetUserMove()
        {
            Defs.UserMove.from = Defs.Squares.NO_SQ;
            Defs.UserMove.to = Defs.Squares.NO_SQ;

            return Ok("user move reset");
        }
    }
}