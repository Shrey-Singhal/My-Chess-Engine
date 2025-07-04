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

            Board.InitHashKeys(); // set up random hash keys
            Board.ParseFEN(Defs.START_FEN); // load initial position
                                            // Board.posKey = Board.GeneratePosKey(); // gen hash for that position

            Board.PrintBoard();

            Movegen movegen = new();
            movegen.GenerateMoves(Board);
            movegen.PrintMoveList(Board);
            Board.CheckBoard();
            MoveManager moveManager = new(Board);
            moveManager.MakeMove(Board.moveList[0], Board);
            Board.PrintBoard();
            Board.CheckBoard();
            moveManager.TakeMove();
            Board.PrintBoard();
            Board.CheckBoard();

        }

        public void SetPositionFromFEN(string FEN)
        {
            Board.ParseFEN(FEN);
            Board.posKey = Board.GeneratePosKey();
            Board.PrintBoard();

            Movegen movegen = new();
            movegen.GenerateMoves(Board);
            movegen.PrintMoveList(Board);
        }
    }
}