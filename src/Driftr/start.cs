using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Driftr
{
    public partial class start : Form
    {
        public start()
        {
            InitializeComponent();
        }

        public void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            help Help = new help();
            Help.MdiParent = this;
            Help.Show();
            Help.Location = new Point(0, 0);
        }

        private void startGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Driftr game = new Driftr();
            game.MdiParent = this;
            game.Show();
            game.Location = new Point(0, 0);
        }


        private void settToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.MdiChildren.Length > 0)    //close childrens only when there are some
            {
                foreach (Form childForm in this.MdiChildren)
                    childForm.Close();

                //e.Cancel = true;  //cancel Form2 closing
            }
        }
    }
}
