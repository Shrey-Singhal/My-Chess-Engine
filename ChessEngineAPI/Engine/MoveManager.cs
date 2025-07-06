using System.Runtime.CompilerServices;
using System.Xml.XPath;
using Microsoft.AspNetCore.Components.Web;

namespace ChessEngineAPI.Engine
{
    public class MoveManager(Gameboard board)
    {
        public void ClearPiece(int sq)
        {
            int pce = board.pieces[sq]; // represents piece type
            int col = (int)PieceProperties.PieceCol[pce];
            int index;
            int temp_pceNum = -1;

            // Hash out or XOR out the effect of the piece from the posKey
            board.HashPiece(pce, sq);

            board.pieces[sq] = (int)Defs.Pieces.EMPTY;
            board.material[col] -= PieceProperties.PieceVal[pce];

            // u find out the index of where the piece that needs to be cleared (pce on sq) exists in the pList array
            for (index = 0; index < board.pceNum[pce]; ++index)
            {
                if (board.pList[Gameboard.PCEINDEX(pce, index)] == sq)
                {
                    temp_pceNum = index;
                    break;
                }
            }

            // if pcenum is 4 and temp_penum is 1 (2nd index), we set square at 2nd index (1) of the type of piece we are 
            // removing in plist to the square at 4th index (3) to essentially remove the piece from plist array.
            board.pceNum[pce]--;
            board.pList[Gameboard.PCEINDEX(pce, temp_pceNum)] = board.pList[Gameboard.PCEINDEX(pce, board.pceNum[pce])];

        }

        public void AddPiece(int sq, int pce, Gameboard board)
        {
            int col = (int)PieceProperties.PieceCol[pce];
            board.HashPiece(pce, sq);

            board.pieces[sq] = pce;
            board.material[col] += PieceProperties.PieceVal[pce];
            board.pList[Gameboard.PCEINDEX(pce, board.pceNum[pce])] = sq;
            board.pceNum[pce]++;
        }

        public void MovePiece(int from, int to, Gameboard board)
        {
            int index;
            int pce = board.pieces[from];

            board.HashPiece(pce, from);
            board.pieces[from] = (int)Defs.Pieces.EMPTY;

            board.HashPiece(pce, to);
            board.pieces[to] = pce;

            for (index = 0; index < board.pceNum[pce]; ++index)
            {
                if (board.pList[Gameboard.PCEINDEX(pce, index)] == from)
                {
                    board.pList[Gameboard.PCEINDEX(pce, index)] = to;
                    break;
                }
            }
        }

        public bool MakeMove(int move, Gameboard board)
        {
            int from = MoveUtils.FromSquare(move);
            int to = MoveUtils.ToSquare(move);
            int side = (int)board.side;

            board.history[board.hisPly].posKey = board.posKey;

            // example move: 0000 0100 0000 0000 1001 0110
            //MFLAG_EN_PASS: 0000 0100 0000 0000 0000 0000
            //  Result of &: 0000 0100 0000 0000 0000 0000
            // so if the result is non 0 then the move has en passant set
            if ((move & MoveUtils.MFLAG_EN_PASSANT) != 0)
            {
                // when en passant capture is made the pawn being captured is not on the destination square (to)
                // but directly behind the destination square thats why we do -10 for w and +10 for black.
                if (side == (int)Defs.Colours.WHITE)
                {
                    ClearPiece(to - 10);
                }
                else
                {
                    ClearPiece(to + 10);
                }
            }
            else if ((move & MoveUtils.MFLAG_CASTLING) != 0)
            {
                switch (to)
                {
                    case Defs.Squares.C1:
                        MovePiece(Defs.Squares.A1, Defs.Squares.D1, board);
                        break;
                    case Defs.Squares.C8:
                        MovePiece(Defs.Squares.A8, Defs.Squares.D8, board);
                        break;
                    case Defs.Squares.G1:
                        MovePiece(Defs.Squares.H1, Defs.Squares.F1, board);
                        break;
                    case Defs.Squares.G8:
                        MovePiece(Defs.Squares.H8, Defs.Squares.F8, board);
                        break;
                    default:
                        break;
                }
            }
            // if enpas square exists on the board we XOR out the enpas square fromt he poskey
            if (board.enPas != Defs.Squares.NO_SQ) board.HashEnPassant();
            board.HashCastle(); // xor out current castling permissions

            board.history[board.hisPly].move = move; // save the curr board to move histry
            board.history[board.hisPly].fiftyMove = board.fiftyMove;
            board.history[board.hisPly].enPas = board.enPas;
            board.history[board.hisPly].castlePerm = board.castlePerm;

            //remove rights based on which piece was moved (king or rook)
            board.castlePerm &= Defs.CastlePermArray[from];
            // remove rights if the move captured a rook on its original square
            board.castlePerm &= Defs.CastlePermArray[to];
            board.enPas = Defs.Squares.NO_SQ; //en passant becomes invalid after a move is made

            board.HashCastle(); // xor in the new castling rights

            int captured = MoveUtils.CapturedPiece(move);
            board.fiftyMove++;

            if (captured != (int)Defs.Pieces.EMPTY)
            {
                ClearPiece(to);
                board.fiftyMove = 0;
            }
            board.hisPly++;
            // enngine will also be calling the makemove function while evaluating the moves
            // and ply will help it to keep track of the depth
            board.ply++;

            if (PieceProperties.PiecePawn[board.pieces[from]])
            {
                board.fiftyMove = 0;
                if ((move & MoveUtils.MFLAG_PAWN_START) != 0)
                {
                    if (side == (int)Defs.Colours.WHITE)
                    {
                        // below the code sets the enpas square to the square behind 
                        // the pawn. so if white goes from e2-e4 and black has a pawn
                        // on d4, it can capture e3.
                        board.enPas = from + 10;
                    }
                    else
                    {
                        board.enPas = from - 10;
                    }
                    board.HashEnPassant();
                }
            }

            MovePiece(from, to, board);

            int prom_pce = MoveUtils.PromotedPiece(move);
            if (prom_pce != (int)Defs.Pieces.EMPTY)
            {
                ClearPiece(to); //clear pawn on to square
                AddPiece(to, prom_pce, board);
            }

            // side = side ^ 1. result is 1 if the bits are different, 0 if they're the same.
            board.side ^= (Defs.Colours)1;
            board.HashSide();

            // is the king in check after the move?
            if (board.SqAttacked(board.pList[Gameboard.PCEINDEX(Defs.Kings[side], 0)], (int)board.side) == Defs.Bool.TRUE)
            {
                TakeMove();
                return false; 

            }
            return true;


        }

