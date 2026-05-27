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
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{");

                GameState gs = gameLogic.GameState;
                sb.AppendLine("  \"gameMode\": \"" + gs.Mode + "\",");
                sb.AppendLine("  \"gameStatus\": \"" + gs.Status + "\",");
                sb.AppendLine("  \"currentPlayer\": \"" + gs.CurrentPlayer + "\",");
                sb.AppendLine("  \"isInCheck\": " + (gs.IsInCheck ? "true" : "false") + ",");
                sb.AppendLine("  \"movesRemainingThisTurn\": " + gs.MovesRemainingThisTurn + ",");
                sb.AppendLine("  \"cardUsedThisTurn\": " + (gs.CardUsedThisTurn ? "true" : "false") + ",");
                sb.AppendLine("  \"skipNextTurn_Red\": " + (gs.SkipNextTurn_Red ? "true" : "false") + ",");
                sb.AppendLine("  \"skipNextTurn_Black\": " + (gs.SkipNextTurn_Black ? "true" : "false") + ",");
                sb.AppendLine("  \"frozenX\": " + gs.FrozenPosition.X + ",");
                sb.AppendLine("  \"frozenY\": " + gs.FrozenPosition.Y + ",");
                sb.AppendLine("  \"defenderBonusMove\": " + (gs.DefenderBonusMove ? "true" : "false") + ",");

                // Active pieces
                sb.AppendLine("  \"pieces\": [");
                List<Piece> pieces = gameLogic.Board.GetAllPieces().ToList();
                for (int i = 0; i < pieces.Count; i++)
                {
                    Piece p = pieces[i];
                    sb.Append("    {\"type\": \"" + p.Type + "\", \"color\": \"" + p.Color + "\", \"x\": " + p.Position.X + ", \"y\": " + p.Position.Y + ", \"frozen\": " + (p.IsFrozen ? "true" : "false") + "}");
                    sb.AppendLine(i < pieces.Count - 1 ? "," : "");
                }
                sb.AppendLine("  ],");

                // Captured pieces
                sb.AppendLine("  \"capturedRed\": [");
                List<Piece> capRed = gameLogic.CapturedRed;
                for (int i = 0; i < capRed.Count; i++)
                {
                    Piece p = capRed[i];
                    sb.Append("    {\"type\": \"" + p.Type + "\", \"color\": \"" + p.Color + "\"}");
                    sb.AppendLine(i < capRed.Count - 1 ? "," : "");
                }
                sb.AppendLine("  ],");

                sb.AppendLine("  \"capturedBlack\": [");
                List<Piece> capBlack = gameLogic.CapturedBlack;
                for (int i = 0; i < capBlack.Count; i++)
                {
                    Piece p = capBlack[i];
                    sb.Append("    {\"type\": \"" + p.Type + "\", \"color\": \"" + p.Color + "\"}");
                    sb.AppendLine(i < capBlack.Count - 1 ? "," : "");
                }
                sb.AppendLine("  ],");

                // Hands
                sb.AppendLine("  \"redHand\": [");
                IReadOnlyList<Card> redCards = gameLogic.CardManager.RedHand.Cards;
                for (int i = 0; i < redCards.Count; i++)
                {
                    Card c = redCards[i];
                    sb.Append("    {\"suit\": " + (int)c.Suit + ", \"point\": " + c.Point + "}");
                    sb.AppendLine(i < redCards.Count - 1 ? "," : "");
                }
                sb.AppendLine("  ],");

                sb.AppendLine("  \"blackHand\": [");
                IReadOnlyList<Card> blackCards = gameLogic.CardManager.BlackHand.Cards;
                for (int i = 0; i < blackCards.Count; i++)
                {
                    Card c = blackCards[i];
                    sb.Append("    {\"suit\": " + (int)c.Suit + ", \"point\": " + c.Point + "}");
                    sb.AppendLine(i < blackCards.Count - 1 ? "," : "");
                }
                sb.AppendLine("  ],");

                // Action log
                sb.AppendLine("  \"actionLog\": [");
                IReadOnlyList<string> log = gameLogic.ActionLog;
                for (int i = 0; i < log.Count; i++)
                {
                    string escaped = log[i].Replace("\\", "\\\\").Replace("\"", "\\\"");
                    sb.Append("    \"" + escaped + "\"");
                    sb.AppendLine(i < log.Count - 1 ? "," : "");
                }
                sb.AppendLine("  ]");

                sb.AppendLine("}");

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
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
            GameState gs = gameLogic.GameState;

            string modeStr = ExtractValue(json, "\"gameMode\": \"", "\"");
            if (Enum.TryParse(modeStr, out GameMode mode)) gs.Mode = mode;

            string statusStr = ExtractValue(json, "\"gameStatus\": \"", "\"");
            if (Enum.TryParse(statusStr, out GameStatus status)) gs.Status = status;

            string playerStr = ExtractValue(json, "\"currentPlayer\": \"", "\"");
            if (Enum.TryParse(playerStr, out PlayerColor player)) gs.CurrentPlayer = player;

            gs.IsInCheck = ExtractValue(json, "\"isInCheck\": ", ",").Trim() == "true";

            if (int.TryParse(ExtractValue(json, "\"movesRemainingThisTurn\": ", ",").Trim(), out int movesRem))
                gs.MovesRemainingThisTurn = movesRem;

            gs.CardUsedThisTurn = ExtractValue(json, "\"cardUsedThisTurn\": ", ",").Trim() == "true";
            gs.SkipNextTurn_Red = ExtractValue(json, "\"skipNextTurn_Red\": ", ",").Trim() == "true";
            gs.SkipNextTurn_Black = ExtractValue(json, "\"skipNextTurn_Black\": ", ",").Trim() == "true";

            if (int.TryParse(ExtractValue(json, "\"frozenX\": ", ",").Trim(), out int frozenX) &&
                int.TryParse(ExtractValue(json, "\"frozenY\": ", ",").Trim(), out int frozenY))
                gs.FrozenPosition = new Position(frozenX, frozenY);

            gs.DefenderBonusMove = ExtractValue(json, "\"defenderBonusMove\": ", ",").Trim() == "true";

            // Active pieces: clear board first to avoid loading over initial setup
            gameLogic.Board.ClearPieces();
            string piecesArr = ExtractArray(json, "pieces");
            foreach (Dictionary<string, string> obj in ParseObjectArray(piecesArr))
            {
                string typeVal, colorVal, xVal, yVal;
                obj.TryGetValue("type", out typeVal);
                obj.TryGetValue("color", out colorVal);
                obj.TryGetValue("x", out xVal);
                obj.TryGetValue("y", out yVal);
                if (Enum.TryParse(typeVal ?? "", out PieceType type) &&
                    Enum.TryParse(colorVal ?? "", out PlayerColor color) &&
                    int.TryParse(xVal ?? "0", out int x) &&
                    int.TryParse(yVal ?? "0", out int y))
                {
                    Piece piece = new Piece(type, color, new Position(x, y));
                    string frozenVal;
                    obj.TryGetValue("frozen", out frozenVal);
                    piece.IsFrozen = frozenVal == "true";
                    gameLogic.Board.AddPiece(piece);
                }
            }

            // Captured pieces
            var capRed = new List<Piece>();
            foreach (Dictionary<string, string> obj in ParseObjectArray(ExtractArray(json, "capturedRed")))
            {
                string typeVal, colorVal;
                obj.TryGetValue("type", out typeVal);
                obj.TryGetValue("color", out colorVal);
                if (Enum.TryParse(typeVal ?? "", out PieceType type) &&
                    Enum.TryParse(colorVal ?? "", out PlayerColor color))
                {
                    Piece p = new Piece(type, color, new Position(-1, -1));
                    p.IsAlive = false;
                    capRed.Add(p);
                }
            }

            var capBlack = new List<Piece>();
            foreach (Dictionary<string, string> obj in ParseObjectArray(ExtractArray(json, "capturedBlack")))
            {
                string typeVal, colorVal;
                obj.TryGetValue("type", out typeVal);
                obj.TryGetValue("color", out colorVal);
                if (Enum.TryParse(typeVal ?? "", out PieceType type) &&
                    Enum.TryParse(colorVal ?? "", out PlayerColor color))
                {
                    Piece p = new Piece(type, color, new Position(-1, -1));
                    p.IsAlive = false;
                    capBlack.Add(p);
                }
            }
            gameLogic.LoadCapturedPieces(capRed, capBlack);

            // Hands
            var redCards = new List<Card>();
            foreach (Dictionary<string, string> obj in ParseObjectArray(ExtractArray(json, "redHand")))
            {
                string suitVal, pointVal;
                obj.TryGetValue("suit", out suitVal);
                obj.TryGetValue("point", out pointVal);
                if (int.TryParse(suitVal ?? "0", out int suit) &&
                    int.TryParse(pointVal ?? "1", out int point))
                    redCards.Add(new Card((CardSuit)suit, point));
            }

            var blackCards = new List<Card>();
            foreach (Dictionary<string, string> obj in ParseObjectArray(ExtractArray(json, "blackHand")))
            {
                string suitVal, pointVal;
                obj.TryGetValue("suit", out suitVal);
                obj.TryGetValue("point", out pointVal);
                if (int.TryParse(suitVal ?? "0", out int suit) &&
                    int.TryParse(pointVal ?? "1", out int point))
                    blackCards.Add(new Card((CardSuit)suit, point));
            }
            gameLogic.CardManager.LoadHands(redCards, blackCards);

            // Action log
            List<string> logEntries = ParseStringArray(ExtractArray(json, "actionLog"));
            gameLogic.LoadActionLog(logEntries);
        }

        // Extracts a JSON array as a substring, respecting string literals.
        private static string ExtractArray(string json, string arrayKey)
        {
            string marker = "\"" + arrayKey + "\": [";
            int start = json.IndexOf(marker);
            if (start == -1) return "[]";
            start += marker.Length - 1; // position of '['

            int depth = 0;
            bool inString = false;
            bool escaped = false;
            for (int i = start; i < json.Length; i++)
            {
                char c = json[i];
                if (escaped) { escaped = false; continue; }
                if (c == '\\' && inString) { escaped = true; continue; }
                if (c == '"') { inString = !inString; continue; }
                if (inString) continue;
                if (c == '[') depth++;
                else if (c == ']') { depth--; if (depth == 0) return json.Substring(start, i - start + 1); }
            }
            return "[]";
        }

        // Parses an array of JSON objects into a list of string dictionaries.
        private static List<Dictionary<string, string>> ParseObjectArray(string arrayStr)
        {
            var result = new List<Dictionary<string, string>>();
            int pos = 0;
            while (true)
            {
                int objStart = arrayStr.IndexOf('{', pos);
                if (objStart == -1) break;

                int depth = 0;
                bool inStr = false;
                bool esc = false;
                int i = objStart;
                for (; i < arrayStr.Length; i++)
                {
                    char c = arrayStr[i];
                    if (esc) { esc = false; continue; }
                    if (c == '\\' && inStr) { esc = true; continue; }
                    if (c == '"') { inStr = !inStr; continue; }
                    if (inStr) continue;
                    if (c == '{') depth++;
                    else if (c == '}') { depth--; if (depth == 0) break; }
                }

                string inner = arrayStr.Substring(objStart + 1, i - objStart - 1);
                result.Add(ParseKeyValuePairs(inner));
                pos = i + 1;
            }
            return result;
        }

        // Parses key-value pairs from the interior of a JSON object.
        private static Dictionary<string, string> ParseKeyValuePairs(string content)
        {
            var result = new Dictionary<string, string>();
            int pos = 0;
            while (pos < content.Length)
            {
                int keyStart = content.IndexOf('"', pos);
                if (keyStart == -1) break;
                int keyEnd = content.IndexOf('"', keyStart + 1);
                if (keyEnd == -1) break;
                string key = content.Substring(keyStart + 1, keyEnd - keyStart - 1);

                int colon = content.IndexOf(':', keyEnd + 1);
                if (colon == -1) break;
                int valStart = colon + 1;
                while (valStart < content.Length && content[valStart] == ' ') valStart++;

                string value;
                if (valStart < content.Length && content[valStart] == '"')
                {
                    int valEnd = valStart + 1;
                    bool esc = false;
                    for (; valEnd < content.Length; valEnd++)
                    {
                        if (esc) { esc = false; continue; }
                        if (content[valEnd] == '\\') { esc = true; continue; }
                        if (content[valEnd] == '"') break;
                    }
                    value = content.Substring(valStart + 1, valEnd - valStart - 1);
                    pos = valEnd + 1;
                }
                else
                {
                    int valEnd = valStart;
                    while (valEnd < content.Length && content[valEnd] != ',' && content[valEnd] != '}') valEnd++;
                    value = content.Substring(valStart, valEnd - valStart).Trim();
                    pos = valEnd + 1;
                }
                result[key] = value;
            }
            return result;
        }

        // Parses a JSON array of strings.
        private static List<string> ParseStringArray(string arrayStr)
        {
            var result = new List<string>();
            int pos = 0;
            while (pos < arrayStr.Length)
            {
                int start = arrayStr.IndexOf('"', pos);
                if (start == -1) break;
                int end = start + 1;
                bool esc = false;
                for (; end < arrayStr.Length; end++)
                {
                    if (esc) { esc = false; continue; }
                    if (arrayStr[end] == '\\') { esc = true; continue; }
                    if (arrayStr[end] == '"') break;
                }
                string value = arrayStr.Substring(start + 1, end - start - 1)
                    .Replace("\\\"", "\"").Replace("\\\\", "\\");
                result.Add(value);
                pos = end + 1;
            }
            return result;
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
