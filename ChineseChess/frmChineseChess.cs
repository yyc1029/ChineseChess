using System;
using System.Collections.Generic;
using System.IO;
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
        private Button btnRules;
        private ListBox lbMoveHistory;
        private MenuStrip menuStrip;
        private Timer aiTimer;
        private AIPlayer aiPlayer;

        // Card panels
        private CardPanel redCardPanel;
        private CardPanel blackCardPanel;

        // Audio
        private AudioManager audioManager;
        private AudioPanel audioPanel;

        // Card effect processor
        private CardEffectProcessor cardEffectProcessor;

        // Freeze selection mode
        private bool selectingFreezeTarget = false;
        private Card pendingFreezeCard = null;

        public frmChineseChess()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 820);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            aiPlayer = new AIPlayer();
            InitializeUI();
            SetupGame();
            InitializeAITimer();
            InitializeAudio();
            this.Resize += (s, e) => UpdateLayout();
            UpdateLayout();
            this.Load += (s, e) =>
            {
                using (var rules = new RulesDialog()) rules.ShowDialog(this);
                using (var mode = new GameModeDialog())
                {
                    mode.ShowDialog(this);
                    if (mode.Choice == GameModeDialog.ModeChoice.AI)
                        StartPlayerVsAI();
                    else
                        StartPvP();
                }
            };
        }

        private void InitializeAITimer()
        {
            aiTimer = new Timer();
            aiTimer.Interval = 1000;
            aiTimer.Tick += (s, e) => ExecuteAITurn();
        }

        private void InitializeAudio()
        {
            audioManager = new AudioManager();
            string musicDir = Path.Combine(Application.StartupPath, "Resources", "Music");
            if (Directory.Exists(musicDir))
            {
                string[] mp3Files = Directory.GetFiles(musicDir, "*.mp3");
                if (mp3Files.Length > 0)
                {
                    audioManager.LoadPlaylist(mp3Files);
                    audioManager.Play();
                }
            }
            audioPanel.SetAudioManager(audioManager);
        }

        private void InitializeUI()
        {
            // Menu
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

            // Status label
            lblStatus = new Label
            {
                Text = "狀態：紅方先手",
                Location = new System.Drawing.Point(10, 30),
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 12)
            };
            this.Controls.Add(lblStatus);

            // Black card panel (above board)
            blackCardPanel = new CardPanel(PlayerColor.Black)
            {
                Location = new System.Drawing.Point(10, 55),
                IsInteractive = false
            };
            blackCardPanel.CardClicked += OnCardClicked;
            this.Controls.Add(blackCardPanel);

            // Chess board
            boardPanel = new BoardPanel
            {
                Location = new System.Drawing.Point(10, 175),
                Size = new System.Drawing.Size(450, 450)
            };
            boardPanel.OnMoveMade += (from, to) => OnMoveMade(from, to);
            boardPanel.OnBoardClicked += OnBoardClickedForFreeze;
            this.Controls.Add(boardPanel);

            // Red card panel (below board)
            redCardPanel = new CardPanel(PlayerColor.Red)
            {
                Location = new System.Drawing.Point(10, 635)
            };
            redCardPanel.CardClicked += OnCardClicked;
            this.Controls.Add(redCardPanel);

            // Right panel controls
            int btnX = 470;

            btnNewGame = new Button
            {
                Text = "新遊戲",
                Location = new System.Drawing.Point(btnX, 60),
                Size = new System.Drawing.Size(120, 40)
            };
            btnNewGame.Click += (s, e) => NewGame();
            this.Controls.Add(btnNewGame);

            btnUndo = new Button
            {
                Text = "悔棋",
                Location = new System.Drawing.Point(btnX, 110),
                Size = new System.Drawing.Size(120, 40)
            };
            btnUndo.Click += (s, e) => Undo();
            this.Controls.Add(btnUndo);

            btnGiveUp = new Button
            {
                Text = "認輸",
                Location = new System.Drawing.Point(btnX, 160),
                Size = new System.Drawing.Size(120, 40)
            };
            btnGiveUp.Click += (s, e) => GiveUp();
            this.Controls.Add(btnGiveUp);

            btnRules = new Button
            {
                Text = "查看規則",
                Location = new System.Drawing.Point(btnX, 210),
                Size = new System.Drawing.Size(120, 40)
            };
            btnRules.Click += (s, e) => { using (var dlg = new RulesDialog()) dlg.ShowDialog(this); };
            this.Controls.Add(btnRules);

            Label lblHistory = new Label
            {
                Text = "行動記錄：",
                Location = new System.Drawing.Point(btnX, 210),
                AutoSize = true
            };
            this.Controls.Add(lblHistory);

            lbMoveHistory = new ListBox
            {
                Location = new System.Drawing.Point(btnX, 235),
                Size = new System.Drawing.Size(120, 200)
            };
            this.Controls.Add(lbMoveHistory);

            // Audio panel
            audioPanel = new AudioPanel
            {
                Location = new System.Drawing.Point(btnX, 500),
                Size = new System.Drawing.Size(120, 145)
            };
            this.Controls.Add(audioPanel);

            ApplyDarkTheme();
        }

        private void ApplyDarkTheme()
        {
            this.BackColor = System.Drawing.Color.FromArgb(50, 40, 30);

            menuStrip.BackColor = System.Drawing.Color.FromArgb(40, 30, 20);
            menuStrip.ForeColor = System.Drawing.Color.FromArgb(220, 200, 150);
            foreach (System.Windows.Forms.ToolStripItem item in menuStrip.Items)
            {
                item.ForeColor = System.Drawing.Color.FromArgb(220, 200, 150);
                item.BackColor = System.Drawing.Color.FromArgb(40, 30, 20);
                System.Windows.Forms.ToolStripMenuItem menuItem = item as System.Windows.Forms.ToolStripMenuItem;
                if (menuItem != null)
                {
                    foreach (System.Windows.Forms.ToolStripItem sub in menuItem.DropDownItems)
                    {
                        sub.ForeColor = System.Drawing.Color.FromArgb(200, 180, 130);
                        sub.BackColor = System.Drawing.Color.FromArgb(50, 40, 30);
                    }
                }
            }

            StyleButton(btnNewGame);
            StyleButton(btnUndo);
            StyleButton(btnGiveUp);
            StyleButton(btnRules);

            lblStatus.ForeColor = System.Drawing.Color.FromArgb(220, 200, 150);
            lblStatus.BackColor = System.Drawing.Color.Transparent;
            lblStatus.Font = new System.Drawing.Font("微軟正黑體", 11, System.Drawing.FontStyle.Bold);

            Label lblHistory = this.Controls.Cast<System.Windows.Forms.Control>()
                .OfType<Label>()
                .FirstOrDefault(l => l.Text == "行動記錄：");
            if (lblHistory != null)
            {
                lblHistory.ForeColor = System.Drawing.Color.FromArgb(180, 160, 120);
                lblHistory.BackColor = System.Drawing.Color.Transparent;
            }

            lbMoveHistory.BackColor = System.Drawing.Color.FromArgb(35, 28, 18);
            lbMoveHistory.ForeColor = System.Drawing.Color.FromArgb(200, 180, 130);
            lbMoveHistory.BorderStyle = BorderStyle.FixedSingle;
        }

        private void StyleButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = System.Drawing.Color.FromArgb(80, 60, 40);
            btn.ForeColor = System.Drawing.Color.White;
            btn.Font = new System.Drawing.Font("微軟正黑體", 10, System.Drawing.FontStyle.Bold);
            btn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(120, 100, 60);
        }

        private void SetupGame()
        {
            cardEffectProcessor = new CardEffectProcessor(boardPanel.GameLogic);
            UpdateCardPanels();
            UpdateStatus();
        }

        private void UpdateLayout()
        {
            if (boardPanel == null || menuStrip == null) return;

            const int margin = 10;
            const int rightPanelWidth = 170;
            const int cardPanelH = 110;
            const int topOffset = 55;
            const int bottomMargin = 15;

            int availW = this.ClientSize.Width - rightPanelWidth - margin * 3;
            int availH = this.ClientSize.Height - topOffset - cardPanelH * 2 - margin * 2 - bottomMargin;

            const int boardMargin = 30;
            int cellByW = (availW - boardMargin * 2) / 8;
            int cellByH = (availH - boardMargin * 2) / 9;
            int cell = Math.Max(Math.Min(cellByW, cellByH), 30);

            int boardW = 8 * cell + boardMargin * 2;
            int boardH = 9 * cell + boardMargin * 2;

            // Black card panel (above board)
            blackCardPanel.Location = new System.Drawing.Point(margin, topOffset);
            blackCardPanel.Size = new System.Drawing.Size(boardW, cardPanelH);

            // Board
            int boardTop = topOffset + cardPanelH + margin;
            boardPanel.Location = new System.Drawing.Point(margin, boardTop);
            boardPanel.Size = new System.Drawing.Size(boardW, boardH);
            boardPanel.Invalidate();

            // Red card panel (below board)
            int redPanelTop = boardTop + boardH + margin;
            redCardPanel.Location = new System.Drawing.Point(margin, redPanelTop);
            redCardPanel.Size = new System.Drawing.Size(boardW, cardPanelH);

            // Right panel
            int rightX = boardPanel.Right + margin;
            int rightW = Math.Max(this.ClientSize.Width - rightX - margin, 120);

            lblStatus.Location = new System.Drawing.Point(rightX, topOffset);
            lblStatus.Width = rightW;

            btnNewGame.Location = new System.Drawing.Point(rightX, topOffset + 30);
            btnNewGame.Width = rightW;

            btnUndo.Location = new System.Drawing.Point(rightX, topOffset + 80);
            btnUndo.Width = rightW;

            btnGiveUp.Location = new System.Drawing.Point(rightX, topOffset + 130);
            btnGiveUp.Width = rightW;

            btnRules.Location = new System.Drawing.Point(rightX, topOffset + 180);
            btnRules.Width = rightW;

            Label lblHistory = this.Controls.Cast<System.Windows.Forms.Control>()
                .OfType<Label>()
                .FirstOrDefault(l => l.Text == "行動記錄：");
            if (lblHistory != null)
                lblHistory.Location = new System.Drawing.Point(rightX, topOffset + 235);

            lbMoveHistory.Location = new System.Drawing.Point(rightX, topOffset + 260);
            lbMoveHistory.Size = new System.Drawing.Size(rightW,
                this.ClientSize.Height - (topOffset + 260) - 150 - margin);

            audioPanel.Location = new System.Drawing.Point(rightX,
                this.ClientSize.Height - 150 - margin);
            audioPanel.Size = new System.Drawing.Size(rightW, 145);
        }

        private void UpdateStatus()
        {
            GameLogic logic = boardPanel.GameLogic;
            GameState state = logic.GameState;

            string player = state.CurrentPlayer == PlayerColor.Red ? "紅" : "黑";
            string statusText = $"狀態：{player}方回合";

            if (state.MovesRemainingThisTurn > 1)
                statusText += $" (剩餘移動: {state.MovesRemainingThisTurn})";

            if (state.IsInCheck)
                statusText += " [將軍！]";

            if (selectingFreezeTarget)
                statusText = "請點擊對方棋子施加凍結";

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

            // Update action log
            lbMoveHistory.Items.Clear();
            foreach (string entry in logic.ActionLog)
                lbMoveHistory.Items.Add(entry);
            if (lbMoveHistory.Items.Count > 0)
                lbMoveHistory.SelectedIndex = lbMoveHistory.Items.Count - 1;

            UpdateCardPanels();
        }

        private void UpdateCardPanels()
        {
            GameLogic logic = boardPanel.GameLogic;
            GameState state = logic.GameState;

            redCardPanel.SetHand(logic.CardManager.RedHand);
            blackCardPanel.SetHand(logic.CardManager.BlackHand);

            // Only allow clicking cards for the current player
            bool gameActive = state.Status == GameStatus.Playing && !selectingFreezeTarget;
            bool isAITurn = state.Mode == GameMode.PlayerVsAI && state.CurrentPlayer == PlayerColor.Black;

            redCardPanel.IsInteractive = gameActive && !isAITurn && state.CurrentPlayer == PlayerColor.Red && !state.CardUsedThisTurn;
            blackCardPanel.IsInteractive = gameActive && !isAITurn && state.CurrentPlayer == PlayerColor.Black && !state.CardUsedThisTurn;

            redCardPanel.Invalidate();
            blackCardPanel.Invalidate();
        }

        private void OnMoveMade(Position from, Position to)
        {
            UpdateStatus();

            GameState state = boardPanel.GameLogic.GameState;
            if (state.Mode == GameMode.PlayerVsAI && state.CurrentPlayer == PlayerColor.Black
                && state.Status == GameStatus.Playing)
            {
                aiTimer.Start();
            }
        }

        private void OnBoardClickedForFreeze(Position pos)
        {
            if (!selectingFreezeTarget) return;

            GameLogic logic = boardPanel.GameLogic;
            Piece piece = logic.Board.GetPiece(pos);

            if (piece != null && piece.Color != logic.GameState.CurrentPlayer)
            {
                bool frozen = cardEffectProcessor.ProcessFreeze(pos);
                if (frozen)
                {
                    string curName = logic.GameState.CurrentPlayer == PlayerColor.Red ? "紅" : "黑";
                    string cardStr = pendingFreezeCard != null ? SuitSymbol(pendingFreezeCard.Suit) + "J" : "J牌";
                    logic.AddLog($"  {cardStr} {curName}方凍結 {piece}({pos.X},{pos.Y})，下回合無法移動");
                    pendingFreezeCard = null;
                    selectingFreezeTarget = false;
                    MessageBox.Show($"已凍結 {piece}，下回合無法移動！", "凍結", MessageBoxButtons.OK);
                    UpdateStatus();
                    boardPanel.Invalidate();
                }
            }
        }

        private void OnCardClicked(Card card)
        {
            GameLogic logic = boardPanel.GameLogic;
            GameState state = logic.GameState;

            if (state.Status != GameStatus.Playing) return;
            if (state.CardUsedThisTurn) return;
            if (selectingFreezeTarget) return;

            PlayerColor current = state.CurrentPlayer;

            // Mark card as used and remove from hand
            state.CardUsedThisTurn = true;
            logic.CardManager.UseCard(current, card);

            switch (card.Effect)
            {
                case CardEffect.Duel:
                    HandleDuel(current, card);
                    break;

                case CardEffect.Freeze:
                    selectingFreezeTarget = true;
                    pendingFreezeCard = card;
                    MessageBox.Show("請在棋盤上點擊要凍結的對方棋子", "J 牌 - 凍結", MessageBoxButtons.OK);
                    break;

                case CardEffect.Revive:
                    HandleRevive(current, card);
                    break;

                case CardEffect.SkipOpponent:
                    cardEffectProcessor.ProcessSkipOpponent();
                    PlayerColor opponent = current == PlayerColor.Red ? PlayerColor.Black : PlayerColor.Red;
                    string curStr = current == PlayerColor.Red ? "紅" : "黑";
                    string oppStr = opponent == PlayerColor.Red ? "紅" : "黑";
                    logic.AddLog($"  {SuitSymbol(card.Suit)}K {curStr}方跳過牌，{oppStr}方下回合跳過");
                    MessageBox.Show($"K 牌效果：{oppStr}方下回合跳過！", "K 牌 - 跳過", MessageBoxButtons.OK);
                    break;
            }

            UpdateStatus();
            boardPanel.Invalidate();
        }

        private void HandleDuel(PlayerColor attacker, Card attackerCard)
        {
            GameLogic logic = boardPanel.GameLogic;
            GameState state = logic.GameState;
            PlayerColor defender = attacker == PlayerColor.Red ? PlayerColor.Black : PlayerColor.Red;
            Hand defenderHand = logic.CardManager.GetHand(defender);

            Card defenderCard = null;

            if (state.Mode == GameMode.PlayerVsAI && defender == PlayerColor.Black)
            {
                defenderCard = aiPlayer.GetDuelResponse(logic, attackerCard.GetDuelValue());
                if (defenderCard != null)
                    logic.CardManager.UseCard(defender, defenderCard);
            }
            else
            {
                using (DuelDialog dlg = new DuelDialog(attackerCard, defender, defenderHand))
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        defenderCard = dlg.SelectedDefenderCard;
                        if (defenderCard != null)
                            logic.CardManager.UseCard(defender, defenderCard);
                    }
                }
            }

            DuelResult result = cardEffectProcessor.ProcessDuel(attacker, attackerCard, defenderCard);
            LogDuelResult(logic, attacker, defender, attackerCard, defenderCard, result);

            string attackerName = attacker == PlayerColor.Red ? "紅" : "黑";
            string defenderName = defender == PlayerColor.Red ? "紅" : "黑";
            string msg;
            if (result.Tie)
                msg = $"對決平手！（{result.AttackerValue} vs {result.DefenderValue}）\n雙方各補一張牌。";
            else if (result.Winner == attacker)
                msg = $"{attackerName}方勝！（{result.AttackerValue} vs {result.DefenderValue}）\n{attackerName}方獲得額外移動機會！";
            else
                msg = $"{defenderName}方勝！（{result.DefenderValue} vs {result.AttackerValue}）\n{defenderName}方下回合獲得額外移動機會！";

            MessageBox.Show(msg, "對決結果", MessageBoxButtons.OK);
        }

        private void LogDuelResult(GameLogic logic, PlayerColor attacker, PlayerColor defender, Card atkCard, Card defCard, DuelResult result)
        {
            string atkName = attacker == PlayerColor.Red ? "紅" : "黑";
            string defName = defender == PlayerColor.Red ? "紅" : "黑";
            string atkStr = SuitSymbol(atkCard.Suit) + PointName(atkCard.Point) + "(" + result.AttackerValue + ")";
            string defStr = defCard != null ? SuitSymbol(defCard.Suit) + PointName(defCard.Point) + "(" + result.DefenderValue + ")" : "無牌(0)";
            string outcome;
            if (result.Tie)
                outcome = "平手，各補1牌";
            else if (result.Winner == attacker)
                outcome = atkName + "方勝，+1步";
            else
                outcome = defName + "方勝，下回合+1步";
            logic.AddLog($"  ⚔ 對決: {atkName}{atkStr} vs {defName}{defStr} → {outcome}");
        }

        private void HandleRevive(PlayerColor player, Card card)
        {
            GameLogic logic = boardPanel.GameLogic;
            List<Piece> captured = player == PlayerColor.Red ? logic.CapturedRed : logic.CapturedBlack;

            using (ReviveDialog dlg = new ReviveDialog(captured, player))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedPiece != null)
                {
                    logic.SaveBoardSnapshot();
                    bool ok = cardEffectProcessor.ProcessRevive(dlg.SelectedPiece);
                    if (ok)
                    {
                        string playerName = player == PlayerColor.Red ? "紅" : "黑";
                        logic.AddLog($"  {SuitSymbol(card.Suit)}Q {playerName}方復活 {dlg.SelectedPiece}");
                        MessageBox.Show("已復活 " + dlg.SelectedPiece + "！", "Q 牌 - 復活", MessageBoxButtons.OK);
                    }
                }
            }
        }

        private static string PointName(int point)
        {
            switch (point) { case 1: return "A"; case 11: return "J"; case 12: return "Q"; case 13: return "K"; default: return point.ToString(); }
        }

        private static string SuitSymbol(CardSuit suit)
        {
            switch (suit) { case CardSuit.Clubs: return "♣"; case CardSuit.Diamonds: return "♦"; case CardSuit.Hearts: return "♥"; case CardSuit.Spades: return "♠"; default: return "?"; }
        }

        private void NewGame()
        {
            using (GameModeDialog dlg = new GameModeDialog())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    if (dlg.Choice == GameModeDialog.ModeChoice.PvP)
                        StartPvP();
                    else if (dlg.Choice == GameModeDialog.ModeChoice.AI)
                        StartPlayerVsAI();
                }
            }
        }

        private void StartPvP()
        {
            boardPanel.GameLogic.SetGameMode(GameMode.PvP);
            boardPanel.ResetGame();
            selectingFreezeTarget = false;
            cardEffectProcessor = new CardEffectProcessor(boardPanel.GameLogic);
            aiTimer.Stop();
            UpdateStatus();
        }

        private void StartPlayerVsAI()
        {
            boardPanel.GameLogic.SetGameMode(GameMode.PlayerVsAI);
            boardPanel.ResetGame();
            selectingFreezeTarget = false;
            cardEffectProcessor = new CardEffectProcessor(boardPanel.GameLogic);
            aiTimer.Start();
            UpdateStatus();
        }

        private void ExecuteAITurn()
        {
            GameLogic logic = boardPanel.GameLogic;
            GameState state = logic.GameState;

            if (state.Mode != GameMode.PlayerVsAI ||
                state.CurrentPlayer != PlayerColor.Black ||
                state.Status != GameStatus.Playing)
            {
                aiTimer.Stop();
                return;
            }

            aiTimer.Stop();

            // AI card use
            if (!state.CardUsedThisTurn)
            {
                Card cardToUse = aiPlayer.GetCardToUse(logic);
                if (cardToUse != null)
                {
                    state.CardUsedThisTurn = true;
                    logic.CardManager.UseCard(PlayerColor.Black, cardToUse);

                    switch (cardToUse.Effect)
                    {
                        case CardEffect.SkipOpponent:
                            cardEffectProcessor.ProcessSkipOpponent();
                            logic.AddLog($"  {SuitSymbol(cardToUse.Suit)}K 黑方跳過牌，紅方下回合跳過");
                            MessageBox.Show("黑方使用 K 牌：紅方下回合跳過！", "AI 出牌", MessageBoxButtons.OK);
                            break;

                        case CardEffect.Freeze:
                            Position freezePos = aiPlayer.GetFreezeTarget(logic);
                            if (freezePos.IsValid())
                            {
                                cardEffectProcessor.ProcessFreeze(freezePos);
                                Piece frozen = logic.Board.GetPiece(freezePos);
                                logic.AddLog($"  {SuitSymbol(cardToUse.Suit)}J 黑方凍結 {frozen}({freezePos.X},{freezePos.Y})");
                                MessageBox.Show($"黑方使用 J 牌：凍結 {frozen}！", "AI 出牌", MessageBoxButtons.OK);
                            }
                            break;

                        case CardEffect.Revive:
                            if (logic.CapturedBlack.Count > 0)
                            {
                                Piece toRevive = logic.CapturedBlack[0];
                                cardEffectProcessor.ProcessRevive(toRevive);
                                logic.AddLog($"  {SuitSymbol(cardToUse.Suit)}Q 黑方復活 {toRevive}");
                                MessageBox.Show("黑方使用 Q 牌：復活棋子！", "AI 出牌", MessageBoxButtons.OK);
                            }
                            break;

                        case CardEffect.Duel:
                            Hand playerHand = logic.CardManager.GetHand(PlayerColor.Red);
                            Card playerCard = null;
                            using (DuelDialog dlg = new DuelDialog(cardToUse, PlayerColor.Red, playerHand))
                            {
                                if (dlg.ShowDialog(this) == DialogResult.OK)
                                {
                                    playerCard = dlg.SelectedDefenderCard;
                                    if (playerCard != null)
                                        logic.CardManager.UseCard(PlayerColor.Red, playerCard);
                                }
                            }
                            DuelResult result = cardEffectProcessor.ProcessDuel(PlayerColor.Black, cardToUse, playerCard);
                            LogDuelResult(logic, PlayerColor.Black, PlayerColor.Red, cardToUse, playerCard, result);
                            string msg = result.Tie
                                ? "對決平手！雙方各補一張牌。"
                                : result.Winner == PlayerColor.Black
                                    ? $"黑方勝！（{result.AttackerValue} vs {result.DefenderValue}）黑方獲得額外移動！"
                                    : $"紅方勝！（{result.DefenderValue} vs {result.AttackerValue}）紅方下回合獲得額外移動！";
                            MessageBox.Show(msg, "對決結果", MessageBoxButtons.OK);
                            break;
                    }

                    UpdateStatus();
                    boardPanel.Invalidate();
                }
            }

            // AI chess move
            Move aiMove = aiPlayer.GetNextMove(logic);
            if (aiMove != null)
            {
                logic.MovePiece(aiMove.From, aiMove.To);
                boardPanel.GameState.SelectedPosition = new Position(-1, -1);
                UpdateStatus();
                boardPanel.Invalidate();
            }

            // If AI has bonus moves left, schedule another turn
            if (state.CurrentPlayer == PlayerColor.Black && state.Status == GameStatus.Playing && state.MovesRemainingThisTurn > 0)
            {
                aiTimer.Start();
            }
            else if (state.CurrentPlayer == PlayerColor.Red && state.Status == GameStatus.Playing)
            {
                // Player's turn: if player has bonus move from defender win, just update UI
                UpdateStatus();
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
                    MessageBox.Show("遊戲已保存：" + dialog.FileName);
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
                    boardPanel.LoadGameLogic(loaded);
                    selectingFreezeTarget = false;
                    cardEffectProcessor = new CardEffectProcessor(boardPanel.GameLogic);
                    aiTimer.Stop();
                    if (loaded.GameState.Mode == GameMode.PlayerVsAI)
                        aiTimer.Start();
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
                selectingFreezeTarget = false;
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (MessageBox.Show("確定要退出遊戲嗎？", "退出確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            audioManager?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