        public void TakeMove()
        {
            board.hisPly--;
            board.ply--;

            int move = board.history[board.hisPly].move;
            int from = MoveUtils.FromSquare(move);
            int to = MoveUtils.ToSquare(move);

            if (board.enPas != Defs.Squares.NO_SQ) board.HashEnPassant();
            board.HashCastle();

            // board.history stores the current board state for each move. so basically now we restore the board to what it looked like before that move happened.
            board.castlePerm = board.history[board.hisPly].castlePerm;
            board.fiftyMove = board.history[board.hisPly].fiftyMove;
            board.enPas = board.history[board.hisPly].enPas;

            if (board.enPas != Defs.Squares.NO_SQ) board.HashEnPassant();
            board.HashCastle();

            board.side ^= (Defs.Colours)1;
            board.HashSide();

            //undo en passant
            if ((MoveUtils.MFLAG_EN_PASSANT & move) != 0)
            {
                if (board.side == (int)Defs.Colours.WHITE)
                {
                    AddPiece(to - 10, (int)Defs.Pieces.bp, board);
                }
                else
                {
                    AddPiece(to + 10, (int)Defs.Pieces.wP, board);
                }
            }
            //undo castling
            else if ((MoveUtils.MFLAG_CASTLING & move) != 0)
            {
                switch (to)
                {
                    case Defs.Squares.C1:
                        MovePiece(Defs.Squares.D1, Defs.Squares.A1, board);
                        break;
                    case Defs.Squares.C8:
                        MovePiece(Defs.Squares.D8, Defs.Squares.A8, board);
                        break;
                    case Defs.Squares.G1:
                        MovePiece(Defs.Squares.F1, Defs.Squares.H1, board);
                        break;
                    case Defs.Squares.G8:
                        MovePiece(Defs.Squares.F8, Defs.Squares.H8, board);
                        break;
                    default:
                        break;

                }
            }
            MovePiece(to, from, board);

            //restore captured piece
            int captured = MoveUtils.CapturedPiece(move);
            if (captured != (int)Defs.Pieces.EMPTY)
            {
                AddPiece(to, captured, board);
            }
            //undo promotion
            if (MoveUtils.PromotedPiece(move) != (int)Defs.Pieces.EMPTY)
            {
                ClearPiece(from);
                if (PieceProperties.PieceCol[MoveUtils.PromotedPiece(move)] == Defs.Colours.WHITE) {
                    AddPiece(from, (int)Defs.Pieces.wP, board);
                }
                else {
                    AddPiece(from, (int)Defs.Pieces.bp, board);
                }
            }

        }
    }
}