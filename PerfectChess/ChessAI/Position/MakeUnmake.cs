using System;
using System.Linq;
using static PerfectChess.Piece;

namespace PerfectChess
{
    partial class Position
    {
        public void Make(int move)
        {
            MoveHistory.Push(move);

            int fromSquare = Move.FromSquare(move);
            int toSquare = Move.ToSquare(move);
            int fromPiece = Move.FromPiece(move);
            int toPiece = Move.ToPiece(move);

            // Fifty moves rule
            if (toPiece != 0 || (fromPiece & Piece.KindMask) == Piece.Pawn)
                MovesFiftyHistory.Push(0);
            else MovesFiftyHistory.Push(MovesFiftyRuleCount + 1);

            // Moving
            SquarePiece[toSquare] = SquarePiece[fromSquare];
            SquarePiece[fromSquare] = 0;

            OccupiedBB ^= 1UL << fromSquare;
            OccupiedBB |= 1UL << toSquare;

            PieceBitboard[fromPiece] ^= 1UL << fromSquare;
            PieceBitboard[fromPiece] ^= 1UL << toSquare;

            PieceBitboard[fromPiece & Piece.ColorMask] ^= 1UL << fromSquare;
            PieceBitboard[fromPiece & Piece.ColorMask] ^= 1UL << toSquare;

            if (CastleShortIndex[ColorToMove] <= 0) CastleShortIndex[ColorToMove]--;
            if (CastleLongIndex[ColorToMove] <= 0) CastleLongIndex[ColorToMove]--;
            if (CastleShortIndex[1 - ColorToMove] <= 0) CastleShortIndex[1 - ColorToMove]--;
            if (CastleLongIndex[1 - ColorToMove] <= 0) CastleLongIndex[1 - ColorToMove]--;

            // Resetting castling rights if king or rook moves         
            if ((fromPiece & Piece.KindMask) == Piece.King)
            {
                if (CastleShortIndex[ColorToMove] == 1) CastleShortIndex[ColorToMove]--;
                if (CastleLongIndex[ColorToMove] == 1) CastleLongIndex[ColorToMove]--;
            }
            else if ((fromPiece & Piece.KindMask) == Piece.Rook)
            {
                if (fromSquare == 56 * ColorToMove + 7)
                {
                    if (CastleShortIndex[ColorToMove] == 1) CastleShortIndex[ColorToMove]--;
                }
                else if (fromSquare == 56 * ColorToMove)
                {
                    if (CastleLongIndex[ColorToMove] == 1) CastleLongIndex[ColorToMove]--;
                }
            }
            // Setting enpassant square if double pawn push
            else if ((fromPiece & Piece.KindMask) == Piece.Pawn && Rank(fromSquare) == 1 + 5 * ColorToMove && Math.Abs(toSquare - fromSquare) == 16)
            {
                EnPassantHistory.Push(fromSquare + 8 - 16 * ColorToMove);
                ColorToMove = 1 - ColorToMove;
                return;
            }

            // Capturing
            if (toPiece != 0)
            {
                PieceBitboard[toPiece] ^= 1UL << toSquare;
                PieceBitboard[toPiece & ColorMask] ^= 1UL << toSquare;

                int enemyLeftRookIndex = 56 * (1 - ColorToMove);
                if (toSquare == enemyLeftRookIndex + 7 && (toPiece & Piece.KindMask) == Piece.Rook && CastleShortIndex[1 - ColorToMove] == 1) CastleShortIndex[1 - ColorToMove]--;
                else if (toSquare == enemyLeftRookIndex &&
                    (toPiece & Piece.KindMask) == Piece.Rook && CastleLongIndex[1 - ColorToMove] == 1)
                    CastleLongIndex[1 - ColorToMove]--;
            }

            // Castling
            if (Move.Castling(move))
            {
                int sidePreIndex = 56 * ColorToMove;

                int rookSquare, rookToSquare;
                // Short castling (getting rook position)
                if (Position.File(toSquare) == 6)
                {
                    rookSquare = sidePreIndex + 7;
                    rookToSquare = fromSquare + 1;
                }
                // Long castling (getting rook position)
                else
                {
                    rookSquare = sidePreIndex;
                    rookToSquare = fromSquare - 1;
                }

                // Moving the rook
                SquarePiece[rookToSquare] = SquarePiece[rookSquare];
                SquarePiece[rookSquare] = 0;

                OccupiedBB ^= 1UL << rookSquare;
                OccupiedBB |= 1UL << rookToSquare;

                PieceBitboard[Piece.Rook | ColorToMove] ^= 1UL << rookSquare;
                PieceBitboard[Piece.Rook | ColorToMove] ^= 1UL << rookToSquare;

                PieceBitboard[ColorToMove] ^= 1UL << rookSquare; // Assuming there is a rook of our colour :)
                PieceBitboard[ColorToMove] ^= 1UL << rookToSquare;
            }

            // Promotion
            if (Move.Promotion(move))
            {
                int promotedTo = Move.PromotionPiece(move); // Either queen, knight, bishop or rook
                SquarePiece[toSquare] = promotedTo;

                PieceBitboard[fromPiece] ^= 1UL << toSquare; // Our pawn is not gonna exist anymore, so delete it from table of pawns
                PieceBitboard[promotedTo] ^= 1UL << toSquare; // There is no piece on that square, so we put our PromotedTo piece there

                // Color maps do not change, neither does occupied bitboard (we're just replacing the piece)
            }

            // Enpassant
            if (Move.EnPassant(move))
            {
                // Deleting the pawn
                int enPassantVictimSquare = (EnPassantSquare + 16 * ColorToMove - 8);
                UInt64 enPassantVictimBitboard = 1UL << enPassantVictimSquare;

                OccupiedBB ^= enPassantVictimBitboard;
                PieceBitboard[Piece.Pawn | 1 - ColorToMove] ^= enPassantVictimBitboard;
                PieceBitboard[1 - ColorToMove] ^= enPassantVictimBitboard;
                SquarePiece[enPassantVictimSquare] = 0;
            }
            ColorToMove = 1 - ColorToMove;
            EnPassantHistory.Push(InvalidSquare);
        }

