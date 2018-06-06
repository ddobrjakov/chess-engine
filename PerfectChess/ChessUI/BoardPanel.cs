using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using static PerfectChess.Piece;

namespace PerfectChess
{    
    /// <summary>
    /// Chess board panel.
    /// </summary>
    public class BoardPanel : Panel
    {
        public BoardPanel()
        {
            ControlStyles flag = 
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint;
            SetStyle(flag, true);

            this.DoubleBuffered = true;
            this.Width = ChessViewSettings.SQUARE_SIZE * 8;
            this.Height = ChessViewSettings.SQUARE_SIZE * 8;
            this.BackgroundImage = new Bitmap(this.Width, this.Height);
            this.Reset();
        }

        #region Stored data.
        /// <summary>
        /// Pieces on their squares.
        /// </summary>
        private int[] Pieces = new int[64];

        /// <summary>
        /// List of squares marked as attacked.
        /// </summary>
        private List<Square> AttackedSquares = new List<Square>();

        /// <summary>
        /// List of highlighted pairs of squares.
        /// </summary>
        private List<(Square From, Square To)> HighlightedSquares = new List<(Square From, Square To)>();

        /// <summary>
        /// Active moves (happenning at the moment).
        /// </summary>
        private Dictionary<int, (Square S, Point CurrentLocation)> ActiveMoves = new Dictionary<int, (Square S, Point CurrentLocation)>();

        /// <summary>
        /// Returns the image of the piece on a given square.
        /// </summary>
        /// <param name="square">Square to get the image of.</param>
        /// <returns>Image of the piece.</returns>
        private Image SquarePieceImage(Square square)
        {
            return ChessView.PieceImage[Pieces[square.Index]];
        }

        /// <summary>
        /// Returns if the board is flipped.
        /// </summary>
        public bool Flipped { get; private set; } = false;

        /// <summary>
        /// Returns the square at given location (throws exception if there is none).
        /// </summary>
        /// <param name="location">Location of the square.</param>
        /// <returns>Square at the location.</returns>
        public Square GetSquare(Point location)
        {
            if (location.X > 8 * ChessViewSettings.SQUARE_SIZE || location.Y > 8 * ChessViewSettings.SQUARE_SIZE)
                throw new ArgumentException();

            int file, rank;
            if (!Flipped)
            {
                file = location.X / ChessViewSettings.SQUARE_SIZE + 1;
                rank = 8 - location.Y / ChessViewSettings.SQUARE_SIZE;
            }
            else
            {
                file = 8 - location.X / ChessViewSettings.SQUARE_SIZE;
                rank = location.Y / ChessViewSettings.SQUARE_SIZE + 1;
            }
            return Square.Get(file, rank);
        }

        /// <summary>
        /// Returns the origin square of the active move with a given identifier.
        /// </summary>
        /// <param name="identifier">Move identifier.</param>
        /// <returns>Origin square of the move.</returns>
        public Square FromSquare(int identifier) => ActiveMoves[identifier].S;
        #endregion

        #region Public methods.
        /// <summary>
        /// Flips the board.
        /// </summary>
        /// <param name="update">Update the view afterwards.</param>
        public void Flip(bool update = true)
        {
            Flipped = !Flipped;
            if (update) UpdateView();
        }

        /// <summary>
        /// Sets new positions of pieces.
        /// </summary>
        /// <param name="pieces">New positions of pieces.</param>
        /// <param name="update">Update the view afterwards.</param>
        public void SetPosition(int[] pieces, bool update = true)
        {
            this.Pieces = new int[64];
            for (int i = 0; i < 64; i++) this.Pieces[i] = pieces[i];
            if (update) UpdateView();
        }

        /// <summary>
        /// Removes all pieces from the board and deletes the effects.
        /// </summary>
        /// <param name="update">Update the view afterwards.</param>
        public void Reset(bool update = true)
        {
            SetPosition(new int[64], false);
            DeleteEffects(update);
        }

        /// <summary>
        /// Removes a piece and deletes the square's effects.
        /// </summary>
        /// <param name="s">Square to reset.</param>
        /// <param name="update">Update the view afterwards.</param>
        private void ResetSquare(Square square, bool update = true)
        {
            Pieces[square.Index] = 0;
            DeleteEffects(square, update);
        }

        /// <summary>
        /// Sets a new piece on a square.
        /// </summary>
        /// <param name="square">Square for the piece.</param>
        /// <param name="piece">Piece to put on the square.</param>
        /// <param name="update">Update the view afterwards.</param>
        public void SetPiece(Square square, int piece, bool update = true)
        {
            Pieces[square.Index] = piece;
            if (update) UpdateSquareView(square);
        }

