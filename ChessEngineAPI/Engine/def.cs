namespace ChessEngine
{
    public static class Defs
    {
        public enum Pieces
        {
            EMPTY = 0,
            wP = 1, wN = 2, wB = 3, wR = 4, wQ = 5, wK = 6,
            bp = 7, bN = 8, bB = 9, bR = 10, bQ = 11, bK = 12
        }
        //10 x 12 board representation instead of 8 x 8
        public const int BRD_SQ_NUM = 120;

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

        public enum Colours
        {
            WHITE = 0, BLACK = 1, BOTH = 2
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


        //function to get square index
        public static int getSquareIndex(int file, int rank)
        {
            return file + 21 + (rank * 10);
        }

    }    

}