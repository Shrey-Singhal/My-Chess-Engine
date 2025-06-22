namespace ChessEngine
{
    // Each piece type has a unique ID, and these arrays store quick info (like value or flags) at 
    // the index matching that ID. So the engine can instantly look up piece properties without if-else checks.

    // 0 -> EMPTY or NO_PIECE. 1 → White Pawn (wP), 2 → White Knight (wN),  3 → White Bishop (wB),  4 → White Rook (wR)
    // 5 → White Queen (wQ),  6 → White King (wK),  7 → Black Pawn (bP),  8 → Black Knight (bN)
    // 9 → Black Bishop (bB),  10 → Black Rook (bR),  11 → Black Queen (bQ), 12 → Black King (bK)
    
    public static class PieceProperties
    {
        //	Is it a big piece (not a pawn)?
        public static readonly bool[] PieceBig = {
            false, false, true, true, true, true, true, false, true, true, true, true, true
        };

        //Is it a major piece (rook, queen)?
        public static readonly bool[] PieceMaj = {
            false, false, false, false, true, true, true, false, false, false, true, true, true
        };

        //	Is it a minor piece (bishop, knight)?
        public static readonly bool[] PieceMin = {
            false, false, true, true, false, false, false, false, true, true, false, false, false
        };

        //What is its material value?
        public static readonly int[] PieceVal = {
            0, 100, 325, 325, 550, 1000, 50000, 100, 325, 325, 550, 1000, 50000
        };

        //PieceCol tells you which side owns a given piece type.
        public static readonly Defs.Colours[] PieceCol = {
            Defs.Colours.BOTH, Defs.Colours.WHITE, Defs.Colours.WHITE, Defs.Colours.WHITE,
            Defs.Colours.WHITE, Defs.Colours.WHITE, Defs.Colours.WHITE,
            Defs.Colours.BLACK, Defs.Colours.BLACK, Defs.Colours.BLACK,
            Defs.Colours.BLACK, Defs.Colours.BLACK, Defs.Colours.BLACK
        };

        public static readonly bool[] PiecePawn = {
            false, true, false, false, false, false, false, true, false, false, false, false, false
        };

        public static readonly bool[] PieceKnight = {
            false, false, true, false, false, false, false, false, true, false, false, false, false
        };

        public static readonly bool[] PieceKing = {
            false, false, false, false, false, false, true, false, false, false, false, false, true
        };

        public static readonly bool[] PieceRookQueen = {
            false, false, false, false, true, true, false, false, false, false, true, true, false
        };

        public static readonly bool[] PieceBishopQueen = {
            false, false, false, true, false, true, false, false, false, true, false, true, false
        };
        // Does it slide (bishop, rook, queen)?
        public static readonly bool[] PieceSlides = {
            false, false, false, true, true, true, false, false, false, true, true, true, false
        };
    }

}