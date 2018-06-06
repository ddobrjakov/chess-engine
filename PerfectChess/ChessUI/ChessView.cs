using System.Collections.Generic;
using System.Drawing;
using static PerfectChess.Piece;

namespace PerfectChess
{
    public static class ChessView
    {
        public static Dictionary<int, Image> PieceImage = new Dictionary<int, Image>
        {
            { White | Pawn, ChessViewSettings.WHITE_PAWN },
            { White | Knight, ChessViewSettings.WHITE_KNIGHT },
            { White | Bishop, ChessViewSettings.WHITE_BISHOP },
            { White | Rook, ChessViewSettings.WHITE_ROOK },
            { White | Queen, ChessViewSettings.WHITE_QUEEN },
            { White | King, ChessViewSettings.WHITE_KING },
            { Black | Pawn, ChessViewSettings.BLACK_PAWN },
            { Black | Knight, ChessViewSettings.BLACK_KNIGHT },
            { Black | Bishop, ChessViewSettings.BLACK_BISHOP },
            { Black | Rook, ChessViewSettings.BLACK_ROOK },
            { Black | Queen, ChessViewSettings.BLACK_QUEEN },
            { Black | King, ChessViewSettings.BLACK_KING }
        };
        public static Dictionary<int, System.Drawing.Color> RealColor = new Dictionary<int, System.Drawing.Color>
        {
            { White, ChessViewSettings.WHITE_SQUARE_COLOR },
            { Black, ChessViewSettings.BLACK_SQUARE_COLOR }
        };
    }
}
