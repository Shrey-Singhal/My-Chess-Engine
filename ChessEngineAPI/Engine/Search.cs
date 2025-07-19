using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Routing.Constraints;

namespace ChessEngineAPI.Engine
{
    public static class SearchController
    {
        public static int Nodes { get; set; } //total nodes searched so far
        public static int Fh { get; set; } // fail high
        public static int Fhf { get; set; } // fail high first
        public static int Depth { get; set; } // max search depth
        public static int Time { get; set; } // time we're going to search for
        public static DateTime Start { get; set; } // starting time for engine
        public static bool Stop { get; set; }
        public static int Best { get; set; } //best move found
        public static bool Thinking { get; set; } // flag to indicate if the engine is currently searching

        public static int LastScore { get; set; }
        public static int LastDepth { get; set; }
    }



    public class Search(Gameboard board, Movegen movegen, MoveManager moveManager)
    {
        // good move ordering = fewer nodes searched. AlphaBeta will cut off large portions of the tree
        // If the best moves (e.g., captures, checks, killer moves) are searched first
        public void PickNextMove(int MoveNum)
        {
            int index = 0;
            int bestScore = -1;
            int bestNum = MoveNum;

            for (index = MoveNum; index < board.moveListStart[board.ply + 1]; ++index)
            {
                if (board.moveScores[index] > bestScore)
                {
                    bestScore = board.moveScores[index];
                    bestNum = index;
                }
            }
            //swap best scoring move into the current index so that it gets picked first during the next alphabeta call
            if (bestNum != MoveNum)
            {
                int temp;
                temp = board.moveScores[MoveNum];
                board.moveScores[MoveNum] = board.moveScores[bestNum];
                board.moveScores[bestNum] = temp;

                temp = board.moveList[MoveNum];
                board.moveList[MoveNum] = board.moveList[bestNum];
                board.moveList[bestNum] = temp;
            }
        }
        //clear pvtable
        public void ClearPvTable()
        {
            for (int index = 0; index < Defs.PVENTRIES; index++)
            {
                board.PvTable[index].move = MoveUtils.NO_MOVE;
                board.PvTable[index].posKey = 0;
            }
        }

        //this function checks whether the engine has used too much time during its search and if it has, it has to stop searching immediately
        public static void CheckUp()
        {
            if ((DateTime.Now - SearchController.Start).TotalMilliseconds > SearchController.Time)
            {
                SearchController.Stop = true;
            }
        }

        public bool IsRepitition()
        {
            int index;
            // we do board.hisply - board.fiftymove because we are trying to see if the current position has occurred since the last pawn move or capture.
            // also bcz board.hisply is the current position, we loop up to board.hisply-1 to check past positions
            for (index = board.hisPly - board.fiftyMove; index < board.hisPly - 1; ++index)
            {
                // if we find the same position hash then its a repitition
                if (board.posKey == board.history[index].posKey)
                {
                    return true;
                }
            }
            return false;
        }
        //Quiescence is special extension of the normal search (alpha beta) that only explores noisy positions like captures, checks, and promotions after the normal search depth is reached.
        public int Quiescense(int alpha, int beta)
        {

            if ((SearchController.Nodes & 2047) == 0)
            {
                CheckUp();
            }

            SearchController.Nodes++;

            if ((IsRepitition() || board.fiftyMove >= 100) && board.ply != 0)
            {
                return 0;
            }

            if (board.ply > Defs.MAXDEPTH - 1)
            {
                return Evaluate.EvalPosition(board);
            }

            int Score = Evaluate.EvalPosition(board);

            //if score exceeds beta, prune. if it improves alpha, store it.
            if (Score >= beta)
            {
                return beta;
            }

            if (Score > alpha)
            {
                alpha = Score;
            }

            movegen.GenerateCaptures(board);


            int MoveNum;
            int Legal = 0; // counts legal moves (needed to detect checkmate/stalemate)
            int OldAlpha = alpha;
            int BestMove = MoveUtils.NO_MOVE;
            int Move;

            // get PvMove
            // order PvMove

            //go through all moves generated for the current ply
            for (MoveNum = board.moveListStart[board.ply]; MoveNum < board.moveListStart[board.ply + 1]; ++MoveNum)
            {
                // Pick the next move
                PickNextMove(MoveNum);

                Move = board.moveList[MoveNum];
                //MakeMove returns false when the move is illegal- especially when the move puts or leaves king in check.
                if (!moveManager.MakeMove(Move, board))
                {
                    continue;
                }
                Legal++; //icrement if the move is legal 
                Score = -Quiescense(-beta, -alpha); //negamax trick

                moveManager.TakeMove(); // this is to reset the board state to what it was at the root

                if (SearchController.Stop)
                {
                    return 0;
                }

                if (Score > alpha)
                {
                    if (Score >= beta)
                    {
                        if (Legal == 1)
                        {
                            SearchController.Fhf++; //The first legal move caused the fail-high
                        }
                        SearchController.Fh++; //A fail high occurred.
                        return beta;
                    }

                    alpha = Score;
                    BestMove = Move;
                }

            }
            if (alpha != OldAlpha)
            {
                PvTable.StorePvMove(board, BestMove);
            }

            return alpha;
        }

