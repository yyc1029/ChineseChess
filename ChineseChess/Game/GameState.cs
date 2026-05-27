using ChineseChess.Models;

namespace ChineseChess.Game
{
    public enum GameStatus
    {
        Playing,
        RedWin,
        BlackWin,
        Draw
    }

    public class GameState
    {
        public GameMode Mode { get; set; }
        public GameStatus Status { get; set; }
        public PlayerColor CurrentPlayer { get; set; }
        public Position SelectedPosition { get; set; }
        public bool IsInCheck { get; set; }

        // Card system state
        public bool CardUsedThisTurn { get; set; }
        public int MovesRemainingThisTurn { get; set; }
        public bool SkipNextTurn_Red { get; set; }
        public bool SkipNextTurn_Black { get; set; }
        public Position FrozenPosition { get; set; }
        public bool DefenderBonusMove { get; set; }

        public GameState()
        {
            Mode = GameMode.PvP;
            Status = GameStatus.Playing;
            CurrentPlayer = PlayerColor.Red;
            SelectedPosition = new Position(-1, -1);
            IsInCheck = false;
            MovesRemainingThisTurn = 1;
            FrozenPosition = new Position(-1, -1);
        }

        public void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer == PlayerColor.Red ? PlayerColor.Black : PlayerColor.Red;
        }

        public void Reset()
        {
            Status = GameStatus.Playing;
            CurrentPlayer = PlayerColor.Red;
            SelectedPosition = new Position(-1, -1);
            IsInCheck = false;
            CardUsedThisTurn = false;
            MovesRemainingThisTurn = 1;
            SkipNextTurn_Red = false;
            SkipNextTurn_Black = false;
            FrozenPosition = new Position(-1, -1);
            DefenderBonusMove = false;
        }
    }
}
