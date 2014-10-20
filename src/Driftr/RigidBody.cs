using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Driftr
{
    public class RigidBody
    {
        // Linear properties.
        private Vector _position = new Vector(); // P
        private Vector _velocity = new Vector(); // A
        private Vector _forces = new Vector(); // F
        private float _mass; // M

        // Angular properties
        private float _angle;
        private float _angularVelocity;
        private float _torque;
        private float _inertia;

        // Graphical properties.
        private Vector _halfsize = new Vector();
        private Rectangle _rect;
        private Color _color;

        public RigidBody()
        {
            // Set defaults to prevent divide by zero.
            _mass = 1.0F;
            _inertia = 1.0F;
        }

        public virtual void Setup(Vector halfsize, float mass, Color color)
        {
            _halfsize = halfsize;
            _mass = mass;
            _color = color;

            _inertia = (1.0F / 12.0F)
                       * (halfsize.X * halfsize.X)
                       * (halfsize.Y * halfsize.Y)
                       * mass;

            _rect.X = (int)-_halfsize.X;
            _rect.Y = (int)-_halfsize.Y;
            _rect.Width = (int)(_halfsize.X * 2.0F);
            _rect.Height = (int)(_halfsize.Y * 2.0F);
        }

        public void SetLocation(Vector position, float angle)
        {
            _position = position;
            _angle = angle;
        }

        public Vector Position
        {
            get
            {
                return _position;
            }
        }

        public void AddForce(Vector worldForce, Vector worldOffset)
        {
            // Add the linear force.
            _forces += worldForce;

            // todo: Add its associated torque.
            _torque += worldOffset % worldForce;
        }

        public virtual void Update(float timeStep)
        {
            // Linear physics.
            Vector acceleration = _forces / _mass; // A = F / M
            _velocity += acceleration * timeStep; // V = V + A * T
            _position += _velocity * timeStep; // P = P + V * T

            // Clear the forces.
            _forces = new Vector(0, 0);

            // Angular physics.
            float angAcceleration = _torque / _inertia;
            _angularVelocity = angAcceleration * timeStep;
            _angle += _angularVelocity * timeStep;

            // Clear the torque.
            _torque = 0;
        }

        public void Draw(Graphics graphics, Size bufferSize)
        {
            Matrix matrix = graphics.Transform;

            graphics.TranslateTransform(_position.X, _position.Y);
            graphics.RotateTransform(_angle / (float)Math.PI * 180.0F);

            try
            {
                graphics.DrawRectangle(new Pen(_color), _rect);

                graphics.DrawLine(new Pen(Color.Yellow), 1, 0, 1, 5);
            }
            catch (StackOverflowException)
            {
                // ...
            }

            graphics.Transform = matrix;
        }

        public Vector RelativeToWorld(Vector relative)
        {
            var matrix = new Matrix();
            var vectors = new PointF[1];

            vectors[0].X = relative.X;
            vectors[0].Y = relative.Y;

            matrix.Rotate(_angle / (float)Math.PI * 180.0F);
            matrix.TransformVectors(vectors);

            return new Vector(vectors[0].X, vectors[0].Y);
        }

        public Vector WorldToRelative(Vector relative)
        {
            var matrix = new Matrix();
            var vectors = new PointF[1];

            vectors[0].X = relative.X;
            vectors[0].Y = relative.Y;

            matrix.Rotate(-_angle / (float)Math.PI * 180.0F);
            matrix.TransformVectors(vectors);

            return new Vector(vectors[0].X, vectors[0].Y);
        }

        public Vector PointVelocity(Vector worldOffset)
        {
            var tangent = new Vector(-worldOffset.Y, worldOffset.X);
            return tangent * _angularVelocity + _velocity;
        }
    }
}
