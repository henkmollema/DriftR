﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Driftr
{
    public class Vehicle : RigidBody
    {
        public Wheel[] _wheels = new Wheel[4];

        public Wheel[] Wheels
        {
            get
            {
                return _wheels;
            }
        }

        public override void Setup(Vector halfsize, float mass, Color color)
        {
            // Front wheels.
            _wheels[0] = new Wheel(new Vector(halfsize.X, halfsize.Y), 0.5f);
            _wheels[1] = new Wheel(new Vector(-halfsize.X, halfsize.Y), 0.5f);

            // Rear wheels.
            _wheels[2] = new Wheel(new Vector(halfsize.X, -halfsize.Y), 0.5f);
            _wheels[3] = new Wheel(new Vector(-halfsize.X, -halfsize.Y), 0.5f);

            base.Setup(halfsize, mass, color);
        }

        public void SetSteering(float steering)
        {
            const float steeringLock = 0.75f;

            // Apply the steering angle to the front wheels.
            _wheels[0].SetSteeringAngle(-steering * steeringLock);
            _wheels[1].SetSteeringAngle(-steering * steeringLock);
        }

        public void SetThrottle(float throttle)
        {
            const float torque = 20.0f;

            // Apply transmission torque on rear wheels.
            _wheels[2].AddTransmissionTorque(throttle * torque);
            _wheels[3].AddTransmissionTorque(throttle * torque);
        }

        public void SetBrakes(float brakes)
        {
            const float brakeTorque = 10.0f;

            // Apply the brake torque on the wheel velocity.
            foreach (var wheel in _wheels)
            {
                wheel.AddTransmissionTorque(-wheel.WheelSpeed * brakeTorque * brakes);
            }
        }

        public override void Update(float timeStep)
        {
            foreach (var wheel in _wheels)
            {
                Vector worldWheelOffset = RelativeToWorld(wheel.AttachPoint);
                Vector worldGroundVelocity = PointVelocity(worldWheelOffset);
                Vector relativeGroundSpeed = WorldToRelative(worldGroundVelocity);
                Vector relativeResponseForce = wheel.CalculateForce(relativeGroundSpeed, timeStep);
                Vector worldResponseForce = RelativeToWorld(relativeResponseForce);
                
                AddForce(worldResponseForce, worldWheelOffset);
            }

            base.Update(timeStep);
        }

        public class Wheel
        {
            private Vector _forwardAxis, _sideAxis;
            private float _wheelTorque, _wheelSpeed, _wheelInertia, _wheelRadius;
            private readonly Vector _position = new Vector();

            public Wheel(Vector position, float radius)
            {
                _position = position;
                SetSteeringAngle(0);
                _wheelSpeed = 0;
                _wheelRadius = radius;
                _wheelInertia = radius * radius; // Fake value.
            }

            public void SetSteeringAngle(float angle)
            {
                var matrix = new Matrix();
                var vectors = new PointF[2];

                // Forward vector.
                vectors[0].X = 0;
                vectors[0].Y = 1;

                // Side vector.
                vectors[1].X = -1;
                vectors[1].Y = 0;

                matrix.Rotate(angle / (float)Math.PI * 180.0f);
                matrix.TransformVectors(vectors);

                _forwardAxis = new Vector(vectors[0].X, vectors[0].Y);
                _sideAxis = new Vector(vectors[1].X, vectors[1].Y);
            }

            public void AddTransmissionTorque(float torque)
            {
                _wheelTorque += torque * 2;
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
                // Calculate the speed of the tire patch at ground.
                Vector patchSpeed = -_forwardAxis * _wheelSpeed * _wheelRadius;

                // Get velocity difference between ground and patch.
                Vector velocityDifference = relativeGroundSpeed + patchSpeed;

                // Calculate ground speed onto side axis.
                float forwardMag;
                Vector sideVelocity = velocityDifference.Project(_sideAxis) * 3;
                Vector forwardVelocity = velocityDifference.Project(_forwardAxis, out forwardMag) * 2;

                // Calculate the response force.
                Vector responseForce = -sideVelocity * 2.0f;
                responseForce -= forwardVelocity;

                // Calculate torque on wheel.
                _wheelTorque += forwardMag * _wheelRadius;

                // Calculate total torque into wheel.
                _wheelSpeed += _wheelTorque / _wheelInertia * timeStep;

                // Clear transmission torque accumulator.
                _wheelTorque = 0;

                // Return the force acting on the body.
                return responseForce;
            }
        }
    }
}