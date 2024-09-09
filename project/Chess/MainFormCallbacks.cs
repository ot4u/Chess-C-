using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Chess
{
    public partial class MainForm : Form, UIBoard
    {
        private ToolStripMenuItem temp; // выбранная сложность
        TimeSpan m_whiteTime = new TimeSpan(0);
        TimeSpan m_blackTime = new TimeSpan(0);

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CreateBoard();

            // установка индикатора хода
            picTurn.SizeMode = PictureBoxSizeMode.StretchImage;
            picTurn.Image = graphics.TurnIndicator[Player.WHITE];

            // установка начальной глубины AI
            temp = mnuDif3;
            AI.DEPTH = 3;

            SetStatus(false, "Выберите Новую Игру или Ручную Расстановку.");

            // настройка меню
            setPieceToolStripMenuItem.Enabled = false;
            manualBoardToolStripMenuItem.Checked = false;
            endCurrentGameToolStripMenuItem.Enabled = false;
        }

        private void windowClosing(object sender, FormClosingEventArgs e)
        {
            Stop();
        }

        private void Shutdown(object sender, EventArgs e)
        {
            Stop();
            this.Close();
        }

        private void endGame(object sender, EventArgs e)
        {
            Stop();
        }

        private void NewGame(object sender, EventArgs e)
        {
            ToolStripMenuItem button = (ToolStripMenuItem)sender;
            if (button.Text.StartsWith("Новая Игра AI против AI"))
            {
                NewGame(0);
            }
            else if (button.Text.StartsWith("Новая Игра AI против Игрока"))
            {
                NewGame(1);
            }
            else if (button.Text.StartsWith("Новая Игра Игрока"))
            {
                NewGame(2);
            }
        }

        private void Difficulty(object sender, EventArgs e)
        {
            // отключаем ранее выбранный уровень сложности
            if (temp != null)
            {
                temp.CheckState = CheckState.Unchecked;
            }

            // если AI думал, останавливаем его
            bool was = AI.RUNNING;
            AI.STOP = true;

            // выбираем новый уровень сложности
            temp = (ToolStripMenuItem)sender;
            temp.CheckState = CheckState.Checked;

            // обновляем уровень сложности AI
            AI.DEPTH = Int32.Parse((String)temp.Tag);
            LogMove("Сложность AI " + (string)temp.Tag + "\n");

            // если AI был активен, перезапускаем его ход
            if (was)
            {
                LogMove("AI Повторный ход\n");
                new Thread(chess.AISelect).Start();
            }
        }

        private void manualBoardToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked) // включение ручной расстановки
            {
                m_manualBoard = false;
                doneToolStripMenuItem.Enabled = false;
                m_finalizedBoard = false;
                Stop();

                m_manualBoard = ((ToolStripMenuItem)sender).Checked;
                endCurrentGameToolStripMenuItem.Enabled = true;
                White_King.Enabled = true;
                Black_King.Enabled = true;

                SetStatus(false, "Выберите однопользовательскую или двухпользовательскую ручную игру для расстановки фигур.");
            }
            else if (!m_finalizedBoard)
            {
                Stop();
            }
        }

        private void manualPieceMenuItem_Click(object sender, EventArgs e)
        {
            String labelName = ((ToolStripMenuItem)sender).Name;

            m_manualPlayer = (labelName.StartsWith("Белый")) ? Player.WHITE : Player.BLACK;

            if (labelName.EndsWith("Пешка"))
            {
                m_manualPiece = Piece.PAWN;
            }
            else if (labelName.EndsWith("Конь"))
            {
                m_manualPiece = Piece.KNIGHT;
            }
            else if (labelName.EndsWith("Слон"))
            {
                m_manualPiece = Piece.BISHOP;
            }
            else if (labelName.EndsWith("Ладья"))
            {
                m_manualPiece = Piece.ROOK;
            }
            else if (labelName.EndsWith("Ферзь"))
            {
                m_manualPiece = Piece.QUEEN;
            }
            else if (labelName.EndsWith("Король"))
            {
                m_manualPiece = Piece.KING;
            }
        }

        private void doneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // завершение расстановки фигур
            m_manualBoard = false;
            m_finalizedBoard = true;

            // изменения в меню
            manualBoardToolStripMenuItem.Checked = false;
            setPieceToolStripMenuItem.Enabled = false;
            doneToolStripMenuItem.Enabled = false;

            // начало игры
            SetStatus(false, "Ход Белых");
            tmrWhite.Start();
            m_checkmate = chess.detectCheckmate();

            if (m_aigame && !m_checkmate)
            {
                new Thread(chess.AISelect).Start();
            }
        }

        // ---
        // Таймер хода игрока
        // ---
        private void tmrWhite_Tick(object sender, EventArgs e)
        {
            m_whiteTime = m_whiteTime.Add(new TimeSpan(0, 0, 0, 0, tmrWhite.Interval));
            lblWhiteTime.Text = string.Format("{0:d2}:{1:d2}:{2:d2}.{3:d1}", m_whiteTime.Hours, m_whiteTime.Minutes, m_whiteTime.Seconds, m_whiteTime.Milliseconds / 100);
        }

        private void tmrBlack_Tick(object sender, EventArgs e)
        {
            m_blackTime = m_blackTime.Add(new TimeSpan(0, 0, 0, 0, tmrBlack.Interval));
            lblBlackTime.Text = string.Format("{0:d2}:{1:d2}:{2:d2}.{3:d1}", m_blackTime.Hours, m_blackTime.Minutes, m_blackTime.Seconds, m_blackTime.Milliseconds / 100);
        }
    }
}
