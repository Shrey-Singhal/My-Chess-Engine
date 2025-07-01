namespace ChessEngineAPI.Engine

// all enums, lists, constants, conversion tables, helper functions
{
    public static class Defs
    {
        public enum Pieces
        {
            EMPTY = 0,
            wP = 1, wN = 2, wB = 3, wR = 4, wQ = 5, wK = 6,
            bp = 7, bN = 8, bB = 9, bR = 10, bQ = 11, bK = 12
        }

        // RANK_NONE and FILE_NONE are used to indicate “no valid rank or file,” 
        // to help handle invalid or off-board squares safely in the engine.
        public enum Files
        {
            FILE_A = 0, FILE_B = 1, FILE_C = 2, FILE_D = 3, FILE_E = 4, FILE_F = 5, FILE_G = 6,
            FILE_H = 7, FILE_NONE = 8
        }

        public enum Ranks
        {
            RANK_1 = 0, RANK_2 = 1, RANK_3 = 2, RANK_4 = 3, RANK_5 = 4, RANK_6 = 5,
            RANK_7 = 6, RANK_8 = 7, RANK_NONE = 8
        }

        //WHITE / BLACK → real piece, belongs to a player
        //BOTH → no owner, used for EMPTY or OFFBOARD
        public enum Colours
        {
            WHITE = 0, BLACK = 1, BOTH = 2
        }

        public enum CASTLEBIT
        {
            WKCA = 1,  //0001
            WQCA = 2, //0010
            BKCA = 4, //0100
            BQCA = 8  //1000
        }

        // NO_SQ = -1 means “no valid square” — used in code to signal that a square doesn’t exist.
        // OFFBOARD = 100 marks squares outside the playable board in the 10×12 array, helping detect invalid moves.
        public static class Squares
        {
            public const int A1 = 21, B1 = 22, C1 = 23, D1 = 24, E1 = 25, F1 = 26, G1 = 27, H1 = 28;
            public const int A8 = 91, B8 = 92, C8 = 93, D8 = 94, E8 = 95, F8 = 96, G8 = 97, H8 = 98;
            public const int NO_SQ = -1;
            public const int OFFBOARD = 100;
        }

        // standardize true or false in a consistent way
        public static class Bool
        {
            public const int FALSE = 0;
            public const int TRUE = 1;
        }

        public const int MAXGAMEMOVES = 2048;
        public const int MAXPOSITIONMOVES = 256;
        public const int MAXDEPTH = 64;

        //10 x 12 board representation instead of 8 x 8
        public const int BRD_SQ_NUM = 120;

        // these 2 lists contain file index and rank index for each square
        public static readonly int[] FilesBrd = new int[BRD_SQ_NUM];
        public static readonly int[] RanksBrd = new int[BRD_SQ_NUM];

        public static void InitFilesRanksBoard()
        {

            for (int i = 0; i < BRD_SQ_NUM; ++i)
            {
                FilesBrd[i] = Squares.OFFBOARD;
                RanksBrd[i] = Squares.OFFBOARD;
            }

            for (int rank = 0; rank < 8; ++rank)
            {
                for (int file = 0; file < 8; ++file)
                {
                    int sq = GetSquareIndex(file, rank);
                    FilesBrd[sq] = file;
                    RanksBrd[sq] = rank;

                }
            }
        }

        private static readonly Random rand = new();
        public static ulong Rand64()
        {
            ulong r1 = (ulong)rand.Next(0, int.MaxValue);
            ulong r2 = (ulong)rand.Next(0, int.MaxValue);
            return (r1 << 32) | r2;
        }

        //function to get square index
        public static int GetSquareIndex(int file, int rank)
        {
            return file + 21 + (rank * 10);
        }

        // the arrays and functions below help you to convert between 64 square board index and
        // 120 square board index.
        public static readonly int[] Sq120ToSq64 = new int[BRD_SQ_NUM];
        public static readonly int[] Sq64ToSq120 = new int[64];

        public const string START_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public static readonly string[] PceChar = [".", "P", "N", "B", "R", "Q", "K", "p", "n", "b", "r", "q", "k"];
        public static readonly string[] FileChar = ["a", "b", "c", "d", "e", "f", "g", "h"];
        public static readonly string[] RankChar = ["1", "2", "3", "4", "5", "6", "7", "8"];
        public static readonly string[] SideChar = ["w", "b"];

        public static void InitSq120To64()
        {
            // setting defaults like 65 and 120 help catch bugs early and prevent undefined behaviour
            for (int i = 0; i < 120; ++i)
            {
                Sq120ToSq64[i] = 65; // default: not on 8x8 board
            }

            for (int i = 0; i < 64; ++i)
            {
                Sq64ToSq120[i] = 120; // default: invalid
            }

            int sq64 = 0;
            for (int rank = (int)Ranks.RANK_1; rank <= (int)Ranks.RANK_8; ++rank)
            {
                for (int file = (int)Files.FILE_A; file <= (int)Files.FILE_H; ++file)
                {
                    int sq120 = GetSquareIndex(file, rank);
                    Sq64ToSq120[sq64] = sq120;
                    Sq120ToSq64[sq120] = sq64;
                    sq64++;
                }
            }
        }

        public static int SQ64(int sq120)
        {
            return Sq120ToSq64[sq120];
        }

        public static int SQ120(int sq64)
        {
            return Sq64ToSq120[sq64];
        }

        public static readonly int[] KnDir = [-8, -19, -21, -12, 8, 19, 21, 12];
        public static readonly int[] RkDir = [-1, -10, 1, 10];
        public static readonly int[] BiDir = [-9, -11, 11, 9];
        public static readonly int[] KiDir = [1, -10, 1, 10, -9, -11, 11, 9];

        // the array below represents how many directions each piece can move in
        public static readonly int[] DirNum = [0, 0, 8, 4, 4, 8, 8, 0, 8, 4, 4, 8, 8];
        // this array tells u how each piece moves on the board by storing the diractions it can go to
        public static readonly int[][] PceDir = [[], [], KnDir, BiDir, RkDir, KiDir, KiDir, [], KnDir, BiDir, RkDir, KiDir, KiDir];
        // 0 in the array below is just used to separate black and white
        public static readonly int[] LoopNonSlidePce = [(int)Pieces.wN, (int)Pieces.wK, 0, (int)Pieces.bN, (int)Pieces.bK, 0];
        public static readonly int[] LoopNonSlideIndex = [0, 3];

        public static readonly int[] LoopSlidePce = [(int)Pieces.wB, (int)Pieces.wR, (int)Pieces.wQ, 0, (int)Pieces.bB, (int)Pieces.bR, (int)Pieces.bQ, 0];
        public static readonly int[] LoopSlideIndex = [0, 4];
        public static int SQOFFBOARD(int sq)
        {
            if (FilesBrd[sq] == Squares.OFFBOARD)
            {
                return Bool.TRUE;
            }
            else
            {
                return Bool.FALSE;
            }
        }
    }
}