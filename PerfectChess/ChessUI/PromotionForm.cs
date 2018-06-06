using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static PerfectChess.Piece;

namespace PerfectChess
{
    public partial class PromotionForm : Form
    {
        public PromotionForm(int color)
        {
            InitializeComponent();

            Bitmap back = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(back);
            LinearGradientBrush backbrush = new LinearGradientBrush(new Point(0, 0), new Point(0, back.Height), System.Drawing.Color.FromArgb(0x2c, 0x2c, 0x2c), System.Drawing.Color.FromArgb(0x1a, 0x1a, 0x1a));
            g.FillRectangle(backbrush, 0, 0, back.Width, back.Height);
            this.BackgroundImage = back;
            this.Color = color;

            pictureBox1.Width = pictureBox1.Height = ChessViewSettings.SQUARE_SIZE;
            pictureBox2.Width = pictureBox2.Height = ChessViewSettings.SQUARE_SIZE;
            pictureBox3.Width = pictureBox3.Height = ChessViewSettings.SQUARE_SIZE;
            pictureBox4.Width = pictureBox4.Height = ChessViewSettings.SQUARE_SIZE;
            SetPiece(pictureBox1, Piece.Knight | color, White);
            SetPiece(pictureBox2, Piece.Bishop | color, Black);
            SetPiece(pictureBox3, Piece.Rook | color, White);
            SetPiece(pictureBox4, Piece.Queen | color, Black);
        }

        private void SetPiece(PictureBox pict, int piece, int squareColor)
        {
            Bitmap b = new Bitmap(ChessViewSettings.SQUARE_SIZE, ChessViewSettings.SQUARE_SIZE);
            Graphics g1 = Graphics.FromImage(b);

            Bitmap solidColorBMP = new Bitmap(ChessViewSettings.SQUARE_SIZE, ChessViewSettings.SQUARE_SIZE);
            Graphics g2 = Graphics.FromImage(solidColorBMP);
            g2.FillRectangle(new SolidBrush(ChessView.RealColor[squareColor]), 0, 0, solidColorBMP.Width, solidColorBMP.Height);

            g2.DrawImage(ChessView.PieceImage[piece], 0, 0, ChessViewSettings.SQUARE_SIZE, ChessViewSettings.SQUARE_SIZE);
            pict.BackgroundImage = solidColorBMP;
        }

        private int Color;

        public int PieceChosen { get; private set; } = 0;

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            PieceChosen = Piece.Knight | Color;
            this.Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            PieceChosen = Piece.Bishop | Color;
            this.Close();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            PieceChosen = Piece.Rook | Color;
            this.Close();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            PieceChosen = Piece.Queen | Color;
            this.Close();
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void pictureBox3_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void pictureBox3_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void pictureBox4_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void pictureBox4_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }
    }
}
