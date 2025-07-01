namespace ChessEngineAPI.Engine
{
    public class Movegen
    {
        public static int Move(int from, int to, int captured, int promoted, int flag)
        {
            return from | (to << 7) | (captured << 14) | (promoted << 20) | flag;
        }

        public void GenerateMoves(Gameboard board)
        {
            board.moveListStart[board.ply + 1] = board.moveListStart[board.ply];

            int pceType, pceNum, sq;

            if (board.side == Defs.Colours.WHITE)
            {
                pceType = (int)Defs.Pieces.wP;

                for (pceNum = 0; pceNum < board.pceNum[pceType]; ++pceNum)
                {
                    sq = board.pList[Gameboard.PCEINDEX(pceType, pceNum)];
                    // if square at the front is empty
                    if (board.pieces[sq + 10] == (int)Defs.Pieces.EMPTY)
                    {
                        // add pawn move
                        // we check if we're on the 2nd rank since white's pawn are initially on 2n rank
                        // and if the 2 squares at the front are empty we can play the double pawn move.
                        if (Defs.RanksBrd[sq] == (int)Defs.Ranks.RANK_2 && board.pieces[sq + 20] == (int)Defs.Pieces.EMPTY)
                        {

                        }
                    }

                    if (Defs.SQOFFBOARD(sq + 9) == Defs.Bool.FALSE && PieceProperties.PieceCol[board.pieces[sq + 9]] == Defs.Colours.BLACK)
                    {
                        // add pawn capture move
                    }
                    if (Defs.SQOFFBOARD(sq + 11) == Defs.Bool.FALSE && PieceProperties.PieceCol[board.pieces[sq + 11]] == Defs.Colours.BLACK)
                    {
                        // add pawn capture move
                    }

                    // check if there's a valid en passant target square in the board.
                    if (board.enPas != Defs.Squares.NO_SQ)
                    {
                        if (sq + 9 == board.enPas)
                        {
                            //add enpas move
                        }
                        if (sq + 11 == board.enPas)
                        {
                            // add enpas move
                        }
                    }
                }

                if ((board.castlePerm & Defs.CASTLEBIT.WKCA) != 0)
                {
                    if (board.pieces[Defs.Squares.F1] == (int)Defs.Pieces.EMPTY && board.pieces[Defs.Squares.G1] == (int)Defs.Pieces.EMPTY)
                    {
                        if (board.SqAttacked(Defs.Squares.F1, (int)Defs.Colours.BLACK) == Defs.Bool.FALSE
                                && board.SqAttacked(Defs.Squares.E1, (int)Defs.Colours.BLACK) == Defs.Bool.FALSE)
                        {

                        }
                    }
                }

                if ((board.castlePerm & Defs.CASTLEBIT.WQCA) != 0)
                {
                    if (board.pieces[Defs.Squares.D1] == (int)Defs.Pieces.EMPTY && board.pieces[Defs.Squares.C1] == (int)Defs.Pieces.EMPTY
                            && board.pieces[Defs.Squares.B1] == (int)Defs.Pieces.EMPTY)
                    {
                        if (board.SqAttacked(Defs.Squares.D1, (int)Defs.Colours.BLACK) == Defs.Bool.FALSE
                                && board.SqAttacked(Defs.Squares.E1, (int)Defs.Colours.BLACK) == Defs.Bool.FALSE)
                        {

                        }
                    }
                }



            }
            else
            {
                pceType = (int)Defs.Pieces.bp;

                for (pceNum = 0; pceNum < board.pceNum[pceType]; ++pceNum)
                {
                    sq = board.pList[Gameboard.PCEINDEX(pceType, pceNum)];
                    // if square at the front is empty
                    if (board.pieces[sq - 10] == (int)Defs.Pieces.EMPTY)
                    {
                        // add pawn move                        
                        if (Defs.RanksBrd[sq] == (int)Defs.Ranks.RANK_7 && board.pieces[sq - 20] == (int)Defs.Pieces.EMPTY)
                        {

                        }
                    }

                    if (Defs.SQOFFBOARD(sq - 9) == Defs.Bool.FALSE && PieceProperties.PieceCol[board.pieces[sq - 9]] == Defs.Colours.WHITE)
                    {
                        // add pawn capture move
                    }
                    if (Defs.SQOFFBOARD(sq - 11) == Defs.Bool.FALSE && PieceProperties.PieceCol[board.pieces[sq - 11]] == Defs.Colours.WHITE)
                    {
                        // add pawn capture move
                    }

                    if (board.enPas != Defs.Squares.NO_SQ)
                    {
                        if (sq - 9 == board.enPas)
                        {
                            //add enpas move
                        }
                        if (sq - 11 == board.enPas)
                        {
                            // add enpas move
                        }
                    }
                }
                
                if ((board.castlePerm & Defs.CASTLEBIT.BKCA) != 0)
                {
                    if (board.pieces[Defs.Squares.F8] == (int)Defs.Pieces.EMPTY && board.pieces[Defs.Squares.G8] == (int)Defs.Pieces.EMPTY)
                    {
                        if (board.SqAttacked(Defs.Squares.F8, (int)Defs.Colours.WHITE) == Defs.Bool.FALSE
                                && board.SqAttacked(Defs.Squares.E8, (int)Defs.Colours.WHITE) == Defs.Bool.FALSE)
                        {

                        }
                    }
                }

                if ((board.castlePerm & Defs.CASTLEBIT.BQCA) != 0)
                {
                    if (board.pieces[Defs.Squares.D8] == (int)Defs.Pieces.EMPTY && board.pieces[Defs.Squares.C8] == (int)Defs.Pieces.EMPTY
                            && board.pieces[Defs.Squares.B8] == (int)Defs.Pieces.EMPTY)
                    {
                        if (board.SqAttacked(Defs.Squares.D8, (int)Defs.Colours.WHITE) == Defs.Bool.FALSE
                                && board.SqAttacked(Defs.Squares.E8, (int)Defs.Colours.WHITE) == Defs.Bool.FALSE)
                        {

                        }
                    }
                }
            }
        }
    }
}