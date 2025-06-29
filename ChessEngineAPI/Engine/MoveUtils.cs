namespace ChessEngineAPI.Engine
{
    public static class MoveUtils
    {
        // Functions to extract move information
        public static int FromSquare(int move)
        {
            return move & 0x7F; // bits 0–6
        }

        public static int ToSquare(int move)
        {
            return (move >> 7) & 0x7F; // bits 7–13
        }

        public static int CapturedPiece(int move)
        {
            return (move >> 14) & 0xF; // bits 14–17
        }

        public static int PromotedPiece(int move)
        {
            return (move >> 20) & 0xF; // bits 20–23
        }

        public const int MFLAG_EN_PASSANT = 0x40000;   // Bit 18
        public const int MFLAG_PAWN_START = 0x80000;   // Bit 19
        public const int MFLAG_CASTLING = 0x1000000; // Bit 24

        // these 2 flags below are for fast filtering in the engine as they require no decoding like the functions.
        public const int MFLAG_CAPTURE = 0x7C000; // Bits 14–18 checks if any capture happened (normal or enpassant)
        public const int MFLAG_PROMOTE = 0xF00000;  // Bits 20–23

        public const int NO_MOVE = 0;
    }
}