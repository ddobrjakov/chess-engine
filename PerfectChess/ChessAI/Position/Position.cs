using System;
using System.Collections.Generic;
using System.Linq;
using static PerfectChess.Piece;

namespace PerfectChess
{
    public partial class Position
    {
        /// <summary>
        /// Creates a Position instance with default parameters (starting chess position).
        /// </summary>
        public Position() : this(InitialPieces, White, 0b1111) { }

        /// <summary>
        /// Creates a Position instance.
        /// </summary>
        /// <param name="Pieces">Arrangement of the pieces on the board.</param>
        /// <param name="ToMove">Color of the side to move.</param>
        /// <param name="CastlingRights">Castling rights for both sides, encoded into 4 bits.</param>
        /// <param name="EnPassant">Enpassant square if one exists.</param>
        /// <exception cref="ArgumentException"></exception>
        public Position(int[] Pieces, int ToMove, int CastlingRights, int EnPassant = InvalidSquare)
        {
            if (Pieces.Count() != 64) throw new ArgumentException();
            for (int i = 0; i < 64; i++)
            {
                int PieceToAdd = Pieces[i];
                SquarePiece[i] = PieceToAdd;
                if (PieceToAdd == None) continue;

                // Piece is not 'None'.
                PieceBitboard[PieceToAdd] |= (1UL << i); // Piece array
                PieceBitboard[PieceToAdd & ColorMask] |= (1UL << i); // Color array
                OccupiedBB |= (1UL << i); // All pieces array
            }

            CastleShortIndex[White] = CastlingRights & 0b0001;
            CastleShortIndex[Black] = (CastlingRights & 0b0010) >> 1;
            CastleLongIndex[White] = (CastlingRights & 0b0100) >> 2;
            CastleLongIndex[Black] = (CastlingRights & 0b1000) >> 3;
            ColorToMove = ToMove;
            EnPassantHistory.Push(EnPassant);
        }

        /// <summary>
        /// Bitboards with squares occupied by pieces of given kind and color.
        /// </summary>
        internal readonly UInt64[] PieceBitboard = new UInt64[15];

        /// <summary>
        /// Bitboard with squares occupied by any piece.
        /// </summary>
        private UInt64 OccupiedBB;

        /// <summary>
        /// Piece located on a given square.
        /// </summary>
        internal readonly int[] SquarePiece = new int[64];

        /// <summary>
        /// Piece located on a given square.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal int this[int index] => SquarePiece[index];

        /// <summary>
        /// King side castling rights for each color.
        /// 1 means "can castle", 0 or less means "can't castle".
        /// </summary>
        internal int[] CastleShortIndex = new int[2];

        /// <summary>
        /// Queen side castling rights for each color.
        /// 1 means "can castle", 0 or less means "can't castle".
        /// </summary>
        internal int[] CastleLongIndex = new int[2];

        /// <summary>
        /// Color of the side to make the next move.
        /// </summary>
        internal int ColorToMove { get; private set; } = White;

        /// <summary>
        /// Square behind enpassant pawn that will be occupied by the taking pawn (-1 if none).
        /// </summary>
        internal int EnPassantSquare => EnPassantHistory.Peek();

        /// <summary>
        /// History of enpassant squares at each move.
        /// </summary>
        private readonly Stack<int> EnPassantHistory = new Stack<int>();

        /// <summary>
        /// Returns the total number of "plies" (half-moves) played.
        /// </summary>
        internal int HalfMoves => MoveHistory.Count();

        /// <summary>
        /// Returns the number of no pawn moves and no capturing moves in a row.
        /// </summary>
        internal int MovesFiftyRuleCount =>
            (MovesFiftyHistory.Any()) ? MovesFiftyHistory.Peek() : 0;

        /// <summary>
        /// History of number of half-moves since the last capture or pawn move.
        /// </summary>
        private readonly Stack<int> MovesFiftyHistory = new Stack<int>();

