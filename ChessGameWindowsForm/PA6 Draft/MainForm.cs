using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PA6_Draft
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            LightColor = Color.FromName("AntiqueWhite");
            DarkColor = Color.FromArgb(200, 100, 20);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void NewGame_Click(object sender, EventArgs e)
        {
            new Chessboard(LightColor,DarkColor, 
                new ChessGame((int)Minute.Value, (int)Seconds.Value, player1.Text, player2.Text)).ShowDialog();
        }

        private void DarkColor_Click(object sender, EventArgs e)
        {
            DialogResult d = colorDialog1.ShowDialog();
            if (d == DialogResult.OK)
                DarkColor = colorDialog1.Color;
        }

        private void LightColor_Click(object sender, EventArgs e)
        {
            DialogResult d = colorDialog1.ShowDialog();
            if (d == DialogResult.OK)
                LightColor = colorDialog1.Color;
        }
    }
}
