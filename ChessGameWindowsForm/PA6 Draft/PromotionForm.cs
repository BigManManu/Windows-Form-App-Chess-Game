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
    public partial class PromotionForm : Form
    {
        private Promotion promote;
        public PromotionForm()
        {
            InitializeComponent();
            AddPromotes();
            comboBox1.SelectedIndex = 0;
        }

        public object WhitePromote()
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    promote = Promotion.WKNIGHT;
                    break;
                case 1:
                    promote= Promotion.WBISHOP;
                    break;
                case 2:
                    promote = Promotion.WROOK;
                    break;
                case 3:
                    promote = Promotion.WQUEEN;
                    break;
            }
            return promote;
        }

        public object BlackPromote()
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    promote = Promotion.BKNIGHT;
                    break;
                case 1:
                    promote = Promotion.BBISHOP;
                    break;
                case 2:
                    promote = Promotion.BROOK;
                    break;
                case 3:
                    promote = Promotion.BQUEEN;
                    break;
            }
            return promote;
        }

        public void AddPromotes()
        {
            comboBox1.Items.Add("Knight");
            comboBox1.Items.Add("Bishop");
            comboBox1.Items.Add("Rook");
            comboBox1.Items.Add("Queen");
        }
    }
}
