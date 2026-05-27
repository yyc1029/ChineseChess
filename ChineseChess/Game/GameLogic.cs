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

        public GameLogic()
        {
            board = new Board();
            gameState = new GameState();
            moveValidator = new MoveValidator();
            moveHistory = new Queue<Move>();
            undoStack = new Stack<(Board, Move)>();
        }

        public Board Board => board;
        public GameState GameState => gameState;
        public IEnumerable<Move> MoveHistory => moveHistory;

        public bool MovePiece(Position from, Position to)
        {
            Piece piece = board.GetPiece(from);

            if (piece == null || piece.Color != gameState.CurrentPlayer)
                return false;

            if (!moveValidator.IsValidMove(piece, to, board))
                return false;

            // 執行移動
            Piece captured = board.MovePiece(from, to);
            Move move = new Move(from, to, captured);
            moveHistory.Enqueue(move);

            // 創建快照用於悔棋
            SaveUndoState(move);

            // 檢查當前玩家的將是否被將軍
            if (IsInCheck(gameState.CurrentPlayer))
            {
                // 復原移動（非法，會導致自己被將軍）
                UndoLastMove();
                return false;
            }

            gameState.CurrentPlayer = gameState.CurrentPlayer == PlayerColor.Red ? PlayerColor.Black : PlayerColor.Red;

            // 檢查新的當前玩家是否被將軍
            gameState.IsInCheck = IsInCheck(gameState.CurrentPlayer);

            // 檢查將死
            if (gameState.IsInCheck && IsCheckmate(gameState.CurrentPlayer))
            {
                gameState.Status = gameState.CurrentPlayer == PlayerColor.Red ? GameStatus.BlackWin : GameStatus.RedWin;
            }

            return true;
        }

        public bool IsValidMove(Position from, Position to)
        {
            Piece piece = board.GetPiece(from);
            if (piece == null || piece.Color != gameState.CurrentPlayer)
                return false;

            if (!moveValidator.IsValidMove(piece, to, board))
                return false;

            // 臨時執行移動，檢查將是否被將軍
            Piece captured = board.MovePiece(from, to);
            bool valid = !IsInCheck(piece.Color);

            // 復原移動
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

            for (int x = 0; x <= 8; x++)
            {
                for (int y = 0; y <= 9; y++)
                {
                    Position target = new Position(x, y);
                    if (IsValidMove(from, target))
                    {
                        possibleMoves.Add(target);
                    }
                }
            }

            return possibleMoves;
        }

        public bool IsInCheck(PlayerColor color)
        {
            Piece general = board.FindGeneral(color);
            if (general == null)
                return false;

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
            if (!IsInCheck(color))
                return false;

            // 檢查是否有任何合法移動能脫離將軍
            foreach (Piece piece in board.GetPiecesByColor(color))
            {
                for (int x = 0; x <= 8; x++)
                {
                    for (int y = 0; y <= 9; y++)
                    {
                        Position target = new Position(x, y);
                        if (IsValidMove(piece.Position, target))
                            return false; // 找到合法移動，不是將死
                    }
                }
            }

            return true; // 沒有合法移動，是將死
        }

        public bool Undo()
        {
            if (undoStack.Count == 0)
                return false;

            var (previousBoard, move) = undoStack.Pop();

            // 復原棋盤狀態
            board = previousBoard;

            // 移除最後一步棋
            var allMoves = moveHistory.ToList();
            if (allMoves.Count > 0)
            {
                allMoves.RemoveAt(allMoves.Count - 1);
                moveHistory.Clear();
                foreach (var m in allMoves)
                {
                    moveHistory.Enqueue(m);
                }
            }

            // 切換回上一個玩家
            gameState.CurrentPlayer = gameState.CurrentPlayer == PlayerColor.Red ? PlayerColor.Black : PlayerColor.Red;
            gameState.IsInCheck = IsInCheck(gameState.CurrentPlayer);
            gameState.Status = GameStatus.Playing;

            return true;
        }

        private void SaveUndoState(Move move)
        {
            // 創建當前棋盤的深度複製
            Board boardCopy = new Board();
            boardCopy.Reset();

            foreach (Piece piece in board.GetAllPieces())
            {
                boardCopy.SetPiece(piece.Position, new Piece(piece.Type, piece.Color, piece.Position) { IsAlive = piece.IsAlive });
            }

            undoStack.Push((boardCopy, move));
        }

        private void UndoLastMove()
        {
            if (undoStack.Count == 0)
                return;

            var (previousBoard, _) = undoStack.Pop();
            board = previousBoard;

            var allMoves = moveHistory.ToList();
            if (allMoves.Count > 0)
            {
                allMoves.RemoveAt(allMoves.Count - 1);
                moveHistory.Clear();
                foreach (var m in allMoves)
                {
                    moveHistory.Enqueue(m);
                }
            }
        }

        public void Reset()
        {
            board = new Board();
            gameState.Reset();
            moveHistory.Clear();
            undoStack.Clear();
        }

        public void SetGameMode(GameMode mode)
        {
            gameState.Mode = mode;
        }
    }
}
