using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Driftr.Properties;

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

        private readonly float[] _steerings = new float[2]; // -1.0 is left, 0 is center,  1.0 is right.
        private readonly float[] _throttles = new float[2]; // 0 is coasting, 1 is full throttle.
        private readonly float[] _brakes = new float[2]; // 0 is no breaks, 1 is full breaks.

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
            pictureBox2.Parent = screen;

            // brandstof
            InitTimer();
            pictureBox1.Image = Resources.dashboard_5_red;
            pictureBox2.Image = Resources.dashboard_5_yellow;
        }

        private void Init(Size size)
        {
            _bufferSize = size;
            _backbuffer = new Bitmap(_bufferSize.Width, _bufferSize.Height);
            _graphics = Graphics.FromImage(_backbuffer);
            
            _timer.GetETime();

            _vehicles[0].Setup(new Vector(3, 8) / 2.0f, 5, Resources.CarRed);
            _vehicles[0].SetLocation(new Vector(0, 0), 0);

            _vehicles[1].Setup(new Vector(3, 8) / 2.0f, 5, Resources.CarYellow);
            _vehicles[1].SetLocation(new Vector(10, 0), 0);
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

            //Color p = new Bitmap(screen.Image).GetPixel((int)Math.Ceiling(_vehicles[0].Position.X), (int)Math.Ceiling(_vehicles[0].Position.Y));
            //lblDebug.Text = "Color: " + p;

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
            speedLabelRed.Text = Convert.ToString(Math.Round(_vehicles[0].DisplaySpeed));
            speedLabelYellow.Text = Convert.ToString(Math.Round(_vehicles[1].DisplaySpeed));

            //Debug.WriteLine("Pos: {0}", screen.PointToClient(new Point((int)p.X, (int)p.Y)));
            //Debug.WriteLine(VehicleRelativePosition(0));
        }

        private Vector VehicleRelativePosition(int vehicle)
        {
            return _vehicles[vehicle].RelativePosition(_bufferSize.Width, _bufferSize.Height, screenScale);
        }

        private void DoFrame()
        {
            float etime = _timer.GetETime();

            ProcessInput();

            // Apply vehicle controls.
            for (int i = 0; i < _vehicles.Length; i++)
            {
                _vehicles[i].SetSteering(_steerings[i]);
                _vehicles[i].SetThrottle(_throttles[i]);
                _vehicles[i].SetBrakes(_brakes[i]);

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
                    _steerings[i] = -1;
                }
                else if (_right[i])
                {
                    _steerings[i] = 1;
                }
                else
                {
                    _steerings[i] = 0;
                }

                _throttles[i] = _up[i] ? GameSettings.Throttle : 0;

                _brakes[i] = _down[i] ? 1 : 0;
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

        private double fuelRed = 100;
        private double fuelYellow = 100;
        private int pitstopsRed;
        private int pitstopsYellow;

        private Timer timerRed;
        private Timer timerRed2;
        private Timer timerYellow;
        private Timer timerYellow2;


        private void InitTimer()
        {
            timerRed = new Timer();
            timerRed.Tick += timerRed_Tick;
            timerRed.Interval = 1000; // in miliseconds
            timerRed.Start();

            timerYellow = new Timer();
            timerYellow.Tick += timerYellow_Tick;
            timerYellow.Interval = 1000; // in miliseconds
            timerYellow.Start();
        }

        private void InittimerRed2()
        {
            timerRed2 = new Timer();
            timerRed2.Tick += timerRed2_Tick;
            timerRed2.Interval = 1000; // in miliseconds
            timerRed2.Start();

            timerYellow2 = new Timer();
            timerYellow2.Tick += timerYellow2_Tick;
            timerYellow2.Interval = 1000; // in miliseconds
            timerYellow2.Start();
        }

        private void timerRed_Tick(object sender, EventArgs e)
        {
            double snelheidRed = Math.Round(_vehicles[0].DisplaySpeed);
            if (snelheidRed == 0)
            {
                fuelRed = fuelRed - 0.5;
            }
            else
            {
                fuelRed = fuelRed - ((1 * snelheidRed) / 80);
            }

            label4.Text = Convert.ToString(fuelRed);

            if (fuelRed == 0)
            {
                timerRed.Stop();
                label4.Text = "Empty";
                pictureBox1.Image = Resources.dashboard_1_red;
            }


            if (fuelRed <= 100 && fuelRed > 80)
            {
                pictureBox1.Image = Resources.dashboard_5_red;
            }
            else if (fuelRed <= 80 && fuelRed > 60)
            {
                pictureBox1.Image = Resources.dashboard_4_red;
            }
            else if (fuelRed <= 60 && fuelRed > 40)
            {
                pictureBox1.Image = Resources.dashboard_3_red;
            }
            else if (fuelRed <= 40 && fuelRed > 20)
            {
                pictureBox1.Image = Resources.dashboard_2_red;
            }
            else if (fuelRed <= 20 && fuelRed > 0)
            {
                pictureBox1.Image = Resources.dashboard_1_red;
            }
            else
            {
                pictureBox1.Image = Resources.dashboard_0_red;
            }
        }

        private void timerRed2_Tick(object sender, EventArgs e)
        {
            timerRed.Stop();
            fuelRed = fuelRed + 10;

            if (fuelRed >= 100)
            {
                timerRed2.Stop();

                if (fuelRed > 100)
                {
                    fuelRed = 100;
                }
            }

            label4.Text = Convert.ToString(fuelRed);

            if (fuelRed == 100)
            {
                pictureBox1.Image = Resources.dashboard_5_red;
            }
            else if (fuelRed >= 80 && fuelRed < 100)
            {
                pictureBox1.Image = Resources.dashboard_4_red;
            }
            else if (fuelRed >= 60 && fuelRed < 80)
            {
                pictureBox1.Image = Resources.dashboard_3_red;
            }
            else if (fuelRed >= 40 && fuelRed < 60)
            {
                pictureBox1.Image = Resources.dashboard_2_red;
            }
            else if (fuelRed >= 20 && fuelRed < 40)
            {
                pictureBox1.Image = Resources.dashboard_1_red;
            }
            else
            {
                pictureBox1.Image = Resources.dashboard_0_red;
            }
        }

        private void timerYellow_Tick(object sender, EventArgs e)
        {
            double snelheidYellow = Math.Round(_vehicles[1].DisplaySpeed);
            if (snelheidYellow == 0)
            {
                fuelYellow = fuelYellow - 0.5;
            }
            else
            {
                fuelYellow = fuelYellow - ((1 * snelheidYellow) / 80);
            }

            label5.Text = Convert.ToString(fuelYellow);

            if (fuelYellow == 0)
            {
                timerRed.Stop();
                label5.Text = "Empty";
                pictureBox2.Image = Resources.dashboard_1_yellow;
            }


            if (fuelYellow <= 100 && fuelYellow > 80)
            {
                pictureBox2.Image = Resources.dashboard_5_yellow;
            }
            else if (fuelYellow <= 80 && fuelYellow > 60)
            {
                pictureBox2.Image = Resources.dashboard_4_yellow;
            }
            else if (fuelYellow <= 60 && fuelYellow > 40)
            {
                pictureBox2.Image = Resources.dashboard_3_yellow;
            }
            else if (fuelYellow <= 40 && fuelYellow > 20)
            {
                pictureBox2.Image = Resources.dashboard_2_yellow;
            }
            else if (fuelYellow <= 20 && fuelYellow > 0)
            {
                pictureBox2.Image = Resources.dashboard_1_yellow;
            }
            else
            {
                pictureBox2.Image = Resources.dashboard_0_yellow;
            }
        }

        private void timerYellow2_Tick(object sender, EventArgs e)
        {
            timerRed.Stop();
            fuelYellow = fuelYellow + 10;

            if (fuelYellow >= 100)
            {
                timerYellow2.Stop();

                if (fuelYellow > 100)
                {
                    fuelYellow = 100;
                }
            }

            label5.Text = Convert.ToString(fuelYellow);

            if (fuelYellow == 100)
            {
                pictureBox2.Image = Resources.dashboard_5_yellow;
            }
            else if (fuelYellow >= 80 && fuelYellow < 100)
            {
                pictureBox2.Image = Resources.dashboard_4_yellow;
            }
            else if (fuelYellow >= 60 && fuelYellow < 80)
            {
                pictureBox2.Image = Resources.dashboard_3_yellow;
            }
            else if (fuelYellow >= 40 && fuelYellow < 60)
            {
                pictureBox2.Image = Resources.dashboard_2_yellow;
            }
            else if (fuelYellow >= 20 && fuelYellow < 40)
            {
                pictureBox2.Image = Resources.dashboard_1_yellow;
            }
            else
            {
                pictureBox2.Image = Resources.dashboard_0_yellow;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right)
            {
                Driftr_KeyDown(this, new KeyEventArgs(keyData));
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
