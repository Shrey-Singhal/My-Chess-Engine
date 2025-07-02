namespace ChessEngineAPI.Engine
{
    public class Movegen
    {
        public static int Move(int from, int to, int captured, int promoted, int flag)
        {
            return from | (to << 7) | (captured << 14) | (promoted << 20) | flag;
        }

        public void AddCaptureMove(int move, Gameboard board)
        {
            // store the move
            board.moveList[board.moveListStart[board.ply + 1]] = move;
            // not scoring the moves yet.
            board.moveScores[board.moveListStart[board.ply + 1]++] = 0;
        }

        public void AddQuietMove(int move, Gameboard board)
        {
            // store the move
            board.moveList[board.moveListStart[board.ply + 1]] = move;
            // not scoring the moves yet.
            board.moveScores[board.moveListStart[board.ply + 1]++] = 0;
        }

        public void AddEnPassantMove(int move, Gameboard board)
        {
            // store the move
            board.moveList[board.moveListStart[board.ply + 1]] = move;
            // not scoring the moves yet.
            board.moveScores[board.moveListStart[board.ply + 1]++] = 0;
        }

        public void AddWhitePawnCaptureMove(int from, int to, int cap, Gameboard board)
        {
            if (Defs.RanksBrd[from] == (int)Defs.Ranks.RANK_7)
            {
                AddCaptureMove(Move(from, to, cap, (int)Defs.Pieces.wQ, 0), board);
                AddCaptureMove(Move(from, to, cap, (int)Defs.Pieces.wR, 0), board);
                AddCaptureMove(Move(from, to, cap, (int)Defs.Pieces.wN, 0), board);
                AddCaptureMove(Move(from, to, cap, (int)Defs.Pieces.wB, 0), board);
            }
            else
            {
                AddCaptureMove(Move(from, to, cap, (int)Defs.Pieces.EMPTY, 0), board);

            }
        }

        public void AddBlackPawnCaptureMove(int from, int to, int cap, Gameboard board)
        {
            if (Defs.RanksBrd[from] == (int)Defs.Ranks.RANK_2)
            {
                AddCaptureMove(Move(from, to, cap, (int)Defs.Pieces.bQ, 0), board);
                AddCaptureMove(Move(from, to, cap, (int)Defs.Pieces.bR, 0), board);
                AddCaptureMove(Move(from, to, cap, (int)Defs.Pieces.bN, 0), board);
                AddCaptureMove(Move(from, to, cap, (int)Defs.Pieces.bB, 0), board);
            }
            else
            {
                AddCaptureMove(Move(from, to, cap, (int)Defs.Pieces.EMPTY, 0), board);

            }
        }

        public void AddWhitePawnQuietMove(int from, int to, Gameboard board)
        {
            if (Defs.RanksBrd[from] == (int)Defs.Ranks.RANK_7)
            {
                AddCaptureMove(Move(from, to, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.wQ, 0), board);
                AddCaptureMove(Move(from, to, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.wR, 0), board);
                AddCaptureMove(Move(from, to, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.wN, 0), board);
                AddCaptureMove(Move(from, to, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.wB, 0), board);
            }
            else
            {
                AddCaptureMove(Move(from, to, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, 0), board);

            }
        }

        public void AddBlackPawnQuietMove(int from, int to, Gameboard board)
        {
            if (Defs.RanksBrd[from] == (int)Defs.Ranks.RANK_2)
            {
                AddCaptureMove(Move(from, to, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.bQ, 0), board);
                AddCaptureMove(Move(from, to, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.bR, 0), board);
                AddCaptureMove(Move(from, to, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.bN, 0), board);
                AddCaptureMove(Move(from, to, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.bB, 0), board);
            }
            else
            {
                AddCaptureMove(Move(from, to, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, 0), board);

            }
        }



        public void GenerateMoves(Gameboard board)
        {
            // start writing the moves for next ply at the same index where the current ply ended up after move gen.
            // if ply = 0 then moveliststart[1] = 0 too bcz we initially set moveliststart[0] = 0
            // so essentially we are usijng moveliststart[ply] as read pointer nad moveliststart[1] as write pointer.
            // so when adding moves to movelist we access moveliststart[ply+1] as the write pointer which tells us where to write moves
            // and we keep incrementing moveliststart[ply+1] as we add moves.
            board.moveListStart[board.ply + 1] = board.moveListStart[board.ply];

            int pceType, pceNum, sq, pceIndex, pce, temp_sq, dir, index;

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
                        AddBlackPawnQuietMove(sq, sq + 10, board);
                        // we check if we're on the 2nd rank since white's pawn are initially on 2n rank
                        // and if the 2 squares at the front are empty we can play the double pawn move.
                        if (Defs.RanksBrd[sq] == (int)Defs.Ranks.RANK_2 && board.pieces[sq + 20] == (int)Defs.Pieces.EMPTY)
                        {
                            AddQuietMove(Move(sq, sq + 20, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, MoveUtils.MFLAG_PAWN_START), board);

                        }
                    }

                    if (Defs.SQOFFBOARD(sq + 9) == Defs.Bool.FALSE && PieceProperties.PieceCol[board.pieces[sq + 9]] == Defs.Colours.BLACK)
                    {
                        // add pawn capture move
                        AddWhitePawnCaptureMove(sq, sq + 9, board.pieces[sq + 9], board);
                    }
                    if (Defs.SQOFFBOARD(sq + 11) == Defs.Bool.FALSE && PieceProperties.PieceCol[board.pieces[sq + 11]] == Defs.Colours.BLACK)
                    {
                        // add pawn capture move
                        AddWhitePawnCaptureMove(sq, sq + 11, board.pieces[sq + 9], board);
                    }

                    // check if there's a valid en passant target square in the board.
                    if (board.enPas != Defs.Squares.NO_SQ)
                    {
                        if (sq + 9 == board.enPas)
                        {
                            //add enpas move
                            AddEnPassantMove(Move(sq, sq + 9, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, MoveUtils.MFLAG_EN_PASSANT), board);

                        }
                        if (sq + 11 == board.enPas)
                        {
                            // add enpas move
                            AddEnPassantMove(Move(sq, sq + 9, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, MoveUtils.MFLAG_EN_PASSANT), board);

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
                            AddQuietMove(Move(Defs.Squares.E1, Defs.Squares.G1, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, MoveUtils.MFLAG_CASTLING), board);
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
                            AddQuietMove(Move(Defs.Squares.E1, Defs.Squares.C1, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, MoveUtils.MFLAG_CASTLING), board);

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
                        AddBlackPawnQuietMove(sq, sq - 10, board);
                                               
                        if (Defs.RanksBrd[sq] == (int)Defs.Ranks.RANK_7 && board.pieces[sq - 20] == (int)Defs.Pieces.EMPTY)
                        {
                            AddQuietMove(Move(sq, sq - 20, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, MoveUtils.MFLAG_PAWN_START), board);
                        }
                    }

                    if (Defs.SQOFFBOARD(sq - 9) == Defs.Bool.FALSE && PieceProperties.PieceCol[board.pieces[sq - 9]] == Defs.Colours.WHITE)
                    {
                        // add pawn capture move
                        AddBlackPawnCaptureMove(sq, sq - 9, board.pieces[sq - 9], board);
                    }
                    if (Defs.SQOFFBOARD(sq - 11) == Defs.Bool.FALSE && PieceProperties.PieceCol[board.pieces[sq - 11]] == Defs.Colours.WHITE)
                    {
                        // add pawn capture move
                        AddBlackPawnCaptureMove(sq, sq - 11, board.pieces[sq - 11], board);
                    }

                    if (board.enPas != Defs.Squares.NO_SQ)
                    {
                        if (sq - 9 == board.enPas)
                        {
                            //add enpas move
                            AddEnPassantMove(Move(sq, sq - 9, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, MoveUtils.MFLAG_EN_PASSANT), board);

                        }
                        if (sq - 11 == board.enPas)
                        {
                            // add enpas move
                            AddEnPassantMove(Move(sq, sq - 11, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, MoveUtils.MFLAG_EN_PASSANT), board);
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
                            AddQuietMove(Move(Defs.Squares.E8, Defs.Squares.G8, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, MoveUtils.MFLAG_CASTLING), board);

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
                            AddQuietMove(Move(Defs.Squares.E8, Defs.Squares.C8, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, MoveUtils.MFLAG_CASTLING), board);
                        }
                    }
                }
            }
            // move gen logic for non sliding pieces
            pceIndex = Defs.LoopNonSlideIndex[(int)board.side];
            pce = Defs.LoopNonSlidePce[pceIndex++];

            while (pce != 0)
            {
                // visit all squares occupied by a parrticular piece
                for (pceNum = 0; pceNum < board.pceNum[pce]; ++pceNum)
                {
                    sq = board.pList[Gameboard.PCEINDEX(pce, pceNum)];

                    for (index = 0; index < Defs.DirNum[pce]; ++index)
                    {
                        dir = Defs.PceDir[pce][index];
                        temp_sq = sq + dir;

                        if (Defs.SQOFFBOARD(temp_sq) == Defs.Bool.TRUE)
                        {
                            continue;
                        }

                        if (board.pieces[temp_sq] != (int)Defs.Pieces.EMPTY)
                        {
                            if (PieceProperties.PieceCol[temp_sq] != board.side)
                            {
                                // add capture
                                AddCaptureMove(Move(sq, temp_sq, board.pieces[temp_sq], (int)Defs.Pieces.EMPTY, 0), board);
                            }
                            else
                            {
                                AddQuietMove(Move(sq, temp_sq, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, 0), board);
                            }
                        }
                    }
                }
                pce = Defs.LoopNonSlidePce[pceIndex++];
            }

            // move gen logic for sliding pieces
            pceIndex = Defs.LoopSlideIndex[(int)board.side];
            pce = Defs.LoopSlidePce[pceIndex++];

            while (pce != 0)
            {
                // visit all squares occupied by a parrticular piece
                for (pceNum = 0; pceNum < board.pceNum[pce]; ++pceNum)
                {
                    sq = board.pList[Gameboard.PCEINDEX(pce, pceNum)];

                    for (index = 0; index < Defs.DirNum[pce]; ++index)
                    {
                        dir = Defs.PceDir[pce][index];
                        temp_sq = sq + dir;

                        while (Defs.SQOFFBOARD(temp_sq) == Defs.Bool.FALSE)
                        {
                            if (board.pieces[temp_sq] != (int)Defs.Pieces.EMPTY)
                            {
                                if (PieceProperties.PieceCol[temp_sq] != board.side)
                                {
                                    // add capture
                                    AddCaptureMove(Move(sq, temp_sq, board.pieces[temp_sq], (int)Defs.Pieces.EMPTY, 0), board);
                                }
                                break;

                            }
                            // non capture move
                            AddQuietMove(Move(sq, temp_sq, (int)Defs.Pieces.EMPTY, (int)Defs.Pieces.EMPTY, 0), board);
                            temp_sq += dir;
                        }
                    }
                }
                pce = Defs.LoopSlidePce[pceIndex++];
            }
        }
    }
}