        public void UnMake()
        {
            if (!MoveHistory.Any()) throw new Exception("No move to unmake");
            int moveToUnmake = MoveHistory.Pop();
            int colorMadeMove = 1 - ColorToMove;

            int fromSquare = Move.FromSquare(moveToUnmake);
            int toSquare = Move.ToSquare(moveToUnmake);
            int fromPiece = Move.FromPiece(moveToUnmake);
            int toPiece = Move.ToPiece(moveToUnmake);

            MovesFiftyHistory.Pop();

            // Moving back and cancelling capturing
            SquarePiece[toSquare] = toPiece;
            SquarePiece[fromSquare] = fromPiece;

            PieceBitboard[fromPiece] ^= 1UL << fromSquare; // no changes
            PieceBitboard[fromPiece] ^= 1UL << toSquare; // no changes

            PieceBitboard[fromPiece & ColorMask] ^= 1UL << fromSquare; // no changes
            PieceBitboard[fromPiece & ColorMask] ^= 1UL << toSquare; // no changes

            OccupiedBB ^= 1UL << fromSquare; // no changes
            if (toPiece != 0)
            {
                PieceBitboard[toPiece] ^= 1UL << toSquare;
                PieceBitboard[toPiece & ColorMask] ^= 1UL << toSquare;

                int enemyLeftRookIndex = 56 * colorMadeMove;
                if (toSquare == enemyLeftRookIndex && toPiece == Piece.Rook && CastleShortIndex[1 - colorMadeMove] <= 0) CastleShortIndex[1 - colorMadeMove]++;
                else if (toSquare == enemyLeftRookIndex + 7 && toPiece == Piece.Rook && CastleLongIndex[1 - colorMadeMove] <= 0) CastleShortIndex[1 - colorMadeMove]++;
            }
            else
            {
                OccupiedBB ^= 1UL << toSquare;
            }

            if (CastleShortIndex[colorMadeMove] <= 0) CastleShortIndex[colorMadeMove]++;
            if (CastleLongIndex[colorMadeMove] <= 0) CastleLongIndex[colorMadeMove]++;
            if (CastleShortIndex[1 - colorMadeMove] <= 0) CastleShortIndex[1 - colorMadeMove]++;
            if (CastleLongIndex[1 - colorMadeMove] <= 0) CastleLongIndex[1 - colorMadeMove]++;

            // Castling back
            if (Move.Castling(moveToUnmake))
            {
                int sidePreIndex = 56 * colorMadeMove;

                int rookSquare, rookToSquare;
                // Short castling (getting rook position)
                if (File(toSquare) == 6)
                {
                    rookSquare = sidePreIndex + 7;
                    rookToSquare = fromSquare + 1;
                }
                // Long castling (getting rook position)
                else
                {
                    rookSquare = sidePreIndex;
                    rookToSquare = fromSquare - 1;
                }

                // Moving the rook back
                SquarePiece[rookSquare] = SquarePiece[rookToSquare];
                SquarePiece[rookToSquare] = 0;

                OccupiedBB ^= 1UL << rookSquare;
                OccupiedBB ^= 1UL << rookToSquare;

                PieceBitboard[Piece.Rook | colorMadeMove] ^= 1UL << rookSquare;
                PieceBitboard[Piece.Rook | colorMadeMove] ^= 1UL << rookToSquare;

                PieceBitboard[colorMadeMove] ^= 1UL << rookSquare;
                PieceBitboard[colorMadeMove] ^= 1UL << rookToSquare;
            }

            // Promotion
            if (Move.Promotion(moveToUnmake))
            {
                int promotedTo = Move.PromotionPiece(moveToUnmake); // Either queen, knight, bishop or rook

                // If we are here, we've just set the PieceBitboard[fromPiece] to 1 at the toSquare thinking of doing the opposite, so we need to cancel that
                PieceBitboard[fromPiece] ^= 1UL << toSquare;
                PieceBitboard[promotedTo] ^= 1UL << toSquare;

                // Color maps do not change, neither does occupied bitboard (we're just replacing the piece)
            }

            // Enpassant
            if (Move.EnPassant(moveToUnmake))
            {
                // Restoring the pawn
                int enPassantVictimSquare = toSquare + 16 * colorMadeMove - 8;
                UInt64 enPassantVictimBitboard = 1UL << enPassantVictimSquare;

                OccupiedBB ^= enPassantVictimBitboard;
                PieceBitboard[Piece.Pawn | ColorToMove] ^= enPassantVictimBitboard;
                PieceBitboard[ColorToMove] ^= enPassantVictimBitboard;
                SquarePiece[enPassantVictimSquare] = Piece.Pawn | ColorToMove;
            }

            ColorToMove = colorMadeMove;
            EnPassantHistory.Pop();
        }
    }
}
