using System;
using System.Collections.Generic;
using System.Linq;
using ChineseChess.Models;

namespace ChineseChess.Game
{
    public class GameLogic
    {
        private Board board;
        private GameState gameState;
        private MoveValidator moveValidator;
        private Queue<Move> moveHistory;
        private Stack<(Board, Move)> undoStack;
        private CardManager cardManager;
        private List<Piece> capturedRed = new List<Piece>();
        private List<Piece> capturedBlack = new List<Piece>();

        public GameLogic()
        {
            board = new Board();
            gameState = new GameState();
            moveValidator = new MoveValidator();
            moveHistory = new Queue<Move>();
            undoStack = new Stack<(Board, Move)>();
            cardManager = new CardManager();
        }

        public Board Board => board;
        public GameState GameState => gameState;
        public IEnumerable<Move> MoveHistory => moveHistory;
        public CardManager CardManager => cardManager;
        public List<Piece> CapturedRed => capturedRed;
        public List<Piece> CapturedBlack => capturedBlack;

        public bool MovePiece(Position from, Position to)
        {
            Piece piece = board.GetPiece(from);

            if (piece == null || piece.Color != gameState.CurrentPlayer)
                return false;

            if (piece.IsFrozen)
                return false;

            if (!moveValidator.IsValidMove(piece, to, board))
                return false;

            // Execute move
            Piece captured = board.MovePiece(from, to);
            if (captured != null)
            {
                if (captured.Color == PlayerColor.Red)
                    capturedRed.Add(captured);
                else
                    capturedBlack.Add(captured);
            }

            Move move = new Move(from, to, captured);
            moveHistory.Enqueue(move);
            SaveUndoState(move);

            // Check that current player's general is not in check (invalid if so)
            if (IsInCheck(gameState.CurrentPlayer))
            {
                // Restore - undo tracking
                UndoLastMove();
                if (captured != null)
                {
                    if (captured.Color == PlayerColor.Red)
                        capturedRed.Remove(captured);
                    else
                        capturedBlack.Remove(captured);
                }
                return false;
            }

            // Handle remaining moves (bonus from duel win)
            gameState.MovesRemainingThisTurn--;

            if (gameState.MovesRemainingThisTurn <= 0)
            {
                EndTurn();
            }
            else
            {
                // Still has bonus move(s) - check opponent for display purposes
                PlayerColor opponent = gameState.CurrentPlayer == PlayerColor.Red ? PlayerColor.Black : PlayerColor.Red;
                gameState.IsInCheck = IsInCheck(opponent);
                if (gameState.IsInCheck && IsCheckmate(opponent))
                    gameState.Status = opponent == PlayerColor.Red ? GameStatus.BlackWin : GameStatus.RedWin;
            }

            return true;
        }

        private void EndTurn()
        {
            // Unfreeze any frozen piece
            if (gameState.FrozenPosition.X >= 0)
            {
                Piece frozen = board.GetPiece(gameState.FrozenPosition);
                if (frozen != null) frozen.IsFrozen = false;
                gameState.FrozenPosition = new Position(-1, -1);
            }

            // Switch to next player
            gameState.SwitchPlayer();
            PlayerColor nextPlayer = gameState.CurrentPlayer;

            // Check if next player's turn should be skipped (K card)
            bool shouldSkip = (nextPlayer == PlayerColor.Red && gameState.SkipNextTurn_Red) ||
                              (nextPlayer == PlayerColor.Black && gameState.SkipNextTurn_Black);

            if (shouldSkip)
            {
                if (nextPlayer == PlayerColor.Red)
                    gameState.SkipNextTurn_Red = false;
                else
                    gameState.SkipNextTurn_Black = false;

                gameState.SwitchPlayer(); // skip this player
            }

            // Check if defender from last duel gets a bonus move this turn
            int movesForNextTurn = 1;
            if (gameState.DefenderBonusMove)
            {
                movesForNextTurn = 2;
                gameState.DefenderBonusMove = false;
            }

            // Check check/checkmate for new current player
            gameState.IsInCheck = IsInCheck(gameState.CurrentPlayer);
            if (gameState.IsInCheck && IsCheckmate(gameState.CurrentPlayer))
                gameState.Status = gameState.CurrentPlayer == PlayerColor.Red ? GameStatus.BlackWin : GameStatus.RedWin;

            // Reset turn state
            gameState.MovesRemainingThisTurn = movesForNextTurn;
            gameState.CardUsedThisTurn = false;

            // Draw a card for new current player
            cardManager.DrawCard(gameState.CurrentPlayer);
        }

