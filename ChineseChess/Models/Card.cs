namespace ChineseChess.Models
{
    public class Card
    {
        public CardSuit Suit { get; }
        public int Point { get; }  // 1(Ace) to 13(King)
        public CardEffect Effect { get; }

        public Card(CardSuit suit, int point)
        {
            Suit = suit;
            Point = point;
            Effect = GetEffect(point);
        }

        // pic1=梅花A, pic2=方塊A ... pic52=黑桃K
        public int GetImageIndex()
        {
            return (Point - 1) * 4 + (int)Suit + 1;
        }

        // Duel value: A=14, others=face value
        public int GetDuelValue()
        {
            return Point == 1 ? 14 : Point;
        }

        public static CardEffect GetEffect(int point)
        {
            if (point == 11) return CardEffect.Freeze;
            if (point == 12) return CardEffect.Revive;
            if (point == 13) return CardEffect.SkipOpponent;
            return CardEffect.Duel; // A(1) and 2-10
        }

        public override string ToString()
        {
            string suitStr;
            switch (Suit)
            {
                case CardSuit.Clubs: suitStr = "♣"; break;
                case CardSuit.Diamonds: suitStr = "♦"; break;
                case CardSuit.Hearts: suitStr = "♥"; break;
                case CardSuit.Spades: suitStr = "♠"; break;
                default: suitStr = "?"; break;
            }

            string pointStr;
            switch (Point)
            {
                case 1: pointStr = "A"; break;
                case 11: pointStr = "J"; break;
                case 12: pointStr = "Q"; break;
                case 13: pointStr = "K"; break;
                default: pointStr = Point.ToString(); break;
            }

            return $"{suitStr}{pointStr}";
        }
    }
}
