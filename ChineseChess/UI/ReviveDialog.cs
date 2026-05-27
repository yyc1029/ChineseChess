using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ChineseChess.Models;

namespace ChineseChess.UI
{
    public class ReviveDialog : Form
    {
        public Piece SelectedPiece { get; private set; }

        public ReviveDialog(List<Piece> capturedPieces, PlayerColor player)
        {
            Text = "復活棋子";
            Width = 380;
            Height = 260;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(50, 40, 30);

            string colorName = player == PlayerColor.Red ? "紅方" : "黑方";

            Label lbl = new Label
            {
                Text = $"選擇要復活的 {colorName} 棋子：",
                ForeColor = Color.FromArgb(220, 200, 150),
                Font = new Font("微軟正黑體", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(lbl);

            if (capturedPieces == null || capturedPieces.Count == 0)
            {
                Label lblNone = new Label
                {
                    Text = "沒有可復活的棋子",
                    ForeColor = Color.OrangeRed,
                    AutoSize = true,
                    Location = new Point(20, 60)
                };
                Controls.Add(lblNone);

                Button btnCancel = new Button
                {
                    Text = "取消",
                    Location = new Point(140, 180),
                    Size = new Size(100, 35),
                    DialogResult = DialogResult.Cancel,
                    BackColor = Color.FromArgb(80, 60, 40),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                Controls.Add(btnCancel);
                CancelButton = btnCancel;
                return;
            }

            int x = 20, y = 60;
            foreach (Piece piece in capturedPieces)
            {
                Piece captured = piece;
                Button btn = new Button
                {
                    Text = captured.ToString() + $"\n({captured.GetCharCode()})",
                    Size = new Size(70, 50),
                    Location = new Point(x, y),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = player == PlayerColor.Red ? Color.FromArgb(180, 50, 50) : Color.FromArgb(40, 40, 100),
                    ForeColor = Color.White,
                    Font = new Font("微軟正黑體", 9)
                };
                btn.Click += (s, e) =>
                {
                    SelectedPiece = captured;
                    DialogResult = DialogResult.OK;
                    Close();
                };
                Controls.Add(btn);

                x += 80;
                if (x + 70 > Width - 20)
                {
                    x = 20;
                    y += 60;
                }
            }

            Button btnCancelBottom = new Button
            {
                Text = "取消",
                Location = new Point(140, 190),
                Size = new Size(100, 35),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(80, 60, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            Controls.Add(btnCancelBottom);
            CancelButton = btnCancelBottom;
        }
    }
}
