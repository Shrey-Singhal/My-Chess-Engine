namespace ChessEngineAPI.Engine
{
    public class PerfTesting(Gameboard board)
    {
        public int perft_leafNodes = 0;
        readonly MoveManager moveManager = new(board);
        readonly Movegen movegen = new();

        // this function is mainly for counting how many legal position exist at a given depth from current board state.
        public void Perft(int depth)
        {
            // recursion base case
            if (depth == 0)
            {
                perft_leafNodes++;
                return;
            }
            movegen.GenerateMoves(board);

            int index, move;

            for (index = board.moveListStart[board.ply]; index < board.moveListStart[board.ply + 1]; ++index)
            {
                move = board.moveList[index];
                //MakeMove returns false when the move is illegal- especially when the move puts or leaves king in check.
                if (!moveManager.MakeMove(move, board))
                {
                    continue;
                }
                Perft(depth - 1);
                moveManager.TakeMove(); // undo the move after recursion returns, otherwise you'd be calculating from the wrong position.
            }
            return;
        }
        //this function runs perft() for each legal move from the current position.
        // prints how many leaf nodes each move leads to
        public void PerftTest(int depth)
        {
            board.PrintBoard();
            Console.WriteLine("Starting to test depth: " + depth);
            perft_leafNodes = 0;

            int index, move;
            int moveNum = 0;

            movegen.GenerateMoves(board);
            for (index = board.moveListStart[board.ply]; index < board.moveListStart[board.ply + 1]; ++index)
            {
                move = board.moveList[index];
                if (!moveManager.MakeMove(move, board))
                {
                    continue;
                }
                moveNum++;
                //cumNodes is used to track how many leaf nodes were added by the current top level node.
                int cumNodes = perft_leafNodes;
                Perft(depth - 1);
                moveManager.TakeMove();
                int oldnodes = perft_leafNodes - cumNodes;
                Console.WriteLine("move: " + moveNum + " " + movegen.PrintMove(move) + " " + oldnodes);

            }

            Console.WriteLine("Test Complete : " + perft_leafNodes + " leaf nodes visited.");

            return;
        }
    }
}