        public bool IsValidMove(Position from, Position to)
        {
            Piece piece = board.GetPiece(from);
            if (piece == null || piece.Color != gameState.CurrentPlayer)
                return false;

            if (piece.IsFrozen)
                return false;

            if (!moveValidator.IsValidMove(piece, to, board))
                return false;

            // Temp-move to verify king not exposed
            Piece captured = board.MovePiece(from, to);
            bool valid = !IsInCheck(piece.Color);

            board.MovePiece(to, from);
            if (captured != null)
            {
                captured.IsAlive = true;
                board.SetPiece(captured.Position, captured);
            }

            return valid;
        }

        public List<Position> GetPossibleMoves(Position from)
        {
            Piece piece = board.GetPiece(from);
            List<Position> possibleMoves = new List<Position>();

            if (piece == null || piece.Color != gameState.CurrentPlayer)
                return possibleMoves;

            if (piece.IsFrozen)
                return possibleMoves;

            for (int x = 0; x <= 8; x++)
            {
                for (int y = 0; y <= 9; y++)
                {
                    Position target = new Position(x, y);
                    if (IsValidMove(from, target))
                        possibleMoves.Add(target);
                }
            }

            return possibleMoves;
        }

        public bool IsInCheck(PlayerColor color)
        {
            Piece general = board.FindGeneral(color);
            if (general == null) return false;

            PlayerColor opponent = color == PlayerColor.Red ? PlayerColor.Black : PlayerColor.Red;
            foreach (Piece enemy in board.GetPiecesByColor(opponent))
            {
                if (moveValidator.IsValidMove(enemy, general.Position, board))
                    return true;
            }

            return false;
        }

        public bool IsCheckmate(PlayerColor color)
        {
            if (!IsInCheck(color)) return false;

            foreach (Piece piece in board.GetPiecesByColor(color))
            {
                if (piece.IsFrozen) continue;
                for (int x = 0; x <= 8; x++)
                {
                    for (int y = 0; y <= 9; y++)
                    {
                        Position target = new Position(x, y);
                        if (IsValidMove(piece.Position, target))
                            return false;
                    }
                }
            }

            return true;
        }

        public bool Undo()
        {
            if (undoStack.Count == 0) return false;

            var (previousBoard, move) = undoStack.Pop();
            board = previousBoard;

            // Restore captured list
            if (move.CapturedPiece != null)
            {
                if (move.CapturedPiece.Color == PlayerColor.Red)
                    capturedRed.Remove(move.CapturedPiece);
                else
                    capturedBlack.Remove(move.CapturedPiece);
            }

            var allMoves = moveHistory.ToList();
            if (allMoves.Count > 0)
            {
                allMoves.RemoveAt(allMoves.Count - 1);
                moveHistory.Clear();
                foreach (var m in allMoves)
                    moveHistory.Enqueue(m);
            }

            gameState.CurrentPlayer = gameState.CurrentPlayer == PlayerColor.Red ? PlayerColor.Black : PlayerColor.Red;
            gameState.IsInCheck = IsInCheck(gameState.CurrentPlayer);
            gameState.Status = GameStatus.Playing;
            gameState.MovesRemainingThisTurn = 1;

            return true;
        }

        private void SaveUndoState(Move move)
        {
            Board boardCopy = new Board();
            boardCopy.Reset();

            foreach (Piece piece in board.GetAllPieces())
                boardCopy.SetPiece(piece.Position, new Piece(piece.Type, piece.Color, piece.Position) { IsAlive = piece.IsAlive, IsFrozen = piece.IsFrozen });

            undoStack.Push((boardCopy, move));
        }

        private void UndoLastMove()
        {
            if (undoStack.Count == 0) return;

            var (previousBoard, _) = undoStack.Pop();
            board = previousBoard;

            var allMoves = moveHistory.ToList();
            if (allMoves.Count > 0)
            {
                allMoves.RemoveAt(allMoves.Count - 1);
                moveHistory.Clear();
                foreach (var m in allMoves)
                    moveHistory.Enqueue(m);
            }
        }

        public void Reset()
        {
            board = new Board();
            gameState.Reset();
            moveHistory.Clear();
            undoStack.Clear();
            capturedRed.Clear();
            capturedBlack.Clear();
            cardManager.Reset();
        }

        public void SetGameMode(GameMode mode)
        {
            gameState.Mode = mode;
        }
    }
}
