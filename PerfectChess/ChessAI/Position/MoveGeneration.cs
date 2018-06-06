using System;
using System.Collections.Generic;
using static PerfectChess.Move;
using static PerfectChess.Piece;

namespace PerfectChess
{
    partial class Position
    {
        public List<int> LegalMoves()
        {
            STATS_LegalMovesCallCount++;

            if (this.MovesFiftyRuleCount >= 50) return new List<int>();
            bool check = IsInCheck(ColorToMove);

            List<int> moves = new List<int>();
            if (!check) GenerateCastleMoves(moves);
            GenerateEnPassantMoves(moves);

            UInt64 pinnedPieces = (check) ? 0 : GetPinnedPieces();

            // TODO: Generate captures first, silent moves later

            GeneratePawnMoves(moves, pinnedPieces, check);
            GenerateKnightMoves(moves, pinnedPieces, check);
            GenerateBishopMoves(moves, pinnedPieces, check);
            GenerateRookMoves(moves, pinnedPieces, check);
            GenerateQueenMoves(moves, pinnedPieces, check);
            GenerateKingMoves(moves);

            return moves;
        }

        private void GenerateCastleMoves(List<int> moves)
        {
            int preIndex = 56 * ColorToMove;

            if (CastleLongIndex[ColorToMove] == 1 && (SquarePiece[1 + preIndex] | SquarePiece[2 + preIndex] | SquarePiece[3 + preIndex]) == None)
                if (!IsAttacked(ColorToMove, 3 + preIndex) && !IsAttacked(ColorToMove, 2 + preIndex))
                    moves.Add(Move.Create(4 + preIndex, 2 + preIndex, ColorToMove | King, None, SpecCastling));

            if (CastleShortIndex[ColorToMove] == 1 && (SquarePiece[5 + preIndex] | SquarePiece[6 + preIndex]) == None)
                if (!IsAttacked(ColorToMove, 5 + preIndex) && !IsAttacked(ColorToMove, 6 + preIndex))
                    moves.Add(Move.Create(4 + preIndex, 6 + preIndex, ColorToMove | King, None, SpecCastling));
        }

        private void GenerateEnPassantMoves(List<int> moves)
        {
            if (EnPassantSquare != InvalidSquare)
            {
                int enemy = 1 - ColorToMove;
                int kingSquare = BitOperations.OnlyBitIndex(PieceBitboard[ColorToMove | King]);

                // Enpassant Square is on the 3rd/6th rank
                UInt64 enPassantPawnBitboard = PieceBitboard[ColorToMove | Piece.Pawn] & Attack.Pawn(enemy, EnPassantSquare);
                UInt64 enPassantVictimBitboard = 1UL << (EnPassantSquare + 8 - 16 * enemy);
                while (enPassantPawnBitboard != 0)
                {
                    // Performing necessary part of the move to determine it's legality
                    int fromSquare = BitOperations.PopLS(ref enPassantPawnBitboard);
                    PieceBitboard[enemy | Pawn] ^= enPassantVictimBitboard;
                    OccupiedBB ^= enPassantVictimBitboard;
                    OccupiedBB ^= (1UL << fromSquare) | (1UL << EnPassantSquare);

                    // Checking legality and adding the move
                    if (!IsAttacked(ColorToMove, kingSquare))
                        moves.Add(Move.Create(fromSquare, EnPassantSquare, ColorToMove | Pawn, None, SpecEnPassant));

                    // Reverting changes
                    PieceBitboard[enemy | Pawn] ^= enPassantVictimBitboard;
                    OccupiedBB ^= enPassantVictimBitboard;
                    OccupiedBB ^= (1UL << fromSquare) | (1UL << EnPassantSquare);
                }
            }
        }

