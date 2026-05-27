using System;
using System.Collections.Generic;
using System.Linq;
using ChineseChess.Models;

namespace ChineseChess.Game
{
    public class MoveScore
    {
        public Position From { get; set; }
        public Position To { get; set; }
        public int Score { get; set; }
    }

    public class AIPlayer
    {
        private Random random = new Random();

        // 棋子價值評估
        private int GetPieceValue(PieceType type)
        {
            switch (type)
            {
                case PieceType.General: return 1000;
                case PieceType.Advisor: return 200;
                case PieceType.Elephant: return 220;
                case PieceType.Horse: return 400;
                case PieceType.Chariot: return 500;
                case PieceType.Cannon: return 450;
                case PieceType.Soldier: return 100;
                default: return 0;
            }
        }

        public Move GetNextMove(GameLogic gameLogic)
        {
            Board board = gameLogic.Board;
            PlayerColor aiColor = gameLogic.GameState.CurrentPlayer;

            List<MoveScore> moves = new List<MoveScore>();

            // 評估所有可能的移動
            foreach (Piece piece in board.GetPiecesByColor(aiColor))
            {
                List<Position> possibleMoves = gameLogic.GetPossibleMoves(piece.Position);

                foreach (Position target in possibleMoves)
                {
                    int score = EvaluateMove(piece, target, gameLogic);
                    moves.Add(new MoveScore { From = piece.Position, To = target, Score = score });
                }
            }

            if (moves.Count == 0)
                return null;

            // 選擇最高分的移動（加入一些隨機性以避免AI太可預測）
            int maxScore = moves.Max(m => m.Score);
            List<MoveScore> bestMoves = moves.Where(m => m.Score >= maxScore - 50).ToList();

            if (bestMoves.Count > 0)
            {
                int index = random.Next(bestMoves.Count);
                return new Move(bestMoves[index].From, bestMoves[index].To);
            }

            return null;
        }

        private int EvaluateMove(Piece piece, Position target, GameLogic gameLogic)
        {
            Board board = gameLogic.Board;
            int score = 0;

            // 基礎分數：棋子價值
            score += GetPieceValue(piece.Type);

            // 捕子獎勵：優先考慮捕子
            Piece targetPiece = board.GetPiece(target);
            if (targetPiece != null && targetPiece.Color != piece.Color)
            {
                score += GetPieceValue(targetPiece.Type) * 2; // 捕子的價值x2
            }

            // 將軍獎勵
            Piece general = board.FindGeneral(targetPiece?.Color ?? PlayerColor.Red);
            if (general != null)
            {
                // 臨時執行移動以檢查是否將軍
                Piece captured = board.MovePiece(piece.Position, target);
                if (gameLogic.IsInCheck(targetPiece?.Color ?? PlayerColor.Red))
                {
                    score += 300; // 將軍獎勵
                    if (gameLogic.IsCheckmate(targetPiece?.Color ?? PlayerColor.Red))
                    {
                        score += 1000; // 將死獎勵
                    }
                }

                // 復原移動
                board.MovePiece(target, piece.Position);
                if (captured != null)
                {
                    captured.IsAlive = true;
                    board.SetPiece(captured.Position, captured);
                }
            }

            // 棋子安全性懲罰：避免讓己方棋子被捕
            foreach (Piece enemy in board.GetPiecesByColor(
                piece.Color == PlayerColor.Red ? PlayerColor.Black : PlayerColor.Red))
            {
                if (CanCapture(enemy, target, board))
                {
                    score -= GetPieceValue(piece.Type) / 2;
                }
            }

            return score;
        }

        private bool CanCapture(Piece attacker, Position target, Board board)
        {
            MoveValidator validator = new MoveValidator();
            return validator.IsValidMove(attacker, target, board);
        }
    }
}