        /// <summary>
        /// Removes a piece (but saves effects).
        /// </summary>
        /// <param name="square">Square to remove a piece from.</param>
        /// <param name="update">Update the view afterwards.</param>
        public void ResetPiece(Square square, bool update = true)
        {
            Pieces[square.Index] = 0;
            if (update) UpdateSquareView(square);
        }

        /// <summary>
        /// Marks all given squares as attacked.
        /// </summary>
        /// <param name="attacked">Squares to mark as attacked.</param>
        /// <param name="update">Update the view afterwards.</param>
        public void MarkAttacked(IEnumerable<Square> attacked, bool update = true)
        {
            foreach (Square square in attacked)
            {
                AttackedSquares.Add(square);
                if (update) UpdateSquareView(square);
            }
        }

        /// <summary>
        /// Highlights the origin and target squares of a move.
        /// </summary>
        /// <param name="from">Origin square of the move.</param>
        /// <param name="to">Target square of the move.</param>
        /// <param name="update">Update the view afterwards.</param>
        public void ShowLastMove(Square from, Square to, bool update = true)
        {
            HighlightedSquares.Add((from, to));
            if (update) UpdateView();
        }

        /// <summary>
        /// Deletes all effects.
        /// </summary>
        /// <param name="update">Update the view afterwards.</param>
        public void DeleteEffects(bool update = true)
        {
            this.AttackedSquares.Clear();
            this.HighlightedSquares.Clear();
            if (update) UpdateView();
        }

        /// <summary>
        /// Deletes all effects on a square.
        /// </summary>
        /// <param name="square">Square to delete effects from.</param>
        /// <param name="update">Update the view afterwards.</param>
        public void DeleteEffects(Square square, bool update = true)
        {
            while (AttackedSquares.Contains(square)) AttackedSquares.Remove(square);
            foreach ((Square s1, Square s2) pair in HighlightedSquares)
                if (pair.s1 == square || pair.s2 == square) HighlightedSquares.Remove(pair);
            if (update) UpdateSquareView(square);
        }

        /// <summary>
        /// Deletes all "attacked" effects.
        /// </summary>
        /// <param name="update">Update the view afterwards.</param>
        public void DeleteMarkedEffects(bool update = true)
        {
            this.AttackedSquares.Clear();
            if (update) UpdateView();
        }

        /// <summary>
        /// Starts new move.
        /// </summary>
        /// <param name="from">Origin square of the move.</param>
        /// <param name="update">Update the view afterwards.</param>
        /// <returns>Move identifier.</returns>
        public int StartMove(Square from, bool update = true)
        {
            int identifier = (ActiveMoves.Keys.Any()) ? ActiveMoves.Keys.Max() + 1 : 1;
            Point initialLocation = new Point(GetLocationRectangle(from).X, GetLocationRectangle(from).Y);
            ActiveMoves.Add(identifier, (from, initialLocation));
            ContinueMove(identifier, initialLocation);
            return identifier;
        }

        /// <summary>
        /// Continues existing active move.
        /// </summary>
        /// <param name="identifier">Move identifier.</param>
        /// <param name="newLocation">New piece location.</param>
        /// <param name="update">Update the view afterwards.</param>
        public void ContinueMove(int identifier, Point newLocation, bool update = true)
        {
            (Square square, Point oldLocation) = ActiveMoves[identifier];
            ActiveMoves[identifier] = (square, newLocation);
            if (update) UpdateView();
        }

        /// <summary>
        /// Deletes existing active move.
        /// </summary>
        /// <param name="identifier">Move identifier.</param>
        /// <param name="update">Update the view afterwards.</param>
        public void DeleteMove(int identifier, bool update = true)
        {
            if (!ActiveMoves.Keys.Contains(identifier)) throw new Exception("There is no active move with such identifier");
            ActiveMoves.Remove(identifier);
            if (update) UpdateView();
        }

        /// <summary>
        /// Executes a chess move, relocating one piece.
        /// </summary>
        /// <param name="from">Origin square of the move.</param>
        /// <param name="to">Target square of the move.</param>
        /// <param name="update">Update the view afterwards.</param>
        public void ExecuteMove(Square from, Square to, bool update = true)
        {
            Pieces[to.Index] = Pieces[from.Index];
            Pieces[from.Index] = 0;
            if (update) UpdateView();
        }
        #endregion

        #region Update view in accordance with stored data.
        /// <summary>
        /// Visualizes stored data for all squares on the board (updates the board with up-to-date state).
        /// </summary>
        private void UpdateView()
        {
            foreach (Square square in Square.ALL)
                UpdateSquareView(square, false);
            Effect_ShowActiveMoves();
        }

