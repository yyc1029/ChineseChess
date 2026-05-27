using System.Collections.Generic;
using ChineseChess.Models;

namespace ChineseChess.Game
{
    public class CardManager
    {
        private CardDeck deck;
        public Hand RedHand { get; private set; }
        public Hand BlackHand { get; private set; }

        public CardManager()
        {
            deck = new CardDeck();
            RedHand = new Hand();
            BlackHand = new Hand();
        }

        public Hand GetHand(PlayerColor color)
        {
            return color == PlayerColor.Red ? RedHand : BlackHand;
        }

        public bool DrawCard(PlayerColor color)
        {
            Hand hand = GetHand(color);
            if (hand.IsFull) return false;
            return hand.Add(deck.DrawCard());
        }

        public void UseCard(PlayerColor color, Card card)
        {
            GetHand(color).Remove(card);
        }

        public void DealInitialHands()
        {
            for (int i = 0; i < Hand.MaxSize; i++)
            {
                RedHand.Add(deck.DrawCard());
                BlackHand.Add(deck.DrawCard());
            }
        }

        public void LoadHands(List<Card> redCards, List<Card> blackCards)
        {
            RedHand.Clear();
            foreach (Card c in redCards) RedHand.Add(c);
            BlackHand.Clear();
            foreach (Card c in blackCards) BlackHand.Add(c);
        }

        public void Reset()
        {
            deck.Reset();
            RedHand.Clear();
            BlackHand.Clear();
            DealInitialHands();
        }
    }
}