        // alpha: the best score found so far for the maximizer (lower bound).
        // beta: the best score found so far for the minimizer (upper bound).
        // depth: how many plies (half-moves) deep we want to search.
        public int AlphaBeta(int alpha, int beta, int depth)
        {
            // base condition. once we've reached max depth, return an evaluation of the current position
            if (depth <= 0)
            {
                return Quiescense(alpha, beta);

            }
            // call check up every 2048 nodes.
            if ((SearchController.Nodes & 2047) == 0)
            {
                CheckUp();
            }

            SearchController.Nodes++;

            // this checks for 2 draw conditions:
            // isrepitition (if the current position has repeated) and if 50 fullmoves passed with no pawn move or capture
            // board.ply != 0 ensures we’re not at the root node, to avoid false triggers
            if ((IsRepitition() || board.fiftyMove >= 100) && board.ply != 0)
            {
                return 0;
            }

            //this is to prevent going beyond engine's max allowed depth
            if (board.ply > Defs.MAXDEPTH - 1)
            {
                return Evaluate.EvalPosition(board);
            }

            //check if the king is in check
            int InCheck = board.SqAttacked(board.pList[Gameboard.PCEINDEX(Defs.Kings[(int)board.side], 0)], (int)board.side ^ 1);

            // add one extra depth to the search if the king is in check.
            // this is a common trick in chess engines to search deeper in critical positions (like when you're in check).
            if (InCheck == Defs.Bool.TRUE)
            {
                depth++;
            }

            int Score = -Defs.INFINITE; //stores current move's evaluation
            movegen.GenerateMoves(board);


            int MoveNum;
            int Legal = 0; // counts legal moves (needed to detect checkmate/stalemate)
            int OldAlpha = alpha;
            int BestMove = MoveUtils.NO_MOVE;
            int Move;
            int PvMove = PvTable.ProbePvTable(board);

            if (PvMove != MoveUtils.NO_MOVE)
            {
                for (MoveNum = board.moveListStart[board.ply]; MoveNum < board.moveListStart[board.ply + 1]; ++MoveNum)
                {
                    if (board.moveList[MoveNum] == PvMove)
                    {
                        board.moveScores[MoveNum] = 2000000;
                        break;
                    }
                }
            }

            // get PvMove
            // order PvMove

            //go through all moves generated for the current ply
            for (MoveNum = board.moveListStart[board.ply]; MoveNum < board.moveListStart[board.ply + 1]; ++MoveNum)
            {
                // Pick the next move
                PickNextMove(MoveNum);

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
                        if ((Move & MoveUtils.MFLAG_CAPTURE) == 0)
                        {
                            board.searchKillers[Defs.MAXDEPTH + board.ply] = board.searchKillers[board.ply];
                            board.searchKillers[board.ply] = Move;
                        }
                        return beta;
                    }
                    if ((Move & MoveUtils.MFLAG_CAPTURE) == 0)
                    {
                        board.searchHistory[board.pieces[MoveUtils.FromSquare(Move)] * Defs.BRD_SQ_NUM + MoveUtils.ToSquare(Move)]
                                += depth * depth;
                    }
                    alpha = Score;
                    BestMove = Move;

                    // update history move
                }

            }



