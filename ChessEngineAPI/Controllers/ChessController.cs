using Microsoft.AspNetCore.Mvc;
using ChessEngineAPI.Models;
using ChessEngineAPI.Engine;

namespace ChessEngineAPI.Controllers
{
    public class EngineSearchParams
    {
        public int Time { get; set; }
    }
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

        [HttpPost("makeusermove")]
        public IActionResult MakeUserMove()
        {
            if (Defs.UserMove.from != Defs.Squares.NO_SQ && Defs.UserMove.to != Defs.Squares.NO_SQ)
            {
                // Parse the move (using your ParseMove logic)
                int parsed = _engine.Movegen.ParseMove(Defs.UserMove.from, Defs.UserMove.to, _engine.Board, _engine.MoveManager);
                if (parsed != MoveUtils.NO_MOVE)
                {
                    _engine.MoveManager.MakeMove(parsed, _engine.Board); // actually make the move
                    _engine.Board.PrintBoard();

                    var result = _engine.CheckResult();

                    if (result != null)
                    {
                        return Ok(new { result });
                    }

                    return Ok(new
                    {
                        message = "Move made",
                        fromSq = Defs.SqToPrSq(Defs.UserMove.from),
                        toSq = Defs.SqToPrSq(Defs.UserMove.to)
                    });
                }
                return BadRequest("Illegal move");
            }
            return BadRequest("Both 'from' and 'to' must be set");
        }

        [HttpGet("enginestats")]
        public IActionResult GetEngineStats()
        {
            var stats = _engine.Search.GetSearchStats();
            return Ok(stats);
        }

        [HttpPost("engineMove")]
        public IActionResult EngineMove([FromBody] EngineSearchParams searchParams)
        {
            var result = _engine.CheckResult();
            if (result != null)
                return BadRequest("Game is over.");

            SearchController.Depth = Defs.MAXDEPTH;
            SearchController.Time = searchParams?.Time > 0 ? searchParams.Time : 1000;

            _engine.Search.SearchPosition();

            int bestMove = SearchController.Best;
            if (bestMove == MoveUtils.NO_MOVE)
                return BadRequest("No valid move found");

            _engine.MoveManager.MakeMove(bestMove, _engine.Board);

            return Ok(new {
                pieces = _engine.GetGuiPieces(),
                stats = _engine.Search.GetSearchStats(),
                result = _engine.CheckResult() // check for mate/draw now
            });
        }
    }
}