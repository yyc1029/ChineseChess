using System.Collections.Generic;

namespace ChineseChess.Models
{
    public class Hand
    {
        private List<Card> cards = new List<Card>();
        public const int MaxSize = 3;

        public IReadOnlyList<Card> Cards => cards.AsReadOnly();
        public bool IsFull => cards.Count >= MaxSize;
        public int Count => cards.Count;

        public bool Add(Card card)
        {
            if (card == null || IsFull) return false;
            cards.Add(card);
            return true;
        }

        public bool Remove(Card card)
        {
            return cards.Remove(card);
        }

        public void Clear()
        {
            cards.Clear();
        }
    }
}
