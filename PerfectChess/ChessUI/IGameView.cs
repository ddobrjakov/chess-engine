using System;
using System.Collections.Generic;

namespace PerfectChess
{
    public interface IGameView
    {
        void Message(string message);
        void Checkmate(bool movedIsHuman, bool lostIsHuman, int colorWin);
        void Stalemate();
        void Check(bool win);
        void FiftyMovesRule();

        int SelectPromotionPiece(int color);
        void Flip();
        bool Flipped { get; }

        void StartMove(Square from, IEnumerable<Square> emptyAvailibleSquares, IEnumerable<Square> enemyAvailibleSquares);
        void CancelMove();
        void FinishMove(int move);

        void UndoMove(int moveToUndo);
        void ExecuteComputerMove(int move);

        void SetMaterial(int white, int black);
        void SetPosition(Position position);

        event EventHandler<int> NewGameRequested;
        event EventHandler<Square> SquareClicked;
        event EventHandler<Tuple<Square, Square>> MoveFinishRequested;
        event EventHandler UndoRequested;
    }
}
