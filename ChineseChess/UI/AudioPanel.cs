using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChineseChess.UI
{
    public class AudioPanel : Panel
    {
        private AudioManager audioManager;
        private Button btnPrev;
        private Button btnPlayPause;
        private Button btnNext;
        private TrackBar trackVolume;
        private Label lblTrack;
        private Label lblVolume;

        public AudioPanel()
        {
            Height = 90;
            BackColor = Color.FromArgb(40, 30, 20);
            BuildControls();
        }

        private void BuildControls()
        {
            lblTrack = new Label
            {
                Text = "♪ 無曲目",
                ForeColor = Color.FromArgb(220, 200, 150),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("微軟正黑體", 9)
            };
            Controls.Add(lblTrack);

            Panel btnPanel = new Panel { Dock = DockStyle.Top, Height = 30, Top = 22 };

            btnPrev = new Button { Text = "◀◀", Width = 40, Height = 26, Left = 5, Top = 2, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(60, 50, 40) };
            btnPrev.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 60);
            btnPrev.Click += (s, e) => audioManager?.Previous();

            btnPlayPause = new Button { Text = "▶", Width = 50, Height = 26, Left = 50, Top = 2, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(60, 50, 40) };
            btnPlayPause.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 60);
            btnPlayPause.Click += (s, e) => TogglePlayPause();

            btnNext = new Button { Text = "▶▶", Width = 40, Height = 26, Left = 105, Top = 2, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(60, 50, 40) };
            btnNext.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 60);
            btnNext.Click += (s, e) => audioManager?.Next();

            btnPanel.Controls.Add(btnPrev);
            btnPanel.Controls.Add(btnPlayPause);
            btnPanel.Controls.Add(btnNext);
            Controls.Add(btnPanel);

            lblVolume = new Label
            {
                Text = "音量",
                ForeColor = Color.FromArgb(180, 160, 120),
                AutoSize = true,
                Left = 5,
                Top = 56
            };
            Controls.Add(lblVolume);

            trackVolume = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 70,
                TickFrequency = 10,
                Left = 40,
                Top = 52,
                Width = Width - 50,
                Height = 30,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            trackVolume.Scroll += (s, e) => audioManager?.SetVolume(trackVolume.Value);
            Controls.Add(trackVolume);

            Resize += (s, e) =>
            {
                if (trackVolume != null)
                    trackVolume.Width = Width - 50;
            };
        }

        public void SetAudioManager(AudioManager manager)
        {
            audioManager = manager;
            if (audioManager != null)
            {
                audioManager.TrackChanged += (s, e) => UpdateUI();
                audioManager.SetVolume(trackVolume.Value);
                UpdateUI();
            }
        }

        private void TogglePlayPause()
        {
            if (audioManager == null) return;

            if (audioManager.IsPlaying)
                audioManager.Pause();
            else
                audioManager.Play();

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (lblTrack == null || btnPlayPause == null) return;

            if (InvokeRequired)
            {
                Invoke(new Action(UpdateUI));
                return;
            }

            lblTrack.Text = $"♪ {(audioManager != null ? audioManager.CurrentTrackName : "無曲目")}";
            btnPlayPause.Text = (audioManager != null && audioManager.IsPlaying) ? "⏸" : "▶";
        }
    }
}
