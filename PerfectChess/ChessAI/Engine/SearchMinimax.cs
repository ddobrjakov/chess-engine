using System;
using System.Collections.Generic;
using System.Linq;
using static PerfectChess.Piece;

namespace PerfectChess
{
    public partial class Engine
    {
        /// <summary>
        /// Returns the best move in a given position (or the special value NO_MOVES if there are no moves).
        /// Uses Minimax approach.
        /// </summary>
        /// <param name="positionToAnalyze">Position to analyze.</param>
        /// <param name="depth">Depth to search to.</param>
        /// <returns>The best move in the position.</returns>
        private int SearchMinimax(Position p, int depth)
        {
            this.Position = p.DeepCopy();

            int alpha = (Position.ColorToMove == White) ? int.MinValue : int.MaxValue;
            int bestmove = -1;
            int bestalpha = alpha;
            R = new Random();

            Dictionary<int, List<int>> bestMoves = new Dictionary<int, List<int>>();
            foreach (int move in Position.LegalMoves())
            {
                if (Position.ColorToMove == White)
                {
                    Position.Make(move);

                    int newalpha = MiniMax(depth, Black);
                    if (alpha <= newalpha)
                    {
                        bestmove = move;
                        if (bestMoves.Keys.Contains(newalpha)) bestMoves[newalpha].Add(move);
                        else bestMoves.Add(newalpha, new List<int> { move });
                        alpha = newalpha;
                    }
                    Position.UnMake();
                }

                else if (Position.ColorToMove == Black)
                {
                    Position.Make(move);

                    int newalpha = MiniMax(depth, White);
                    if (alpha >= newalpha)
                    {
                        bestmove = move;
                        if (bestMoves.Keys.Contains(newalpha)) bestMoves[newalpha].Add(move);
                        else bestMoves.Add(newalpha, new List<int> { move });
                        alpha = newalpha;
                    }
                    Position.UnMake();
                }
            }

            try
            {
                bestmove = bestMoves[alpha][R.Next(bestMoves[alpha].Count)];
            }

            catch
            {
                this.IsThinking = false;
                return NO_MOVES;
            }

            this.IsThinking = false;

            return bestmove;
        }

        private int MiniMax(int depth, int player)
        {
            STATS_Nodes++;
            if (depth <= 0) return Evaluate();
            int alpha = (Position.ColorToMove == White) ? int.MinValue : int.MaxValue;
            int alphasaved = alpha;

            foreach (int move in Position.LegalMoves())
            {
                if (player == White)
                {
                    Position.Make(move);
                    alpha = Math.Max(alpha, MiniMax(depth - 1, Black));
                    Position.UnMake();
                }
                else if (player == Black)
                {
                    Position.Make(move);
                    alpha = Math.Min(alpha, MiniMax(depth - 1, White));
                    Position.UnMake();
                }
            }

            // No moves
            if (alpha == alphasaved)
            {
                // Checkmate or Stalemate
                if (Position.IsInCheck(Position.ColorToMove))
                    return (Position.ColorToMove == Black) ? Evaluation.CheckmateBonus : -Evaluation.CheckmateBonus;
                else return 0;
            }

            return alpha;
        }
    }
}
