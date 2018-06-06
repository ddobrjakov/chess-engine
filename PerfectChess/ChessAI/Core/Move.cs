namespace PerfectChess
{
    /// <summary>
    /// Move — 26 bits. promotion(2) special(2) to-piece(5) from-piece(5) to-square(6) from-square(6).
    /// Promotion(2): 00 — knight, 01 — bishop, 10 — rook, 11 — queen.
    /// Special(2): 00 — none, 01 — promotion, 10 — en passant, 11 — castling. 
    /// </summary>
    internal static class Move
    {
        internal const int PromotionToKnight = 0;
        internal const int PromotionToBishop = 1;
        internal const int PromotionToRook = 2;
        internal const int PromotionToQueen = 3;

        internal const int SpecNone = 0;
        internal const int SpecPromotion = 1;
        internal const int SpecEnPassant = 2;
        internal const int SpecCastling = 3;

        internal static int FromSquare(int move) => move & 0b111111;
        internal static int ToSquare(int move) => (move >> 6) & 0b111111;
        internal static int FromPiece(int move) => (move >> 12) & 0b11111;
        internal static int ToPiece(int move) => (move >> 17) & 0b11111;
        internal static bool Castling(int move) => SpecialCode(move) == SpecCastling;
        internal static bool EnPassant(int move) => SpecialCode(move) == SpecEnPassant;
        internal static bool Promotion(int move) => SpecialCode(move) == SpecPromotion;
        internal static int PromotionPiece(int move) => (FromPiece(move) & Piece.ColorMask) | ((PromotionCode(move) + 2) * 2);

        private static int SpecialCode(int move) => (move >> 22) & 0b11;
        private static int PromotionCode(int move) => move >> 24;

        internal static int Create(
            int fromSquare,
            int toSquare,
            int fromPiece,
            int toPiece,
            int specCode = 0,
            int promCode = 0
            ) => (((((((((promCode << 2) | specCode) << 5) | toPiece) << 5) | fromPiece) << 6) | toSquare) << 6) | fromSquare;
    }
}
