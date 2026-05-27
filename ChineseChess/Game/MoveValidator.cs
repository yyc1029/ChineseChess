using System;
using System.Collections.Generic;
using System.Linq;
using ChineseChess.Models;

namespace ChineseChess.Game
{
    public class MoveValidator
    {
        public bool IsValidMove(Piece piece, Position target, Board board)
        {
            if (piece == null || !target.IsValid())
                return false;

            // 不能移動到己方棋子上
            Piece targetPiece = board.GetPiece(target);
            if (targetPiece != null && targetPiece.Color == piece.Color)
                return false;

            // 根據棋子類型驗證移動規則
            switch (piece.Type)
            {
                case PieceType.General:
                    return IsValidGeneralMove(piece, target, board);
                case PieceType.Advisor:
                    return IsValidAdvisorMove(piece, target, board);
                case PieceType.Elephant:
                    return IsValidElephantMove(piece, target, board);
                case PieceType.Horse:
                    return IsValidHorseMove(piece, target, board);
                case PieceType.Chariot:
                    return IsValidChariotMove(piece, target, board);
                case PieceType.Cannon:
                    return IsValidCannonMove(piece, target, board);
                case PieceType.Soldier:
                    return IsValidSoldierMove(piece, target, board);
                default:
                    return false;
            }
        }

        private bool IsValidGeneralMove(Piece piece, Position target, Board board)
        {
            // 將只能在9宮內活動 (宮的範圍: x=3-5, y=0-2 紅方; x=3-5, y=7-9 黑方)
            int minX = 3, maxX = 5;
            int minY = piece.Color == PlayerColor.Red ? 0 : 7;
            int maxY = piece.Color == PlayerColor.Red ? 2 : 9;

            if (target.X < minX || target.X > maxX || target.Y < minY || target.Y > maxY)
                return false;

            // 只能前後左右走，不能走對角線
            int deltaX = Math.Abs(target.X - piece.Position.X);
            int deltaY = Math.Abs(target.Y - piece.Position.Y);

            return (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
        }

        private bool IsValidAdvisorMove(Piece piece, Position target, Board board)
        {
            // 士只能在9宮內活動
            int minX = 3, maxX = 5;
            int minY = piece.Color == PlayerColor.Red ? 0 : 7;
            int maxY = piece.Color == PlayerColor.Red ? 2 : 9;

            if (target.X < minX || target.X > maxX || target.Y < minY || target.Y > maxY)
                return false;

            // 士只能走對角線，距離為1
            int deltaX = Math.Abs(target.X - piece.Position.X);
            int deltaY = Math.Abs(target.Y - piece.Position.Y);

            return deltaX == 1 && deltaY == 1;
        }

        private bool IsValidElephantMove(Piece piece, Position target, Board board)
        {
            // 象不過河
            int riverY = piece.Color == PlayerColor.Red ? 4 : 5;
            if (piece.Color == PlayerColor.Red && target.Y > riverY)
                return false;
            if (piece.Color == PlayerColor.Black && target.Y < riverY)
                return false;

            // 象走對角線，距離為2
            int deltaX = target.X - piece.Position.X;
            int deltaY = target.Y - piece.Position.Y;

            if (Math.Abs(deltaX) != 2 || Math.Abs(deltaY) != 2)
                return false;

            // 檢查象眼是否被卡
            int eyeX = piece.Position.X + deltaX / 2;
            int eyeY = piece.Position.Y + deltaY / 2;
            Piece eyePiece = board.GetPiece(new Position(eyeX, eyeY));

            return eyePiece == null;
        }

        private bool IsValidHorseMove(Piece piece, Position target, Board board)
        {
            int deltaX = Math.Abs(target.X - piece.Position.X);
            int deltaY = Math.Abs(target.Y - piece.Position.Y);

            // 馬走日字：先走1格直線，再走1格斜線
            if ((deltaX == 2 && deltaY == 1) || (deltaX == 1 && deltaY == 2))
            {
                // 檢查馬腳是否被卡
                int footX, footY;
                if (deltaX == 2)
                {
                    footX = piece.Position.X + (target.X > piece.Position.X ? 1 : -1);
                    footY = piece.Position.Y;
                }
                else
                {
                    footX = piece.Position.X;
                    footY = piece.Position.Y + (target.Y > piece.Position.Y ? 1 : -1);
                }

                Piece footPiece = board.GetPiece(new Position(footX, footY));
                return footPiece == null;
            }

            return false;
        }

        private bool IsValidChariotMove(Piece piece, Position target, Board board)
        {
            // 車走直線，沒有距離限制
            if (piece.Position.X != target.X && piece.Position.Y != target.Y)
                return false;

            // 檢查路徑上是否有棋子
            return IsPathClear(piece.Position, target, board);
        }

        private bool IsValidCannonMove(Piece piece, Position target, Board board)
        {
            // 砲走直線
            if (piece.Position.X != target.X && piece.Position.Y != target.Y)
                return false;

            Piece targetPiece = board.GetPiece(target);

            if (targetPiece == null)
            {
                // 沒有目標，路徑必須清空
                return IsPathClear(piece.Position, target, board);
            }
            else
            {
                // 有目標（吃子），路徑上必須恰好有1個棋子
                return CountPiecesInPath(piece.Position, target, board) == 1;
            }
        }

        private bool IsValidSoldierMove(Piece piece, Position target, Board board)
        {
            int deltaX = Math.Abs(target.X - piece.Position.X);
            int deltaY = Math.Abs(target.Y - piece.Position.Y);

            // 兵只能走1格
            if (deltaX + deltaY != 1)
                return false;

            bool isRed = piece.Color == PlayerColor.Red;
            bool passedRiver = isRed ? piece.Position.Y > 4 : piece.Position.Y < 5;

            if (!passedRiver)
            {
                // 未過河只能前進
                bool movedForward = isRed ? target.Y > piece.Position.Y : target.Y < piece.Position.Y;
                return movedForward && deltaX == 0;
            }
            else
            {
                // 已過河可以前後左右移動
                return true;
            }
        }

        private bool IsPathClear(Position from, Position to, Board board)
        {
            int deltaX = to.X > from.X ? 1 : (to.X < from.X ? -1 : 0);
            int deltaY = to.Y > from.Y ? 1 : (to.Y < from.Y ? -1 : 0);

            int x = from.X + deltaX;
            int y = from.Y + deltaY;

            while (x != to.X || y != to.Y)
            {
                if (board.GetPiece(new Position(x, y)) != null)
                    return false;

                x += deltaX;
                y += deltaY;
            }

            return true;
        }

        private int CountPiecesInPath(Position from, Position to, Board board)
        {
            int count = 0;
            int deltaX = to.X > from.X ? 1 : (to.X < from.X ? -1 : 0);
            int deltaY = to.Y > from.Y ? 1 : (to.Y < from.Y ? -1 : 0);

            int x = from.X + deltaX;
            int y = from.Y + deltaY;

            while (x != to.X || y != to.Y)
            {
                if (board.GetPiece(new Position(x, y)) != null)
                    count++;

                x += deltaX;
                y += deltaY;
            }

            return count;
        }
    }
}
