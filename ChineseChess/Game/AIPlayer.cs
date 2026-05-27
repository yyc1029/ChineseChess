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

            // 先複製棋子列表，避免遍歷時集合被 GetPossibleMoves 內部修改
            List<Piece> piecesToEval = board.GetPiecesByColor(aiColor).ToList();

            // 評估所有可能的移動
            foreach (Piece piece in piecesToEval)
            {
                if (piece.IsFrozen) continue;
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

        // AI card decision: returns best card to use, or null if no card should be used
        public Models.Card GetCardToUse(GameLogic gameLogic)
        {
            PlayerColor aiColor = gameLogic.GameState.CurrentPlayer;
            Hand hand = gameLogic.CardManager.GetHand(aiColor);

            if (hand.Count == 0 || gameLogic.GameState.CardUsedThisTurn)
                return null;

            // Prioritize K (skip opponent) then J (freeze) then highest duel card
            Models.Card kCard = null;
            Models.Card jCard = null;
            Models.Card bestDuel = null;

            foreach (Models.Card card in hand.Cards)
            {
                if (card.Effect == Models.CardEffect.SkipOpponent) kCard = card;
                else if (card.Effect == Models.CardEffect.Freeze) jCard = card;
                else if (card.Effect == Models.CardEffect.Duel)
                {
                    if (bestDuel == null || card.GetDuelValue() > bestDuel.GetDuelValue())
                        bestDuel = card;
                }
            }

            // Use K if available
            if (kCard != null) return kCard;
            // Use high duel card (A or 10+)
            if (bestDuel != null && bestDuel.GetDuelValue() >= 10) return bestDuel;
            // Use J to freeze opponent's strongest piece
            if (jCard != null) return jCard;

            return null;
        }

        // AI picks the best duel card to respond with
        public Models.Card GetDuelResponse(GameLogic gameLogic, int attackerValue)
        {
            PlayerColor aiColor = gameLogic.GameState.CurrentPlayer;
            Hand hand = gameLogic.CardManager.GetHand(aiColor);

            Models.Card best = null;
            foreach (Models.Card card in hand.Cards)
            {
                if (card.Effect != Models.CardEffect.Duel) continue;
                if (best == null || card.GetDuelValue() > best.GetDuelValue())
                    best = card;
            }
            return best;
        }

        // AI picks which opponent piece to freeze (strongest non-general piece)
        public Position GetFreezeTarget(GameLogic gameLogic)
        {
            PlayerColor aiColor = gameLogic.GameState.CurrentPlayer;
            PlayerColor opponent = aiColor == PlayerColor.Red ? PlayerColor.Black : PlayerColor.Red;

            Piece best = null;
            int bestVal = -1;
            foreach (Piece p in gameLogic.Board.GetPiecesByColor(opponent).ToList())
            {
                if (p.Type == PieceType.General) continue;
                int val = GetPieceValue(p.Type);
                if (val > bestVal) { bestVal = val; best = p; }
            }
            return best != null ? best.Position : new Position(-1, -1);
        }

        private int EvaluateMove(Piece piece, Position target, GameLogic gameLogic)
        {
            Board board = gameLogic.Board;
            int score = 0;

            PlayerColor opponent = piece.Color == PlayerColor.Red ? PlayerColor.Black : PlayerColor.Red;

            // 捕子獎勵
            Piece targetPiece = board.GetPiece(target);
            if (targetPiece != null && targetPiece.Color != piece.Color)
            {
                score += GetPieceValue(targetPiece.Type) * 2;
            }

            // 臨時執行移動以評估將軍
            Position origPos = piece.Position;
            Piece captured = board.MovePiece(origPos, target);

            if (gameLogic.IsInCheck(opponent))
            {
                score += 300;
                if (gameLogic.IsCheckmate(opponent))
                    score += 1000;
            }

            // 檢查己方棋子是否暴露在攻擊下（安全性懲罰）
            MoveValidator validator = new MoveValidator();
            List<Piece> enemies = board.GetPiecesByColor(opponent).ToList();
            foreach (Piece enemy in enemies)
            {
                if (validator.IsValidMove(enemy, target, board))
                    score -= GetPieceValue(piece.Type) / 2;
            }

            // 復原移動
            board.MovePiece(target, origPos);
            if (captured != null)
            {
                captured.IsAlive = true;
                board.SetPiece(captured.Position, captured);
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
