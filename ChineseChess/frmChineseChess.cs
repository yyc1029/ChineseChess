using System;
using System.Linq;
using System.Windows.Forms;
using ChineseChess.UI;
using ChineseChess.Game;
using ChineseChess.Models;

namespace ChineseChess
{
    public partial class frmChineseChess : Form
    {
        private BoardPanel boardPanel;
        private Label lblStatus;
        private Button btnNewGame;
        private Button btnUndo;
        private Button btnGiveUp;
        private ListBox lbMoveHistory;
        private MenuStrip menuStrip;
        private Timer aiTimer;
        private AIPlayer aiPlayer;

        public frmChineseChess()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 620);
            this.MinimumSize = new System.Drawing.Size(750, 520);
            aiPlayer = new AIPlayer();
            InitializeUI();
            SetupGame();
            InitializeAITimer();
            // Resize 事件在 InitializeUI 後掛載，避免控件未初始化時被呼叫
            this.Resize += (s, e) => UpdateLayout();
            // 初始化後立即執行一次布局
            UpdateLayout();
        }

        private void InitializeAITimer()
        {
            aiTimer = new Timer();
            aiTimer.Interval = 1000; // 1 秒延遲，讓遊戲更流暢
            aiTimer.Tick += (s, e) => ExecuteAIMove();
        }

        private void InitializeUI()
        {
            // 菜單
            menuStrip = new MenuStrip();
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("文件");
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("新遊戲", null, (s, e) => NewGame()));
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("保存遊戲", null, (s, e) => SaveGame()));
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("讀取遊戲", null, (s, e) => LoadGame()));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("退出", null, (s, e) => this.Close()));
            menuStrip.Items.Add(fileMenu);

            ToolStripMenuItem gameMenu = new ToolStripMenuItem("遊戲");
            gameMenu.DropDownItems.Add(new ToolStripMenuItem("雙人對戰", null, (s, e) => StartPvP()));
            gameMenu.DropDownItems.Add(new ToolStripMenuItem("玩家 vs AI", null, (s, e) => StartPlayerVsAI()));
            menuStrip.Items.Add(gameMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // 狀態標籤
            lblStatus = new Label
            {
                Text = "狀態：紅方先手",
                Location = new System.Drawing.Point(10, 30),
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 12)
            };
            this.Controls.Add(lblStatus);

            // 棋盤
            boardPanel = new BoardPanel
            {
                Location = new System.Drawing.Point(10, 60),
                Size = new System.Drawing.Size(450, 450)
            };
            boardPanel.OnMoveMade += (from, to) => UpdateStatus();
            this.Controls.Add(boardPanel);

            // 按鈕
            int btnX = 470;
            btnNewGame = new Button
            {
                Text = "新遊戲",
                Location = new System.Drawing.Point(btnX, 60),
                Size = new System.Drawing.Size(100, 40)
            };
            btnNewGame.Click += (s, e) => NewGame();
            this.Controls.Add(btnNewGame);

            btnUndo = new Button
            {
                Text = "悔棋",
                Location = new System.Drawing.Point(btnX, 110),
                Size = new System.Drawing.Size(100, 40)
            };
            btnUndo.Click += (s, e) => Undo();
            this.Controls.Add(btnUndo);

            btnGiveUp = new Button
            {
                Text = "認輸",
                Location = new System.Drawing.Point(btnX, 160),
                Size = new System.Drawing.Size(100, 40)
            };
            btnGiveUp.Click += (s, e) => GiveUp();
            this.Controls.Add(btnGiveUp);

            // 移動歷史
            Label lblHistory = new Label
            {
                Text = "移動歷史：",
                Location = new System.Drawing.Point(btnX, 210),
                AutoSize = true
            };
            this.Controls.Add(lblHistory);

            lbMoveHistory = new ListBox
            {
                Location = new System.Drawing.Point(btnX, 235),
                Size = new System.Drawing.Size(100, 275)
            };
            this.Controls.Add(lbMoveHistory);
        }

        private void SetupGame()
        {
            UpdateStatus();
        }

        private void UpdateLayout()
        {
            if (boardPanel == null || menuStrip == null) return;

            const int margin = 10;
            const int rightPanelWidth = 160;
            const int topOffset = 55; // 菜單欄 + 狀態列

            // 可用空間（扣除右側面板和邊距）
            int availW = this.ClientSize.Width - rightPanelWidth - margin * 3;
            int availH = this.ClientSize.Height - topOffset - margin;

            // 根據棋盤比例（8列:9行 的交叉點間距）計算棋格大小
            // 加上 margin*2 給 BoardPanel 的留白
            const int boardMargin = 30;
            int cellByW = (availW - boardMargin * 2) / 8;
            int cellByH = (availH - boardMargin * 2) / 9;
            int cell = Math.Max(Math.Min(cellByW, cellByH), 30);

            // 棋盤控件大小：棋格 + 留白
            int boardW = 8 * cell + boardMargin * 2;
            int boardH = 9 * cell + boardMargin * 2;

            boardPanel.Location = new System.Drawing.Point(margin, topOffset);
            boardPanel.Size = new System.Drawing.Size(boardW, boardH);
            boardPanel.Invalidate();

            // 右側面板
            int rightX = boardPanel.Right + margin;
            int rightW = this.ClientSize.Width - rightX - margin;
            if (rightW < 100) rightW = 100;

            lblStatus.Location = new System.Drawing.Point(rightX, topOffset);
            lblStatus.Width = rightW;

            btnNewGame.Location = new System.Drawing.Point(rightX, topOffset + 30);
            btnNewGame.Width = rightW;

            btnUndo.Location = new System.Drawing.Point(rightX, topOffset + 80);
            btnUndo.Width = rightW;

            btnGiveUp.Location = new System.Drawing.Point(rightX, topOffset + 130);
            btnGiveUp.Width = rightW;

            Label lblHistory = this.Controls.Cast<Control>()
                .OfType<Label>()
                .FirstOrDefault(l => l.Text == "移動歷史：");
            if (lblHistory != null)
                lblHistory.Location = new System.Drawing.Point(rightX, topOffset + 185);

            lbMoveHistory.Location = new System.Drawing.Point(rightX, topOffset + 210);
            lbMoveHistory.Size = new System.Drawing.Size(rightW,
                this.ClientSize.Height - (topOffset + 210) - margin);
        }

        private void UpdateStatus()
        {
            GameLogic logic = boardPanel.GameLogic;
            GameState state = logic.GameState;

            string statusText = $"狀態：{(state.CurrentPlayer == PlayerColor.Red ? "紅" : "黑")}方";
            if (state.IsInCheck)
                statusText += " [將軍！]";

            switch (state.Status)
            {
                case GameStatus.RedWin:
                    statusText = "遊戲結束：紅方勝！";
                    btnNewGame.Enabled = true;
                    btnUndo.Enabled = false;
                    break;
                case GameStatus.BlackWin:
                    statusText = "遊戲結束：黑方勝！";
                    btnNewGame.Enabled = true;
                    btnUndo.Enabled = false;
                    break;
                case GameStatus.Draw:
                    statusText = "遊戲結束：和棋";
                    btnNewGame.Enabled = true;
                    btnUndo.Enabled = false;
                    break;
                default:
                    btnNewGame.Enabled = true;
                    btnUndo.Enabled = true;
                    break;
            }

            lblStatus.Text = statusText;

            // 更新移動歷史
            lbMoveHistory.Items.Clear();
            int moveIndex = 1;
            foreach (Move move in logic.MoveHistory)
            {
                string moveStr = $"{moveIndex}. {move.From} → {move.To}";
                if (move.CapturedPiece != null)
                    moveStr += $" 吃{move.CapturedPiece.GetCharCode()}";
                lbMoveHistory.Items.Add(moveStr);
                moveIndex++;
            }

            if (lbMoveHistory.Items.Count > 0)
                lbMoveHistory.SelectedIndex = lbMoveHistory.Items.Count - 1;
        }

        private void NewGame()
        {
            DialogResult result = MessageBox.Show("選擇遊戲模式\n\nYES: 雙人對戰\nNO: 玩家 vs AI", "新遊戲", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                StartPvP();
            }
            else
            {
                StartPlayerVsAI();
            }
        }

        private void StartPvP()
        {
            boardPanel.GameLogic.SetGameMode(GameMode.PvP);
            boardPanel.ResetGame();
            aiTimer.Stop();
            UpdateStatus();
        }

        private void StartPlayerVsAI()
        {
            boardPanel.GameLogic.SetGameMode(GameMode.PlayerVsAI);
            boardPanel.ResetGame();
            aiTimer.Start();
            UpdateStatus();
        }

        private void ExecuteAIMove()
        {
            GameLogic logic = boardPanel.GameLogic;

            // 只在AI模式且黑方回合時執行
            if (logic.GameState.Mode != GameMode.PlayerVsAI ||
                logic.GameState.CurrentPlayer != PlayerColor.Black ||
                logic.GameState.Status != GameStatus.Playing)
            {
                return;
            }

            Move aiMove = aiPlayer.GetNextMove(logic);
            if (aiMove != null)
            {
                logic.MovePiece(aiMove.From, aiMove.To);
                boardPanel.GameState.SelectedPosition = new Position(-1, -1);
                UpdateStatus();
                boardPanel.Invalidate();
            }
        }

        private void SaveGame()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "象棋遊戲檔 (*.chess)|*.chess|All files (*.*)|*.*",
                DefaultExt = "chess"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (GameSerializer.SaveGame(boardPanel.GameLogic, dialog.FileName))
                {
                    MessageBox.Show("遊戲已保存：" + dialog.FileName);
                }
            }
        }

        private void LoadGame()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "象棋遊戲檔 (*.chess)|*.chess|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                GameLogic loaded = GameSerializer.LoadGame(dialog.FileName);
                if (loaded != null)
                {
                    // 替換當前遊戲邏輯
                    boardPanel.LoadGameLogic(loaded);
                    aiTimer.Stop();
                    if (loaded.GameState.Mode == GameMode.PlayerVsAI)
                    {
                        aiTimer.Start();
                    }
                    UpdateStatus();
                    boardPanel.Invalidate();
                    MessageBox.Show("遊戲已讀取");
                }
            }
        }

        private void Undo()
        {
            if (boardPanel.GameLogic.Undo())
            {
                UpdateStatus();
                boardPanel.Invalidate();
            }
        }

        private void GiveUp()
        {
            DialogResult result = MessageBox.Show("確定要認輸嗎？", "認輸", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                GameState state = boardPanel.GameLogic.GameState;
                state.Status = state.CurrentPlayer == PlayerColor.Red ? GameStatus.BlackWin : GameStatus.RedWin;
                UpdateStatus();
                boardPanel.Invalidate();
            }
        }
    }
}