        /// <summary>
        /// Visualizes stored data about a given square (updates the square [and anything related] with up-to-date state).
        /// </summary>
        /// <param name="square">Square to refresh.</param>
        /// <param name="updateActiveMoves">Update active moves after updating the square.</param>
        private void UpdateSquareView(Square square, bool updateActiveMoves = true)
        {
            // Reset the square completely
            FillSquareWithDefaultColor(square);

            // Draw a piece image
            int piece = Pieces[square.Index];
            if (piece != 0)
                DrawImage(square, ChessView.PieceImage[piece]);

            // Apply effects

            // Attacked
            if (AttackedSquares.Contains(square))
                Effect_MarkAttacked(square);

            // Highlighted
            foreach ((Square from, Square to) in HighlightedSquares)
                if (from == square || to == square)
                    Effect_HighlightLastMove(from, to);

            // Active moves
            if (updateActiveMoves)
                Effect_ShowActiveMoves();
        }
        #endregion

        #region Apply visual effects to the board.
        /// <summary>
        /// Visually marks a square as attacked.
        /// </summary>
        /// <param name="square">Square to mark as attacked.</param>
        private void Effect_MarkAttacked(Square square)
        {
            if (Pieces[square.Index] != 0) Effect_MarkAttackedOccupied(square);
            else Effect_MarkAttackedFree(square);
        }

        /// <summary>
        /// Visually marks an occupied square as attacked.
        /// </summary>
        /// <param name="square">Square to mark as attacked.</param>
        private void Effect_MarkAttackedOccupied(Square square)
        {
            System.Drawing.Color colorToFill = (square.Color == Piece.White) ? ChessViewSettings.WHITE_AVAILIBLE_COLOR : ChessViewSettings.BLACK_AVAILIBLE_COLOR;
            FillSquareWithColor(square, colorToFill);
            DrawImage(square, SquarePieceImage(square));
        }

        /// <summary>
        /// Visually marks an empty square as attacked.
        /// If the square contains a piece, method `Effect_MarkAttackedOccupied` should be called instead.
        /// </summary>
        /// <param name="square"></param>
        private void Effect_MarkAttackedFree(Square square)
        {
            DrawImageCenter(square, ChessViewSettings.CIRCLE_FILLED);
        }

        /// <summary>
        /// Visually highlights origin and target squares of a move with different colors.
        /// </summary>
        /// <param name="from">Origin square of a move.</param>
        /// <param name="to">Target square of a move.</param>
        private void Effect_HighlightLastMove(Square from, Square to)
        {
            System.Drawing.Color fromColor = (from.Color == Piece.White) ? System.Drawing.Color.FromArgb(205, 210, 106) : System.Drawing.Color.FromArgb(170, 162, 58);
            System.Drawing.Color toColor = (to.Color == Piece.White) ? System.Drawing.Color.FromArgb(205, 210, 106) : System.Drawing.Color.FromArgb(170, 162, 58);

            FillSquareWithColor(from, fromColor);
            if (Pieces[from.Index] != None) DrawImage(from, SquarePieceImage(from));
            FillSquareWithColor(to, toColor);
            if (Pieces[to.Index] != None) DrawImage(to, SquarePieceImage(to));
        }

        /// <summary>
        /// Updates the board with active moves.
        /// </summary>
        private void Effect_ShowActiveMoves()
        {
            // Clearing origin squares
            foreach (var (square, currentLocation) in ActiveMoves.Values)
            {
                Effect_MarkMovingFrom(square);
            }

            // Drawing the floating pieces
            foreach (var (square, currentLocation) in ActiveMoves.Values)
            {
                Size squareSize = new Size(ChessViewSettings.SQUARE_SIZE, ChessViewSettings.SQUARE_SIZE);
                Rectangle floatingRectangle = new Rectangle(currentLocation, squareSize);
                BoardGraphics.DrawImage(SquarePieceImage(square), floatingRectangle);
            }
        }

        /// <summary>
        /// Visually marks a square as origin of active move.
        /// </summary>
        /// <param name="square">Square to mark as origin of active move.</param>
        private void Effect_MarkMovingFrom(Square square)
        {
            FillSquareWithDefaultColor(square);

            ColorMatrix cm = new ColorMatrix();
            cm.Matrix33 = 0.60f;

            ImageAttributes ia = new ImageAttributes();
            ia.SetColorMatrix(cm);

            BoardGraphics.DrawImage(SquarePieceImage(square), GetLocationRectangle(square), 0, 0, ChessViewSettings.SQUARE_SIZE, ChessViewSettings.SQUARE_SIZE, GraphicsUnit.Pixel, ia);
        }
        #endregion

