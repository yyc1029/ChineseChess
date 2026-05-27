using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ChineseChess.Models;

namespace ChineseChess.UI
{
    public class CardPanel : Control
    {
        private static Dictionary<int, Image> imageCache = new Dictionary<int, Image>();
        private static Image backImage = null;

        private Hand hand;
        private PlayerColor playerColor;
        private int hoveredCardIndex = -1;

        public event Action<Card> CardClicked;
        public bool IsInteractive { get; set; } = true;

        public CardPanel(PlayerColor color)
        {
            playerColor = color;
            Height = 110;
            BackColor = color == PlayerColor.Red
                ? Color.FromArgb(80, 20, 20)
                : Color.FromArgb(20, 20, 60);

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            MouseMove += (s, e) =>
            {
                int idx = GetCardIndexAt(e.X);
                if (idx != hoveredCardIndex)
                {
                    hoveredCardIndex = idx;
                    Invalidate();
                }
            };

            MouseLeave += (s, e) =>
            {
                hoveredCardIndex = -1;
                Invalidate();
            };

            MouseClick += (s, e) =>
            {
                if (!IsInteractive || hand == null) return;
                int idx = GetCardIndexAt(e.X);
                if (idx >= 0 && idx < hand.Count)
                    CardClicked?.Invoke(hand.Cards[idx]);
            };
        }

        public void SetHand(Hand h)
        {
            hand = h;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            // Header label
            string label = playerColor == PlayerColor.Red ? "紅方手牌" : "黑方手牌";
            using (Font f = new Font("微軟正黑體", 9, FontStyle.Bold))
            using (Brush b = new SolidBrush(Color.FromArgb(220, 200, 150)))
            {
                g.DrawString(label, f, b, 4, 2);
            }

            if (hand == null || hand.Count == 0)
            {
                using (Font f = new Font("微軟正黑體", 9))
                using (Brush b = new SolidBrush(Color.FromArgb(150, 150, 150)))
                {
                    g.DrawString("（無手牌）", f, b, Width / 2 - 30, Height / 2 - 8);
                }
                return;
            }

            int cardW = 60;
            int cardH = 84;
            int totalW = hand.Count * (cardW + 8) - 8;
            int startX = (Width - totalW) / 2;
            int cardY = (Height - cardH) / 2 + 8;

            for (int i = 0; i < hand.Count; i++)
            {
                Card card = hand.Cards[i];
                int x = startX + i * (cardW + 8);
                int y = hoveredCardIndex == i ? cardY - 6 : cardY;

                bool showFace = (card.Effect != CardEffect.Duel);
                Image img = showFace ? GetCardImage(card) : GetBackImage();
                if (img != null)
                {
                    g.DrawImage(img, x, y, cardW, cardH);
                }
                else if (showFace)
                {
                    DrawFallbackCard(g, card, x, y, cardW, cardH);
                }
                else
                {
                    g.FillRectangle(Brushes.DarkBlue, x, y, cardW, cardH);
                    g.DrawRectangle(Pens.White, x, y, cardW, cardH);
                }

                // Hover highlight
                if (hoveredCardIndex == i && IsInteractive)
                {
                    using (Pen p = new Pen(Color.Yellow, 2))
                        g.DrawRectangle(p, x, y, cardW, cardH);
                }
            }
        }

        private void DrawFallbackCard(Graphics g, Card card, int x, int y, int w, int h)
        {
            g.FillRectangle(Brushes.White, x, y, w, h);
            g.DrawRectangle(Pens.DarkGray, x, y, w, h);

            bool isRed = card.Suit == CardSuit.Hearts || card.Suit == CardSuit.Diamonds;
            Color textColor = isRed ? Color.Red : Color.Black;

            using (Font f = new Font("微軟正黑體", 12, FontStyle.Bold))
            using (Brush b = new SolidBrush(textColor))
            {
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(card.ToString(), f, b, new RectangleF(x, y, w, h), sf);
            }
        }

        private int GetCardIndexAt(int mouseX)
        {
            if (hand == null || hand.Count == 0) return -1;

            int cardW = 60;
            int totalW = hand.Count * (cardW + 8) - 8;
            int startX = (Width - totalW) / 2;

            for (int i = 0; i < hand.Count; i++)
            {
                int x = startX + i * (cardW + 8);
                if (mouseX >= x && mouseX <= x + cardW)
                    return i;
            }
            return -1;
        }

        private static Image GetCardImage(Card card)
        {
            int idx = card.GetImageIndex();
            if (imageCache.ContainsKey(idx))
                return imageCache[idx];

            string path = Path.Combine(Application.StartupPath, "poker", $"pic{idx}.png");
            Image img = null;
            if (File.Exists(path))
            {
                try { img = Image.FromFile(path); }
                catch { }
            }
            imageCache[idx] = img;
            return img;
        }

        public static Image GetBackImage()
        {
            if (backImage != null) return backImage;
            string path = Path.Combine(Application.StartupPath, "poker", "back.png");
            if (File.Exists(path))
            {
                try { backImage = Image.FromFile(path); }
                catch { }
            }
            return backImage;
        }
    }
}
