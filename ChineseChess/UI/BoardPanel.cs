using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ChineseChess.Game;
using ChineseChess.Models;

namespace ChineseChess.UI
{
    public class BoardPanel : Control
    {
        private GameLogic gameLogic;
        private List<Position> possibleMoves = new List<Position>();
        private SoundManager soundManager;

        // 棋盤繪製參數（每次 OnPaint 重新計算）
        private int cellSize = 50;
        private int startX = 0;
        private int startY = 0;

        public event Action<Position> OnPieceSelected;
        public event Action<Position, Position> OnMoveMade;
        public event Action<Position> OnBoardClicked;

        public BoardPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            BackColor = Color.FromArgb(220, 160, 80);
            gameLogic = new GameLogic();
            soundManager = new SoundManager();
        }

        public GameLogic GameLogic => gameLogic;
        public GameState GameState => gameLogic.GameState;

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            // 棋盤有 9 條垂直線（col 0-8）和 10 條水平線（row 0-9）
            // 兩線間距為 cellSize，棋子在交叉點上
            // 所以棋格寬度 = 8 * cellSize，高度 = 9 * cellSize
            const int margin = 30;
            cellSize = Math.Min((Width - margin * 2) / 8, (Height - margin * 2) / 9);
            if (cellSize < 10) cellSize = 10;

            // 讓棋盤在控件中居中
            startX = (Width - 8 * cellSize) / 2;
            startY = (Height - 9 * cellSize) / 2;

