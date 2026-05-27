using System;
using System.Collections.Generic;
using System.Linq;
using ChineseChess.Models;

namespace ChineseChess.Game
{
    public class Board
    {
        private Dictionary<Position, Piece> pieces;

        public Board()
        {
            pieces = new Dictionary<Position, Piece>();
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            pieces.Clear();

            // 紅方棋子 (上方，Y = 0-4)
            // 紅將在 (4, 0)
            AddPiece(new Piece(PieceType.General, PlayerColor.Red, new Position(4, 0)));

            // 紅士在 (3, 0) 和 (5, 0)
            AddPiece(new Piece(PieceType.Advisor, PlayerColor.Red, new Position(3, 0)));
            AddPiece(new Piece(PieceType.Advisor, PlayerColor.Red, new Position(5, 0)));

            // 紅象在 (2, 0) 和 (6, 0)
            AddPiece(new Piece(PieceType.Elephant, PlayerColor.Red, new Position(2, 0)));
            AddPiece(new Piece(PieceType.Elephant, PlayerColor.Red, new Position(6, 0)));

            // 紅馬在 (1, 0) 和 (7, 0)
            AddPiece(new Piece(PieceType.Horse, PlayerColor.Red, new Position(1, 0)));
            AddPiece(new Piece(PieceType.Horse, PlayerColor.Red, new Position(7, 0)));

            // 紅車在 (0, 0) 和 (8, 0)
            AddPiece(new Piece(PieceType.Chariot, PlayerColor.Red, new Position(0, 0)));
            AddPiece(new Piece(PieceType.Chariot, PlayerColor.Red, new Position(8, 0)));

            // 紅砲在 (1, 2) 和 (7, 2)
            AddPiece(new Piece(PieceType.Cannon, PlayerColor.Red, new Position(1, 2)));
            AddPiece(new Piece(PieceType.Cannon, PlayerColor.Red, new Position(7, 2)));

            // 紅兵在 (0, 3), (2, 3), (4, 3), (6, 3), (8, 3)
            for (int x = 0; x <= 8; x += 2)
            {
                AddPiece(new Piece(PieceType.Soldier, PlayerColor.Red, new Position(x, 3)));
            }

            // 黑方棋子 (下方，Y = 5-9)
            // 黑將在 (4, 9)
            AddPiece(new Piece(PieceType.General, PlayerColor.Black, new Position(4, 9)));

            // 黑士在 (3, 9) 和 (5, 9)
            AddPiece(new Piece(PieceType.Advisor, PlayerColor.Black, new Position(3, 9)));
            AddPiece(new Piece(PieceType.Advisor, PlayerColor.Black, new Position(5, 9)));

            // 黑象在 (2, 9) 和 (6, 9)
            AddPiece(new Piece(PieceType.Elephant, PlayerColor.Black, new Position(2, 9)));
            AddPiece(new Piece(PieceType.Elephant, PlayerColor.Black, new Position(6, 9)));

            // 黑馬在 (1, 9) 和 (7, 9)
            AddPiece(new Piece(PieceType.Horse, PlayerColor.Black, new Position(1, 9)));
            AddPiece(new Piece(PieceType.Horse, PlayerColor.Black, new Position(7, 9)));

            // 黑車在 (0, 9) 和 (8, 9)
            AddPiece(new Piece(PieceType.Chariot, PlayerColor.Black, new Position(0, 9)));
            AddPiece(new Piece(PieceType.Chariot, PlayerColor.Black, new Position(8, 9)));

            // 黑砲在 (1, 7) 和 (7, 7)
            AddPiece(new Piece(PieceType.Cannon, PlayerColor.Black, new Position(1, 7)));
            AddPiece(new Piece(PieceType.Cannon, PlayerColor.Black, new Position(7, 7)));

            // 黑兵在 (0, 6), (2, 6), (4, 6), (6, 6), (8, 6)
            for (int x = 0; x <= 8; x += 2)
            {
                AddPiece(new Piece(PieceType.Soldier, PlayerColor.Black, new Position(x, 6)));
            }
        }

        public Piece GetPiece(Position pos)
        {
            if (pieces.TryGetValue(pos, out var piece))
            {
                return piece;
            }
            return null;
        }

        public void SetPiece(Position pos, Piece piece)
        {
            if (piece == null)
            {
                pieces.Remove(pos);
            }
            else
            {
                pieces[pos] = piece;
            }
        }

        public void AddPiece(Piece piece)
        {
            if (piece != null && piece.Position.IsValid())
            {
                pieces[piece.Position] = piece;
            }
        }

        public void RemovePiece(Position pos)
        {
            pieces.Remove(pos);
        }

        public Piece MovePiece(Position from, Position to)
        {
            Piece piece = GetPiece(from);
            if (piece == null)
            {
                return null;
            }

            Piece captured = GetPiece(to);
            if (captured != null)
            {
                captured.IsAlive = false;
                RemovePiece(to);
            }

            RemovePiece(from);
            piece.Position = to;
            SetPiece(to, piece);

            return captured;
        }

        public IEnumerable<Piece> GetAllPieces()
        {
            return pieces.Values.Where(p => p.IsAlive);
        }

        public IEnumerable<Piece> GetPiecesByColor(PlayerColor color)
        {
            return GetAllPieces().Where(p => p.Color == color);
        }

        public Piece FindGeneral(PlayerColor color)
        {
            return GetAllPieces().FirstOrDefault(p => p.Type == PieceType.General && p.Color == color);
        }

        public void ClearPieces()
        {
            pieces.Clear();
        }

        public void Reset()
        {
            InitializeBoard();
        }

        public string PrintBoard()
        {
            string result = "";
            for (int y = 0; y <= 9; y++)
            {
                for (int x = 0; x <= 8; x++)
                {
                    Piece piece = GetPiece(new Position(x, y));
                    if (piece != null)
                    {
                        result += piece.GetCharCode();
                    }
                    else if (y == 4)
                    {
                        result += "─";
                    }
                    else
                    {
                        result += "·";
                    }
                    result += " ";
                }
                result += "\n";
            }
            return result;
        }
    }
}
