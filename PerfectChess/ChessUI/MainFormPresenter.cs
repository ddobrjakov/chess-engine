using System;
using System.Collections.Generic;
using System.Linq;

namespace PerfectChess
{  
    public class MainFormPresenter
    {
        public MainFormPresenter(IGameView boardView)
        {
            GameModel = new GameModel(new HumanPlayer(ChessColor.White), new EnginePlayer(ChessColor.Black), new Position());
            GameModel.PlayerMakesMove += Player_MakesMove;
            GameView = boardView;
            GameView.MoveFinishRequested += BoardView_MoveFinishRequested;
            GameView.UndoRequested += BoardView_UndoRequested;
            GameView.NewGameRequested += BoardView_NewGameRequested;
            GameView.SquareClicked += BoardView_SquareClicked;
            GameView.SetPosition(GameModel.GamePosition);
            if (AppSettings.USE_STATS) Stats.Show();
        }

        private void BoardView_NewGameRequested(object sender, int players)
        {
            GameModel.StartNewGame(players);
            GameView.SetPosition(GameModel.GamePosition);
            if (GameModel.ShouldFlipBoard(GameView.Flipped))
                GameView.Flip();
            GameModel.PlayerWhite.YourMove(GameModel.GamePosition);
        }

        private void BoardView_SquareClicked(object sender, Square square)
        {
            // Disable the board if game is finished
            if (GameModel.GameFinished) return;

            // Disable the board if two computers are playing
            if (GameModel.PlayerToMove is EnginePlayer && GameModel.PlayerWaiting is EnginePlayer) return;

            // Abort if trying to make a move from an empty square
            if (GameModel.EmptySquare(square)) return;

            // Abort if trying to move a piece of thinking engine.
            if (GameModel.PlayerToMove is EnginePlayer && GameModel.PieceOfPlayer(square, GameModel.PlayerToMove)) return;

            // Abort if trying to move an opponent's piece
            // TODO: Enable premoves
            if (GameModel.PlayerToMove is HumanPlayer && GameModel.PieceOfPlayer(square, GameModel.PlayerWaiting)) return;

            // Start the move on the board
            (var emptyAvailableSquares, var enemyAvailableSquares) = GameModel.TargetSquares(square);
            GameView.StartMove(square, emptyAvailableSquares, enemyAvailableSquares);
        }

        private void BoardView_MoveFinishRequested(object sender, Tuple<Square, Square> e)
        {
            // Abort if the game is finished
            if (GameModel.GameFinished)
            {
                GameView.CancelMove();
                GameView.Message("Error. Can't finish the move. Game is finished.");
                return;
            }

            // Find the move
            int? move = FindMove(e.Item1, e.Item2);

            // Cancel if no move is found 
            if (move is null)
                GameView.CancelMove();

            // If the move is expected, make the move
            else if (GameModel.PlayerToMove is HumanPlayer)
                GameModel.PlayerToMove.MakeMove((int)move);

            // If the move is not expected, add the move to premoves
            else if (GameModel.PlayerWaiting is HumanPlayer)
                GameModel.PlayerWaiting.PreMoves.Push((int)move);

            // If two computers are playing, disregard the move
            else
                GameView.Message("Don't bother the comps! Let them play!");
        }

        private void BoardView_UndoRequested(object sender, EventArgs e)
        {
            // TODO: Abort thinking engine
            if (GameModel.PlayerToMove is EnginePlayer && GameModel.PlayerToMove.IsThinking) return;
            if (GameModel.GamePosition.GameFinished) return;

            // Undo moves
            IEnumerable<int> moves = GameModel.UndoMoves();
            foreach (int move in moves)
                GameView.UndoMove(move);

            // Update material imbalance
            int whiteMaterialAdvantage = GameModel.CountMaterial();
            GameView.SetMaterial(whiteMaterialAdvantage, -whiteMaterialAdvantage);
        }

        /// <summary>
        /// Reacts when a player decides to make a move.
        /// </summary>
        /// <param name="sender">Player that wants to make the move.</param>
        /// <param name="move">Move that the player wants to make.</param>
        private void Player_MakesMove(object sender, int move)
        {
            // Check if the game is finished
            if (GameModel.GameFinished)
                GameView.Message("Error. Game is finished. No moves allowed");

            // Check if the move is illegal
            if (GameModel.IllegalMove(move))
                GameView.Message(StringPresentation.MoveToString(move));

            // Make the move in position
            GameModel.GamePosition.Make(move);

            // Make the move on the board
            if (GameModel.PlayerWaiting is HumanPlayer)
                GameView.FinishMove(move);
            else GameView.ExecuteComputerMove(move);

            // Perform post-move actions
            PostMoveActions(move);

            // Tell the other player that they should now move
            if (!GameModel.GameFinished)
                GameModel.PlayerToMove.YourMove(GameModel.GamePosition.DeepCopy());
        }

        /// <summary>
        /// Returns the move from a specified origin to a specified target square or null if there is no such legal move.
        /// </summary>
        /// <param name="from">Origin square.</param>
        /// <param name="to">Target square.</param>
        /// <returns>Move from origin square to target square.</returns>
        private int? FindMove(Square from, Square to)
        {
            IEnumerable<int> candidateMoves = GameModel.FindMoves(from, to);
            if (!candidateMoves.Any())
                return null;

            // Not a promotion
            if (candidateMoves.Count() == 1)
                return candidateMoves.First();
            
            // Promotion
            int chosenPiece = GameView.SelectPromotionPiece(GameModel.GamePosition.ColorToMove);

            // Promotion is canceled
            if (chosenPiece == Piece.None)
                return null;

            // Promotion is completed
            return GameModel.SelectPromotionMove(candidateMoves, chosenPiece);
        }

        private void PostMoveActions(int move)
        {
            // Checkmate
            if (GameModel.GamePosition.Checkmate)
                GameView.Checkmate(
                    GameModel.PlayerWaiting is HumanPlayer,
                    GameModel.PlayerToMove is HumanPlayer,
                    (int) GameModel.PlayerWaiting.Color);

            // Check
            else if (GameModel.GamePosition.Check)
                GameView.Check(true);

            // Fifty moves
            else if (GameModel.GamePosition.MovesFiftyRuleCount >= 50)
                GameView.FiftyMovesRule();

            // Stalemate
            else if (GameModel.GamePosition.Stalemate)
                GameView.Stalemate();       

            // Update material imbalance
            int whiteMaterialAdvantage = GameModel.CountMaterial();
            GameView.SetMaterial(whiteMaterialAdvantage, -whiteMaterialAdvantage);

            // Show stats
            if (AppSettings.USE_STATS)
            {
                if (GameModel.PlayerWaiting is EnginePlayer ep)
                {
                    Stats.ShowStats("Time: " + ep.STATS_ThinkTime + "\n");
                    Stats.ShowStats("Total nodes considered:    " + ep.STATS_Nodes + " (" + Math.Round(ep.STATS_Nodes / ep.STATS_ThinkTime.TotalSeconds) + " n/s)\n");
                    Stats.ShowStats("Total positions evaluated: " + ep.STATS_Evaluated.ToString() + " (" + Math.Round(ep.STATS_Evaluated / ep.STATS_ThinkTime.TotalSeconds) + " n/s)\n");
                    Stats.ShowStats("LegalMoves calculated " + ep.STATS_LegalMovesCallCount.ToString() + " times\n");
                    // Stats.ShowStats("Attacks called " + EP.TEST_AttacksCallCount.ToString() + " times\n");
                }
            }
        }

        private IGameView GameView;
        private GameModel GameModel;
        private StatsForm Stats = new StatsForm();
    }
}
