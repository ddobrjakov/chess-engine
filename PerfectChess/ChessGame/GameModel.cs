using System;
using System.Collections.Generic;
using System.Linq;

namespace PerfectChess
{
    public class GameModel
    {
        public GameModel(Player playerWhite, Player playerBlack, Position startPosition)
        {
            this.PlayerWhite = playerWhite;
            this.PlayerBlack = playerBlack;
            this.GamePosition = startPosition;
        }

        private Player _PlayerWhite;
        public Player PlayerWhite
        {
            get => _PlayerWhite;
            set
            {
                if (_PlayerWhite != null) _PlayerWhite.MakesMove -= PropagatePlayerMakesMove;
                _PlayerWhite = value;
                _PlayerWhite.MakesMove += PropagatePlayerMakesMove;
            }
        }

        private Player _PlayerBlack;
        public Player PlayerBlack
        {
            get => _PlayerBlack;
            set
            {
                if (_PlayerBlack != null) _PlayerBlack.MakesMove -= PropagatePlayerMakesMove;
                _PlayerBlack = value;
                _PlayerBlack.MakesMove += PropagatePlayerMakesMove;
            }
        }

        public Position GamePosition;

        public Player PlayerToMove => (GamePosition.ColorToMove == Piece.White) ? PlayerWhite : PlayerBlack;
        public Player PlayerWaiting => (GamePosition.ColorToMove == Piece.Black) ? PlayerWhite : PlayerBlack;

        public event EventHandler<int> PlayerMakesMove;
        private void PropagatePlayerMakesMove(object sender, int move) => PlayerMakesMove?.Invoke(sender, move);

        /// <summary>
        /// Starts new game.
        /// </summary>
        /// <param name="players"></param>
        public void StartNewGame(int players)
        {
            PlayerWhite = (((players & 0b10) >> 1) == 1) ? (Player)(new HumanPlayer(ChessColor.White)) : (new EnginePlayer(ChessColor.White));
            PlayerBlack = ((players & 0b01) == 1) ? (Player)(new HumanPlayer(ChessColor.Black)) : (new EnginePlayer(ChessColor.Black));
            GamePosition = new Position();
        }

        /// <summary>
        /// Counts material advantage for white (can be negative).
        /// </summary>
        /// <returns>Material advantage for white (can be negative).</returns>
        public int CountMaterial()
        {
            int[] res = new int[2] { 0, 0 };
            for (int color = Piece.White; color <= Piece.Black; color++)
            {
                res[color] += BitOperations.PopCount(GamePosition.PieceBitboard[Piece.Pawn | color]);
                res[color] += 3 * BitOperations.PopCount(GamePosition.PieceBitboard[Piece.Knight | color]);
                res[color] += 3 * BitOperations.PopCount(GamePosition.PieceBitboard[Piece.Bishop | color]);
                res[color] += 5 * BitOperations.PopCount(GamePosition.PieceBitboard[Piece.Rook | color]);
                res[color] += 9 * BitOperations.PopCount(GamePosition.PieceBitboard[Piece.Queen | color]);
            }
            return res[0] - res[1];
        }

        /// <summary>
        /// Returns if the board should be flipped.
        /// </summary>
        /// <param name="isFlipped">The board is currently flipped.</param>
        /// <returns>If the board should be flipped.</returns>
        public bool ShouldFlipBoard(bool isFlipped) =>
            (PlayerBlack is HumanPlayer && PlayerWhite is EnginePlayer && !isFlipped) ||
            (PlayerWhite is HumanPlayer && PlayerBlack is EnginePlayer && isFlipped);

        /// <summary>
        /// The game is finished (or not started).
        /// </summary>
        public bool GameFinished => GamePosition.Status != Position.GameResult.InProcess;

        /// <summary>
        /// Returns if the move is illegal.
        /// </summary>
        /// <param name="move">Move to check.</param>
        /// <returns>The move is illegal.</returns>
        public bool IllegalMove(int move) =>
            move == Engine.NO_MOVES || !GamePosition.LegalMoves().Contains(move);

