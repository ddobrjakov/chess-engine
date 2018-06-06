using System;
using static PerfectChess.Piece;

namespace PerfectChess
{
    static class Attack
    {
        #region Precomputed

        static Attack()
        {
            for (int square = 0; square < 64; square++)
            {
                int file = Position.File(square);
                int rank = Position.Rank(square);

                // Knight
                for (int x = -2; x <= 2; x++)
                    for (int y = -2; y <= 2; y++)
                        if (Math.Abs(x) + Math.Abs(y) == 3)
                            KnightAttack[square] ^= TryGetBitboard(file + x, rank + y);

                // King
                for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                        if ((x | y) != 0)
                            KingAttack[square] ^= TryGetBitboard(file + x, rank + y);

                // Pawns
                PawnAttack[White][square] ^= TryGetBitboard(file - 1, rank + 1);
                PawnAttack[White][square] ^= TryGetBitboard(file + 1, rank + 1);
                PawnAttack[Black][square] ^= TryGetBitboard(file - 1, rank - 1);
                PawnAttack[Black][square] ^= TryGetBitboard(file + 1, rank - 1);
            }
        }

        static readonly UInt64[] KnightAttack = new UInt64[64];

        static readonly UInt64[] KingAttack = new UInt64[64];

        static readonly UInt64[][] PawnAttack = { new UInt64[64], new UInt64[64] };

        private static UInt64 TryGetBitboard(int x, int y)
        {
            if (x < 0 || x > 7 || y < 0 || y > 7) return 0;
            return 1UL << (x + 8 * y);
        }

        #endregion

        internal static UInt64 Knight(int square)
        {
            return KnightAttack[square];
        }

        internal static UInt64 King(int square)
        {
            return KingAttack[square];
        }

        internal static UInt64 Pawn(int color, int square)
        {
            return PawnAttack[color][square];
        }

        internal static UInt64 Rook(int square, UInt64 occupiedBitboard)
        {
            UInt64 blockersBitboard, partAttackBitboard, attackBitboard;

            attackBitboard = Bitboard.RayN[square];
            blockersBitboard = attackBitboard & occupiedBitboard;
            if (blockersBitboard != 0)
            {
                int blockingSquare = BitOperations.BitScanForward(blockersBitboard);
                attackBitboard ^= Bitboard.RayN[blockingSquare];
            }

            partAttackBitboard = Bitboard.RayE[square];
            blockersBitboard = partAttackBitboard & occupiedBitboard;
            if (blockersBitboard != 0)
            {
                int blockingSquare = BitOperations.BitScanForward(blockersBitboard);
                partAttackBitboard ^= Bitboard.RayE[blockingSquare];
            }
            attackBitboard |= partAttackBitboard;

            partAttackBitboard = Bitboard.RayS[square];
            blockersBitboard = partAttackBitboard & occupiedBitboard;
            if (blockersBitboard != 0)
            {
                int blockingSquare = BitOperations.BitScanReverse(blockersBitboard);
                partAttackBitboard ^= Bitboard.RayS[blockingSquare];
            }
            attackBitboard |= partAttackBitboard;

            partAttackBitboard = Bitboard.RayW[square];
            blockersBitboard = partAttackBitboard & occupiedBitboard;
            if (blockersBitboard != 0)
            {
                int blockingSquare = BitOperations.BitScanReverse(blockersBitboard);
                partAttackBitboard ^= Bitboard.RayW[blockingSquare];
            }

            return attackBitboard | partAttackBitboard;
        }

        internal static UInt64 Bishop(int square, UInt64 occupiedBitboard)
        {
            UInt64 blockersBitboard, partAttackBitboard, attackBitboard;

            attackBitboard = Bitboard.RayNE[square];
            blockersBitboard = attackBitboard & occupiedBitboard;
            if (blockersBitboard != 0)
            {
                int blockingSquare = BitOperations.BitScanForward(blockersBitboard);
                attackBitboard ^= Bitboard.RayNE[blockingSquare];
            }

            partAttackBitboard = Bitboard.RaySE[square];
            blockersBitboard = partAttackBitboard & occupiedBitboard;
            if (blockersBitboard != 0)
            {
                int blockingSquare = BitOperations.BitScanReverse(blockersBitboard);
                partAttackBitboard ^= Bitboard.RaySE[blockingSquare];
            }
            attackBitboard |= partAttackBitboard;

            partAttackBitboard = Bitboard.RaySW[square];
            blockersBitboard = partAttackBitboard & occupiedBitboard;
            if (blockersBitboard != 0)
            {
                int blockingSquare = BitOperations.BitScanReverse(blockersBitboard);
                partAttackBitboard ^= Bitboard.RaySW[blockingSquare];
            }
            attackBitboard |= partAttackBitboard;

            partAttackBitboard = Bitboard.RayNW[square];
            blockersBitboard = partAttackBitboard & occupiedBitboard;
            if (blockersBitboard != 0)
            {
                int blockingSquare = BitOperations.BitScanForward(blockersBitboard);
                partAttackBitboard ^= Bitboard.RayNW[blockingSquare];
            }

            return attackBitboard | partAttackBitboard;
        }

        internal static UInt64 Queen(int square, UInt64 occupiedBitboard)
        {
            return Rook(square, occupiedBitboard) | Bishop(square, occupiedBitboard);
        }
    }
}
