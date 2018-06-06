using System;
using System.Collections.Generic;
using System.Linq;

namespace PerfectChess
{
    public partial class Engine
    {
        /// <summary>
        /// Create an Engine instance.
        /// </summary>
        public Engine() { }

        /// <summary>
        /// Current position that is being analyzed.
        /// </summary>
        private Position Position;

        /// <summary>
        /// To avoid repeating the same moves every game.
        /// </summary>
        private Random R = new Random();

        /// <summary>
        /// Returns the best move in a given position.
        /// If no moves are possible, returns the special value Engine.NO_MOVES.
        /// </summary>
        /// <param name="positionToAnalyze">Position to analyze.</param>
        /// <returns>The best move in the position.</returns>
        public int BestMove(Position positionToAnalyze)
        {
            // Stats
            STATS_Reset();
            Evaluation.STATS_Reset();
            DateTime timeBefore = DateTime.Now;

            IsThinking = true;
            MovesWithScore = new Dictionary<int, List<int>>();

            // Find the best move using Alpha-Beta approach
            int depth = Engine.Depth;
            int res = SearchAlphaBeta(positionToAnalyze, depth);

            IsThinking = false;

            // Stats
            DateTime timeAfter = DateTime.Now;
            STATS_ThinkTime = timeAfter - timeBefore;

            return res;
        }

        /// <summary>
        /// Returns if the engine is busy analyzing a position at the moment.
        /// </summary>
        public bool IsThinking { get; private set; }

        /// <summary>
        /// Stores all the moves with their evaluation as a key.
        /// </summary>
        private Dictionary<int, List<int>> MovesWithScore = new Dictionary<int, List<int>>();

        /// <summary>
        /// Adds a move to the MovesWithScore dictionary.
        /// </summary>
        /// <param name="score">Evaluation of the move.</param>
        /// <param name="move">Move to add.</param>
        private void AddMove(int score, int move)
        {
            if (MovesWithScore.Keys.Contains(score)) MovesWithScore[score].Add(move);
            else MovesWithScore.Add(score, new List<int> { move });
        }

        /// <summary>
        /// Special value representing absence of moves in the position.
        /// </summary>
        public const int NO_MOVES = -1;

        /// <summary>
        /// Position evaluator.
        /// </summary>
        private readonly Evaluation Evaluation = new Evaluation();

        /// <summary>
        /// Evaluate the current position.
        /// </summary>
        /// <returns>Evaluation of the current position.</returns>
        private int Evaluate() => Evaluation.Evaluate(Position);

        /// <summary>
        /// Sorts legal moves in a position by their rating.
        /// </summary>
        /// <param name="position">Position for the moves.</param>
        /// <returns>Moves sorted by rating.</returns>
        private IEnumerable<int> SortedMoves(Position position)
        {
            int[] moves = position.LegalMoves().ToArray();
            if (!moves.Any()) return moves;
            int[] movesRating = new int[moves.Length];
            for (int i = 0; i < moves.Length; i++)
                movesRating[i] = MoveRating(moves[i], position);

            SortMoves(moves, movesRating);
            return moves;
        }

        /// <summary>
        /// Sort moves by their corresponding values.
        /// </summary>
        /// <param name="moves">Moves to sort.</param>
        /// <param name="values">Corresponding values.</param>
        private void SortMoves(int[] moves, int[] values)
        {
            Array.Sort(values, moves, Comparer<int>.Create(new Comparison<int>((a, b) => (a <= b) ? 1 : -1)));
        }

        /// <summary>
        /// Calculates move rating using heuristics.
        /// </summary>
        /// <param name="move">Move to calculate rating of.</param>
        /// <param name="position">Position for the move.</param>
        /// <returns>Rating of the move.</returns>
        private int MoveRating(int move, Position position)
        {
            // Heuristic: good captures first
            // Least valuable attacker — most valuable victim
            if (Move.ToPiece(move) != 0)
            {
                int LVA_MVV = Move.ToPiece(move) & Piece.KindMask - Move.FromPiece(move) & Piece.KindMask;
                if (LVA_MVV >= 0)
                {
                    if (position.AnyMoves && Move.ToSquare((int)(position.LastMove)) == Move.ToSquare(move))
                        return (LVA_MVV + 2) * 5 + 200;
                    return (LVA_MVV + 2) * 5;
                }
                return (LVA_MVV - 2) * 5;
            }
            return 0;
        }

        #region STATS
        public TimeSpan STATS_ThinkTime = new TimeSpan();
        public int STATS_Nodes = 0;
        public int STATS_Evaluated => Evaluation.STATS_Evaluated;
        public int STATS_LegalMovesCallCount => Position.STATS_LegalMovesCallCount;
        public int STATS_AttacksCallCount => Position.STATS_AttacksCallCount;
        public void STATS_Reset()
        {
            STATS_ThinkTime = new TimeSpan();
            STATS_Nodes = 0;
        }
        #endregion
    }
}
