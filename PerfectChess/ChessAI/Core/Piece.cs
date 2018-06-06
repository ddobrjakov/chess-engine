namespace PerfectChess
{
    /// <summary>
    /// Piece — 4 bits. First three bits define kind (bishop, queen etc), last one defines color.
    /// </summary>
    internal static class Piece
    {
        internal const int None = 0;
        internal const int Pawn = 2;
        internal const int Knight = 4;
        internal const int Bishop = 6;
        internal const int Rook = 8;
        internal const int Queen = 10;
        internal const int King = 12;
        internal const int White = 0;
        internal const int Black = 1;
        internal const int KindMask = 0b1110;
        internal const int ColorMask = 0b0001;
        internal static int Color(int coloredPiece) => coloredPiece & ColorMask;
        internal static int Kind(int coloredPiece) => coloredPiece & KindMask;
    }
}
