using System;
using System.Collections.Generic;
using ChineseChess.Models;

namespace ChineseChess.Game
{
    public class CardDeck
    {
        private List<Card> deck = new List<Card>();
        private Random random = new Random();

        public CardDeck()
        {
            Build();
        }

        private void Build()
        {
            deck.Clear();
            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
            {
                for (int point = 1; point <= 13; point++)
                    deck.Add(new Card(suit, point));
            }
            Shuffle();
        }

        private void Shuffle()
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                Card tmp = deck[i];
                deck[i] = deck[j];
                deck[j] = tmp;
            }
        }

        public Card DrawCard()
        {
            if (deck.Count == 0) Build(); // Rebuild and reshuffle when depleted
            Card card = deck[0];
            deck.RemoveAt(0);
            return card;
        }

        public void Reset()
        {
            Build();
        }
    }
}
