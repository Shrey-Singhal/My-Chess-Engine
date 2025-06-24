using System.Security.Cryptography.X509Certificates;

namespace ChessEngine
{
    public class Gameboard
    {
        // represents chess board squares. each index shows what piece is on that square.
        public int[] pieces = new int[Defs.BRD_SQ_NUM];
        public Defs.Colours side; //which side to move next
        public int fiftyMove; // Number of half-moves since the last pawn move or capture.
        public int hisPly; // half moves played in the entire game

        // ply is just engine calculating a certain number of moves ahead at a particular 
        // position to find the best move
        public int ply;
        public Defs.CASTLEBIT castlePerm; // track which side can castle legally
        public int[] material = new int[2]; // total material value for black and white
        public int[] pceNum = new int[13];

        // pList is a piece list — it keeps track of where every piece is on the board, organized by piece type.
        // each piece type gets a block of 10 slots

        // there are 13 valid piece types (0-12) but we add one extra for defensive coding 
        // to avoid index overflow if someone makes a bug with piece IDs.

        // 10 means the max no of pieces per piece type (8 pawns, 2 knights), we take 10 bcz those 8 pawns
        // can be promoted and become a knight.
        public int[] pList = new int[14 * 10];

        public int enPas;

        // posKey is ONE single hash for the entire board position.
        // It’s like a fingerprint for the whole position, not for each square separately.
        // mainly used to detect a draw situation
        public ulong posKey;

        // This is the big table of random numbers for all piece-square combinations.
        // 14 = total no of piece types and 120 = total squares.
        public ulong[] PieceKeys = new ulong[14 * 120];

        // random no for each castling rights state. there are 16 possible combinations.
        public ulong[] CastleKeys = new ulong[16];

        // random number for who's turn it is.
        public ulong SideKey;


        public int[] moveList = new int[Defs.MAXDEPTH * Defs.MAXPOSITIONMOVES];
        public int[] moveScores = new int[Defs.MAXDEPTH * Defs.MAXPOSITIONMOVES];
        public int[] moveListStart = new int[Defs.MAXDEPTH];

        public Gameboard()
        {
            side = Defs.Colours.WHITE;
            fiftyMove = 0;
            hisPly = 0;
            ply = 0;
            castlePerm = 0;
            enPas = 0;
            posKey = 0;

        }
        // this gives you which square the 9th piece of x on
        public static int PCEINDEX(int pce, int pceNum)
        {
            return pce * 10 + pceNum;
        }
        // Now you have true 64-bit random numbers for each piece-square, castling, and side. 
        public void InitHashKeys()
        {
            for (int i = 0; i < 14 * 120; ++i)
            {
                PieceKeys[i] = Defs.Rand64();
            }

            SideKey = Defs.Rand64();

            for (int i = 0; i < 16; ++i)
            {
                CastleKeys[i] = Defs.Rand64();
            }
        }

        public ulong GeneratePosKey()
        {
            ulong finalKey = 0;

            for (int sq = 0; sq < Defs.BRD_SQ_NUM; ++sq)
            {
                int piece = pieces[sq];

                if (piece != (int)Defs.Pieces.EMPTY && piece != (int)Defs.Squares.OFFBOARD)
                {
                    finalKey ^= PieceKeys[(piece * 120) + sq];
                }
            }

            if (side == Defs.Colours.WHITE)
            {
                finalKey ^= SideKey;
            }

            if (enPas != Defs.Squares.NO_SQ)
            {
                finalKey ^= PieceKeys[enPas];
            }

            finalKey ^= CastleKeys[(int)castlePerm];

            return finalKey;
        }

        public void ResetBoard()
        {
            // 1. Set all 120 squares to OFFBOARD
            for (int i = 0; i < Defs.BRD_SQ_NUM; ++i)
            {
                pieces[i] = Defs.Squares.OFFBOARD;
            }

            // 2. Set real 64 squares to EMPTY
            for (int i = 0; i < 64; ++i)
            {
                pieces[Defs.SQ120(i)] = (int)Defs.Pieces.EMPTY;
            }

            // 3. Clear piece list
            for (int i = 0; i < 14 * 10; ++i)
            {
                pList[i] = (int)Defs.Pieces.EMPTY;
            }

            // 4. Reset material scores (white, black)
            for (int i = 0; i < 2; ++i)
            {
                material[i] = 0;
            }

            // 5. Reset piece counts
            for (int i = 0; i < 13; ++i)
            {
                pceNum[i] = 0;
            }

            // 6. Reset other board info
            side = Defs.Colours.BOTH;
            enPas = Defs.Squares.NO_SQ;
            fiftyMove = 0;
            ply = 0;
            hisPly = 0;
            castlePerm = 0;
            posKey = 0;

            // 7. Reset move list start for this ply
            moveListStart[ply] = 0;
        }
    }

    
}