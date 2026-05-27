using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ChineseChess.Models;

namespace ChineseChess.UI
{
    public class DuelDialog : Form
    {
        public Card SelectedDefenderCard { get; private set; }

        private Card attackerCard;
        private PlayerColor defender;
        private Hand defenderHand;

        public DuelDialog(Card attackerCard, PlayerColor defender, Hand defenderHand)
        {
            this.attackerCard = attackerCard;
            this.defender = defender;
            this.defenderHand = defenderHand;

            Text = "對決！";
            Width = 500;
            Height = 320;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(50, 40, 30);

            BuildPhase1();
        }

        private void BuildPhase1()
        {
            Controls.Clear();

            string attackerName = defender == PlayerColor.Black ? "紅方" : "黑方";
            string defenderName = defender == PlayerColor.Red ? "紅方" : "黑方";

            Label lbl = new Label
            {
                Text = $"{attackerName} 發起對決，{defenderName} 請選擇回應牌：",
                ForeColor = Color.FromArgb(220, 200, 150),
                Font = new Font("微軟正黑體", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(20, 15)
            };
            Controls.Add(lbl);

            // Attacker card shown as back
            PictureBox picAttacker = new PictureBox
            {
                Size = new System.Drawing.Size(60, 84),
                Location = new System.Drawing.Point(20, 50),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = GetBackImage()
            };
            Controls.Add(picAttacker);

            Label lblTag = new Label
            {
                Text = attackerName,
                ForeColor = Color.FromArgb(180, 160, 120),
                AutoSize = true,
                Location = new System.Drawing.Point(20, 138)
            };
            Controls.Add(lblTag);

            // Defender's duel cards shown as backs
            bool hasDuelCards = false;
            int cardX = 120;

            foreach (Card card in defenderHand.Cards)
            {
                if (card.Effect != CardEffect.Duel) continue;
                hasDuelCards = true;
                Card captured = card;

                Button btn = new Button
                {
                    Size = new System.Drawing.Size(60, 84),
                    Location = new System.Drawing.Point(cardX, 50),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    Padding = new System.Windows.Forms.Padding(0)
                };

                Image backImg = GetBackImage();
                if (backImg != null)
                {
                    btn.BackgroundImage = backImg;
                    btn.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else
                {
                    btn.Text = "?";
                }

                btn.Click += (s, e) =>
                {
                    SelectedDefenderCard = captured;
                    ShowRevealPhase();
                };
                Controls.Add(btn);
                cardX += 70;
            }

            if (!hasDuelCards)
            {
                Label lblNone = new Label
                {
                    Text = $"{defenderName} 無對決牌，自動判負！",
                    ForeColor = Color.OrangeRed,
                    Font = new Font("微軟正黑體", 10),
                    AutoSize = true,
                    Location = new System.Drawing.Point(120, 90)
                };
                Controls.Add(lblNone);

                Button btnOK = new Button
                {
                    Text = "確認",
                    Location = new System.Drawing.Point(185, 220),
                    Size = new System.Drawing.Size(100, 35),
                    DialogResult = DialogResult.OK,
                    BackColor = Color.FromArgb(80, 60, 40),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                Controls.Add(btnOK);
                AcceptButton = btnOK;
            }
        }

        private void ShowRevealPhase()
        {
            Controls.Clear();

            string attackerName = defender == PlayerColor.Black ? "紅方" : "黑方";
            string defenderName = defender == PlayerColor.Red ? "紅方" : "黑方";

            int attackerVal = attackerCard.GetDuelValue();
            int defenderVal = SelectedDefenderCard != null ? SelectedDefenderCard.GetDuelValue() : 0;

            string resultText;
            if (SelectedDefenderCard == null)
                resultText = $"{attackerName} 勝！（{defenderName}無牌自動判負）";
            else if (attackerVal > defenderVal)
                resultText = $"{attackerName} 勝！{PointName(attackerCard.Point)}={attackerVal} vs {PointName(SelectedDefenderCard.Point)}={defenderVal}，{attackerName}獲得額外行動！";
            else if (defenderVal > attackerVal)
                resultText = $"{defenderName} 勝！{PointName(SelectedDefenderCard.Point)}={defenderVal} vs {PointName(attackerCard.Point)}={attackerVal}，{defenderName}下回合獲得額外行動！";
            else
                resultText = $"平手！（雙方皆為 {PointName(attackerCard.Point)}={attackerVal}），雙方各補一張牌。";

            Label lblResult = new Label
            {
                Text = resultText,
                ForeColor = Color.FromArgb(255, 230, 100),
                Font = new Font("微軟正黑體", 10, FontStyle.Bold),
                Location = new System.Drawing.Point(15, 15),
                Size = new System.Drawing.Size(465, 40),
                AutoSize = false
            };
            Controls.Add(lblResult);

            // Attacker card face
            PictureBox picAttacker = new PictureBox
            {
                Size = new System.Drawing.Size(60, 84),
                Location = new System.Drawing.Point(70, 65),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = LoadCardImage(attackerCard)
            };
            Controls.Add(picAttacker);

            Label lblA = new Label
            {
                Text = attackerName + "\n" + PointName(attackerCard.Point) + " = " + attackerVal + "點",
                ForeColor = Color.FromArgb(220, 200, 150),
                Location = new System.Drawing.Point(40, 152),
                AutoSize = true,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            Controls.Add(lblA);

            // VS label
            Label lblVS = new Label
            {
                Text = "VS",
                ForeColor = Color.OrangeRed,
                Font = new Font("微軟正黑體", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(165, 95)
            };
            Controls.Add(lblVS);

            // Defender card face
            PictureBox picDefender = new PictureBox
            {
                Size = new System.Drawing.Size(60, 84),
                Location = new System.Drawing.Point(230, 65),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = SelectedDefenderCard != null ? LoadCardImage(SelectedDefenderCard) : GetBackImage()
            };
            Controls.Add(picDefender);

            string defenderCardLabel = SelectedDefenderCard != null
                ? defenderName + "\n" + PointName(SelectedDefenderCard.Point) + " = " + defenderVal + "點"
                : defenderName + "\n無牌";
            Label lblD = new Label
            {
                Text = defenderCardLabel,
                ForeColor = Color.FromArgb(220, 200, 150),
                Location = new System.Drawing.Point(205, 152),
                AutoSize = true,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            Controls.Add(lblD);

            Button btnOK = new Button
            {
                Text = "確認",
                Location = new System.Drawing.Point(185, 240),
                Size = new System.Drawing.Size(100, 35),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(80, 60, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 60);
            Controls.Add(btnOK);
            AcceptButton = btnOK;
        }

        private string PointName(int point)
        {
            if (point == 1) return "A";
            if (point == 11) return "J";
            if (point == 12) return "Q";
            if (point == 13) return "K";
            return point.ToString();
        }

        private Image LoadCardImage(Card card)
        {
            string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "poker", "pic" + card.GetImageIndex() + ".png");
            try
            {
                if (File.Exists(path)) return Image.FromFile(path);
            }
            catch { }
            return null;
        }

        private Image GetBackImage()
        {
            string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "poker", "back.png");
            try
            {
                if (File.Exists(path)) return Image.FromFile(path);
            }
            catch { }
            return null;
        }
    }
}
