using System.Drawing;
using System.Windows.Forms;

namespace ChineseChess.UI
{
    public class GameModeDialog : Form
    {
        public enum ModeChoice { PvP, AI, Cancel }
        public ModeChoice Choice { get; private set; } = ModeChoice.PvP;

        public GameModeDialog()
        {
            Text = "新遊戲";
            Width = 300;
            Height = 190;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(50, 40, 30);

            Label lbl = new Label
            {
                Text = "請選擇遊戲模式：",
                ForeColor = Color.FromArgb(220, 200, 150),
                Font = new Font("微軟正黑體", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(30, 20)
            };
            Controls.Add(lbl);

            Button btnPvP = new Button
            {
                Text = "雙人對戰",
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(80, 60, 40),
                ForeColor = Color.White
            };
            btnPvP.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 60);
            btnPvP.Click += (s, e) => { Choice = ModeChoice.PvP; DialogResult = DialogResult.OK; Close(); };
            Controls.Add(btnPvP);

            Button btnAI = new Button
            {
                Text = "對戰 AI",
                Location = new System.Drawing.Point(160, 60),
                Size = new System.Drawing.Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(80, 60, 40),
                ForeColor = Color.White
            };
            btnAI.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 60);
            btnAI.Click += (s, e) => { Choice = ModeChoice.AI; DialogResult = DialogResult.OK; Close(); };
            Controls.Add(btnAI);

            Button btnCancel = new Button
            {
                Text = "取消",
                Location = new System.Drawing.Point(90, 120),
                Size = new System.Drawing.Size(100, 32),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(60, 50, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(100, 80, 50);
            Controls.Add(btnCancel);
            CancelButton = btnCancel;
        }
    }
}
