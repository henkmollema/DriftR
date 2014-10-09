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
    public partial class Driftr : Form
    {
        private const float screenScale = 3.0F;
        private Graphics _graphics;
        private Bitmap _backbuffer;
        private Size _bufferSize;
        private Timer timer = new Timer();

        private bool _left = false, _right = false, _up = false, _down = false;

        private float steering = 0; // -1.0 is left, 0 is center,  1.0 is right.
        private float throttle = 0; // 0 is coasting, 1 is full throttle.
        private float breaks;   // 0 is no breaks, 1 is full breaks.

        public Driftr()
        {
            InitializeComponent();
            Application.Idle += Application_Idle;

            screen.Paint += screen_Paint;
            KeyUp += Driftr_KeyUp;
            KeyDown += Driftr_KeyDown;

            Init(screen.Size);
        }

        private void Driftr_Paint(object sender, PaintEventArgs e)
        {
            _backbuffer = new Bitmap(_bufferSize.Width, _bufferSize.Height);
            _graphics = Graphics.FromImage(_backbuffer);
        }

        private void Init(Size size)
        {

        }

        void screen_Paint(object sender, PaintEventArgs e)
        {
            throw new NotImplementedException();
        }

        void Driftr_KeyUp(object sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        void Driftr_KeyDown(object sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        void Application_Idle(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
