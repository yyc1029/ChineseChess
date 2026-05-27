using System;
using System.Media;

namespace ChineseChess.UI
{
#pragma warning disable CS0169
    public class SoundManager
    {
        private SoundPlayer moveSound;
        private SoundPlayer captureSound;
        private SoundPlayer checkSound;
#pragma warning restore CS0169

        public SoundManager()
        {
            // 暫時未加載音效檔案，可在Resources文件夾中添加音效後啟用
            // moveSound = new SoundPlayer("Resources/Sounds/Move.wav");
            // captureSound = new SoundPlayer("Resources/Sounds/Capture.wav");
            // checkSound = new SoundPlayer("Resources/Sounds/Check.wav");
        }

        public void PlayMoveSound()
        {
            try
            {
                // if (moveSound != null)
                //     moveSound.PlaySync();
                System.Media.SystemSounds.Beep.Play(); // 臨時使用系統蜂鳴聲
            }
            catch (Exception ex)
            {
                Console.WriteLine("播放移動音效失敗：" + ex.Message);
            }
        }

        public void PlayCaptureSound()
        {
            try
            {
                // if (captureSound != null)
                //     captureSound.PlaySync();
                System.Media.SystemSounds.Exclamation.Play(); // 臨時使用系統聲音
            }
            catch (Exception ex)
            {
                Console.WriteLine("播放捕子音效失敗：" + ex.Message);
            }
        }

        public void PlayCheckSound()
        {
            try
            {
                // if (checkSound != null)
                //     checkSound.PlaySync();
                System.Media.SystemSounds.Beep.Play(); // 臨時使用系統蜂鳴聲
            }
            catch (Exception ex)
            {
                Console.WriteLine("播放將軍音效失敗：" + ex.Message);
            }
        }
    }
}
