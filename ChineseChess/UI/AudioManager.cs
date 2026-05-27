using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ChineseChess.UI
{
    public class AudioManager : IDisposable
    {
        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern int mciSendString(string command, StringBuilder returnValue, int returnLength, IntPtr winHandle);

        private List<string> playlist = new List<string>();
        private int currentIndex = -1;
        private bool isPlaying = false;
        private bool isPaused = false;
        private const string Alias = "bgm_track";
        private Timer checkTimer;
        private bool disposed = false;
        private int currentVolume = 70;

        public bool IsPlaying => isPlaying;
        public bool IsPaused => isPaused;
        public int CurrentVolume => currentVolume;
        public int TrackCount => playlist.Count;

        public string CurrentTrackName
        {
            get
            {
                if (currentIndex >= 0 && currentIndex < playlist.Count)
                    return Path.GetFileNameWithoutExtension(playlist[currentIndex]);
                return "無曲目";
            }
        }

        public event EventHandler TrackChanged;

        public AudioManager()
        {
            checkTimer = new Timer();
            checkTimer.Interval = 1000;
            checkTimer.Tick += (s, e) => CheckPlaybackStatus();
        }

        public void LoadPlaylist(IEnumerable<string> paths)
        {
            Stop();
            playlist.Clear();
            foreach (string path in paths)
            {
                if (File.Exists(path))
                    playlist.Add(path);
            }
            currentIndex = playlist.Count > 0 ? 0 : -1;
        }

        public void Play()
        {
            if (playlist.Count == 0) return;

            if (isPaused)
            {
                mciSendString($"resume {Alias}", null, 0, IntPtr.Zero);
                isPlaying = true;
                isPaused = false;
                checkTimer.Start();
                return;
            }

            if (currentIndex < 0) currentIndex = 0;
            PlayCurrent();
        }

        public void Pause()
        {
            if (!isPlaying) return;
            mciSendString($"pause {Alias}", null, 0, IntPtr.Zero);
            isPlaying = false;
            isPaused = true;
            checkTimer.Stop();
        }

        public void Stop()
        {
            try
            {
                mciSendString($"stop {Alias}", null, 0, IntPtr.Zero);
                mciSendString($"close {Alias}", null, 0, IntPtr.Zero);
            }
            catch { }
            isPlaying = false;
            isPaused = false;
            checkTimer.Stop();
        }

        public void Next()
        {
            if (playlist.Count == 0) return;
            Stop();
            currentIndex = (currentIndex + 1) % playlist.Count;
            PlayCurrent();
            TrackChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Previous()
        {
            if (playlist.Count == 0) return;
            Stop();
            currentIndex = (currentIndex - 1 + playlist.Count) % playlist.Count;
            PlayCurrent();
            TrackChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetVolume(int volume)
        {
            currentVolume = Math.Max(0, Math.Min(100, volume));
            try
            {
                // MCI volume is 0-1000
                mciSendString($"setaudio {Alias} volume to {currentVolume * 10}", null, 0, IntPtr.Zero);
            }
            catch { }
        }

        private void PlayCurrent()
        {
            if (currentIndex < 0 || currentIndex >= playlist.Count) return;

            try
            {
                mciSendString($"close {Alias}", null, 0, IntPtr.Zero);
                string path = playlist[currentIndex];
                int result = mciSendString($"open \"{path}\" type mpegvideo alias {Alias}", null, 0, IntPtr.Zero);
                if (result == 0)
                {
                    mciSendString($"setaudio {Alias} volume to {currentVolume * 10}", null, 0, IntPtr.Zero);
                    mciSendString($"play {Alias}", null, 0, IntPtr.Zero);
                    isPlaying = true;
                    isPaused = false;
                    checkTimer.Start();
                    TrackChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            catch
            {
                isPlaying = false;
            }
        }

        private void CheckPlaybackStatus()
        {
            if (!isPlaying) return;

            try
            {
                StringBuilder status = new StringBuilder(128);
                mciSendString($"status {Alias} mode", status, 128, IntPtr.Zero);

                if (status.ToString().Trim().ToLower() == "stopped")
                {
                    // Track ended - advance to next
                    checkTimer.Stop();
                    isPlaying = false;
                    currentIndex = (currentIndex + 1) % playlist.Count;
                    PlayCurrent();
                    TrackChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            catch { }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                Stop();
                checkTimer.Dispose();
                disposed = true;
            }
        }
    }
}
