using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
        private readonly Dictionary<Vehicle, int> _vehicleLaps = new Dictionary<Vehicle, int>();

        private readonly Dictionary<Vehicle, int> _vehiclePitstops = new Dictionary<Vehicle, int>();
        private readonly Dictionary<Vehicle, bool> _vehiclePitstopsCheckpoint = new Dictionary<Vehicle, bool>();
        private readonly Dictionary<Vehicle, bool[]> _vehicleCheckpoints = new Dictionary<Vehicle, bool[]>();

        private readonly List<Color> _checkpoints = new List<Color>
            {
                Color.FromArgb(255, 241, 0),
                Color.FromArgb(0, 162, 232),
                Color.FromArgb(185, 122, 87),
                Color.FromArgb(254, 126, 39),
                Color.FromArgb(236, 27, 36),
                Color.FromArgb(162, 73, 164),
                Color.FromArgb(255, 174, 200)
            };

        private static readonly Color _pitstop = Color.FromArgb(92, 92, 92);
        private static readonly Color _obstacle = Color.FromArgb(16, 245, 0);
        private static readonly Color _grass = Color.FromArgb(21, 115, 0);
        private static readonly Color _pitstopCheckpoint = Color.FromArgb(0, 246, 255);

        public Driftr()
        {
            LapTimes();
            InitializeComponent();
            Application.Idle += Application_Idle;
            screen.Paint += screen_Paint;
            screen.MouseUp += screen_MouseUp;
            KeyUp += Driftr_KeyUp;
            KeyDown += Driftr_KeyDown;

            // Add vehicles with zero laps.
            foreach (var v in _vehicles)
            {
                _vehicleLaps.Add(v, 0);
                _vehicleCheckpoints.Add(v, new bool[7]);
                _vehiclePitstops.Add(v, 0);
                _vehiclePitstopsCheckpoint.Add(v, false);
            }

            Init(screen.Size);
            pictureBox1.Parent = screen;
            pictureBox2.Parent = screen;
            //lapTimeRedLabel.Parent = screen;
            //lapTimeYellowLabel.Parent = screen;

            // brandstof
            InitTimer();
            pictureBox1.Image = Resources.dashboard_5_red;
            pictureBox2.Image = Resources.dashboard_5_yellow;
        }

        void screen_MouseUp(object sender, MouseEventArgs e)
        {
            var p = ((Bitmap)screen.BackgroundImage).GetPixel(e.X, e.Y);
            Debug.WriteLine(p);
        }

        private void Init(Size size)
        {
            screen.BackgroundImage = Resources.MapBackground;
            screen.BackgroundImageLayout = ImageLayout.None;
            //screen.Image = null;

            _bufferSize = size;
            _backbuffer = new Bitmap(_bufferSize.Width, _bufferSize.Height);
            _graphics = Graphics.FromImage(_backbuffer);
            
            _timer.GetETime();

            _vehicles[0].Setup(new Vector(3, 8) / 2.0f, 5, Resources.CarRed);
            _vehicles[0].SetLocation(new Vector(-117.5f, 3.5f), 0);

            _vehicles[1].Setup(new Vector(3, 8) / 2.0f, 5, Resources.CarYellow);
            _vehicles[1].SetLocation(new Vector(-103.0f, 3.5f), 0);
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

            ProcessCheckpoints();

            int lapsRed = _vehicleLaps[_vehicles[0]];
            int lapsYellow = _vehicleLaps[_vehicles[1]];

            roundsLabelYellow.Text = lapsYellow.ToString();
            roundsLabelRed.Text = lapsRed.ToString();
        }

        private void ProcessCheckpoints()
        {
            var background = (Bitmap)screen.BackgroundImage;
            for (int i = 0; i < _vehicles.Length; i++)
            {
                var vehicle = _vehicles[i];
                var pos = VehicleRelativePosition(i);

                var color = background.GetPixel((int)pos.X, (int)pos.Y);
                if (_checkpoints.Any(x => x == color))
                {
                    int index = _checkpoints.IndexOf(color);

                    // If every checkpoint is hit and we hit the finish line again,
                    // reset every checkpoints except the finish and add a lap.
                    if (_vehicleCheckpoints[vehicle].All(x => x) && index == 0)
                    {
                        for (int n = 1; n < _vehicleCheckpoints[vehicle].Length; n++)
                        {
                            _vehicleCheckpoints[vehicle][n] = false;
                        }

                        //Debug.WriteLine("Lap: {0}", _vehicleLaps[vehicle] + 1);

                        // Increment lap count.
                        if (++_vehicleLaps[vehicle] == 3)
                        {
                            //Debug.WriteLine("finished");

                            // todo: finish..   
                        }
                        return;
                    }

                    // Check if previous checkpoint was hit.
                    int prev = index - 1;
                    if (prev == -1 || _vehicleCheckpoints[vehicle][prev])
                    {
                        //Debug.WriteLine("Checkpoint index {0}: {1} hit", index, color);
                        _vehicleCheckpoints[vehicle][index] = true;
                    }
                }
            }
        }

        private Vector VehicleRelativePosition(Vehicle v)
        {
            return VehicleRelativePosition(Array.FindIndex(_vehicles, x => v == x));
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
                var v = _vehicles[i];

                v.Collision = ObstacleCollision(v) || Collission();

                v.SetSteering(_steerings[i]);
                v.SetThrottle(_throttles[i], IsOffroad(v));
                v.SetBrakes(_brakes[i]);

                _vehicles[i].Update(etime);

                v.Collision = false;
            }

            ConstrainVehicle();

            screen.Invalidate();
        }

        private bool IsInPitstop(Vehicle v)
        {
            return GetColor(v) == _pitstop;
        }

        private bool IsOffroad(Vehicle v)
        {
            return GetColor(v) == _grass;
        }

        private bool ObstacleCollision(Vehicle v)
        {
            return GetColor(v) == _obstacle;
        }

        private Color GetColor(Vehicle v)
        {
            var pos = VehicleRelativePosition(v);
            var background = (Bitmap)screen.BackgroundImage;
            var color = background.GetPixel((int)pos.X, (int)pos.Y);
            return color;
        }

        private bool Collission()
        {
            var red = _vehicles[0];
            var yellow = _vehicles[1];

            return red.CollissionWith(yellow);
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

        private Timer timerRed;
        private Timer timerYellow;
        private Timer timerLaps;
        private Stopwatch lapTimeRed;
        private Stopwatch lapTimeYellow;

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

            timerLaps = new Timer();
            timerLaps.Tick += timerLaps_Tick;
            timerLaps.Interval = 100; // in miliseconds
            timerLaps.Start();
        }

        private void timerRed_Tick(object sender, EventArgs e)
        {
            Vehicle red = _vehicles[0];
            red.UpdateFuel(IsInPitstop(red));
            double fuelRed = red.Fuel;

            label4.Text = Convert.ToString(fuelRed);

            if (fuelRed <= 0)
            {
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

        private void timerYellow_Tick(object sender, EventArgs e)
        {
            Vehicle yellow = _vehicles[1];
            yellow.UpdateFuel(IsInPitstop(yellow));
            double fuelYellow = yellow.Fuel;

            label5.Text = Convert.ToString(fuelYellow);

            if (fuelYellow <= 0)
            {
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

        private void timerLaps_Tick(object sender, EventArgs e)
        {
            string ealpsedRed = lapTimeRed.Elapsed.Duration().ToString("mm':'ss':'ff");
            lapTimeRedLabel.Text = ealpsedRed;

            string ealpsedYellow = lapTimeYellow.Elapsed.Duration().ToString("mm':'ss':'ff");
            lapTimeYellowLabel.Text = ealpsedYellow;
        }

        private void LapTimes()
        {
            lapTimeRed = new Stopwatch();
            lapTimeRed.Start();

            lapTimeYellow = new Stopwatch();
            lapTimeYellow.Start();
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