            // check if the player has no legal moves left - could be checkmate or stalemate

            if (Legal == 0)
            {
                if (InCheck == Defs.Bool.TRUE)
                {
                    //lets assume ply=4 (we're 4 half moves deep)
                    //return -28996 (29000-4) this means checkmate in 4 half moves (2 full moves)
                    // essentially it represents how close a mate is and its negative cz its a loss
                    // the engine uses this to prefer faster mates and avoid slower losses
                    return -Defs.MATE + board.ply;
                }
                else
                {
                    return 0; // stalemate so return draw
                }
            }

            if (alpha != OldAlpha)
            {
                PvTable.StorePvMove(board, BestMove);
            }

            return alpha;

        }
        // this function resets the search state before starting a new search.
        // we need to reset bcz the information stored before was based on previous board state
        public void ClearForSearch()
        {
            int index, index2;
            for (index = 0; index < 14 * Defs.BRD_SQ_NUM; ++index)
            {
                board.searchHistory[index] = 0;
            }

            for (index2 = 0; index2 < 3 * Defs.MAXDEPTH; ++index2)
            {
                board.searchKillers[index2] = 0;
            }

            ClearPvTable();

            board.ply = 0;
            SearchController.Nodes = 0;
            SearchController.Fh = 0;
            SearchController.Fhf = 0;
            SearchController.Start = DateTime.Now;
            SearchController.Stop = false;
        }


        public void SearchPosition()
        {
            int bestMove = MoveUtils.NO_MOVE;
            int bestScore = -Defs.INFINITE;
            int currentDepth;
            string line;
            int pvNum;

            ClearForSearch();
            SearchController.Time = 10000; // 10 seconds

            // starts a loop for iterative deepening search
            for (currentDepth = 1; currentDepth <= SearchController.Depth; ++currentDepth)
            {
                bestScore = AlphaBeta(-Defs.INFINITE, Defs.INFINITE, currentDepth);
                if (SearchController.Stop) //breaks early if search us inturrupted
                {
                    break;
                }

                bestMove = PvTable.ProbePvTable(board);
                line = "D:" + currentDepth + " Best: " + movegen.PrintMove(bestMove) + " Score: " + bestScore
                            + " nodes: " + SearchController.Nodes;

                pvNum = PvTable.GetPvLine(board, currentDepth, movegen, moveManager);
                line += " Pv:";

                for (int c = 0; c < pvNum; ++c)
                {
                    line += " " + movegen.PrintMove(board.PvArray[c]);
                }

                if (currentDepth != 1)
                {
                    line += " Ordering: " + Math.Round((double)SearchController.Fhf / SearchController.Fh * 100, 2) + "%";
                }
                Console.WriteLine(line);
            }


            SearchController.Best = bestMove;
            SearchController.Thinking = false;

            SearchController.LastScore = bestScore;
            SearchController.LastDepth = currentDepth - 1; // currentDepth overshoots by 1 after loop

        }
        public object GetSearchStats()
        {
            double ordering = SearchController.Fh > 0 ? SearchController.Fhf / (double)SearchController.Fh * 100 : 0;
            double elapsedTime = (DateTime.Now - SearchController.Start).TotalSeconds;
            string bestMoveStr = movegen.PrintMove(SearchController.Best);

            int MATE = 1000000;
            int MAXDEPTH = 64;

            string scoreText;
            int score = SearchController.LastScore;    // <-- Use this now
            int depth = SearchController.LastDepth;    // <-- And this

            if (Math.Abs(score) > MATE - MAXDEPTH)
            {
                scoreText = $"Score: Mate In {MATE - Math.Abs(score) - 1} moves";
            }
            else
            {
                scoreText = $"Score: {(score / 100.0):F2}";
            }

            return new
            {
                ordering = ordering.ToString("F2") + "%",
                depth = depth,
                scoreText,
                nodes = SearchController.Nodes,
                time = elapsedTime.ToString("F1") + "s",
                bestMove = bestMoveStr
            };
        }

    }
}