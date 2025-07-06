namespace ChessEngineAPI.Engine
{
    public class ChessEngineState
    {
        // auto property with a public getter and a private setter so that only this class can modify the board.
        public Gameboard Board { get; private set; }
        private readonly Movegen movegen;
        private readonly MoveManager moveManager;
        private readonly PerfTesting perfTesting;

        public ChessEngineState()
        {
            Board = new Gameboard();
            movegen = new();
            moveManager = new(Board);
            perfTesting = new(Board);

            Defs.InitFilesRanksBoard();
            Board.InitHashKeys(); // set up random hash keys            
            Defs.InitSq120To64();

            for (int index = 0; index < Defs.MAXGAMEMOVES; ++index)
            {
                Board.history.Add(new Defs.MoveHistory
                {
                    move = MoveUtils.NO_MOVE,
                    castlePerm = 0,
                    enPas = 0,
                    fiftyMove = 0,
                    posKey = 0
                });
            }


            Board.ParseFEN(Defs.START_FEN); // load initial position
                                            // Board.posKey = Board.GeneratePosKey(); // gen hash for that position

            Board.PrintBoard();


            // movegen.GenerateMoves(Board);
            // movegen.PrintMoveList(Board);
            // Board.CheckBoard();

            // moveManager.MakeMove(Board.moveList[0], Board);
            // Board.PrintBoard();
            // Board.CheckBoard();
            // moveManager.TakeMove();
            // Board.PrintBoard();
            // Board.CheckBoard();

        }

        public void SetPositionFromFEN(string FEN)
        {
            Board.ParseFEN(FEN);
            Board.PrintBoard();

            perfTesting.PerftTest(5);
        }
    }
}