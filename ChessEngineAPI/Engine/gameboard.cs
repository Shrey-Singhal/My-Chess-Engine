using System.Security.Cryptography.X509Certificates;

namespace ChessEngineAPI.Engine
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

        //The board (pieces[120]) holds what piece is at each square.
        //pList tells you where each piece is. It’s a reverse mapping.

        // Quickly loop over all white rooks: piece = 4, check pceNum[4] for count, look at pList[40..49].
        // Useful for move generation, evaluation, etc.
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
        // gives a unique index in pList based on the piece type and how many of that piece have been seen so far.
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
            // Set all 120 squares to OFFBOARD
            for (int i = 0; i < Defs.BRD_SQ_NUM; ++i)
            {
                pieces[i] = Defs.Squares.OFFBOARD;
            }

            // Set real 64 squares to EMPTY
            for (int i = 0; i < 64; ++i)
            {
                pieces[Defs.SQ120(i)] = (int)Defs.Pieces.EMPTY;
            }

            // Reset other board info
            side = Defs.Colours.BOTH;
            enPas = Defs.Squares.NO_SQ;
            fiftyMove = 0;
            ply = 0;
            hisPly = 0;
            castlePerm = 0;
            posKey = 0;

            // Reset move list start for this ply
            moveListStart[ply] = 0;
        }

        public void ParseFEN(string fen)

        // example FEN - rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
        {
            ResetBoard();

            int rank = (int)Defs.Ranks.RANK_8;
            int file = (int)Defs.Files.FILE_A;
            int piece;
            int count; // this is for handling digits in the fen string that represent empyty squares
            int sq120;
            int fenCnt = 0; // pointer for the FEN string

            while (rank >= (int)Defs.Ranks.RANK_1 && fenCnt < fen.Length)
            {
                count = 1;

                switch (fen[fenCnt])
                {
                    case 'p': piece = (int)Defs.Pieces.bp; break;
                    case 'r': piece = (int)Defs.Pieces.bR; break;
                    case 'n': piece = (int)Defs.Pieces.bN; break;
                    case 'b': piece = (int)Defs.Pieces.bB; break;
                    case 'k': piece = (int)Defs.Pieces.bK; break;
                    case 'q': piece = (int)Defs.Pieces.bQ; break;
                    case 'P': piece = (int)Defs.Pieces.wP; break;
                    case 'R': piece = (int)Defs.Pieces.wR; break;
                    case 'N': piece = (int)Defs.Pieces.wN; break;
                    case 'B': piece = (int)Defs.Pieces.wB; break;
                    case 'K': piece = (int)Defs.Pieces.wK; break;
                    case 'Q': piece = (int)Defs.Pieces.wQ; break;

                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                        piece = (int)Defs.Pieces.EMPTY;
                        // converting digit char to no.
                        // ASCII value of 0 is 48. if the fen[fenCnt] was 3 then ASCII val would be 51
                        // so 51 - 48 would be 3. Hence, this is how you get convert char to no.
                        count = fen[fenCnt] - '0';
                        break;

                    case '/':
                    case ' ':
                        rank--;
                        file = (int)Defs.Files.FILE_A;
                        fenCnt++;
                        continue;

                    default:
                        throw new Exception("FEN error");
                }

                for (int i = 0; i < count; i++)
                {
                    sq120 = Defs.GetSquareIndex(file, rank);
                    pieces[sq120] = piece;
                    file++;
                }
                fenCnt++;
            }

            // done with the piece placement. read side to move. 
            side = (fen[fenCnt] == 'w') ? Defs.Colours.WHITE : Defs.Colours.BLACK;
            fenCnt += 2;

            // read castling rights
            for (int i = 0; i < 4; i++)
            {
                if (fen[fenCnt] == ' ')
                    break;

                switch (fen[fenCnt])
                // |= is bitwise OR + assignment
                // bcz enum is just an int under the hood you can do bitwise operations on it
                {
                    case 'K': castlePerm |= Defs.CASTLEBIT.WKCA; break;
                    case 'Q': castlePerm |= Defs.CASTLEBIT.WQCA; break;
                    case 'k': castlePerm |= Defs.CASTLEBIT.BKCA; break;
                    case 'q': castlePerm |= Defs.CASTLEBIT.BQCA; break;
                }
                fenCnt++;
            }
            fenCnt++;

            // read en passant square
            if (fen[fenCnt] != '-')
            {
                file = fen[fenCnt] - 'a';
                rank = fen[fenCnt + 1] - '1';
                enPas = Defs.GetSquareIndex(file, rank);
            }

            posKey = GeneratePosKey();  // build new zorbist hash for the board.
            UpdateListsMaterial();
        }

        public void PrintBoard()
        {
            int sq, file, rank, piece;

            Console.WriteLine("\nGame Board:\n");
            for (rank = (int)Defs.Ranks.RANK_8; rank >= (int)Defs.Ranks.RANK_1; rank--)
            {
                string line = Defs.RankChar[rank] + "  ";

                for (file = (int)Defs.Files.FILE_A; file <= (int)Defs.Files.FILE_H; file++)
                {
                    sq = Defs.GetSquareIndex(file, rank);
                    piece = pieces[sq];
                    line += Defs.PceChar[piece] + " ";
                }
                Console.WriteLine(line);
            }

            Console.WriteLine("");
            string footer = "   ";

            for (file = (int)Defs.Files.FILE_A; file <= (int)Defs.Files.FILE_H; file++)
            {
                footer += Defs.FileChar[file] + " ";
            }
            Console.WriteLine(footer);

            Console.WriteLine("side: " + Defs.SideChar[(int)side]);
            Console.WriteLine("enPas: " + enPas);

            string castleRights = "";
            if ((castlePerm & Defs.CASTLEBIT.WKCA) != 0) castleRights += "K";
            if ((castlePerm & Defs.CASTLEBIT.WQCA) != 0) castleRights += "Q";
            if ((castlePerm & Defs.CASTLEBIT.BKCA) != 0) castleRights += "k";
            if ((castlePerm & Defs.CASTLEBIT.BQCA) != 0) castleRights += "q";

            Console.WriteLine("castle: " + castleRights);
            Console.WriteLine("key: " + posKey.ToString("X")); // hexadecimal
        }

        public void UpdateListsMaterial()
        {
            int piece, sq, index, color;

            // Clear piece list
            for (int i = 0; i < 14 * 10; ++i)
            {
                pList[i] = (int)Defs.Pieces.EMPTY;
            }

            // Reset material scores (white, black)
            for (int i = 0; i < 2; ++i)
            {
                material[i] = 0;
            }

            // Reset piece counts
            for (int i = 0; i < 13; ++i)
            {
                pceNum[i] = 0;
            }

            for (index = 0; index < 64; ++index)
            {
                sq = Defs.SQ120(index);
                piece = pieces[sq];
                if (piece != (int)Defs.Pieces.EMPTY)
                {
                    color = (int)PieceProperties.PieceCol[piece];
                    material[color] += PieceProperties.PieceVal[piece];

                    // Example: pList[PCEINDEX(2, 0)] = 57
                    // This means:

                    // 1st white knight (piece = 2, num = 0) is on square 57.

                    // PCEINDEX(2, 0) = 2 * 10 + 0 = 20

                    // So pList[20] = 57                        
                    pList[PCEINDEX(piece, pceNum[piece])] = sq;

                    pceNum[piece]++;

                    Console.WriteLine("piece: " + piece + " Square: " + sq);
                }

            }
        }
        // this function is asking - "If I place a king on this square, is it in danger from any enemy piece?"
        // its not just for king but all pieces.
        public int SqAttacked(int sq, int side)
        {
            int pce, temp_sq, index;

            // pawn attacks
            if (side == (int)Defs.Colours.WHITE)
            {
                if (pieces[sq - 11] == (int)Defs.Pieces.wP || pieces[sq - 9] == (int)Defs.Pieces.wP)
                {
                    return Defs.Bool.TRUE;
                }
            }
            else
            {
                if (pieces[sq + 11] == (int)Defs.Pieces.bp || pieces[sq + 9] == (int)Defs.Pieces.bp)
                {
                    return Defs.Bool.TRUE;
                }
            }
            // knight attacks
            for (index = 0; index < 8; index++)
            {
                pce = pieces[sq + Defs.KnDir[index]];
                if (pce != Defs.Squares.OFFBOARD && (int)PieceProperties.PieceCol[pce] == side
                    && PieceProperties.PieceKnight[pce])
                {
                    return Defs.Bool.TRUE;
                }
            }
            // sliding pieces
            for (index = 0; index < 4; index++)
            {
                int dir = Defs.RkDir[index];
                temp_sq = sq + dir;
                pce = pieces[temp_sq];
                while (pce != Defs.Squares.OFFBOARD)
                {
                    if (pce != (int)Defs.Pieces.EMPTY)
                    {
                        if (PieceProperties.PieceRookQueen[pce] && (int)PieceProperties.PieceCol[pce] == side)
                        {
                            return Defs.Bool.TRUE;
                        }
                        break;
                    }
                    temp_sq += dir;
                    pce = pieces[temp_sq];
                }                
            }
            // diagonal sliding pieces
            for (index = 0; index < 4; index++)
            {
                int dir = Defs.BiDir[index];
                temp_sq = sq + dir;
                pce = pieces[temp_sq];

                while (pce != Defs.Squares.OFFBOARD)
                {
                    if (pce != (int)Defs.Pieces.EMPTY)
                    {
                        if (PieceProperties.PieceBishopQueen[pce] && (int)PieceProperties.PieceCol[pce] == side)
                        {
                            return Defs.Bool.TRUE;
                        }
                        break;
                    }
                    temp_sq += dir;
                    pce = pieces[temp_sq];
                }
            }
            // kings attack
            for (index = 0; index < 8; index++)
            {
                pce = pieces[Defs.KiDir[index] + sq];

                if (pce != Defs.Squares.OFFBOARD &&
                    PieceProperties.PieceKing[pce] && (int)PieceProperties.PieceCol[pce] == side)
                {
                    return Defs.Bool.TRUE;
                } 
            }

            return Defs.Bool.FALSE;
        }

    }

    
}