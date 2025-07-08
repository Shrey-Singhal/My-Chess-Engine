namespace ChessEngineAPI.Engine
{
    public static class PvTable
    {
        // looks up the best move for current position in PvTable
        public static int ProbePvTable(Gameboard board)
        {
            int index = (int)(board.posKey % (ulong)Defs.PVENTRIES);
            //checks if the stored key at that index matches the current position
            // this is imp bcz different positions can land at the same index
            if (board.PvTable[index].posKey == board.posKey)
            {
                return board.PvTable[index].move; //return best move
            }

            return MoveUtils.NO_MOVE;
        }
        //this function stores the best move found for that position
        public static void StorePvMove(Gameboard board, int move)
        {
            // this creates a number between 0 and pventries-1
            int index = (int)(board.posKey % (ulong)Defs.PVENTRIES);

            board.PvTable[index].posKey = board.posKey;
            board.PvTable[index].move = move;
        }
        //this function rebuilds and returns the best line of play that engine has found upto a given depth
        public static int GetPvLine(Gameboard board, int depth, Movegen movegen, MoveManager moveManager)
        {
            int move = PvTable.ProbePvTable(board); // Get the best move from the PV table
            int count = 0;

            // loop until the desired depth is reached or the table doesnt return a move
            while (move != MoveUtils.NO_MOVE && count < depth)
            {
                //check if the move is legal in the current position
                if (movegen.MoveExists(board, moveManager, move))
                {
                    moveManager.MakeMove(move, board); // Make the move to progress the PV line
                    board.PvArray[count++] = move;     // Store the move in the PV array
                }
                else
                {
                    break; // Exit if the move is not legal
                }

                move = PvTable.ProbePvTable(board); // Probe the next PV move for the updated board state
            }

            // Take back all moves to return board to original state
            while (board.ply > 0)
            {
                moveManager.TakeMove();
            }

            return count; // Return the number of moves in the PV line
        }
        
    }
}