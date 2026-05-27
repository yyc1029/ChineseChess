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
        private ListBox lstTracks;
        private bool updatingList = false;

        public AudioPanel()
        {
            Height = 145;
            BackColor = Color.FromArgb(40, 30, 20);
            BuildControls();
        }

        private void BuildControls()
        {
            lstTracks = new ListBox
            {
                Location = new Point(0, 0),
                Size = new Size(170, 50),
                BackColor = Color.FromArgb(35, 28, 18),
                ForeColor = Color.FromArgb(200, 180, 130),
                Font = new Font("微軟正黑體", 8),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            lstTracks.SelectedIndexChanged += (s, e) =>
            {
                if (updatingList) return;
                if (audioManager != null && lstTracks.SelectedIndex >= 0)
                    audioManager.JumpTo(lstTracks.SelectedIndex);
            };
            Controls.Add(lstTracks);

            lblTrack = new Label
            {
                Text = "♪ 無曲目",
                ForeColor = Color.FromArgb(220, 200, 150),
                Location = new Point(0, 52),
                Size = new Size(170, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("微軟正黑體", 9),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            Controls.Add(lblTrack);

            btnPrev = new Button
            {
                Text = "◀◀", Width = 40, Height = 26, Left = 5, Top = 74,
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(60, 50, 40)
            };
            btnPrev.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 60);
            btnPrev.Click += (s, e) => audioManager?.Previous();
            Controls.Add(btnPrev);

            btnPlayPause = new Button
            {
                Text = "▶", Width = 50, Height = 26, Left = 50, Top = 74,
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(60, 50, 40)
            };
            btnPlayPause.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 60);
            btnPlayPause.Click += (s, e) => TogglePlayPause();
            Controls.Add(btnPlayPause);

            btnNext = new Button
            {
                Text = "▶▶", Width = 40, Height = 26, Left = 105, Top = 74,
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(60, 50, 40)
            };
            btnNext.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 60);
            btnNext.Click += (s, e) => audioManager?.Next();
            Controls.Add(btnNext);

            Label lblVolume = new Label
            {
                Text = "音量",
                ForeColor = Color.FromArgb(180, 160, 120),
                AutoSize = true,
                Left = 5,
                Top = 108
            };
            Controls.Add(lblVolume);

            trackVolume = new TrackBar
            {
                Minimum = 0, Maximum = 100, Value = 70,
                TickFrequency = 10,
                Left = 40, Top = 104,
                Width = 120, Height = 30,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            trackVolume.Scroll += (s, e) => audioManager?.SetVolume(trackVolume.Value);
            Controls.Add(trackVolume);

            Resize += (s, e) =>
            {
                if (trackVolume != null) trackVolume.Width = Math.Max(60, Width - 50);
                if (lstTracks != null) lstTracks.Width = Width;
                if (lblTrack != null) lblTrack.Width = Width;
            };
        }

        public void SetAudioManager(AudioManager manager)
        {
            audioManager = manager;
            if (audioManager != null)
            {
                audioManager.TrackChanged += (s, e) => UpdateUI();
                audioManager.SetVolume(trackVolume.Value);
                PopulateTrackList();
                UpdateUI();
            }
        }

        private void PopulateTrackList()
        {
            if (lstTracks == null || audioManager == null) return;
            updatingList = true;
            lstTracks.Items.Clear();
            foreach (string name in audioManager.TrackNames)
                lstTracks.Items.Add(name);
            updatingList = false;
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

            lblTrack.Text = "♪ " + (audioManager != null ? audioManager.CurrentTrackName : "無曲目");
            btnPlayPause.Text = (audioManager != null && audioManager.IsPlaying) ? "⏸" : "▶";

            if (audioManager != null && lstTracks != null && lstTracks.Items.Count > 0)
            {
                int idx = audioManager.CurrentTrackIndex;
                if (idx >= 0 && idx < lstTracks.Items.Count && lstTracks.SelectedIndex != idx)
                {
                    updatingList = true;
                    lstTracks.SelectedIndex = idx;
                    updatingList = false;
                }
            }
        }
    }
}
