namespace ChineseChess.Models
{
    public class Piece
    {
        public PieceType Type { get; set; }
        public PlayerColor Color { get; set; }
        public Position Position { get; set; }
        public bool IsAlive { get; set; }

        public Piece(PieceType type, PlayerColor color, Position position)
        {
            Type = type;
            Color = color;
            Position = position;
            IsAlive = true;
        }

        public override string ToString()
        {
            string colorStr = Color == PlayerColor.Red ? "紅" : "黑";
            string typeStr = Type switch
            {
                PieceType.General => "將",
                PieceType.Advisor => "士",
                PieceType.Elephant => "象",
                PieceType.Horse => "馬",
                PieceType.Chariot => "車",
                PieceType.Cannon => "砲",
                PieceType.Soldier => "兵",
                _ => "?"
            };
            return $"{colorStr}{typeStr}";
        }

        public char GetCharCode()
        {
            return Type switch
            {
                PieceType.General => Color == PlayerColor.Red ? '帥' : '將',
                PieceType.Advisor => '士',
                PieceType.Elephant => Color == PlayerColor.Red ? '相' : '象',
                PieceType.Horse => '馬',
                PieceType.Chariot => '車',
                PieceType.Cannon => '砲',
                PieceType.Soldier => Color == PlayerColor.Red ? '卒' : '兵',
                _ => '?'
            };
        }
    }
}
