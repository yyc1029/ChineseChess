using System;

namespace ChineseChess.Models
{
    public class Move
    {
        public Position From { get; set; }
        public Position To { get; set; }
        public Piece CapturedPiece { get; set; }
        public DateTime Timestamp { get; set; }

        public Move(Position from, Position to, Piece capturedPiece = null)
        {
            From = from;
            To = to;
            CapturedPiece = capturedPiece;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            string desc = $"{From} -> {To}";
            if (CapturedPiece != null)
            {
                desc += $" 吃掉 {CapturedPiece}";
            }
            return desc;
        }
    }
}
