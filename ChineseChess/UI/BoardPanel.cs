using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ChineseChess.Game;
using ChineseChess.Models;

namespace ChineseChess.UI
{
    public class BoardPanel : Control
    {
        private GameLogic gameLogic;
        private int cellSize = 50;
        private List<Position> possibleMoves = new List<Position>();

        public event Action<Position> OnPieceSelected;
        public event Action<Position, Position> OnMoveMade;

        public BoardPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            BackColor = Color.SandyBrown;
            gameLogic = new GameLogic();
        }

        public GameLogic GameLogic => gameLogic;
        public GameState GameState => gameLogic.GameState;

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);

            DrawBoard(g);
            DrawRiver(g);
            DrawPieces(g);
            DrawHighlights(g);
            DrawPossibleMoves(g);
        }

        private void DrawBoard(Graphics g)
        {
            Pen pen = new Pen(Color.Black, 2);

            // 繪製棋盤網格
            for (int i = 0; i <= 8; i++)
            {
                // 垂直線
                g.DrawLine(pen, i * cellSize, 0, i * cellSize, 9 * cellSize);
            }

            for (int i = 0; i <= 9; i++)
            {
                // 水平線
                g.DrawLine(pen, 0, i * cellSize, 8 * cellSize, i * cellSize);
            }

            // 繪製棋盤邊框
            g.DrawRectangle(pen, 0, 0, 8 * cellSize, 9 * cellSize);
        }

        private void DrawRiver(Graphics g)
        {
            // 河界在 Y=4 和 Y=5 之間
            Pen riverPen = new Pen(Color.Blue, 2);
            g.DrawLine(riverPen, 0, 4.5f * cellSize, 8 * cellSize, 4.5f * cellSize);

            Font font = new Font("Arial", 10);
            Brush textBrush = new SolidBrush(Color.DarkBlue);
            g.DrawString("河", font, textBrush, 3.5f * cellSize, 4.2f * cellSize);
        }

        private void DrawPieces(Graphics g)
        {
            Brush redBrush = new SolidBrush(Color.Red);
            Brush blackBrush = new SolidBrush(Color.Black);
            Font font = new Font("Arial", 12, FontStyle.Bold);
            StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            foreach (Piece piece in gameLogic.Board.GetAllPieces())
            {
                float x = piece.Position.X * cellSize + cellSize / 2;
                float y = piece.Position.Y * cellSize + cellSize / 2;
                Brush brush = piece.Color == PlayerColor.Red ? redBrush : blackBrush;

                // 繪製棋子圓形
                RectangleF circle = new RectangleF(x - 20, y - 20, 40, 40);
                g.FillEllipse(brush, circle);
                g.DrawEllipse(new Pen(Color.Black, 1), circle);

                // 繪製棋子文字
                g.DrawString(piece.GetCharCode().ToString(), font, new SolidBrush(Color.White), x, y, format);
            }
        }

        private void DrawHighlights(Graphics g)
        {
            Position selected = gameLogic.GameState.SelectedPosition;
            if (selected.X >= 0 && selected.Y >= 0)
            {
                float x = selected.X * cellSize;
                float y = selected.Y * cellSize;
                Pen highlightPen = new Pen(Color.Yellow, 3);
                g.DrawRectangle(highlightPen, x, y, cellSize, cellSize);
            }

            // 繪製將軍警告
            if (gameLogic.GameState.IsInCheck)
            {
                Piece general = gameLogic.Board.FindGeneral(gameLogic.GameState.CurrentPlayer);
                if (general != null)
                {
                    float x = general.Position.X * cellSize;
                    float y = general.Position.Y * cellSize;
                    Pen checkPen = new Pen(Color.Red, 4);
                    g.DrawRectangle(checkPen, x, y, cellSize, cellSize);
                }
            }
        }

        private void DrawPossibleMoves(Graphics g)
        {
            Brush moveBrush = new SolidBrush(Color.FromArgb(100, 0, 255, 0));
            foreach (Position pos in possibleMoves)
            {
                float x = pos.X * cellSize + cellSize / 2;
                float y = pos.Y * cellSize + cellSize / 2;
                RectangleF circle = new RectangleF(x - 8, y - 8, 16, 16);
                g.FillEllipse(moveBrush, circle);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            Position clickPos = new Position(e.X / cellSize, e.Y / cellSize);
            if (!clickPos.IsValid())
                return;

            Position selected = gameLogic.GameState.SelectedPosition;

            // 第一次點擊：選中棋子
            if (selected.X < 0 || selected.Y < 0)
            {
                Piece piece = gameLogic.Board.GetPiece(clickPos);
                if (piece != null && piece.Color == gameLogic.GameState.CurrentPlayer)
                {
                    gameLogic.GameState.SelectedPosition = clickPos;
                    possibleMoves = gameLogic.GetPossibleMoves(clickPos);
                    OnPieceSelected?.Invoke(clickPos);
                }
            }
            // 第二次點擊：移動棋子
            else if (clickPos.Equals(selected))
            {
                // 取消選擇
                gameLogic.GameState.SelectedPosition = new Position(-1, -1);
                possibleMoves.Clear();
            }
            else if (possibleMoves.Contains(clickPos))
            {
                // 執行移動
                if (gameLogic.MovePiece(selected, clickPos))
                {
                    OnMoveMade?.Invoke(selected, clickPos);
                }
                gameLogic.GameState.SelectedPosition = new Position(-1, -1);
                possibleMoves.Clear();
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