        /// <summary>
        /// Checks if the king of the moving side is not attacked afterwards.
        /// Does not check castling and enpassant moves.
        /// Does not check that the moving piece exists.
        /// Does not check that the target square is not occupied by a piece of the same color.
        /// </summary>
        /// <param name="squareFrom">Square that a piece departs from.</param>
        /// <param name="squareTo">Square that a piece arrives to.</param>
        /// <param name="isKingMove">King is doing the move.</param>
        /// <returns>If the move is legal (except mentioned scenarios).</returns>
        private bool IsPseudoLegal(int squareFrom, int squareTo, bool isKingMove = false)
        {
            // 1) Performing necessary part of the move to determine if it's legal

            // Saving Occupied Pieces Bitboard to restore it in near future
            UInt64 occupiedBBSaved = OccupiedBB;

            // Getting the captured piece (might be Piece.None)
            Int32 capture = SquarePiece[squareTo];

            // Setting up properties to prepare for the check test

            PieceBitboard[capture] ^= 1UL << squareTo; // Deletes piece from it's table if it's there (if not, nothing bad happens, 
                                                       // because we don't care about PieceBitBoard[0] and we'll put the value back anyway)

            OccupiedBB ^= 1UL << squareFrom; // origin square now frees up
            OccupiedBB |= 1UL << squareTo; // destination square gets our piece

            // 2) Now everything is ready to test if the king is in check
            int kingSquare = isKingMove ? squareTo : BitOperations.OnlyBitIndex(PieceBitboard[ColorToMove | King]);
            bool res = !IsAttacked(ColorToMove, kingSquare);

            // 3) We've got the answer, but have to return everything back

            PieceBitboard[capture] ^= (1UL << squareTo); // XOR is commutative, so that one is pretty easy
            OccupiedBB = occupiedBBSaved; // thanks to us in the past saving it, now we can simply restore the occupied bitboard

            return res;
        }

        private void GeneratePawnMoves(List<int> moves, UInt64 pinnedPieces, bool check)
        {
            int ourColor = ColorToMove;
            UInt64 ourPieces = PieceBitboard[ourColor | Pawn];
            UInt64 enemyALLpieces = PieceBitboard[1 - ourColor];

            while (ourPieces != 0)
            {
                // Getting the origin square
                int fromSquare = BitOperations.PopLS(ref ourPieces);

                // Checking for pin
                bool pinned = (pinnedPieces & (1UL << fromSquare)) != 0;

                // One square advance
                int toSquare = fromSquare + 8 - 16 * ColorToMove;
                UInt64 moveBitboard = ~OccupiedBB & (1UL << toSquare); // 0 if destination square is occupied

                // Two squares advance
                if (moveBitboard != 0 && (Rank(fromSquare) == 1 + 5 * ColorToMove))
                    moveBitboard |= ~OccupiedBB & (1UL << (fromSquare + 16 - 32 * ColorToMove)); // 0 => doesn't change if destination square's occupied

                // Captures
                UInt64 attackBitboard = Attack.Pawn(ColorToMove, fromSquare);
                moveBitboard |= enemyALLpieces & attackBitboard;

                // Now all the pseudo-legal moves are stored in moveBitboard and we're ready to check and add them
                while (moveBitboard != 0)
                {
                    // Iterating over each possible destination square
                    toSquare = BitOperations.PopLS(ref moveBitboard);
                    int capturedPiece = SquarePiece[toSquare];

                    bool legal = (check) ? IsPseudoLegal(fromSquare, toSquare) : (!pinned || Bitboard.Aligned(KingSquare(ColorToMove), fromSquare, toSquare));
                    if (legal)
                    {
                        if (((1UL << toSquare) & (Bitboard.Rank1BB | Bitboard.Rank8BB)) != 0) // Check for promotion
                        {
                            moves.Add(Move.Create(fromSquare, toSquare, ColorToMove | Pawn, capturedPiece, SpecPromotion, PromotionToQueen));
                            moves.Add(Move.Create(fromSquare, toSquare, ColorToMove | Pawn, capturedPiece, SpecPromotion, PromotionToKnight));
                            moves.Add(Move.Create(fromSquare, toSquare, ColorToMove | Pawn, capturedPiece, SpecPromotion, PromotionToRook));
                            moves.Add(Move.Create(fromSquare, toSquare, ColorToMove | Pawn, capturedPiece, SpecPromotion, PromotionToBishop));
                        }
                        else moves.Add(Move.Create(fromSquare, toSquare, ColorToMove | Pawn, capturedPiece));
                    }
                }
            }
        }

        private UInt64 KnightAttack(int fromSquare) => Attack.Knight(fromSquare);
        private void GenerateKnightMoves(List<int> moves, UInt64 pinnedPieces, bool check)
        {
            GenerateStandardPieceMoves(moves, Knight, KnightAttack, pinnedPieces, check);
        }

        private UInt64 BishopAttack(int fromSquare) => Attack.Bishop(fromSquare, OccupiedBB);
        private void GenerateBishopMoves(List<int> moves, UInt64 pinnedPieces, bool check)
        {
            GenerateStandardPieceMoves(moves, Bishop, BishopAttack, pinnedPieces, check);
        }