        /// <summary>
        /// History of half-moves.
        /// </summary>
        internal readonly Stack<int> MoveHistory = new Stack<int>();

        /// <summary>
        /// Returns if any moves were played.
        /// </summary>
        internal bool AnyMoves => MoveHistory.Any();

        /// <summary>
        /// Returns the most recent move played.
        /// </summary>
        internal int? LastMove =>
            MoveHistory.Any() ? MoveHistory.Peek() : (int?)null;

        /// <summary>
        /// Returns the file of a given square.
        /// </summary>
        /// <param name="square">Square index.</param>
        /// <returns></returns>
        internal static int File(int square) => square & 7;

        /// <summary>
        /// Returns the rank of a given square.
        /// </summary>
        /// <param name="square">Square index.</param>
        /// <returns></returns>
        internal static int Rank(int square) => square >> 3;

        /// <summary>
        /// Represents an invalid square.
        /// </summary>
        private const int InvalidSquare = -1;

        /// <summary>
        /// Arrangement of pieces in the starting position.
        /// </summary>
        internal static readonly int[] InitialPieces =
        {
            (White|Rook), (White|Knight), (White|Bishop), (White|Queen), (White|King), (White|Bishop), (White|Knight), (White|Rook),
            (White|Pawn), (White|Pawn),   (White|Pawn),   (White|Pawn),  (White|Pawn), (White|Pawn),   (White|Pawn),   (White|Pawn),
            None,          None,          None,           None,          None,         None,           None,           None,
            None,          None,          None,           None,          None,         None,           None,           None,
            None,          None,          None,           None,          None,         None,           None,           None,
            None,          None,          None,           None,          None,         None,           None,           None,
            (Black|Pawn), (Black|Pawn),   (Black|Pawn),   (Black|Pawn),  (Black|Pawn), (Black|Pawn),   (Black|Pawn),   (Black|Pawn),
            (Black|Rook), (Black|Knight), (Black|Bishop), (Black|Queen), (Black|King), (Black|Bishop), (Black|Knight), (Black|Rook)
        };

        /// <summary>
        /// Returns if a given square is attacked by enemy pieces.
        /// </summary>
        /// <param name="color">Color to be attacked on the square.</param>
        /// <param name="square">Square to check for enemy attacks.</param>
        /// <returns></returns>
        internal bool IsAttacked(int color, int square)
        {
            int enemy = 1 - color;

            // Pieces that cannot slide
            if ((PieceBitboard[enemy | Knight] & Attack.Knight(square)) != 0
             || (PieceBitboard[enemy | Pawn] & Attack.Pawn(color, square)) != 0
             || (PieceBitboard[enemy | King] & Attack.King(square)) != 0)
                return true;

            // Diagonally sliding pieces
            UInt64 bishopQueenBitboard = PieceBitboard[enemy | Bishop] | PieceBitboard[enemy | Queen];
            if ((bishopQueenBitboard & Bitboard.Diagonals[square]) != 0
             && (bishopQueenBitboard & Attack.Bishop(square, OccupiedBB)) != 0)
                return true;

            // Horizontally/vertically sliding pieces
            UInt64 rookQueenBitboard = PieceBitboard[enemy | Rook] | PieceBitboard[enemy | Queen];
            if ((rookQueenBitboard & Bitboard.Axes[square]) != 0
             && (rookQueenBitboard & Attack.Rook(square, OccupiedBB)) != 0)
                return true;

            return false;
        }

