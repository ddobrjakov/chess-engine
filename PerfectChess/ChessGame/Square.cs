using System;
using System.Linq;

namespace PerfectChess
{
    /// <summary>
    /// Chess board square. 
    /// </summary>
    public struct Square
    {
        /// <summary>
        /// Get the square by its index.
        /// </summary>
        /// <param name="index">Index of the square.</param>
        /// <returns>The square.</returns>
        public static Square Get(int index)
        {
            return ALL[index];
        }

        /// <summary>
        /// Get the square by its string representation.
        /// </summary>
        /// <param name="square">String representation of the square.</param>
        /// <returns>The square.</returns>
        public static Square Get(string square)
        {
            if (square.Length != 2) throw new ArgumentException();
            string lower = square.ToLower();

            ChessFile file;
            switch (lower[0])
            {
                case 'a':
                    file = ChessFile.A;
                    break;
                case 'b':
                    file = ChessFile.B;
                    break;
                case 'c':
                    file = ChessFile.C;
                    break;
                case 'd':
                    file = ChessFile.D;
                    break;
                case 'e':
                    file = ChessFile.E;
                    break;
                case 'f':
                    file = ChessFile.F;
                    break;
                case 'g':
                    file = ChessFile.G;
                    break;
                case 'h':
                    file = ChessFile.H;
                    break;
                default: throw new ArgumentException();
            }

            if (!Char.IsDigit(lower[1]) || (Char.GetNumericValue(lower[1]) > 8) || (Char.GetNumericValue(lower[1]) < 1))
                throw new ArgumentException();

            int rank = (int)Char.GetNumericValue(lower[1]);
            return Get(file, rank);
        }

        /// <summary>
        /// Get the square by its file and rank.
        /// </summary>
        /// <param name="file">File of the square.</param>
        /// <param name="rank">Rank of the square.</param>
        /// <returns>The square.</returns>
        public static Square Get(ChessFile file, int rank)
        {
            return Get((int)file, rank);
        }

        /// <summary>
        /// Get the square by its numerical file and rank.
        /// </summary>
        /// <param name="file">Numerical file of the square.</param>
        /// <param name="rank">Rank of the square.</param>
        /// <returns>The square.</returns>
        public static Square Get(int file, int rank)
        {
            return ALL[(file - 1) + (rank - 1) * 8];
        }

        /// <summary>
        /// Array of all 64 squares.
        /// </summary>
        public static readonly Square[] ALL;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static Square()
        {
            // Initialize 64 squares
            ALL = Enumerable.Range(0, 64).Select(i => new Square(i % 8 + 1, i / 8 + 1)).ToArray();
        }

        /// <summary>
        /// Create a Square instance by file and rank.
        /// </summary>
        /// <param name="file">File of the square.</param>
        /// <param name="rank">Rank of the square.</param>
        private Square(ChessFile file, int rank)
        {
            if (rank < 1 || rank > 8) throw new ArgumentException();
            this.File = file;
            this.Rank = rank;
        }

        /// <summary>
        /// Create a Square instance by numerical file and rank.
        /// </summary>
        /// <param name="file">Numerical file of the square.</param>
        /// <param name="rank">Rank of the square.</param>
        private Square(int file, int rank)
        {
            this.File = (ChessFile)file;
            this.Rank = rank;
        }

        /// <summary>
        /// Enumeration of 8 chess files.
        /// </summary>
        public enum ChessFile { A = 1, B, C, D, E, F, G, H }

        /// <summary>
        /// File of the square.
        /// </summary>
        public ChessFile File { get; private set; }

        /// <summary>
        /// Rank of the square.
        /// </summary>
        public int Rank { get; private set; }

        /// <summary>
        /// Color of the square.
        /// </summary>
        public int Color => (X + Y + 1) % 2;

        /// <summary>
        /// File of the square as integer in the range of 0-7.
        /// </summary>
        public int X => (int)File - 1;

        /// <summary>
        /// Rank of the square as integer in the range of 0-7.
        /// </summary>
        public int Y => Rank - 1;

        /// <summary>
        /// Index of the square as integer in the range of 0-63.
        /// </summary>
        public int Index => X + 8 * Y;

        public override string ToString()
        {
            return "[" + File + Rank + "]";
        }

        public static bool operator ==(Square s1, Square s2)
        {
            return (s1.File == s2.File && s1.Rank == s2.Rank);
        }

        public static bool operator !=(Square s1, Square s2)
        {
            return !(s1 == s2);
        }

        public override bool Equals(object o)
        {
            if (!(o is Square)) return false;
            return ((Square)(o) == this);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}