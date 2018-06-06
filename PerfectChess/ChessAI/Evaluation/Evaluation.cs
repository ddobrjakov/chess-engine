using System;
using System.Runtime.CompilerServices;
using static PerfectChess.Piece;
using PerfectChess._Evaluation;

namespace PerfectChess
{
    internal class Evaluation
    {
        internal int Evaluate(Position position)
        {
            STATS_Evaluated++;

            bool isEnding = IsEnding(position);
            int[] value = new int[2];
            for (int color = White; color <= Black; color++)
            {
                // Pawns
                int[] pawnPositionTable = isEnding ? PiecePositionTables.PawnEnding[color] : PiecePositionTables.Pawn[color];
                UInt64 pawnBitboard = position.PieceBitboard[Pawn | color];
                while (pawnBitboard != 0)
                {
                    int index = BitOperations.PopLS(ref pawnBitboard);
                    value[color] += PieceCost.Pawn + pawnPositionTable[index];
                }

                // Knights
                UInt64 knightBitboard = position.PieceBitboard[Knight | color];
                while (knightBitboard != 0)
                {
                    int index = BitOperations.PopLS(ref knightBitboard);
                    value[color] += PieceCost.Knight + PiecePositionTables.Knight[color][index];
                }

                // Bishops
                UInt64 bishopBitboard = position.PieceBitboard[Bishop | color];
                while (bishopBitboard != 0)
                {
                    int index = BitOperations.PopLS(ref bishopBitboard);
                    value[color] += PieceCost.Bishop + PiecePositionTables.Bishop[color][index];
                }

                // Rooks
                UInt64 rookBitboard = position.PieceBitboard[Rook | color];
                while (rookBitboard != 0)
                {
                    int index = BitOperations.PopLS(ref rookBitboard);
                    value[color] += PieceCost.Rook + PiecePositionTables.Rook[color][index];
                }

                value[color] += PieceCost.Queen * BitOperations.PopCount(position.PieceBitboard[Queen | color]);

                // Kings
                int[] positionTable = isEnding ? PiecePositionTables.KingEnding[color] : PiecePositionTables.King[color];
                UInt64 kingBitboard = position.PieceBitboard[King | color];
                while (kingBitboard != 0)
                {
                    int index = BitOperations.PopLS(ref kingBitboard);
                    value[color] += PieceCost.King + positionTable[index];
                }
            }

            return value[White] - value[Black];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsEnding(Position position) =>
            (position.PieceBitboard[Queen | Black] | position.PieceBitboard[Queen | White]) == 0;

        internal const int Min = -500000;
        internal const int Max = +500000;
        internal const int CheckmateBonus = 100000;

        #region STATS
        public int STATS_Evaluated = 0;
        public void STATS_Reset()
        {
            STATS_Evaluated = 0;
        }
        #endregion
    }
}
