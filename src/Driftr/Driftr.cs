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

        private readonly bool[] _left = new bool[2];
        private readonly bool[] _right = new bool[2];
        private readonly bool[] _up = new bool[2];
        private readonly bool[] _down = new bool[2];

        private readonly float[] _steering1 = new float[2]; // -1.0 is left, 0 is center,  1.0 is right.
        private readonly float[] _throttle1 = new float[2]; // 0 is coasting, 1 is full throttle.
        private readonly float[] _brakes1 = new float[2]; // 0 is no breaks, 1 is full breaks.

        private readonly Vehicle[] _vehicles = { new Vehicle(), new Vehicle() };

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

            _vehicles[0].Setup(new Vector(3, 8) / 2.0f, 5, Brushes.Aqua);
            _vehicles[0].SetLocation(new Vector(0, 0), 0);

            _vehicles[1].Setup(new Vector(3, 8) / 2.0f, 5, Brushes.Yellow);
            _vehicles[1].SetLocation(new Vector(0, 0), 3);
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
            _vehicles[0].Draw(_graphics, _bufferSize);
            _vehicles[1].Draw(_graphics, _bufferSize);
            label1.Text = Convert.ToString(Math.Round(_vehicles[1].Speed));
        }

        private void DoFrame()
        {
            float etime = _timer.GetETime();

            ProcessInput();

            // Apply vehicle controls.
            for (int i = 0; i < _vehicles.Length; i++)
            {
                _vehicles[i].SetSteering(_steering1[i]);
                _vehicles[i].SetThrottle(_throttle1[i]);
                _vehicles[i].SetBrakes(_brakes1[i]);

                _vehicles[i].Update(etime);
            }

            ConstrainVehicle();

            screen.Invalidate();
        }

        private void ConstrainVehicle()
        {
            foreach (Vehicle v in _vehicles)
            {
                Vector position = v.Position;
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
        }

        private void ProcessInput()
        {
            for (int i = 0; i < _vehicles.Length; i++)
            {
                if (_left[i])
                {
                    _steering1[i] = -1;
                }
                else if (_right[i])
                {
                    _steering1[i] = 1;
                }
                else
                {
                    _steering1[i] = 0;
                }

                _throttle1[i] = _up[i] ? GameSettings.Throttle : 0;

                _brakes1[i] = _down[i] ? 1 : 0;
            }
        }

        private void Driftr_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    _left[0] = true;
                    break;
                case Keys.Right:
                    _right[0] = true;
                    break;
                case Keys.Up:
                    _up[0] = true;
                    break;
                case Keys.Down:
                    _down[0] = true;
                    break;
                case Keys.A:
                    _left[1] = true;
                    break;
                case Keys.D:
                    _right[1] = true;
                    break;
                case Keys.W:
                    _up[1] = true;
                    break;
                case Keys.S:
                    _down[1] = true;
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
                    _left[0] = false;
                    break;
                case Keys.Right:
                    _right[0] = false;
                    break;
                case Keys.Up:
                    _up[0] = false;
                    break;
                case Keys.Down:
                    _down[0] = false;
                    break;
                case Keys.A:
                    _left[1] = false;
                    break;
                case Keys.D:
                    _right[1] = false;
                    break;
                case Keys.W:
                    _up[1] = false;
                    break;
                case Keys.S:
                    _down[1] = false;
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
            double snelheid = Math.Round(_vehicles[0].Speed);
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
