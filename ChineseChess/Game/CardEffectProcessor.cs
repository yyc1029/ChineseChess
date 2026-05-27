using System.Collections.Generic;
using ChineseChess.Models;

namespace ChineseChess.Game
{
    public class DuelResult
    {
        public bool Tie { get; set; }
        public PlayerColor? Winner { get; set; }
        public int AttackerValue { get; set; }
        public int DefenderValue { get; set; }
    }

    public class CardEffectProcessor
    {
        private GameLogic gameLogic;

        public CardEffectProcessor(GameLogic logic)
        {
            gameLogic = logic;
        }

        // Process a duel between attacker and defender.
        // Defender card may be null if defender has no duel cards (auto-lose).
        public DuelResult ProcessDuel(PlayerColor attacker, Card attackerCard, Card defenderCard)
        {
            PlayerColor defender = attacker == PlayerColor.Red ? PlayerColor.Black : PlayerColor.Red;
            var result = new DuelResult();
            result.AttackerValue = attackerCard.GetDuelValue();
            result.DefenderValue = defenderCard != null ? defenderCard.GetDuelValue() : 0;

            if (defenderCard == null || result.AttackerValue > result.DefenderValue)
            {
                result.Winner = attacker;
                gameLogic.GameState.MovesRemainingThisTurn++;
            }
            else if (result.AttackerValue == result.DefenderValue)
            {
                result.Tie = true;
                // Tie: both draw a card
                gameLogic.CardManager.DrawCard(attacker);
                gameLogic.CardManager.DrawCard(defender);
            }
            else
            {
                result.Winner = defender;
                // Defender wins: they get a bonus move on their next turn
                if (defender == PlayerColor.Red)
                    gameLogic.GameState.SkipNextTurn_Red = false; // Don't skip; give bonus
                gameLogic.GameState.DefenderBonusMove = true;
            }

            return result;
        }

        // Freeze an opponent's piece at the given position for 1 turn.
        public bool ProcessFreeze(Position targetPos)
        {
            Piece piece = gameLogic.Board.GetPiece(targetPos);
            if (piece == null || piece.Color == gameLogic.GameState.CurrentPlayer)
                return false;

            piece.IsFrozen = true;
            gameLogic.GameState.FrozenPosition = targetPos;
            return true;
        }

        // Revive a captured piece and place it on the board.
        public bool ProcessRevive(Piece capturedPiece)
        {
            if (capturedPiece == null || capturedPiece.IsAlive) return false;

            Position revivePos = FindRevivePosition(capturedPiece.Color);
            if (!revivePos.IsValid()) return false;

            capturedPiece.IsAlive = true;
            capturedPiece.Position = revivePos;
            gameLogic.Board.SetPiece(revivePos, capturedPiece);

            // Remove from captured list
            if (capturedPiece.Color == PlayerColor.Red)
                gameLogic.CapturedRed.Remove(capturedPiece);
            else
                gameLogic.CapturedBlack.Remove(capturedPiece);

            return true;
        }

        private Position FindRevivePosition(PlayerColor color)
        {
            // Prioritize home rows
            int[] yRows = color == PlayerColor.Red
                ? new[] { 0, 1, 2, 3, 4 }
                : new[] { 9, 8, 7, 6, 5 };

            foreach (int y in yRows)
            {
                for (int x = 0; x <= 8; x++)
                {
                    Position pos = new Position(x, y);
                    if (gameLogic.Board.GetPiece(pos) == null)
                        return pos;
                }
            }
            return new Position(-1, -1);
        }

        // Skip the opponent's next turn.
        public void ProcessSkipOpponent()
        {
            PlayerColor opponent = gameLogic.GameState.CurrentPlayer == PlayerColor.Red
                ? PlayerColor.Black : PlayerColor.Red;

            if (opponent == PlayerColor.Red)
                gameLogic.GameState.SkipNextTurn_Red = true;
            else
                gameLogic.GameState.SkipNextTurn_Black = true;
        }
    }
}
