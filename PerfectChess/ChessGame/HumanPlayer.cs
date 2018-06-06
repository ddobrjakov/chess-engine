using System.Linq;

namespace PerfectChess
{
    /// <summary>
    /// Represents a human player. Moves are made by the user interface.
    /// </summary>
    public class HumanPlayer : Player
    {
        /// <summary>
        /// Creates a human player of specified color.
        /// </summary>
        /// <param name="color">Color of the human player.</param>
        public HumanPlayer(ChessColor color) : base(color) { }

        /// <summary>
        /// Handles a new move request.
        /// </summary>
        /// <param name="position">Current position.</param>
        protected override void MoveRequest(Position position)
        {
            if (PreMoves.Any()) MakeMove(PreMoves.Pop());

            // Does nothing and fires the event whenever a move is made by the user interface
        }
    }
}
