using System;
using System.Collections.Generic;

namespace PerfectChess
{
    public abstract class Player
    {
        /// <summary>
        /// Creates a player of specified color.
        /// </summary>
        /// <param name="color">Color of the player.</param>
        public Player(ChessColor color)
        {
            this.Color = color;
        }

        /// <summary>
        /// Color of the player.
        /// </summary>
        public ChessColor Color { get; set; }

        /// <summary>
        /// Notifies the player that it's his turn to move.
        /// </summary>
        /// <param name="position">Current position.</param>
        public void YourMove(Position position)
        {
            this.IsThinking = true;
            this.MoveRequest(position);
        }

        /// <summary>
        /// Handles new move request.
        /// </summary>
        /// <param name="position">Current position.</param>
        protected abstract void MoveRequest(Position position);

        /// <summary>
        /// Dispatches an event to notify listeners that the player has made a move.
        /// </summary>
        /// <param name="move">Move that was made by the player.</param>
        public void MakeMove(int move)
        {
            this.IsThinking = false;
            MakesMove?.Invoke(this, move);
        }

        /// <summary>
        /// Premoves made by the player. Not implemented yet.
        /// </summary>
        public Stack<int> PreMoves { get; private set; } = new Stack<int>();

        /// <summary>
        /// Event that is fired when the player makes a move.
        /// </summary>
        public event EventHandler<int> MakesMove;

        /// <summary>
        /// Player is currently thinking.
        /// </summary>
        public bool IsThinking { get; private set; }
    }
}
