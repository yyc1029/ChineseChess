using System.Drawing;
using System.Windows.Forms;

namespace ChineseChess.UI
{
    public class RulesDialog : Form
    {
        public RulesDialog()
        {
            Text = "遊戲規則";
            Width = 520;
            Height = 530;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(50, 40, 30);

            RichTextBox rtb = new RichTextBox
            {
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(480, 450),
                BackColor = Color.FromArgb(60, 50, 35),
                ForeColor = Color.FromArgb(230, 210, 160),
                Font = new Font("微軟正黑體", 10),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Text = BuildRulesText()
            };
            Controls.Add(rtb);

            Button btnOK = new Button
            {
                Text = "明白了！",
                Location = new System.Drawing.Point(195, 467),
                Size = new System.Drawing.Size(110, 35),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(80, 60, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 60);
            Controls.Add(btnOK);
            AcceptButton = btnOK;
        }

        private string BuildRulesText()
        {
            return
"=== 中國象棋 + 卡牌特效版規則 ===\r\n\r\n" +
"【基本象棋規則】\r\n" +
"‧ 紅方先走，雙方輪流行棋\r\n" +
"‧ 帥/將（將軍）：在九宮格內移動，每次走一格（含斜）\r\n" +
"‧ 仕/士：斜走一格，不可出宮\r\n" +
"‧ 相/象：斜走兩格（田字），不可過河，若腳下有子則不能跳\r\n" +
"‧ 馬：先直後斜，走「日」字型，腳下有子則不能向該方向走\r\n" +
"‧ 車：直線走任意格數\r\n" +
"‧ 炮：直線走，吃子時中間必須隔一顆子\r\n" +
"‧ 兵/卒：未過河只能前進，過河後可橫走一格，不可後退\r\n" +
"‧ 雙將規則：兩方帥/將不得面對面直線無遮擋\r\n" +
"‧ 被將死（無法解除將軍且無合法棋步）則輸\r\n\r\n" +
"【卡牌機制】\r\n" +
"‧ 每回合結束時自動抽一張牌，手牌上限 3 張\r\n" +
"‧ 每回合最多使用一張牌\r\n" +
"‧ 遊戲開始時雙方各有 3 張初始手牌\r\n" +
"‧ 點擊手牌區的牌即可使用\r\n\r\n" +
"【卡牌效果一覽】\r\n" +
"‧ J 牌（凍結）：點擊後再點棋盤上對方一顆棋子\r\n" +
"  → 該棋子下一回合無法移動\r\n" +
"‧ Q 牌（復活）：從陣亡棋子中選一顆放回原位\r\n" +
"  → 若原位已被佔，棋子放至附近空格\r\n" +
"‧ K 牌（跳過）：使用後對方下一回合直接跳過\r\n" +
"‧ A / 2-10（對決）：與對方發起比牌大小\r\n\r\n" +
"【對決規則（A / 2-10 牌）】\r\n" +
"‧ 發起對決後，對方須選一張對決牌應對\r\n" +
"‧ 牌值：A=14，其餘依點數（2=2，10=10）\r\n" +
"‧ 手牌以牌背顯示，對決時才翻牌揭曉\r\n" +
"‧ 牌值較高者獲勝 → 本回合可多走一步棋\r\n" +
"‧ 平手 → 雙方各補一張牌，無額外效果\r\n" +
"‧ 若對方無對決牌 → 對方自動判負\r\n\r\n" +
"【其他規則】\r\n" +
"‧ 悔棋：可撤回上一步走棋；若用 Q 牌復活後悔棋，復活的棋子也會消失（牌不歸還）\r\n" +
"‧ 認輸：直接結束遊戲，對方獲勝\r\n" +
"‧ AI 模式：AI 也會使用卡牌（優先使用 K/J，再考慮對決牌）";
        }
    }
}
