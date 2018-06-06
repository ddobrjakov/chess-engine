using System;

namespace PerfectChess
{
    static class BitOperations
    {
        private const UInt64 DeBruijn_64 = 0x37E84A99DAE458F;
        private static readonly int[] MagicTable =
        {
            0, 1, 17, 2, 18, 50, 3, 57,
            47, 19, 22, 51, 29, 4, 33, 58,
            15, 48, 20, 27, 25, 23, 52, 41,
            54, 30, 38, 5, 43, 34, 59, 8,
            63, 16, 49, 56, 46, 21, 28, 32,
            14, 26, 24, 40, 53, 37, 42, 7,
            62, 55, 45, 31, 13, 39, 36, 6,
            61, 44, 12, 35, 60, 11, 10, 9,
        };

        /// <summary>
        /// Returns index of Least Significant Bit.
        /// </summary>
        /// <param name="b">Binary number to scan.</param>
        /// <returns>Index of Least Significant Bit.</returns>
        internal static int BitScanForward(UInt64 b)
        {
            return MagicTable[((ulong)((long)b & -(long)b) * DeBruijn_64) >> 58];
        }

        /// <summary>
        /// Returns index of Most Significant Bit.
        /// </summary>
        /// <param name="b">Binary number to scan.</param>
        /// <returns>Index of Most Significant Bit.</returns>
        internal static int BitScanReverse(UInt64 b)
        {
            b |= b >> 1;
            b |= b >> 2;
            b |= b >> 4;
            b |= b >> 8;
            b |= b >> 16;
            b |= b >> 32;
            b &= ~(b >> 1);
            return MagicTable[b * DeBruijn_64 >> 58];
        }

        /// <summary>
        /// Returns index of the only set bit (assuming there is only one).
        /// </summary>
        /// <param name="b">Binary number to scan.</param>
        /// <returns>Index of the only set bit.</returns>
        internal static int OnlyBitIndex(UInt64 b)
        {
            return MagicTable[(b * DeBruijn_64) >> 58];
        }

        /// <summary>
        /// Returns index of Least Significant set bit and resets it.
        /// </summary>
        /// <param name="b">Binary number to scan.</param>
        /// <returns>Index of least significant set bit.</returns>
        internal static int PopLS(ref UInt64 b)
        {
            UInt64 isolatedLSBit = b & (0UL - b);
            b &= b - 1;
            return MagicTable[(isolatedLSBit * DeBruijn_64) >> 58];
        }

        /// <summary>
        /// Removes all set bits from the number, except the least significant one.
        /// </summary>
        /// <param name="bitboard">Binary number.</param>
        /// <returns>A binary number with only one set bit.</returns>
        internal static UInt64 Isolate(UInt64 b)
        {
            return b & (0UL - b);
        }

        /// <summary>
        /// Returns the number of set bits.
        /// </summary>
        /// <param name="b">Binary number.</param>
        /// <returns>Number of set bits.</returns>
        internal static int PopCount(UInt64 b)
        {
            int cnt = 0;
            while (b != 0)
            {
                b &= (b - 1);
                cnt++;
            }
            return cnt;
        }
    }
}
