using Microsoft.AspNetCore.Components.Web;

namespace ChessEngineAPI.Engine
{
    public class MakeMove(Gameboard board)
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
    }
}