        private UInt64 RookAttack(int fromSquare) => Attack.Rook(fromSquare, OccupiedBB);
        private void GenerateRookMoves(List<int> moves, UInt64 pinnedPieces, bool check)
        {
            GenerateStandardPieceMoves(moves, Rook, RookAttack, pinnedPieces, check);
        }

        private UInt64 QueenAttack(int fromSquare) => Attack.Queen(fromSquare, OccupiedBB);
        private void GenerateQueenMoves(List<int> moves, UInt64 pinnedPieces, bool check)
        {
            GenerateStandardPieceMoves(moves, Queen, QueenAttack, pinnedPieces, check);
        }

        private void GenerateStandardPieceMoves(List<int> moves, int Piece, Func<int, UInt64> attack, UInt64 pinnedPieces, bool check)
        {
            int ourColor = ColorToMove;
            UInt64 ourPieces = PieceBitboard[ourColor | Piece];
            UInt64 targetBitboard = ~PieceBitboard[ColorToMove];

            while (ourPieces != 0)
            {
                // Getting the origin square
                int fromSquare = BitOperations.PopLS(ref ourPieces);

                // Checking for pin
                bool pinned = (pinnedPieces & (1UL << fromSquare)) != 0;

                UInt64 moveBitboard = targetBitboard & attack(fromSquare);
                while (moveBitboard != 0)
                {
                    // Performing minimal necessary state changes to simulate real move and check for legality
                    Int32 toSquare = BitOperations.PopLS(ref moveBitboard);
                    int capturedPiece = SquarePiece[toSquare];

                    bool legal = (check) ? IsPseudoLegal(fromSquare, toSquare) : (!pinned || Bitboard.Aligned(KingSquare(ColorToMove), fromSquare, toSquare));
                    if (legal)
                        moves.Add(Move.Create(fromSquare, toSquare, ColorToMove | Piece, capturedPiece));
                }
            }
        }

        private void GenerateKingMoves(List<int> moves)
        {
            int ourColor = ColorToMove;
            UInt64 ourPieces = PieceBitboard[ourColor | King];
            UInt64 targetBitboard = ~PieceBitboard[ColorToMove];

            while (ourPieces != 0)
            {
                // Getting the origin square
                int squareFrom = BitOperations.PopLS(ref ourPieces);

                UInt64 moveBitboard = targetBitboard & Attack.King(squareFrom);
                while (moveBitboard != 0)
                {
                    Int32 squareTo = BitOperations.PopLS(ref moveBitboard);
                    int capturedPiece = SquarePiece[squareTo];

                    if (IsPseudoLegal(squareFrom, squareTo, true))
                        moves.Add(Move.Create(squareFrom, squareTo, ColorToMove | King, capturedPiece));
                }
            }
        }

        private UInt64 GetPinnedPieces()
        {
            UInt64 pinnedPieces = 0;

            int kingSq = KingSquare(ColorToMove);

            UInt64 blockingPieces = 0;
            blockingPieces = Attack.Bishop(kingSq, OccupiedBB) & PieceBitboard[ColorToMove];
            UInt64 xRayBishopAttacks = Attack.Bishop(kingSq, OccupiedBB ^ blockingPieces);
            UInt64 bishopAttackers = xRayBishopAttacks & (PieceBitboard[(1 - ColorToMove) | Bishop] | PieceBitboard[(1 - ColorToMove) | Queen]);
            while (bishopAttackers != 0)
            {
                int attacker = BitOperations.PopLS(ref bishopAttackers);
                pinnedPieces ^= Bitboard.SquaresBetween[attacker][kingSq] & blockingPieces;
            }

            blockingPieces = Attack.Rook(kingSq, OccupiedBB) & PieceBitboard[ColorToMove];
            UInt64 xRayRookAttacks = Attack.Rook(kingSq, OccupiedBB ^ blockingPieces);
            UInt64 rookAttackers = xRayRookAttacks & (PieceBitboard[(1 - ColorToMove) | Rook] | PieceBitboard[(1 - ColorToMove) | Queen]);
            while (rookAttackers != 0)
            {
                int attacker = BitOperations.PopLS(ref rookAttackers);
                pinnedPieces ^= Bitboard.SquaresBetween[attacker][kingSq] & blockingPieces;
            }

            return pinnedPieces;
        }
    }
}
