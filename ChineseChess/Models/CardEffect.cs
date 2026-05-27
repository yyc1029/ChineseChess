namespace ChineseChess.Models
{
    public enum CardEffect
    {
        Duel,         // 2-10 and A: compare card values, winner gets bonus move
        Freeze,       // J: freeze one opponent piece for 1 turn
        Revive,       // Q: revive one captured own piece
        SkipOpponent  // K: skip opponent's next turn
    }
}