        #region Draw.
        /// <summary>
        /// Graphics we are always drawing on (graphics of background image).
        /// </summary>
        private Graphics BoardGraphics;

        /// <summary>
        /// Chess board background image.
        /// Overrides the BackgroundImage setter in order to maintain BoardGraphics up-to-date.
        /// </summary>
        public override Image BackgroundImage
        {
            get => base.BackgroundImage;
            set
            {
                base.BackgroundImage = value;
                BoardGraphics = Graphics.FromImage(this.BackgroundImage);
            }
        }

        /// <summary>
        /// Draws an image in a square.
        /// </summary>
        /// <param name="square">Square to draw an image in.</param>
        /// <param name="image">Image to draw.</param>
        private void DrawImage(Square square, Image image)
        {
            BoardGraphics.DrawImage(image, GetLocationRectangle(square));
        }

        /// <summary>
        /// Draws an image in the center of a square using its original size.
        /// </summary>
        /// <param name="square">Square to draw the image in.</param>
        /// <param name="image">Image to draw.</param>
        private void DrawImageCenter(Square square, Image image)
        {
            Rectangle locationRectangle = new Rectangle();
            int x, y;

            if (!Flipped)
            {
                x = (int)(square.File - 1) * ChessViewSettings.SQUARE_SIZE;
                y = (8 - square.Rank) * ChessViewSettings.SQUARE_SIZE;
            }
            else
            {
                x = (int)(8 - square.File) * ChessViewSettings.SQUARE_SIZE;
                y = (square.Rank - 1) * ChessViewSettings.SQUARE_SIZE;
            }

            locationRectangle.X = x + (ChessViewSettings.SQUARE_SIZE - image.Width) / 2;
            locationRectangle.Y = y + (ChessViewSettings.SQUARE_SIZE - image.Height) / 2;
            locationRectangle.Width = image.Width;
            locationRectangle.Height = image.Height;

            BoardGraphics.DrawImage(image, locationRectangle);
        }

        /// <summary>
        /// Fills a square with one color.
        /// </summary>
        /// <param name="square">Square to fill.</param>
        /// <param name="color">Color to fill with.</param>
        private void FillSquareWithColor(Square square, System.Drawing.Color color)
        {
            Bitmap colorBMP = new Bitmap(ChessViewSettings.SQUARE_SIZE, ChessViewSettings.SQUARE_SIZE);
            Graphics.FromImage(colorBMP).FillRectangle(new SolidBrush(color), 0, 0, colorBMP.Width, colorBMP.Height);
            DrawImage(square, colorBMP);
        }

        /// <summary>
        /// Fills a square with default color.
        /// </summary>
        /// <param name="square">Square to fill.</param>
        private void FillSquareWithDefaultColor(Square square)
        {
            FillSquareWithColor(square, (square.Color == White) ?
                        ChessViewSettings.WHITE_SQUARE_COLOR : ChessViewSettings.BLACK_SQUARE_COLOR);
        }

        /// <summary>
        /// Returns the rectangle containing a chess board square.
        /// </summary>
        /// <param name="square">Square to get the rectangle for.</param>
        /// <returns>Rectangle containing a chess board square.</returns>
        private Rectangle GetLocationRectangle(Square square)
        {
            Rectangle locationRectangle = new Rectangle();
            if (!Flipped)
            {
                locationRectangle.X = (int)(square.File - 1) * ChessViewSettings.SQUARE_SIZE;
                locationRectangle.Y = (8 - square.Rank) * ChessViewSettings.SQUARE_SIZE;
                locationRectangle.Width = locationRectangle.Height = ChessViewSettings.SQUARE_SIZE;
            }
            else
            {
                locationRectangle.X = (int)(8 - square.File) * ChessViewSettings.SQUARE_SIZE;
                locationRectangle.Y = (square.Rank - 1) * ChessViewSettings.SQUARE_SIZE;
                locationRectangle.Width = locationRectangle.Height = ChessViewSettings.SQUARE_SIZE;
            }
            return locationRectangle;
        }

        /// <summary>
        /// Draws a border of requested color around the given square.
        /// </summary>
        /// <param name="square">Square to draw a border around.</param>
        /// <param name="color">Color of the border.</param>
        private void DrawBorder(Square square, System.Drawing.Color color)
        {
            Pen pen = new Pen(color) { Alignment = PenAlignment.Inset };
            BoardGraphics.DrawRectangle(pen, GetLocationRectangle(square));
        }
        #endregion
    }
}
