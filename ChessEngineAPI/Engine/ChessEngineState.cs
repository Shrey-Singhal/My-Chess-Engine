namespace ChessEngineAPI.Engine
{
    using ChessEngineAPI.Models;
    public class ChessEngineState
    {
        // auto property with a public getter and a private setter so that only this class can modify the board.
        public Gameboard Board { get; }
        public Movegen Movegen { get; }
        public MoveManager MoveManager { get; }
        public PerfTesting PerfTesting { get; }
        public Search Search { get; }

        public ChessEngineState()
        {
            Board = new Gameboard();
            Movegen = new Movegen();
            MoveManager = new MoveManager(Board);
            PerfTesting = new PerfTesting(Board);
            Search = new Search(Board, Movegen, MoveManager);

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
            for (int index = 0; index < Defs.PVENTRIES; ++index)
            {
                Board.PvTable[index] = new Gameboard.PvEntry
                {
                    move = MoveUtils.NO_MOVE,
                    posKey = 0
                };
            }
            Movegen.InitMvvLva();



            Board.ParseFEN(Defs.START_FEN); // load initial position
                                            // Board.posKey = Board.GeneratePosKey(); // gen hash for that position

            Board.PrintBoard();

        }

        public void SetPositionFromFEN(string FEN)
        {
            Board.ParseFEN(FEN);
            Board.PrintBoard();

            Search.SearchPosition();
            Console.WriteLine("Success");
        }

        public List<GuiPiece> GetGuiPieces()
        {
            var guiPieces = new List<GuiPiece>();

            for (int sq = 0; sq < 64; ++sq)
            {
                int sq120 = Defs.Sq64ToSq120[sq];
                int piece = Board.pieces[sq120];

                if (piece == (int)Defs.Pieces.EMPTY) continue; // Skip empty squares
                if (piece < 0 || piece >= Defs.PceChar.Length) continue; // Skip invalid pieces

                int file = Defs.FilesBrd[sq120];
                int rank = Defs.RanksBrd[sq120];

                string rankClass = "rank" + (rank + 1);
                string fileClass = "file" + (file + 1);
                string colorChar = piece <= 6 ? "w" : "b"; // w = white pieces (1-6), b = black (7-12)

                string pieceStr = colorChar + Defs.PceChar[piece].ToUpper(); // e.g. bQ, wK
                string imagePath = $"{pieceStr}.png";

                guiPieces.Add(new GuiPiece
                {
                    RankClass = rankClass,
                    FileClass = fileClass,
                    ImagePath = imagePath
                });
            }

            return guiPieces;
        }

    }
}