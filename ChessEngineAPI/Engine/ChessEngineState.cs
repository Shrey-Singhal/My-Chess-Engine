namespace ChessEngineAPI.Engine
{
    public class ChessEngineState
    {
        // auto property with a public getter and a private setter so that only this class can modify the board.
        public Gameboard Board { get; private set; }

        public ChessEngineState()
        {
            Defs.InitFilesRanksBoard();
            Defs.InitSq120To64();

            Board = new Gameboard();
            Board.InitHashKeys(); // set up random hash keys
            Board.ParseFEN(Defs.START_FEN); // load initial position
            Board.posKey = Board.GeneratePosKey(); // gen hash for that position

            Board.PrintBoard();
        }

        public void SetPositionFromFEN(string FEN)
        {
            Board.ParseFEN(FEN);
            Board.posKey = Board.GeneratePosKey();
            Board.PrintBoard();
        }
    }
}