            DrawBoard(g);
            DrawRiver(g);
            DrawHighlights(g);
            DrawPieces(g);
            DrawPossibleMoves(g);
        }

        private void DrawBoard(Graphics g)
        {
            Pen pen = new Pen(Color.FromArgb(80, 50, 20), 2);

            // 水平線（10 條，row 0-9）
            for (int row = 0; row <= 9; row++)
            {
                int y = startY + row * cellSize;
                g.DrawLine(pen, startX, y, startX + 8 * cellSize, y);
            }

            // 垂直線（9 條，col 0-8），但河界中間不畫
            for (int col = 0; col <= 8; col++)
            {
                int x = startX + col * cellSize;
                if (col == 0 || col == 8)
                {
                    // 最左和最右的線貫穿整個棋盤
                    g.DrawLine(pen, x, startY, x, startY + 9 * cellSize);
                }
                else
                {
                    // 中間的線在河界處斷開：上半（row 0-4）和下半（row 5-9）分開
                    g.DrawLine(pen, x, startY, x, startY + 4 * cellSize);
                    g.DrawLine(pen, x, startY + 5 * cellSize, x, startY + 9 * cellSize);
                }
            }

            // 繪製宮格斜線（紅方宮 col 3-5, row 0-2）
            g.DrawLine(pen, startX + 3 * cellSize, startY, startX + 5 * cellSize, startY + 2 * cellSize);
            g.DrawLine(pen, startX + 5 * cellSize, startY, startX + 3 * cellSize, startY + 2 * cellSize);

            // 繪製宮格斜線（黑方宮 col 3-5, row 7-9）
            g.DrawLine(pen, startX + 3 * cellSize, startY + 7 * cellSize, startX + 5 * cellSize, startY + 9 * cellSize);
            g.DrawLine(pen, startX + 5 * cellSize, startY + 7 * cellSize, startX + 3 * cellSize, startY + 9 * cellSize);
        }

        private void DrawRiver(Graphics g)
        {
            // 河界在 row 4 和 row 5 之間
            int riverY = startY + 4 * cellSize + cellSize / 2;

            int fontSize = Math.Max(10, cellSize / 3);
            Font font = new Font("微軟正黑體", fontSize, FontStyle.Bold);
            Brush textBrush = new SolidBrush(Color.FromArgb(80, 50, 20));
            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            float leftCenterX = startX + 2 * cellSize;
            float rightCenterX = startX + 6 * cellSize;

            g.DrawString("楚河", font, textBrush, new RectangleF(leftCenterX - cellSize, riverY - fontSize, cellSize * 2, fontSize * 2), sf);
            g.DrawString("漢界", font, textBrush, new RectangleF(rightCenterX - cellSize, riverY - fontSize, cellSize * 2, fontSize * 2), sf);
        }

        private void DrawPieces(Graphics g)
        {
            int fontSize = Math.Max(8, cellSize * 2 / 5);
            Font font = new Font("微軟正黑體", fontSize, FontStyle.Bold);
            StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            foreach (Piece piece in gameLogic.Board.GetAllPieces().ToList())
            {
                float cx = startX + piece.Position.X * cellSize;
                float cy = startY + (9 - piece.Position.Y) * cellSize;
                float radius = cellSize * 0.42f;

                bool isRed = piece.Color == PlayerColor.Red;
                Color fillColor = isRed ? Color.FromArgb(200, 50, 50) : Color.FromArgb(40, 40, 40);
                Color borderColor = isRed ? Color.FromArgb(255, 220, 100) : Color.FromArgb(180, 180, 180);
                Color textColor = isRed ? Color.FromArgb(255, 240, 150) : Color.White;

                RectangleF circle = new RectangleF(cx - radius, cy - radius, radius * 2, radius * 2);

                // 背景圓（陰影效果）
                RectangleF shadow = new RectangleF(cx - radius + 2, cy - radius + 2, radius * 2, radius * 2);
                g.FillEllipse(new SolidBrush(Color.FromArgb(80, 0, 0, 0)), shadow);

                // 棋子主體
                g.FillEllipse(new SolidBrush(fillColor), circle);

                // 外圈邊框
                g.DrawEllipse(new Pen(borderColor, 2), circle);

                // 內圈（裝飾）
                float innerRadius = radius * 0.82f;
                RectangleF inner = new RectangleF(cx - innerRadius, cy - innerRadius, innerRadius * 2, innerRadius * 2);
                g.DrawEllipse(new Pen(borderColor, 1), inner);

                // 棋子文字
                g.DrawString(piece.GetCharCode().ToString(), font, new SolidBrush(textColor), new RectangleF(cx - radius, cy - radius, radius * 2, radius * 2), format);
            }
        }

        private void DrawHighlights(Graphics g)
        {
            Position selected = gameLogic.GameState.SelectedPosition;
            if (selected.X >= 0 && selected.Y >= 0)
            {
                float cx = startX + selected.X * cellSize;
                float cy = startY + (9 - selected.Y) * cellSize;
                float r = cellSize * 0.46f;
                g.DrawEllipse(new Pen(Color.Yellow, 3), cx - r, cy - r, r * 2, r * 2);
            }

            if (gameLogic.GameState.IsInCheck)
            {
                Piece general = gameLogic.Board.FindGeneral(gameLogic.GameState.CurrentPlayer);
                if (general != null)
                {
                    float cx = startX + general.Position.X * cellSize;
                    float cy = startY + (9 - general.Position.Y) * cellSize;
                    float r = cellSize * 0.46f;
                    g.DrawEllipse(new Pen(Color.OrangeRed, 4), cx - r, cy - r, r * 2, r * 2);
                }
            }
        }

        private void DrawPossibleMoves(Graphics g)
        {
            foreach (Position pos in possibleMoves)
            {
                float cx = startX + pos.X * cellSize;
                float cy = startY + (9 - pos.Y) * cellSize;

                Piece target = gameLogic.Board.GetPiece(pos);
                if (target != null)
                {
                    // 可吃的棋子：紅色角標
                    float r = cellSize * 0.46f;
                    g.DrawEllipse(new Pen(Color.FromArgb(200, 255, 80, 80), 3), cx - r, cy - r, r * 2, r * 2);
                }
                else
                {
                    // 可移動的空格：綠色小點
                    float r = cellSize * 0.15f;
                    g.FillEllipse(new SolidBrush(Color.FromArgb(180, 80, 200, 80)), cx - r, cy - r, r * 2, r * 2);
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (cellSize <= 0) return;

            // 將屏幕坐標轉換為棋盤坐標（視覺翻轉：紅在下，視覺row 0=內部Y 9）
            int col = (int)Math.Round((float)(e.X - startX) / cellSize);
            int visualRow = (int)Math.Round((float)(e.Y - startY) / cellSize);
            Position clickPos = new Position(col, 9 - visualRow);

            if (!clickPos.IsValid()) return;

            // Fire raw click event (used for freeze target selection)
            OnBoardClicked?.Invoke(clickPos);

            Position selected = gameLogic.GameState.SelectedPosition;

            if (selected.X < 0 || selected.Y < 0)
            {
                // 第一次點擊：選中棋子
                Piece piece = gameLogic.Board.GetPiece(clickPos);
                if (piece != null && piece.Color == gameLogic.GameState.CurrentPlayer)
                {
                    gameLogic.GameState.SelectedPosition = clickPos;
                    possibleMoves = gameLogic.GetPossibleMoves(clickPos);
                    OnPieceSelected?.Invoke(clickPos);
                }
            }
            else if (clickPos.Equals(selected))
            {
                // 點擊同一個棋子：取消選擇
                gameLogic.GameState.SelectedPosition = new Position(-1, -1);
                possibleMoves.Clear();
            }
            else if (possibleMoves.Contains(clickPos))
            {
                // 第二次點擊合法目標：執行移動
                Piece targetPiece = gameLogic.Board.GetPiece(clickPos);
                if (gameLogic.MovePiece(selected, clickPos))
                {
                    if (targetPiece != null)
                        soundManager.PlayCaptureSound();
                    else
                        soundManager.PlayMoveSound();

                    if (gameLogic.GameState.IsInCheck)
                        soundManager.PlayCheckSound();

                    OnMoveMade?.Invoke(selected, clickPos);
                }
                gameLogic.GameState.SelectedPosition = new Position(-1, -1);
                possibleMoves.Clear();
            }
            else
            {
                // 點擊其他己方棋子：切換選擇
                Piece piece = gameLogic.Board.GetPiece(clickPos);
                if (piece != null && piece.Color == gameLogic.GameState.CurrentPlayer)
                {
                    gameLogic.GameState.SelectedPosition = clickPos;
                    possibleMoves = gameLogic.GetPossibleMoves(clickPos);
                    OnPieceSelected?.Invoke(clickPos);
                }
                else
                {
                    gameLogic.GameState.SelectedPosition = new Position(-1, -1);
                    possibleMoves.Clear();
                }
            }

            Invalidate();
        }

        public void ResetGame()
        {
            gameLogic.Reset();
            gameLogic.GameState.SelectedPosition = new Position(-1, -1);
            possibleMoves.Clear();
            Invalidate();
        }

        public void LoadGameLogic(GameLogic newGameLogic)
        {
            gameLogic = newGameLogic;
            gameLogic.GameState.SelectedPosition = new Position(-1, -1);
            possibleMoves.Clear();
            Invalidate();
        }
    }
}
