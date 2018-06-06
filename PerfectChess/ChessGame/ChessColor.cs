namespace PerfectChess
{
    public enum ChessColor { White, Black }
    public static class ChessColorExtensions
    {
        static ChessColor Opposite(this ChessColor color) =>
            color == ChessColor.White ? ChessColor.Black : ChessColor.White;
    }
}