        /// <summary>
        /// Returns if a piece located on fromSquare is attacking the toSquare.
        /// </summary>
        /// <param name="fromSquare">Square containing attacking piece.</param>
        /// <param name="toSquare">Square to be attacked.</param>
        /// <returns></returns>
        internal bool Attacks(int fromSquare, int toSquare)
        {
            STATS_AttacksCallCount++;

            int piece = SquarePiece[fromSquare];
            switch (piece & Piece.KindMask)
            {
                case Piece.None: return false;
                case Piece.Pawn: return (Attack.Pawn(SquarePiece[fromSquare] & Piece.ColorMask, fromSquare) & (1UL << toSquare)) != 0;
                case Piece.Knight: return (Attack.Knight(fromSquare) & (1UL << toSquare)) != 0;
                case Piece.Bishop: return (Attack.Bishop(fromSquare, OccupiedBB) & (1UL << toSquare)) != 0;
                case Piece.Rook: return (Attack.Rook(fromSquare, OccupiedBB) & (1UL << toSquare)) != 0;
                case Piece.Queen: return (Attack.Queen(fromSquare, OccupiedBB) & (1UL << toSquare)) != 0;
                case Piece.King: return (Attack.King(fromSquare) & (1UL << toSquare)) != 0;
                default: throw new Exception();
            }
        }

        /// <summary>
        /// Finds the king of a given color.
        /// </summary>
        /// <param name="color">Color of the king.</param>
        /// <returns>Index of the square with the king.</returns>
        internal int KingSquare(int color) =>
            BitOperations.OnlyBitIndex(PieceBitboard[color | King]);

        /// <summary>
        /// Returns if a king is in check.
        /// </summary>
        /// <param name="color">Color of the king.</param>
        /// <returns></returns>
        internal bool IsInCheck(int color) =>
            IsAttacked(color, BitOperations.OnlyBitIndex(PieceBitboard[color | King]));

        /// <summary>
        /// Check is given in the current position.
        /// </summary>
        internal bool Check => IsInCheck(ColorToMove);

        /// <summary>
        /// Checkmate is given in the current position.
        /// </summary>
        internal bool Checkmate => Check && !LegalMoves().Any();

        /// <summary>
        /// Current position is a stalemate.
        /// </summary>
        internal bool Stalemate => !LegalMoves().Any() && !Check;

        /// <summary>
        /// Game is finished by the chess rules.
        /// </summary>
        internal bool GameFinished => !LegalMoves().Any() || MovesFiftyRuleCount >= 50;

        /// <summary>
        /// Enum of possible states for the chess game.
        /// </summary>
        internal enum GameResult { WhiteWinCheckmate, BlackWinCheckmate, DrawStalemate, DrawFiftyMoves, InProcess }

        /// <summary>
        /// Current state of the game.
        /// </summary>
        internal GameResult Status
        {
            get
            {
                if (MovesFiftyRuleCount >= 50) return GameResult.DrawFiftyMoves;
                if (!LegalMoves().Any())
                {
                    if (Check) return (ColorToMove == Piece.Black) ? GameResult.WhiteWinCheckmate : GameResult.BlackWinCheckmate;
                    else return GameResult.DrawStalemate;
                }
                return GameResult.InProcess;
            }
        }

        /// <summary>
        /// Creates and returns a new copy of this Position instance.
        /// </summary>
        /// <returns>A copy of this Position instance.</returns>
        internal Position DeepCopy()
        {
            Position copied = new Position();
            List<int> movesInOrder = MoveHistory.ToList();
            movesInOrder.Reverse();
            foreach (int move in movesInOrder)
                copied.Make(move);
            return copied;
        }

        public override string ToString()
        {
            string res = string.Empty;
            UInt64 occ = OccupiedBB;
            UInt64 bits8;
            for (int j = 0; j < 8; j++)
            {
                bits8 = (occ & 0xFF00000000000000) >> 56;
                for (int i = 0; i < 8; i++)
                {
                    res += bits8 & 1;
                    bits8 >>= 1;
                }
                res += "\n";
                occ <<= 8;
            }
            return res;
        }

        #region STATS
        public int STATS_AttacksCallCount = 0;
        public int STATS_LegalMovesCallCount = 0;
        public void STATS_Reset()
        {
            STATS_AttacksCallCount = 0;
            STATS_LegalMovesCallCount = 0;
        }
        #endregion
    }
}
