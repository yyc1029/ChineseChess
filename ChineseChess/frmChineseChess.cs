using System;
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

        public frmChineseChess()
        {
            InitializeComponent();
            InitializeUI();
            SetupGame();
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
                Size = new System.Drawing.Size(450, 450),
                BorderStyle = BorderStyle.FixedSingle
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
            UpdateStatus();
        }

        private void StartPlayerVsAI()
        {
            boardPanel.GameLogic.SetGameMode(GameMode.PlayerVsAI);
            boardPanel.ResetGame();
            UpdateStatus();
        }

        private void SaveGame()
        {
            MessageBox.Show("保存遊戲功能將在第4階段實現", "提示");
        }

        private void LoadGame()
        {
            MessageBox.Show("讀取遊戲功能將在第4階段實現", "提示");
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
