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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();            
        }

        private void startGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Driftr game = new Driftr();
            game.MdiParent = this;
            game.Show();
            game.Location = new Point(0, 0);
            menuStrip1.Items[0].Visible = false;
            menuStrip1.Items[1].Visible = false; 
        }


        private void settToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void start_Load(object sender, EventArgs e)
        {
            Welcome welcome = new Welcome();
            welcome.MdiParent = this;
            welcome.Show();
        }
    }
}
