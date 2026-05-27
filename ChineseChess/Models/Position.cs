using System;

namespace ChineseChess.Models
{
    public struct Position
    {
        public int X { get; set; } // 0-8 (left to right)
        public int Y { get; set; } // 0-9 (top to bottom)

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool IsValid()
        {
            return X >= 0 && X <= 8 && Y >= 0 && Y <= 9;
        }

        public override bool Equals(object obj)
        {
            if (obj is Position pos)
            {
                return X == pos.X && Y == pos.Y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return X * 10 + Y;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public static bool operator ==(Position a, Position b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Position a, Position b)
        {
            return !a.Equals(b);
        }
    }
}
