using System;

namespace PerfectChess
{
    static class Bitboard
    {
        internal static readonly UInt64[] RayN = new UInt64[64];
        internal static readonly UInt64[] RayE = new UInt64[64];
        internal static readonly UInt64[] RayS = new UInt64[64];
        internal static readonly UInt64[] RayW = new UInt64[64];

        internal static readonly UInt64[] RayNE = new UInt64[64];
        internal static readonly UInt64[] RaySE = new UInt64[64];
        internal static readonly UInt64[] RaySW = new UInt64[64];
        internal static readonly UInt64[] RayNW = new UInt64[64];

        internal static readonly UInt64[] Diagonals = new UInt64[64];
        internal static readonly UInt64[] Axes = new UInt64[64];
        internal static readonly UInt64[] Square = new UInt64[64];
        internal static readonly UInt64[][] SquaresBetween = new UInt64[64][];

        internal const UInt64 FileABB = 0b00000001000000010000000100000001000000010000000100000001;
        internal const UInt64 FileBBB = 0b00000010000000100000001000000010000000100000001000000010;
        internal const UInt64 FileCBB = 0b00000100000001000000010000000100000001000000010000000100;
        internal const UInt64 FileDBB = 0b00001000000010000000100000001000000010000000100000001000;
        internal const UInt64 FileEBB = 0b00010000000100000001000000010000000100000001000000010000;
        internal const UInt64 FileFBB = 0b00100000001000000010000000100000001000000010000000100000;
        internal const UInt64 FileGBB = 0b01000000010000000100000001000000010000000100000001000000;
        internal const UInt64 FileHBB = 0b10000000100000001000000010000000100000001000000010000000;

        internal const UInt64 Rank1BB = 0xFF;
        internal const UInt64 Rank2BB = 0xFF00;
        internal const UInt64 Rank3BB = 0xFF0000;
        internal const UInt64 Rank4BB = 0xFF000000;
        internal const UInt64 Rank5BB = 0xFF00000000;
        internal const UInt64 Rank6BB = 0xFF0000000000;
        internal const UInt64 Rank7BB = 0xFF000000000000;
        internal const UInt64 Rank8BB = 0xFF00000000000000;

        internal const UInt64 InitialPawnPositionsBB = Rank2BB | Rank7BB;

        static Bitboard()
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                RayN[squareIndex] = GetRayBitboard(squareIndex, 0, 1) ^ (1UL << squareIndex);
                RayE[squareIndex] = GetRayBitboard(squareIndex, 1, 0) ^ (1UL << squareIndex);
                RayS[squareIndex] = GetRayBitboard(squareIndex, 0, -1) ^ (1UL << squareIndex);
                RayW[squareIndex] = GetRayBitboard(squareIndex, -1, 0) ^ (1UL << squareIndex);

                RayNE[squareIndex] = GetRayBitboard(squareIndex, 1, 1) ^ (1UL << squareIndex);
                RaySE[squareIndex] = GetRayBitboard(squareIndex, 1, -1) ^ (1UL << squareIndex);
                RaySW[squareIndex] = GetRayBitboard(squareIndex, -1, -1) ^ (1UL << squareIndex);
                RayNW[squareIndex] = GetRayBitboard(squareIndex, -1, 1) ^ (1UL << squareIndex);

                Axes[squareIndex] = RayN[squareIndex] | RayE[squareIndex] | RayS[squareIndex] | RayW[squareIndex];
                Diagonals[squareIndex] = RayNE[squareIndex] | RaySE[squareIndex] | RaySW[squareIndex] | RayNW[squareIndex];
                Square[squareIndex] = 1UL << squareIndex;
            }

            for (int squareIndex1 = 0; squareIndex1 < 64; squareIndex1++)
            {
                SquaresBetween[squareIndex1] = new UInt64[64];
                for (int squareIndex2 = 0; squareIndex2 < 64; squareIndex2++)
                {
                    if ((RayN[squareIndex1] & (1UL << squareIndex2)) != 0) SquaresBetween[squareIndex1][squareIndex2] = RayN[squareIndex1] ^ RayN[squareIndex2] ^ (1UL << squareIndex2);
                    if ((RayE[squareIndex1] & (1UL << squareIndex2)) != 0) SquaresBetween[squareIndex1][squareIndex2] = RayE[squareIndex1] ^ RayE[squareIndex2] ^ (1UL << squareIndex2);
                    if ((RayS[squareIndex1] & (1UL << squareIndex2)) != 0) SquaresBetween[squareIndex1][squareIndex2] = RayS[squareIndex1] ^ RayS[squareIndex2] ^ (1UL << squareIndex2);
                    if ((RayW[squareIndex1] & (1UL << squareIndex2)) != 0) SquaresBetween[squareIndex1][squareIndex2] = RayW[squareIndex1] ^ RayW[squareIndex2] ^ (1UL << squareIndex2);
                    if ((RayNE[squareIndex1] & (1UL << squareIndex2)) != 0) SquaresBetween[squareIndex1][squareIndex2] = RayNE[squareIndex1] ^ RayNE[squareIndex2] ^ (1UL << squareIndex2);
                    if ((RaySE[squareIndex1] & (1UL << squareIndex2)) != 0) SquaresBetween[squareIndex1][squareIndex2] = RaySE[squareIndex1] ^ RaySE[squareIndex2] ^ (1UL << squareIndex2);
                    if ((RaySW[squareIndex1] & (1UL << squareIndex2)) != 0) SquaresBetween[squareIndex1][squareIndex2] = RaySW[squareIndex1] ^ RaySW[squareIndex2] ^ (1UL << squareIndex2);
                    if ((RayNW[squareIndex1] & (1UL << squareIndex2)) != 0) SquaresBetween[squareIndex1][squareIndex2] = RayNW[squareIndex1] ^ RayNW[squareIndex2] ^ (1UL << squareIndex2);
                }
            }
        }

        private static UInt64 GetRayBitboard(int square, int dx, int dy)
        {
            if (square > 63 || square < 0) return 0;
            UInt64 bitboard = 1UL << square;
            if (Math.Floor(square / 8F) == Math.Floor((square + dx) / 8F))
                bitboard |= GetRayBitboard(square + dx + 8 * dy, dx, dy);
            return bitboard;
        }

        internal static int X(int square) => square & 7;
        internal static int Y(int square) => square >> 3;

        internal static bool Aligned(int square1, int square2, int square3)
        {
            int dy1dx2 = (Y(square2) - Y(square1)) * (X(square3) - X(square2));
            int dx1dy2 = (X(square2) - X(square1)) * (Y(square3) - Y(square2));
            return dy1dx2 == dx1dy2;
        }

        internal static string ToString(UInt64 bitboard)
        {
            string res = string.Empty;

            UInt64 bits8;
            for (int j = 0; j < 8; j++)
            {
                bits8 = (bitboard & 0xFF00000000000000) >> 56;
                for (int i = 0; i < 8; i++)
                {
                    res += bits8 & 1;
                    bits8 >>= 1;
                }
                res += "\n";
                bitboard <<= 8;
            }

            return res;
        }
    }
}
