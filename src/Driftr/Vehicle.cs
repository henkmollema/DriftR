using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Driftr
{
    public class Vehicle : RigidBody
    {
        private Wheel[] _wheels = new Wheel[4];

        public override void Setup(Vector halfsize, float mass, Color color)
        {
            // Front wheels.
            _wheels[0] = new Wheel(new Vector(halfsize.X, halfsize.Y), 0.5F);
            _wheels[1] = new Wheel(new Vector(-halfsize.X, halfsize.Y), 0.5F);

            // Rear wheels.
            _wheels[2] = new Wheel(new Vector(halfsize.X, -halfsize.Y), 0.5F);
            _wheels[3] = new Wheel(new Vector(-halfsize.X, halfsize.Y), 0.5F);

            base.Setup(halfsize, mass, color);
        }

        public void SetSteering(float steering)
        {
            _wheels[0].SetSteeringAngle(-steering * 0.75F);
            _wheels[1].SetSteeringAngle(-steering * 0.75F);
        }

        public void SetThrottle(float throttle, bool allWheels)
        {
            const float torque = 20.0F;

            if (allWheels)
            {
                _wheels[0].AddTransmissionTorque(throttle * torque);
                _wheels[1].AddTransmissionTorque(throttle * torque);
            }

            _wheels[2].AddTransmissionTorque(throttle * torque);
            _wheels[3].AddTransmissionTorque(throttle * torque);
        }

        private class Wheel
        {
            private Vector _forwardAxis, _sideAxis;
            private float _wheelTorque, _wheelSpeed, _wheelInertia, _wheelRadius;
            private Vector _position = new Vector();

            public Wheel(Vector position, float radius)
            {
                _position = position;
                _wheelSpeed = 0;
                _wheelRadius = radius;
                _wheelInertia = radius * radius; // Fake value.
            }

            public void SetSteeringAngle(float angle)
            {
                var matrix = new Matrix();
                var vectors = new PointF[2];

                // Forward vectors.
                vectors[0].X = 0;
                vectors[0].Y = 1;

                // Side vectors.
                vectors[1].X = -1;
                vectors[1].Y = 0;

                matrix.Rotate(angle / (float)Math.PI * 180.0F);
                matrix.TransformVectors(vectors);

                _forwardAxis = new Vector(vectors[0].X, vectors[1].Y);
                _sideAxis = new Vector(vectors[1].X, vectors[1].Y);
            }

            public void AddTransmissionTorque(float torque)
            {
                _wheelTorque += torque;
            }

            public float WheelSpeed
            {
                get
                {
                    return _wheelSpeed;
                }
            }

            public Vector AttachPoint
            {
                get
                {
                    return _position;
                }
            }

            public Vector CalculateForce(Vector relativeGroundSpeed, float timeStep)
            {
                Vector patchSpeed = -_forwardAxis * _wheelSpeed * _wheelRadius;

                Vector velocityDifference = relativeGroundSpeed + patchSpeed;

                float forwardMag;
                Vector sideVelocity = velocityDifference.Project(_sideAxis);
                Vector forwardVelocity = velocityDifference.Project(_forwardAxis, out forwardMag);

                Vector responseForce = -sideVelocity * 2.0F;
                responseForce -= forwardVelocity;

                _wheelTorque += forwardMag * _wheelRadius;

                _wheelSpeed += _wheelTorque / _wheelInertia * timeStep;

                _wheelTorque = 0;

                return responseForce;
            }
        }
    }
}