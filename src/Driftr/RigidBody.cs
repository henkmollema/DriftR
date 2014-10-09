using System;
using System.Windows;
using System.Drawing;

namespace Driftr
{
    public class RigidBody
    {
        // Linear properties.
        private Vector _position = new Vector();
        // P
        private Vector _velocity = new Vector();
        // A
        private Vector _forces = new Vector();
        // F
        private float _mass;
        // M

        // Angular properties
        private float _angle;
        private float _angularVelocity;
        private float _torque;
        private float _inertia;

        // Graphical properties.
        private Vector _halfsize = new Vector();
        private Rectangle _rect = new Rectangle();
        private Color _color;

        public RigidBody()
        {
            // Set defaults to prevent divide by zero.
            _mass = 1.0F;
            _inertia = 1.0F;
        }

        public void Setup(Vector halfsize, float mass, Color color)
        {
            _halfsize = halfsize;
            _mass = mass;
            _color = color;

            _inertia = (float)((1.0F / 12.0F)
            * (halfsize.X * halfsize.X)
            * (halfsize.Y * halfsize.Y)
            * mass);

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

        public void AddForce(Vector worldForce, Vector worldOffset)
        {
            // Add the linear force.
            _forces += worldForce;

            // todo: Add its associated torque.
            //_torque += worldOffset % worldForce;
        }
    }
}

