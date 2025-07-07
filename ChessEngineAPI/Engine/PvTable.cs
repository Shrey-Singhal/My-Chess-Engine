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
    }
}