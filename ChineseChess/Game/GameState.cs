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

        public GameState()
        {
            Mode = GameMode.PvP;
            Status = GameStatus.Playing;
            CurrentPlayer = PlayerColor.Red;
            SelectedPosition = new Position(-1, -1);
            IsInCheck = false;
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
        }
    }
}
