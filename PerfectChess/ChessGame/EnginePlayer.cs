using System;
using System.Linq;
using System.Threading.Tasks;

namespace PerfectChess
{
    /// <summary>
    /// Represents an engine player. Moves are calculated by an engine.
    /// </summary>
    public class EnginePlayer : Player
    {
        /// <summary>
        /// Creates an engine player of specified color.
        /// </summary>
        /// <param name="color">Color of the engine player.</param>
        public EnginePlayer(ChessColor color) : base(color) { }

        /// <summary>
        /// Engine to come up with moves.
        /// </summary>
        private Engine E = new Engine();

        /// <summary>
        /// Handles a new move request.
        /// </summary>
        /// <param name="position">Current position.</param>
        protected override async void MoveRequest(Position position)
        {
            if (PreMoves.Any()) MakeMove(PreMoves.Pop());
            else
            {
                int move = -10;
                await Task.Run(() => move = E.BestMove(position));
                MakeMove(move);
            }
        }

        // Stats
        public int STATS_Evaluated => E.STATS_Evaluated;
        public TimeSpan STATS_ThinkTime => E.STATS_ThinkTime;
        public int STATS_LegalMovesCallCount => E.STATS_LegalMovesCallCount;
        public int STATS_Nodes => E.STATS_Nodes;
        public int STATS_AttacksCallCount => E.STATS_AttacksCallCount;
        // public EngineStats Stats => E.Stats;
    }
}
