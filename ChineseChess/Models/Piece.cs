namespace ChineseChess.Models
{
    public class Piece
    {
        public PieceType Type { get; set; }
        public PlayerColor Color { get; set; }
        public Position Position { get; set; }
        public bool IsAlive { get; set; }
        public bool IsFrozen { get; set; }

        public Piece(PieceType type, PlayerColor color, Position position)
        {
            Type = type;
            Color = color;
            Position = position;
            IsAlive = true;
            IsFrozen = false;
        }

        public override string ToString()
        {
            string colorStr = Color == PlayerColor.Red ? "紅" : "黑";
            string typeStr;
            switch (Type)
            {
                case PieceType.General: typeStr = "將"; break;
                case PieceType.Advisor: typeStr = "士"; break;
                case PieceType.Elephant: typeStr = "象"; break;
                case PieceType.Horse: typeStr = "馬"; break;
                case PieceType.Chariot: typeStr = "車"; break;
                case PieceType.Cannon: typeStr = "砲"; break;
                case PieceType.Soldier: typeStr = "兵"; break;
                default: typeStr = "?"; break;
            }
            return $"{colorStr}{typeStr}";
        }

        public char GetCharCode()
        {
            switch (Type)
            {
                case PieceType.General:
                    return Color == PlayerColor.Red ? '帥' : '將';
                case PieceType.Advisor:
                    return '士';
                case PieceType.Elephant:
                    return Color == PlayerColor.Red ? '相' : '象';
                case PieceType.Horse:
                    return '馬';
                case PieceType.Chariot:
                    return '車';
                case PieceType.Cannon:
                    return '砲';
                case PieceType.Soldier:
                    return Color == PlayerColor.Red ? '卒' : '兵';
                default:
                    return '?';
            }
        }
    }
}
