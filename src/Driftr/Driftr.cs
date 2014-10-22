using Driftr.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Driftr
{
    public partial class Driftr : Form
    {
        private const float screenScale = 3.0f;
        private Graphics _graphics;
        private Bitmap _backbuffer;
        private Size _bufferSize;
        private readonly GameTimer _timer = new GameTimer();

        private bool _left, _right, _up, _down;

        private float _steering; // -1.0 is left, 0 is center,  1.0 is right.
        private float _throttle; // 0 is coasting, 1 is full throttle.
        private float _brakes; // 0 is no breaks, 1 is full breaks.

        private readonly Vehicle _vehicle = new Vehicle();

        public Driftr()
        {
            InitializeComponent();
            Application.Idle += Application_Idle;
            screen.Paint += screen_Paint;
            KeyUp += Driftr_KeyUp;
            KeyDown += Driftr_KeyDown;

            Init(screen.Size);
            pictureBox1.Parent = screen;

            // brandstof
            InitTimer();
            pictureBox1.Image = Resources.dashboard_5;
        }

        private void Init(Size size)
        {
            _bufferSize = size;
            _backbuffer = new Bitmap(_bufferSize.Width, _bufferSize.Height);
            _graphics = Graphics.FromImage(_backbuffer);

            _timer.GetETime();

            _vehicle.Setup(new Vector(3, 8) / 2.0f, 5, Color.Red);
            _vehicle.SetLocation(new Vector(0, 0), 0);
        }

        private void Render(Graphics g)
        {
            _graphics.Clear(Color.Transparent);
            _graphics.ResetTransform();
            _graphics.ScaleTransform(screenScale, -screenScale);
            _graphics.TranslateTransform(
                _bufferSize.Width / 2.0f / screenScale,
                -_bufferSize.Height / 2.0f / screenScale);

            DrawScreen();

            //Color p = _backbuffer.GetPixel((int)Math.Ceiling(_vehicle.Position.X), (int)Math.Ceiling(_vehicle.Position.Y));

            g.DrawImage(
                _backbuffer,
                new Rectangle(0, 0, _bufferSize.Width, _bufferSize.Height),
                0,
                0,
                _bufferSize.Width,
                _bufferSize.Height,
                GraphicsUnit.Pixel);
        }

        private void DrawScreen()
        {
            _vehicle.Draw(_graphics, _bufferSize); 
            label1.Text = Convert.ToString(Math.Round(_vehicle.Wheels[2].WheelSpeed));
        }

        private void DoFrame()
        {
            float etime = _timer.GetETime();

            ProcessInput();

            // Apply vehicle controls.
            _vehicle.SetSteering(_steering);
            _vehicle.SetThrottle(_throttle);
            _vehicle.SetBrakes(_brakes);

            _vehicle.Update(etime);

            ConstrainVehicle();

            screen.Invalidate();
        }

        private void ConstrainVehicle()
        {
            Vector position = _vehicle.Position;
            var screenSize = new Vector(screen.Width / screenScale, screen.Height / screenScale);

            while (position.X > screenSize.X / 2.0f)
            {
                position.X -= screenSize.X;
            }
            while (position.Y > screenSize.Y / 2.0f)
            {
                position.Y -= screenSize.Y;
            }
            while (position.X < -screenSize.X / 2.0f)
            {
                position.X += screenSize.X;
            }
            while (position.Y < -screenSize.Y / 2.0f)
            {
                position.Y += screenSize.Y;
            }
        }

        private void ProcessInput()
        {
            if (_left)
            {
                _steering = -1;
            }
            else if (_right)
            {
                _steering = 1;
            }
            else
            {
                _steering = 0;
            }

            _throttle = _up ? GameSettings.Throttle : 0;

            _brakes = _down ? 1 : 0;
        }

        private void Driftr_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    _left = true;
                    break;
                case Keys.Right:
                    _right = true;
                    break;
                case Keys.Up:
                    _up = true;
                    break;
                case Keys.Down:
                    _down = true;
                    break;
                default:
                    return;
            }

            e.Handled = true;
        }

        private void Driftr_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    _left = false;
                    break;
                case Keys.Right:
                    _right = false;
                    break;
                case Keys.Up:
                    _up = false;
                    break;
                case Keys.Down:
                    _down = false;
                    break;
                default:
                    return;
            }

            e.Handled = true;
        }

        private void screen_Paint(object sender, PaintEventArgs e)
        {
            Render(e.Graphics);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            DoFrame();
        }

        double benzine = 100;
        int pitstops = 0;

        private Timer timer1;
        private Timer timer2;


        public void InitTimer()
        {
            timer1 = new Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000; // in miliseconds
            timer1.Start();
        }

        public void InitTimer2()
        {
            timer2 = new Timer();
            timer2.Tick += new EventHandler(timer2_Tick);
            timer2.Interval = 1000; // in miliseconds
            timer2.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            double snelheid = Math.Round(_vehicle.Wheels[2].WheelSpeed);
            if (snelheid == 0)
            {
                benzine = benzine - 1;
            }
            else
            {
                benzine = benzine - ((1 * snelheid) / 80);
            }

            label4.Text = Convert.ToString(benzine);

            if (benzine == 0)
            {
                timer1.Stop();
                label4.Text = "Empty";
                pictureBox1.Image = Resources.dashboard_1;
            }


            if (benzine <= 100 && benzine > 80)
            {
                pictureBox1.Image = Resources.dashboard_5;
            }
            else if (benzine <= 80 && benzine > 60)
            {
                pictureBox1.Image = Resources.dashboard_4;
            }
            else if (benzine <= 60 && benzine > 40)
            {
                pictureBox1.Image = Resources.dashboard_3;
            }
            else if (benzine <= 40 && benzine > 20)
            {
                pictureBox1.Image = Resources.dashboard_2;
            }
            else if (benzine <= 20 && benzine > 0)
            {
                pictureBox1.Image = Resources.dashboard_1;
            }
            else
            {
                pictureBox1.Image = Resources.dashboard_0;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            benzine = benzine + 10;

            if (benzine >= 100)
            {
                timer2.Stop();

                if (benzine > 100)
                {
                    benzine = 100;
                }
            }

            label4.Text = Convert.ToString(benzine);

            if (benzine == 100)
            {
                pictureBox1.Image = Resources.dashboard_5;
            }
            else if (benzine >= 80 && benzine < 100)
            {
                pictureBox1.Image = Resources.dashboard_4;
            }
            else if (benzine >= 60 && benzine < 80)
            {
                pictureBox1.Image = Resources.dashboard_3;
            }
            else if (benzine >= 40 && benzine < 60)
            {
                pictureBox1.Image = Resources.dashboard_2;
            }
            else if (benzine >= 20 && benzine < 40)
            {
                pictureBox1.Image = Resources.dashboard_1;
            }
            else
            {
                pictureBox1.Image = Resources.dashboard_0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            InitTimer2();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer2.Stop();
            timer1.Start();
            pitstops++;
            label3.Text = Convert.ToString(pitstops);
        }
    }
}
