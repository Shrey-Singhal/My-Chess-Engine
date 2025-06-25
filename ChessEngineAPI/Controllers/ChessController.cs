using Microsoft.AspNetCore.Mvc;
using ChessEngineAPI.Models;

namespace ChessEngineAPI.Controllers
{
    [ApiController]
    [Route("api/chess")]
    public class ChessController : ControllerBase
    {
        [HttpPost("setfen")]
        // [FromBody] FenRequest requestgets the json body from request and converts it to FenRequest object
        public IActionResult SetFEN([FromBody] FenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Fen))
            {
                return BadRequest("FEN string is required");
            }
            Console.WriteLine("Received FEN: " + request.Fen);
            return Ok(new { message = "Fen received", fen = request.Fen });
        }

    }
}