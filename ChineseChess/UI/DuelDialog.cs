using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ChineseChess.Models;

namespace ChineseChess.UI
{
    public class DuelDialog : Form
    {
        public Card SelectedDefenderCard { get; private set; }

        private Hand defenderHand;
        private Card attackerCard;
        private PlayerColor defender;

        public DuelDialog(Card attackerCard, PlayerColor defender, Hand defenderHand)
        {
            this.attackerCard = attackerCard;
            this.defender = defender;
            this.defenderHand = defenderHand;

            Text = "對決！";
            Width = 460;
            Height = 300;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(50, 40, 30);

            BuildUI();
        }

        private void BuildUI()
        {
            string attackerName = defender == PlayerColor.Black ? "紅方" : "黑方";
            string defenderName = defender == PlayerColor.Red ? "紅方" : "黑方";

            Label lblAttacker = new Label
            {
                Text = $"{attackerName} 出牌：{attackerCard}（對決值 {attackerCard.GetDuelValue()}）",
                ForeColor = Color.FromArgb(220, 200, 150),
                Font = new Font("微軟正黑體", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(lblAttacker);

            // Show attacker card image
            PictureBox picAttacker = new PictureBox
            {
                Size = new Size(60, 84),
                Location = new Point(20, 50),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            picAttacker.Image = LoadCardImage(attackerCard);
            Controls.Add(picAttacker);

            Label lblDefender = new Label
            {
                Text = $"{defenderName} 選擇回應牌（點擊選牌）：",
                ForeColor = Color.FromArgb(220, 200, 150),
                Font = new Font("微軟正黑體", 10),
                AutoSize = true,
                Location = new Point(110, 50)
            };
            Controls.Add(lblDefender);

            int cardX = 110;
            bool hasDuelCards = false;
            foreach (Card card in defenderHand.Cards)
            {
                if (card.Effect != CardEffect.Duel) continue;
                hasDuelCards = true;
                Card captured = card;

                Button btn = new Button
                {
                    Size = new Size(70, 94),
                    Location = new Point(cardX, 75),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    Padding = new Padding(2)
                };

                Image img = LoadCardImage(captured);
                if (img != null)
                    btn.BackgroundImage = img;
                else
                    btn.Text = captured.ToString();

                btn.Click += (s, e) =>
                {
                    SelectedDefenderCard = captured;
                    DialogResult = DialogResult.OK;
                    Close();
                };
                Controls.Add(btn);
                cardX += 78;
            }

            if (!hasDuelCards)
            {
                Label lblNone = new Label
                {
                    Text = $"{defenderName} 無對決牌，自動判負！",
                    ForeColor = Color.OrangeRed,
                    Font = new Font("微軟正黑體", 10),
                    AutoSize = true,
                    Location = new Point(110, 90)
                };
                Controls.Add(lblNone);

                Button btnOK = new Button
                {
                    Text = "確認",
                    Location = new Point(180, 220),
                    Size = new Size(100, 35),
                    DialogResult = DialogResult.OK,
                    BackColor = Color.FromArgb(80, 60, 40),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                Controls.Add(btnOK);
                AcceptButton = btnOK;
            }
        }

        private Image LoadCardImage(Card card)
        {
            string path = Path.Combine(Application.StartupPath, "poker", $"pic{card.GetImageIndex()}.png");
            try { if (File.Exists(path)) return Image.FromFile(path); }
            catch { }
            return null;
        }
    }
}
