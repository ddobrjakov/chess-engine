using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static PerfectChess.Piece;
using static PerfectChess.Move;
using System.Drawing.Drawing2D;

namespace PerfectChess
{
    public partial class MainForm : Form, IGameView
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeCustomControls();
            InitializeStyle();
            SetPosition(new Position());
        }

        private void InitializeStyle()
        {
            // Background colors
            var backgroundColor1 = System.Drawing.Color.FromArgb(0x2c, 0x2c, 0x2c);
            var backgroundColor2 = System.Drawing.Color.FromArgb(0x1a, 0x1a, 0x1a);
            
            // Button colors
            var buttonColor1 = System.Drawing.Color.FromArgb(0xce, 0xc5, 0xc5);
            var buttonColor2 = System.Drawing.Color.FromArgb(0xA3, 0x8f, 0x8f);

            // Other colors
            var transparent = System.Drawing.Color.FromArgb(0, System.Drawing.Color.White);

            // MainForm
            SetGradientBackground(this, new Point(0, 0), new Point(0, this.Height), backgroundColor1, backgroundColor2);

            // ButtonUndo
            SetGradientBackground(UndoButton, new Point(0, 0), new Point(0, UndoButton.Height), buttonColor1, buttonColor2);
            UndoButton.FlatStyle = FlatStyle.Flat;
            UndoButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            UndoButton.FlatAppearance.BorderSize = 1;

            // NewGameButton
            SetGradientBackground(NewGameButton, new Point(0, 0), new Point(0, NewGameButton.Height), buttonColor1, buttonColor2);
            NewGameButton.FlatStyle = FlatStyle.Flat;
            NewGameButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            NewGameButton.FlatAppearance.BorderSize = 1;

            // FlipButton
            SetGradientBackground(FlipButton, new Point(0, 0), new Point(FlipButton.Width, FlipButton.Height), buttonColor1, buttonColor2);
            FlipButton.FlatStyle = FlatStyle.Flat;
            FlipButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            FlipButton.FlatAppearance.BorderSize = 1;

            // MaterialLabel1 and MaterialLabel2
            MaterialLabel1.BackColor = transparent;
            MaterialLabel2.BackColor = transparent;

            // LogTextBox
            LogTextBox.BackColor = buttonColor1;
        }

        private void InitializeCustomControls()
        {
            // Add chess board
            BoardPanel = new BoardPanel { Location = new Point(50, 50) };
            this.Controls.Add(BoardPanel);

            // Subscribe to chess board events
            BoardPanel.MouseDown += BoardPanel_MouseDown;
            BoardPanel.MouseMove += BoardPanel_MouseMove;
            BoardPanel.MouseUp += BoardPanel_MouseUp;
            BoardPanel.MouseEnter += BoardPanel_MouseEnter;
            BoardPanel.MouseLeave += BoardPanel_MouseLeave;
        }

        private void SetGradientBackground(Control control, Point from, Point to, System.Drawing.Color fromColor, System.Drawing.Color toColor)
        {
            Bitmap back = new Bitmap(control.Width, control.Height);
            Graphics graphics = Graphics.FromImage(back);
            LinearGradientBrush backbrush = new LinearGradientBrush(from, to, fromColor, toColor);
            graphics.FillRectangle(backbrush, 0, 0, back.Width, back.Height);
            control.BackgroundImage = back;
            control.BackgroundImageLayout = ImageLayout.Stretch;
        }

        public BoardPanel BoardPanel { get; private set; }
       
        /// <summary>
        /// Allow to move pieces with the mouse.
        /// </summary>
        private bool MoveStartAllowed = false;

        /// <summary>
        /// Mouse is pressed.
        /// </summary>
        private bool MousePressed = false;

        /// <summary>
        /// Identifier of the move that is started with the mouse.
        /// </summary>
        private int CurrentMoveIdentifier = 0;

        /// <summary>
        /// Executes a move without using the mouse.
        /// </summary>
        /// <param name="move"></param>
        private void ExecuteMove(int move)
        {
            Square to = Square.Get(PerfectChess.Move.ToSquare(move));
            Square from = Square.Get(PerfectChess.Move.FromSquare(move));

            // Does the main part of the move
            BoardPanel.ExecuteMove(from, to);

            // Moves the rook if it's castling
            if (PerfectChess.Move.Castling(move))
            {
                Square rookTo = Square.Get((PerfectChess.Move.FromSquare(move) + PerfectChess.Move.ToSquare(move)) / 2);
                Square rookFrom = Square.Get((PerfectChess.Move.ToSquare(move) > PerfectChess.Move.FromSquare(move)) ? PerfectChess.Move.FromSquare(move) + 3 : PerfectChess.Move.FromSquare(move) - 4);
                BoardPanel.ExecuteMove(rookFrom, rookTo);
            }

            // Places the promotion piece instead of promoted pawn
            else if (PerfectChess.Move.Promotion(move))
                BoardPanel.SetPiece(to, PerfectChess.Move.PromotionPiece(move));

            // Deletes the pawn if it's en passant
            else if (PerfectChess.Move.EnPassant(move))
                BoardPanel.ResetPiece(Square.Get(PerfectChess.Move.ToSquare(move) - 8 + 16 * (PerfectChess.Move.FromPiece(move) & ColorMask)));

            BoardPanel.Invalidate(true);
            BoardPanel.Refresh();
        }

        #region BoardView
        /// <summary>
        /// Displays a message.
        /// </summary>
        /// <param name="message"></param>
        public void Message(string message)
        {
            MessageBox.Show(message);
        }

        /// <summary>
        /// Notifies about checkmate.
        /// </summary>
        /// <param name="movedIsHuman">Winner is a human.</param>
        /// <param name="lostIsHuman">Loser is a human.</param>
        /// <param name="colorWin">Color of the winner.</param>
        public void Checkmate(bool movedIsHuman, bool lostIsHuman, int colorWin)
        {
            string winlossMessage = "Checkmate!\n";
            if (movedIsHuman)
            {
                if (!lostIsHuman) winlossMessage += "You won, congratulations!";
                else winlossMessage += (colorWin == White) ? "White wins!" : "Black wins!";
            }
            else
            {
                if (lostIsHuman) winlossMessage += "You lost! Try again?";
                else winlossMessage += (colorWin == White) ? "White wins!" : "Black wins!";
            }

            LogTextBox.AddText(winlossMessage, System.Drawing.Color.Red);
        }

        /// <summary>
        /// Notifies about stalemate.
        /// </summary>
        public void Stalemate()
        {
            string logText = "Stalemate! It's a draw.\n";
            LogTextBox.AddText(logText, System.Drawing.Color.Green);
        }

        /// <summary>
        /// Notifies about check.
        /// </summary>
        /// <param name="win"></param>
        public void Check(bool win)
        {
            string logText = "Check!\n";
            LogTextBox.AddText(logText, System.Drawing.Color.Red);
        }

        /// <summary>
        /// Notifies about fifty moves rule.
        /// </summary>
        public void FiftyMovesRule()
        {
            MessageBox.Show("Game is drawn by fifty-moves rule");
        }

        /// <summary>
        /// Selects a promotion piece with a dialog.
        /// </summary>
        /// <param name="color">Color of the candidate pieces.</param>
        /// <returns>The selected promotion piece.</returns>
        public int SelectPromotionPiece(int color)
        {
            PromotionForm prom = new PromotionForm(color);
            prom.StartPosition = FormStartPosition.Manual;
            prom.Location = new Point(MousePosition.X - ChessViewSettings.SQUARE_SIZE * 4, MousePosition.Y - ChessViewSettings.SQUARE_SIZE);
            prom.ShowDialog();
            return prom.PieceChosen;
        }

        /// <summary>
        /// Flips the board.
        /// </summary>
        public void Flip()
        {
            this.BoardPanel.Flip();
        }

        /// <summary>
        /// The board is flipped (black side is in the front).
        /// </summary>
        public bool Flipped => BoardPanel.Flipped;

        /// <summary>
        /// Starts a new mouse move.
        /// </summary>
        /// <param name="from">Square to start the move from.</param>
        /// <param name="emptyAvailibleSquares">Empty available target squares.</param>
        /// <param name="enemyAvailibleSquares">Enemy available target squares.</param>
        public void StartMove(Square from, IEnumerable<Square> emptyAvailibleSquares, IEnumerable<Square> enemyAvailibleSquares)
        {
            MoveStartAllowed = true;

            LogTextBox.SavedText = LogTextBox.Text;
            BoardPanel.MarkAttacked(emptyAvailibleSquares, false);
            BoardPanel.MarkAttacked(enemyAvailibleSquares, false);
            CurrentMoveIdentifier = BoardPanel.StartMove(from);

            Point e = BoardPanel.PointToClient(MousePosition);
            BoardPanel.ContinueMove(CurrentMoveIdentifier, new Point(e.X - ChessViewSettings.SQUARE_SIZE / 2, e.Y - ChessViewSettings.SQUARE_SIZE / 2));
            BoardPanel.Refresh();
        }

        /// <summary>
        /// Cancels the current mouse move.
        /// </summary>
        public void CancelMove()
        {
            BoardPanel.DeleteMove(CurrentMoveIdentifier, false);
            BoardPanel.DeleteMarkedEffects();
            BoardPanel.Refresh();
            LogTextBox.SetText(LogTextBox.SavedText);
            MoveStartAllowed = false;
        }

        /// <summary>
        /// Finishes the current mouse move.
        /// </summary>
        /// <param name="move">Move to finish.</param>
        public void FinishMove(int move)
        {
            // Log the move
            string logOutput = StringPresentation.MoveToString(move) + "\n";
            LogTextBox.SetText(logOutput);

            // Execute the move on the board
            BoardPanel.DeleteMove(CurrentMoveIdentifier, false);
            ExecuteMove(move);
            BoardPanel.DeleteEffects(false);
            BoardPanel.ShowLastMove(Square.Get(PerfectChess.Move.FromSquare(move)), Square.Get(PerfectChess.Move.ToSquare(move)));
            BoardPanel.Refresh();

            MoveStartAllowed = false;
        }

        /// <summary>
        /// Undoes the last move.
        /// </summary>
        /// <param name="moveToUndo">Move to undo.</param>
        public void UndoMove(int moveToUndo)
        {
            BoardPanel.DeleteEffects();

            Square to = Square.Get(ToSquare(moveToUndo));
            Square from = Square.Get(FromSquare(moveToUndo));

            // Castling
            if (Castling(moveToUndo))
            {
                Square rookTo = Square.Get((FromSquare(moveToUndo) + ToSquare(moveToUndo)) / 2);
                Square rookFrom = Square.Get((ToSquare(moveToUndo) > FromSquare(moveToUndo))
                    ? FromSquare(moveToUndo) + 3 : FromSquare(moveToUndo) - 4);

                BoardPanel.ExecuteMove(rookTo, rookFrom);
            }

            // Promotion
            else if (Promotion(moveToUndo))
                BoardPanel.SetPiece(to, FromPiece(moveToUndo));

            // Enpassant
            else if (EnPassant(moveToUndo))
            {
                Square capturedPawnSquare = Square.Get(ToSquare(moveToUndo) - 8 + 16 * (FromPiece(moveToUndo) & Piece.ColorMask));
                BoardPanel.SetPiece(capturedPawnSquare, Piece.Pawn | ((1 - FromPiece(moveToUndo)) & Piece.ColorMask));
            }

            // Main part of the move
            BoardPanel.ExecuteMove(to, from);
            BoardPanel.ShowLastMove(from, to);
            if (ToPiece(moveToUndo) != 0)
                BoardPanel.SetPiece(to, ToPiece(moveToUndo));

            BoardPanel.Invalidate(true);
            BoardPanel.Refresh();

            LogTextBox.ResetText();
        }

        /// <summary>
        /// Executes a move by computer.
        /// </summary>
        /// <param name="move">Move to execute.</param>
        public void ExecuteComputerMove(int move)
        {
            // Log the move
            string logOutput = "Engine " + StringPresentation.MoveToString(move) + "\n";
            LogTextBox.AddText(logOutput);

            // Execute the move on the board
            ExecuteMove(move); 
            BoardPanel.DeleteEffects(false);
            BoardPanel.ShowLastMove(Square.Get(PerfectChess.Move.FromSquare(move)), Square.Get(PerfectChess.Move.ToSquare(move)));
            BoardPanel.Refresh();
        }

        /// <summary>
        /// Sets the material imbalance.
        /// </summary>
        /// <param name="white">White material imbalance.</param>
        /// <param name="black">Black material imbalance.</param>
        public void SetMaterial(int white, int black)
        {
            string materialString(int material) => (material > 0 ? "+" : "") + material.ToString();
            int material1 = !BoardPanel.Flipped ? white : black;
            int material2 = !BoardPanel.Flipped ? black : white;
            MaterialLabel1.Text = materialString(material1);
            MaterialLabel2.Text = materialString(material2);
        }

        /// <summary>
        /// Sets a new position.
        /// </summary>
        /// <param name="position">Position to set.</param>
        public void SetPosition(Position position)
        {
            // Set the position on the board
            BoardPanel.Reset();
            BoardPanel.SetPosition(position.SquarePiece);
            BoardPanel.Refresh();

            // Reset the log
            LogTextBox.ResetText();

            // Reset material imbalance
            SetMaterial(0, 0);

            MoveStartAllowed = false;
            MousePressed = false;
        }

        /// <summary>
        /// New game is requested.
        /// Argument: first bit - white, second bit - black, 1 - human, 0 - engine.
        /// </summary>
        public event EventHandler<int> NewGameRequested;

        /// <summary>
        /// Square on the board is clicked.
        /// </summary>
        public event EventHandler<Square> SquareClicked;

        /// <summary>
        /// Mouse is released to finish the move.
        /// </summary>
        public event EventHandler<Tuple<Square, Square>> MoveFinishRequested;

        /// <summary>
        /// Request to undo the last move is initiated.
        /// </summary>
        public event EventHandler UndoRequested;
        #endregion

        #region Buttons
        private void NewGameButton_Click(object sender, EventArgs e)
        {
            NewGameForm gameForm = new NewGameForm();
            gameForm.StartPosition = FormStartPosition.CenterParent;
            gameForm.ShowDialog();

            // Do nothing if canceled
            if (gameForm.DialogResult != DialogResult.OK) return;

            // Otherwise, notify about the new game request
            int state = (gameForm.WhiteHuman ? 1 : 0) * 2 + (gameForm.BlackHuman ? 1 : 0);
            NewGameRequested?.Invoke(this, state);
        }

        private void NewGameButton_MouseEnter(object sender, EventArgs e)
        {
            SetGradientBackground(NewGameButton, new Point(0, 0), new Point(0, NewGameButton.Height), System.Drawing.Color.FromArgb(0xA3, 0x8f, 0x8f), System.Drawing.Color.FromArgb(0xce, 0xc5, 0xc5));
            NewGameButton.FlatAppearance.BorderColor = System.Drawing.Color.GhostWhite;
            this.Cursor = Cursors.Hand;
        }

        private void NewGameButton_MouseLeave(object sender, EventArgs e)
        {
            SetGradientBackground(NewGameButton, new Point(0, 0), new Point(0, NewGameButton.Height), System.Drawing.Color.FromArgb(0xce, 0xc5, 0xc5), System.Drawing.Color.FromArgb(0xA3, 0x8f, 0x8f));
            NewGameButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.Cursor = Cursors.Default;
        }

        private void UndoButton_Click(object sender, EventArgs e)
        {
            UndoRequested?.Invoke(this, EventArgs.Empty);
        }

        private void UndoButton_MouseEnter(object sender, EventArgs e)
        {
            SetGradientBackground(UndoButton, new Point(0, 0), new Point(0, NewGameButton.Height), System.Drawing.Color.FromArgb(0xA3, 0x8f, 0x8f), System.Drawing.Color.FromArgb(0xce, 0xc5, 0xc5));
            UndoButton.FlatAppearance.BorderColor = System.Drawing.Color.GhostWhite;
            this.Cursor = Cursors.Hand;
        }

        private void UndoButton_MouseLeave(object sender, EventArgs e)
        {
            SetGradientBackground(UndoButton, new Point(0, 0), new Point(0, UndoButton.Height), System.Drawing.Color.FromArgb(0xce, 0xc5, 0xc5), System.Drawing.Color.FromArgb(0xA3, 0x8f, 0x8f));
            UndoButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.Cursor = Cursors.Default;
        }

        private void FlipButton_Click(object sender, EventArgs e)
        {
            BoardPanel.Flip();
            BoardPanel.Refresh();
            string tmp = MaterialLabel1.Text;
            MaterialLabel1.Text = MaterialLabel2.Text;
            MaterialLabel2.Text = tmp;
        }

        private void FlipButton_MouseEnter(object sender, EventArgs e)
        {
            SetGradientBackground(FlipButton, new Point(0, 0), new Point(FlipButton.Width, FlipButton.Height), System.Drawing.Color.FromArgb(0xA3, 0x8f, 0x8f), System.Drawing.Color.FromArgb(0xce, 0xc5, 0xc5));
            FlipButton.FlatAppearance.BorderColor = System.Drawing.Color.GhostWhite;
            this.Cursor = Cursors.Hand;
        }

        private void FlipButton_MouseLeave(object sender, EventArgs e)
        {
            SetGradientBackground(FlipButton, new Point(0, 0), new Point(FlipButton.Width, FlipButton.Height), System.Drawing.Color.FromArgb(0xce, 0xc5, 0xc5), System.Drawing.Color.FromArgb(0xA3, 0x8f, 0x8f));
            FlipButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.Cursor = Cursors.Default;
        }
        #endregion

        #region BoardPanel
        private void BoardPanel_MouseDown(object sender, MouseEventArgs e)
        {
            MousePressed = true;
            Square tappedSquare;
            try
            {
                tappedSquare = BoardPanel.GetSquare(e.Location);
            }
            catch
            {
                MessageBox.Show("Choose a square to move from");
                return;
            }
            SquareClicked?.Invoke(this, tappedSquare);
        }

        private void BoardPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!MoveStartAllowed) return;
            if (MousePressed)
            {
                BoardPanel.ContinueMove(CurrentMoveIdentifier, new Point(e.X - ChessViewSettings.SQUARE_SIZE / 2, e.Y - ChessViewSettings.SQUARE_SIZE / 2));
                BoardPanel.Refresh();
            }
        }

        private void BoardPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (MousePressed == false) return;
            if (!MoveStartAllowed) return;
            MousePressed = false;

            Square movingToSquare;
            try
            {
                // Throws an exception when released out of board bounds
                movingToSquare = BoardPanel.GetSquare(e.Location);
            }
            catch
            {
                CancelMove();
                return;
            }

            MoveFinishRequested?.Invoke(this, new Tuple<Square, Square>(BoardPanel.FromSquare(CurrentMoveIdentifier), movingToSquare));
            MoveStartAllowed = false;
        }

        private void BoardPanel_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void BoardPanel_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }
        #endregion
    }
}