        /// <summary>
        /// Finds all legal moves from specified origin square to specified target square.
        /// </summary>
        /// <param name="from">Origin square of the moves.</param>
        /// <param name="to">Target square of the moves.</param>
        /// <returns>Collection of all legal moves from origin to target square.</returns>
        public IEnumerable<int> FindMoves(Square from, Square to) =>
            GamePosition.LegalMoves().Where(m =>
                Move.FromSquare(m) == from.X + 8 * from.Y &&
                Move.ToSquare(m) == to.X + 8 * to.Y);

        /// <summary>
        /// Selects the promotion move with the chosen piece out of a collection of candidate promotion moves.
        /// </summary>
        /// <param name="candidateMoves">Candidate promotion moves.</param>
        /// <param name="chosenPiece">Chosen promotion piece.</param>
        /// <returns>Promotion move with the chosen piece.</returns>
        public int? SelectPromotionMove(IEnumerable<int> candidateMoves, int chosenPiece)
        {
            IEnumerable<int> moves = candidateMoves.Where(move => Move.PromotionPiece(move) == chosenPiece);
            if (moves.Count() != 1) return null;
            return moves.First();
        }

        /// <summary>
        /// Cancels the last one or two halfmoves and returns the canceled halfmoves.
        /// </summary>
        /// <returns>Canceled halfmoves.</returns>
        public IEnumerable<int> UndoMoves()
        {
            int halfMovesCount = Math.Min((PlayerWaiting is EnginePlayer) ? 2 : 1, GamePosition.MoveHistory.Count());
            IEnumerable<int> halfMoves = GamePosition.MoveHistory.Take(halfMovesCount).ToList();
            foreach (int halfMove in halfMoves)
                GamePosition.UnMake();
            return halfMoves;
        }

        /// <summary>
        /// Gets the piece on a square.
        /// </summary>
        /// <param name="square">Square to get the piece.</param>
        /// <returns>Piece on the square.</returns>
        public int GetPiece(Square square) =>
            GamePosition[square.X + 8 * square.Y];

        /// <summary>
        /// Returns if the square is empty (does not contain a piece).
        /// </summary>
        /// <param name="square">Square to check.</param>
        /// <returns>If the square is empty.</returns>
        public bool EmptySquare(Square square) =>
            GetPiece(square) == Piece.None;

        /// <summary>
        /// Returns if the square contains a piece of specified player.
        /// </summary>
        /// <param name="square">Square to check.</param>
        /// <param name="player">Player to check if his piece occupies the square.</param>
        /// <returns>If the square contains a piece of specified player.</returns>
        public bool PieceOfPlayer(Square square, Player player) =>
            (GetPiece(square) & Piece.ColorMask) == (int)player.Color;

        /// <summary>
        /// Returns target squares for a specified origin square grouped into categories of empty and enemy squares.
        /// </summary>
        /// <param name="origin">Origin square.</param>
        /// <returns>Empty target squares and target squares occupied by oppenent's pieces.</returns>
        public (IEnumerable<Square> empty, IEnumerable<Square> enemy) TargetSquares(Square origin)
        {
            List<int> moves = GamePosition.LegalMoves();
            List<Square> emptyAvailableSquares = new List<Square>();
            List<Square> enemyAvailableSquares = new List<Square>();

            foreach (int move in moves.Where(m => PerfectChess.Move.FromSquare(m) == origin.X + 8 * origin.Y))
            {
                if (PerfectChess.Move.ToPiece(move) == 0)
                    emptyAvailableSquares.Add(Square.Get(PerfectChess.Move.ToSquare(move) % 8 + 1, PerfectChess.Move.ToSquare(move) / 8 + 1));
                else enemyAvailableSquares.Add(Square.Get(PerfectChess.Move.ToSquare(move) % 8 + 1, PerfectChess.Move.ToSquare(move) / 8 + 1));
            }

            return (emptyAvailableSquares, enemyAvailableSquares);
        }
    }
}
