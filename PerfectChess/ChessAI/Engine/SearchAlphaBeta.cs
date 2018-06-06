using System.Collections.Generic;
using System.Linq;
using static PerfectChess.Piece;

namespace PerfectChess
{
    public partial class Engine
    {
        /// <summary>
        /// Returns the best move in a given position (or the special value NO_MOVES if there are no moves).
        /// Uses Alpha-Beta pruning approach.
        /// </summary>
        /// <param name="positionToAnalyze">Position to analyze.</param>
        /// <param name="depth">Depth to search to.</param>
        /// <returns>The best move in the position.</returns>
        private int SearchAlphaBeta(Position positionToAnalyze, int depth)
        {
            // Copying the position (so that sender can use it while we're busy analyzing)
            this.Position = positionToAnalyze.DeepCopy();

            // Early evaluation finish in case of fifty-move rule
            if (Position.MovesFiftyRuleCount >= 50) return NO_MOVES;

            int alpha = Evaluation.Min;
            int beta = Evaluation.Max;

            // TODO: Iterative deepening
            if (Position.ColorToMove == White)
            {
                foreach (int move in SortedMoves(Position))
                {
                    Position.Make(move);
                    // Pruning takes score deviation into account to randomize moves
                    // -1 so that positions that are just as good as the current alpha are not pruned
                    int score = AlphaBetaMin(depth - 1, alpha - 1 - RandomisationMaxDifference, beta);
                    Position.UnMake();

                    if (score > alpha) alpha = score;
                    AddMove(score, move);
                }

                // No moves
                if (!MovesWithScore.Keys.Contains(alpha))
                    return NO_MOVES;

                // Getting all the potential moves - moves with the best score and moves that are close enough to the best score
                IEnumerable<int> potentialMoves = Enumerable.Empty<int>();
                bool closeEnough(int score) =>
                    score >= alpha - RandomisationMaxDifference &&
                    score >= 0 &&
                    score <= Evaluation.CheckmateBonus - 1000;

                foreach (int key in MovesWithScore.Keys.OrderBy(key => key).Where(score => closeEnough(score) || score == alpha))
                    potentialMoves = potentialMoves.Concat(MovesWithScore[key]);

                // Choosing one of the best moves at random
                List<int> listMoves = potentialMoves.ToList();
                return listMoves[R.Next(listMoves.Count)];
            }
            else
            {
                foreach (int move in SortedMoves(Position))
                {
                    Position.Make(move);
                    // Pruning takes score deviation into account to randomize moves
                    // +1 so that positions that are just as good as the current beta are not pruned
                    int score = AlphaBetaMax(depth - 1, alpha, beta + 1 + RandomisationMaxDifference);
                    Position.UnMake();

                    if (score < beta) beta = score;
                    AddMove(score, move);
                }

                // No moves
                if (!MovesWithScore.Keys.Contains(beta))
                    return NO_MOVES;

                // Getting all the potential moves - moves with the best score and moves that are close enough to the best score
                IEnumerable<int> potentialMoves = Enumerable.Empty<int>();
                bool closeEnough(int score) =>
                    score <= beta + RandomisationMaxDifference &&
                    score <= 0 &&
                    score >= -Evaluation.CheckmateBonus + 1000;

                foreach (int key in MovesWithScore.Keys.Where(score => closeEnough(score) || score == beta))
                    potentialMoves = potentialMoves.Concat(MovesWithScore[key]);

                // Choosing one of the best moves at random
                List<int> listMoves = potentialMoves.ToList();
                return listMoves[R.Next(listMoves.Count)];
            }
        }

        private int AlphaBetaMax(int depth, int alpha, int beta)
        {
            STATS_Nodes++;
            if (Position.MovesFiftyRuleCount >= 50) return 0;
            if (depth == 0) return Evaluate();

            bool moves = false;
            foreach (int move in SortedMoves(Position))
            {
                moves = true;
                Position.Make(move);
                int score = AlphaBetaMin(depth - 1, alpha, beta);
                Position.UnMake();

                // Found a move for white which gives them a position better than (or equal to) the one they could at best get with the other black's move
                if (score >= beta) return beta;
                if (score > alpha) alpha = score;
            }
            if (!moves)
            {
                // Checkmate or Stalemate
                if (Position.IsInCheck(Position.ColorToMove))
                    return (Position.ColorToMove == Black)
                        ? Evaluation.CheckmateBonus - (Engine.Depth - depth)
                        : -Evaluation.CheckmateBonus + (Engine.Depth - depth);
                else return 0;
            }
            return alpha;
        }

        private int AlphaBetaMin(int depth, int alpha, int beta)
        {
            STATS_Nodes++;
            if (Position.MovesFiftyRuleCount >= 50) return 0;
            if (depth == 0) return Evaluate();

            bool moves = false;
            foreach (int move in SortedMoves(Position))
            {
                moves = true;
                Position.Make(move);
                int score = AlphaBetaMax(depth - 1, alpha, beta);
                Position.UnMake();
                if (score <= alpha) return alpha;
                if (score < beta) beta = score;
            }

            if (!moves)
            {
                // Checkmate or Stalemate
                if (Position.IsInCheck(Position.ColorToMove))
                    return (Position.ColorToMove == Black)
                        ? Evaluation.CheckmateBonus - (Engine.Depth - depth) 
                        : -Evaluation.CheckmateBonus + (Engine.Depth - depth);
                else return 0;
            }

            return beta;
        }
    }
}
