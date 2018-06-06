using System;
using System.Collections.Generic;

namespace PerfectChess
{
    internal static class StringPresentation
    {
        internal static string SquareToString(int square)
        {
            char file = (char)(square % 8 + (int)'A');
            int rank = square / 8 + 1;
            return file + rank.ToString();
        }

        internal static string ColorToString(int color)
        {
            if (color == Piece.White) return "White";
            if (color == Piece.Black) return "Black";
            return "[Error]";
        }

        internal static string PieceToString(int piece)
        {
            var names = new Dictionary<int, string>()
            {
                { Piece.Pawn, "Pawn" },
                { Piece.Knight, "Knight" },
                { Piece.Bishop, "Bishop" },
                { Piece.Rook, "Rook" },
                { Piece.Queen, "Queen" },
                { Piece.King, "King" }
            };
            return (names.TryGetValue(piece & Piece.KindMask, out string name)) ? name : "[Error]";
        }

        internal static string MoveToString(int move)
        {
            (int pieceFrom, int pieceTo) = (Move.FromPiece(move), Move.ToPiece(move));
            string strPieceFrom = PieceToString(Move.FromPiece(move));
            string strSquareFrom = SquareToString(Move.FromSquare(move));
            string strPieceTo = PieceToString(Move.ToPiece(move));
            string strSquareTo = SquareToString(Move.ToSquare(move));

            bool captures = Move.ToPiece(move) != Piece.None;
            bool capturesEnpassant = Move.EnPassant(move);
            bool castles = Move.Castling(move);
            bool castlesShort = castles && (Move.ToSquare(move) % 8 == 6);
            bool castlesLong = castles && (Move.ToSquare(move) % 8 == 2);
            int promotesTo = Move.Promotion(move) ? Move.PromotionPiece(move) : Piece.None;

            string description = new Func<string>(() =>
            {
                if (promotesTo != Piece.None) return "Pawn promotes to " + PieceToString(promotesTo);
                if (capturesEnpassant) return "Pawn takes pawn enpassant";
                if (castlesShort) return "King castles short";
                if (castlesLong) return "King castles long";
                if (castles) return "[Error] King's trying to castle but he's completely lost";
                return strPieceFrom + (captures ? (" takes " + strPieceTo) : " moves");
            })();

            string movingColor = ColorToString(Piece.Color(Move.FromPiece(move)));
            string captureSign = captures ? "x" : "-";
            return $"[{movingColor}]: {strSquareFrom}{captureSign}{strSquareTo} ({description})";
        }
    }
}
