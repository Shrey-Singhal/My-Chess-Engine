using System.Security.Cryptography.X509Certificates;

namespace ChessEngineAPI.Engine
{
    public static class SearchController
    {
        public static int Nodes { get; set; } //total nodes searched so far
        public static int Fh { get; set; } // fail high
        public static int Fhf { get; set; } // fail high first
        public static int Depth { get; set; } // max search depth
        public static int Time { get; set; } // time we're going to search for
        public static int Start { get; set; } // starting time for engine
        public static bool Stop { get; set; }
        public static int Best { get; set; } //best move found
        public static bool Thinking { get; set; } // flag to indicate if the engine is currently searching
    }

    public class Search(Gameboard board, Movegen movegen, MoveManager moveManager, PerfTesting perfTesting)
    {
        // alpha: the best score found so far for the maximizer (lower bound).
        // beta: the best score found so far for the minimizer (upper bound).
        // depth: how many plies (half-moves) deep we want to search.
        public int AlphaBeta(int alpha, int beta, int depth)
        {
            // base condition. once we've reached max depth, return an evaluation of the current position
            if (depth <= 0)
            {
                //return evaluate()

            }
            //check time up
            SearchController.Nodes++;
            // check Rep() fifty move rule

            //this is to prevent going beyond engine's max allowed depth
            if (board.ply > Defs.MAXDEPTH - 1)
            {
                //return evaluate()
            }

            int Score = -Defs.INFINITE; //stores current move's evaluation
            movegen.GenerateMoves(board);

            int MoveNum = 0;
            int Legal = 0; // counts legal moves (needed to detect checkmate/stalemate)
            int OldAlpha = alpha;
            int BestMove = MoveUtils.NO_MOVE;
            int Move = MoveUtils.NO_MOVE;

            // get PvMove
            // order PvMove

            //go through all moves generated for the current ply
            for (MoveNum = board.moveListStart[board.ply]; MoveNum < board.moveListStart[board.ply + 1]; ++MoveNum)
            {
                // Pick the next move

                Move = board.moveList[MoveNum];
                //MakeMove returns false when the move is illegal- especially when the move puts or leaves king in check.
                if (!moveManager.MakeMove(Move, board))
                {
                    continue;
                }
                Legal++; //icrement if the move is legal 
                Score = -AlphaBeta(-beta, -alpha, depth - 1); //negamax trick

                moveManager.TakeMove(); // this is to reset the board state to what it was at the root

                if (SearchController.Stop)
                {
                    return 0;
                }

                if (Score > alpha) //is the move better than all the moves seen so far?
                {
                    // this is for alpha beta pruning. 
                    // if we find a move that gives a score >= beta it means:
                    // “This move is so good, it violates the opponent’s limits. They’d never let me reach this position in a real game.”
                    // So, you prune - skip all remaining moves in this branch.

                    if (Score >= beta)
                    {
                        if (Legal == 1)
                        {
                            SearchController.Fhf++; //The first legal move caused the fail-high
                        }
                        SearchController.Fh++; //A fail high occurred.

                        // Update Killer Moves

                        return beta;
                    }

                    alpha = Score;
                    BestMove = Move;

                    // update history move
                }

            }
            // Mate check

            if (alpha != OldAlpha)
            {
                //store PvMove
            }

            return alpha;

        }

        public void SearchPosition()
        {
            int bestMove = MoveUtils.NO_MOVE;
            int bestScore = Defs.INFINITE;
            int currentDepth = 0;
            
            // starts a loop for iterative deepening search
            for (currentDepth = 1; currentDepth <= SearchController.Depth; ++currentDepth)
            {
                if (SearchController.Stop) //breaks early if search us inturrupted
                {
                    break;
                }
            }

            SearchController.Best = bestMove;
            SearchController.Thinking = false;

        }

    }
}