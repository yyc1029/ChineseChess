using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ChineseChess.Models;

namespace ChineseChess.Game
{
    public class GameSerializer
    {
        public static bool SaveGame(GameLogic gameLogic, string filePath)
        {
            try
            {
                StringBuilder json = new StringBuilder();
                json.AppendLine("{");
                json.AppendLine("  \"gameMode\": \"" + gameLogic.GameState.Mode + "\",");
                json.AppendLine("  \"gameStatus\": \"" + gameLogic.GameState.Status + "\",");
                json.AppendLine("  \"currentPlayer\": \"" + gameLogic.GameState.CurrentPlayer + "\",");
                json.AppendLine("  \"isInCheck\": " + (gameLogic.GameState.IsInCheck ? "true" : "false") + ",");
                json.AppendLine("  \"pieces\": [");

                List<Piece> pieces = gameLogic.Board.GetAllPieces().ToList();
                for (int i = 0; i < pieces.Count; i++)
                {
                    Piece piece = pieces[i];
                    json.Append("    {");
                    json.Append("\"type\": \"" + piece.Type + "\", ");
                    json.Append("\"color\": \"" + piece.Color + "\", ");
                    json.Append("\"x\": " + piece.Position.X + ", ");
                    json.Append("\"y\": " + piece.Position.Y);
                    json.Append("}");

                    if (i < pieces.Count - 1)
                        json.AppendLine(",");
                    else
                        json.AppendLine();
                }

                json.AppendLine("  ],");
                json.AppendLine("  \"moveHistory\": [");

                Move[] moves = gameLogic.MoveHistory.ToArray();
                for (int i = 0; i < moves.Length; i++)
                {
                    Move move = moves[i];
                    json.Append("    {");
                    json.Append("\"fromX\": " + move.From.X + ", ");
                    json.Append("\"fromY\": " + move.From.Y + ", ");
                    json.Append("\"toX\": " + move.To.X + ", ");
                    json.Append("\"toY\": " + move.To.Y);
                    json.Append("}");

                    if (i < moves.Length - 1)
                        json.AppendLine(",");
                    else
                        json.AppendLine();
                }

                json.AppendLine("  ]");
                json.AppendLine("}");

                File.WriteAllText(filePath, json.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("保存遊戲失敗：" + ex.Message);
                return false;
            }
        }

        public static GameLogic LoadGame(string filePath)
        {
            try
            {
                GameLogic gameLogic = new GameLogic();
                string json = File.ReadAllText(filePath, Encoding.UTF8);

                // 簡單的JSON解析（因為.NET Framework 4.7.2不包含System.Text.Json）
                ParseGameData(gameLogic, json);

                return gameLogic;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("讀取遊戲失敗：" + ex.Message);
                return null;
            }
        }

        private static void ParseGameData(GameLogic gameLogic, string json)
        {
            // 提取gameMode
            string modeStr = ExtractValue(json, "\"gameMode\": \"", "\"");
            if (Enum.TryParse(modeStr, out GameMode mode))
            {
                gameLogic.GameState.Mode = mode;
            }

            // 提取gameStatus
            string statusStr = ExtractValue(json, "\"gameStatus\": \"", "\"");
            if (Enum.TryParse(statusStr, out GameStatus status))
            {
                gameLogic.GameState.Status = status;
            }

            // 提取currentPlayer
            string playerStr = ExtractValue(json, "\"currentPlayer\": \"", "\"");
            if (Enum.TryParse(playerStr, out PlayerColor player))
            {
                gameLogic.GameState.CurrentPlayer = player;
            }

            // 提取isInCheck
            string checkStr = ExtractValue(json, "\"isInCheck\": ", ",");
            gameLogic.GameState.IsInCheck = checkStr.ToLower() == "true";

            // 清空棋盤並重新加載棋子
            gameLogic.Board.Reset();
            int piecesStart = json.IndexOf("\"pieces\": [");
            int piecesEnd = json.IndexOf("]", piecesStart);
            string piecesJson = json.Substring(piecesStart, piecesEnd - piecesStart + 1);

            int pos = 0;
            while (true)
            {
                int typeStart = piecesJson.IndexOf("\"type\": \"", pos);
                if (typeStart == -1) break;

                string typeStr = ExtractValue(piecesJson, "\"type\": \"", "\"", typeStart);
                string colorStr = ExtractValue(piecesJson, "\"color\": \"", "\"", typeStart);
                string xStr = ExtractValue(piecesJson, "\"x\": ", ",", typeStart);
                string yStr = ExtractValue(piecesJson, "\"y\": ", "}", typeStart);

                if (Enum.TryParse(typeStr, out PieceType type) &&
                    Enum.TryParse(colorStr, out PlayerColor color) &&
                    int.TryParse(xStr, out int x) &&
                    int.TryParse(yStr, out int y))
                {
                    Piece piece = new Piece(type, color, new Position(x, y));
                    gameLogic.Board.AddPiece(piece);
                }

                pos = typeStart + 1;
            }
        }

        private static string ExtractValue(string json, string startMarker, string endMarker, int searchStart = 0)
        {
            int start = json.IndexOf(startMarker, searchStart);
            if (start == -1) return "";

            start += startMarker.Length;
            int end = json.IndexOf(endMarker, start);
            if (end == -1) return "";

            return json.Substring(start, end - start).Trim();
        }
    }
}
