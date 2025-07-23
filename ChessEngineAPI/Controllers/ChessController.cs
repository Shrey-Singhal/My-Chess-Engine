using Microsoft.AspNetCore.Mvc;
using ChessEngineAPI.Models;
using ChessEngineAPI.Engine;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Cors;

namespace ChessEngineAPI.Controllers
{
    public class EngineSearchParams
    {
        public int Time { get; set; }
    }
    [ApiController]
    [Route("api/chess")]
    public class ChessController(ConcurrentDictionary<string, ChessEngineState> games) : ControllerBase
    {
        private readonly ConcurrentDictionary<string, ChessEngineState> _games = games;

        // Helper to fetch the current engine or throw
        private ChessEngineState GetEngine()
        {
            var id = HttpContext.Session.Id;
            return _games.GetOrAdd(id, _ =>
            {
                var e = new ChessEngineState();
                e.Board.ParseFEN(Defs.START_FEN);
                e.Search.ClearForSearch();
                return e;
            });
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
            var _engine = GetEngine();

            _engine.SetPositionFromFEN(request.Fen);

            return Ok(new { message = "Fen received and applied", fen = request.Fen });
        }

        [HttpGet("getpieces")]
        public IActionResult GetPieces()
        {
            var _engine = GetEngine();
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
            var engine = GetEngine();

            var from = Defs.UserMove.from;
            var to   = Defs.UserMove.to;
            Defs.UserMove.from = Defs.Squares.NO_SQ;
            Defs.UserMove.to   = Defs.Squares.NO_SQ;

            if (from == Defs.Squares.NO_SQ || to == Defs.Squares.NO_SQ)
                return BadRequest("Both 'from' and 'to' must be set");

            int parsed = engine.Movegen.ParseMove(from, to, engine.Board, engine.MoveManager);
            if (parsed == MoveUtils.NO_MOVE)
                return BadRequest("Illegal move");

            engine.MoveManager.MakeMove(parsed, engine.Board);

            var pieces = engine.GetGuiPieces();
            var result = engine.CheckResult();

            return Ok(new
            {
                message = "Move made",
                fromSq  = Defs.SqToPrSq(from),
                toSq    = Defs.SqToPrSq(to),
                pieces,       // send the updated GUI pieces
                result
            });
        }

        [HttpGet("enginestats")]
        public IActionResult GetEngineStats()
        {
            var _engine = GetEngine();
            var stats = _engine.Search.GetSearchStats();
            return Ok(stats);
        }

        [HttpPost("engineMove")]
        public IActionResult EngineMove([FromBody] EngineSearchParams searchParams)
        {
            var _engine = GetEngine();
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

            return Ok(new
            {
                pieces = _engine.GetGuiPieces(),
                stats = _engine.Search.GetSearchStats(),
                result = _engine.CheckResult() // check for mate/draw now
            });
        }

        [HttpPost("takemove")]
        public IActionResult TakeMove()
        {
            var _engine = GetEngine();
            if (_engine.Board.hisPly > 0)
            {
                _engine.MoveManager.TakeMove();
                _engine.Board.ply = 0;


                // Refresh the board state
                return Ok(new
                {
                    pieces = _engine.GetGuiPieces(),
                    stats = _engine.Search.GetSearchStats(),
                    result = _engine.CheckResult()
                });
            }
            else
            {
                return Ok(new
                {
                    pieces = _engine.GetGuiPieces(),
                    stats = _engine.Search.GetSearchStats(),
                    result = _engine.CheckResult(),
                    info = "No more moves to take back"
                });
            }

        }

        [HttpPost("newgame")]
        public IActionResult NewGame()
        {
            var sessionId = HttpContext.Session.Id;
            var _engine = new ChessEngineState();
            _engine.Board.ParseFEN(Defs.START_FEN);    // reset to initial position
            _engine.Search.ClearForSearch();

            _games[sessionId] = _engine;

            return Ok(new
            {
                pieces = _engine.GetGuiPieces(),
            });
        }

    }
}