namespace ChessEngineAPI.Engine
{

    public class Evaluate
    {
        // positional evaluation table for pawns - each square on the board is given a value.
        // the values represent how favourable it is for a white pawn to stand on that square.
        public int[] PawnTable = [
            0   ,   0   ,   0   ,   0   ,   0   ,   0   ,   0   ,   0   ,
            10  ,   10  ,   0   ,   -10 ,   -10 ,   0   ,   10  ,   10  ,
            5   ,   0   ,   0   ,   5   ,   5   ,   0   ,   0   ,   5   ,
            0   ,   0   ,   10  ,   20  ,   20  ,   10  ,   0   ,   0   ,
            5   ,   5   ,   5   ,   10  ,   10  ,   5   ,   5   ,   5   ,
            10  ,   10  ,   10  ,   20  ,   20  ,   10  ,   10  ,   10  ,
            20  ,   20  ,   20  ,   30  ,   30  ,   20  ,   20  ,   20  ,
            0   ,   0   ,   0   ,   0   ,   0   ,   0   ,   0   ,   0
        ];

        public int[] KnightTable = [
            0   ,   -10 ,   0   ,   0   ,   0   ,   0   ,   -10 ,   0   ,
            0   ,   0   ,   0   ,   5   ,   5   ,   0   ,   0   ,   0   ,
            0   ,   0   ,   10  ,   10  ,   10  ,   10  ,   0   ,   0   ,
            0   ,   0   ,   10  ,   20  ,   20  ,   10  ,   5   ,   0   ,
            5   ,   10  ,   15  ,   20  ,   20  ,   15  ,   10  ,   5   ,
            5   ,   10  ,   10  ,   20  ,   20  ,   10  ,   10  ,   5   ,
            0   ,   0   ,   5   ,   10  ,   10  ,   5   ,   0   ,   0   ,
            0   ,   0   ,   0   ,   0   ,   0   ,   0   ,   0   ,   0
        ];

        public int[] BishopTable = [
            0   ,   0   ,   -10 ,   0   ,   0   ,   -10 ,   0   ,   0   ,
            0   ,   0   ,   0   ,   10  ,   10  ,   0   ,   0   ,   0   ,
            0   ,   0   ,   10  ,   15  ,   15  ,   10  ,   0   ,   0   ,
            0   ,   10  ,   15  ,   20  ,   20  ,   15  ,   10  ,   0   ,
            0   ,   10  ,   15  ,   20  ,   20  ,   15  ,   10  ,   0   ,
            0   ,   0   ,   10  ,   15  ,   15  ,   10  ,   0   ,   0   ,
            0   ,   0   ,   0   ,   10  ,   10  ,   0   ,   0   ,   0   ,
            0   ,   0   ,   0   ,   0   ,   0   ,   0   ,   0   ,   0
        ];

        public int[] RookTable = [
            0   ,   0   ,   5   ,   10  ,   10  ,   5   ,   0   ,   0   ,
            0   ,   0   ,   5   ,   10  ,   10  ,   5   ,   0   ,   0   ,
            0   ,   0   ,   5   ,   10  ,   10  ,   5   ,   0   ,   0   ,
            0   ,   0   ,   5   ,   10  ,   10  ,   5   ,   0   ,   0   ,
            0   ,   0   ,   5   ,   10  ,   10  ,   5   ,   0   ,   0   ,
            0   ,   0   ,   5   ,   10  ,   10  ,   5   ,   0   ,   0   ,
            25  ,   25  ,   25  ,   25  ,   25  ,   25  ,   25  ,   25  ,
            0   ,   0   ,   5   ,   10  ,   10  ,   5   ,   0   ,   0
        ];

        public const int BishopPair = 40; //bishop pair bonues - classic chess engine heuristic
        public int EvalPosition(Gameboard board)
        {
            int score = board.material[(int)Defs.Colours.WHITE] - board.material[(int)Defs.Colours.BLACK];

            int pce, sq, pceNum;

            // the code below goes through every piece type on the board for both white and black and looks up the square the pawn is on.
            // then gets a positional score from the pawn table and updates the score.
            pce = (int)Defs.Pieces.wP;
            for (pceNum = 0; pceNum < board.pceNum[pce]; ++pceNum)
            {
                sq = board.pList[Gameboard.PCEINDEX(pce, pceNum)];
                score += PawnTable[Defs.SQ64(sq)];
            }

            pce = (int)Defs.Pieces.bp;
            for (pceNum = 0; pceNum < board.pceNum[pce]; ++pceNum)
            {
                sq = board.pList[Gameboard.PCEINDEX(pce, pceNum)];
                score -= PawnTable[Defs.MIRROR64(Defs.SQ64(sq))];
            }
            
            pce = (int)Defs.Pieces.wN;	
            for(pceNum = 0; pceNum < board.pceNum[pce]; ++pceNum) {
                sq = board.pList[Gameboard.PCEINDEX(pce, pceNum)];
                score += KnightTable[Defs.SQ64(sq)];
            }	

            pce = (int)Defs.Pieces.bN;	
            for(pceNum = 0; pceNum < board.pceNum[pce]; ++pceNum) {
                sq = board.pList[Gameboard.PCEINDEX(pce, pceNum)];
                score -= KnightTable[Defs.MIRROR64(Defs.SQ64(sq))];
            }			
            
            pce = (int)Defs.Pieces.wB;	
            for(pceNum = 0; pceNum < board.pceNum[pce]; ++pceNum) {
                sq = board.pList[Gameboard.PCEINDEX(pce, pceNum)];
                score += BishopTable[Defs.SQ64(sq)];
            }	

            pce = (int)Defs.Pieces.bB;	
            for(pceNum = 0; pceNum < board.pceNum[pce]; ++pceNum) {
                sq = board.pList[Gameboard.PCEINDEX(pce, pceNum)];
                score -= BishopTable[Defs.MIRROR64(Defs.SQ64(sq))];
            }
            
            pce = (int)Defs.Pieces.wR;
            for(pceNum = 0; pceNum < board.pceNum[pce]; ++pceNum) {
                sq = board.pList[Gameboard.PCEINDEX(pce, pceNum)];
                score += RookTable[Defs.SQ64(sq)];
            }	

            pce = (int)Defs.Pieces.bR;	
            for(pceNum = 0; pceNum < board.pceNum[pce]; ++pceNum) {
                sq = board.pList[Gameboard.PCEINDEX(pce, pceNum)];
                score -= RookTable[Defs.MIRROR64(Defs.SQ64(sq))];
            }
            
            pce = (int)Defs.Pieces.wQ;	
            for(pceNum = 0; pceNum < board.pceNum[pce]; ++pceNum) {
                sq = board.pList[Gameboard.PCEINDEX(pce, pceNum)];
                score += RookTable[Defs.SQ64(sq)]/2;
            }	

            pce = (int)Defs.Pieces.bQ;	
            for(pceNum = 0; pceNum < board.pceNum[pce]; ++pceNum) {
                sq = board.pList[Gameboard.PCEINDEX(pce, pceNum)];
                score -= RookTable[Defs.MIRROR64(Defs.SQ64(sq))]/2;
            }	
            

            if (board.pceNum[(int)Defs.Pieces.wB] >= 2)
            {
                score += BishopPair;
            }
            if (board.pceNum[(int)Defs.Pieces.bB] >= 2) {
                score -= BishopPair;
            }

            if (board.side == (int)Defs.Colours.WHITE)
            {
                return score;
            }
            else
            {
                return -score;
            }

        }
    }
}