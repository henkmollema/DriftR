using System;

namespace Driftr
{
    /// <summary>
    /// Represents a 2 dimensional vector.
    /// </summary>
    public class Vector
    {
        public float X, Y;

        public Vector()
        {
            X = 0;
            Y = 0;
        }

        public Vector(float x, float y)
        {
            X = x;
            Y = y;
        }
     
        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        public float Length
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y);
            }
        }

        public static Vector operator +(Vector left, Vector right)
        {
            return new Vector(left.X + right.X, left.Y + right.Y);
        }

        public static Vector operator -(Vector left, Vector right)
        {
            return new Vector(left.X - right.X, left.Y - right.Y);
        }

        public static Vector operator -(Vector vector)
        {
            return new Vector(-vector.X, -vector.Y);
        }

        public static Vector operator *(Vector left, float right)
        {
            return new Vector(left.X * right, left.Y * right);
        }

        public static Vector operator /(Vector left, float right)
        {
            return new Vector(left.X / right, left.Y / right);
        }

        public static float operator *(Vector left, Vector right)
        {
            return left.X * right.X + left.Y * right.Y;
        }

        public static float operator %(Vector left, Vector right)
        {
            // Cross product of the vectors.
            return left.X * right.Y - left.Y * right.X;
        }

        /// <summary>
        /// Normalize the X and Y components using the length of the vector. 
        /// </summary>
        public void Normalize()
        {
            float mag = Length;

            X /= mag;
            Y /= mag;
        }

        /// <summary>
        /// Projects this vector onto vector <paramref name="v"/>.
        /// </summary>
        /// <param name="v">The vector to project onto.</param>
        /// <returns>A projected vector.</returns>
        public Vector Project(Vector v)
        {
            // Projected vector = (this dot v) * v
            float thisDotV = this * v;
            return v * thisDotV;
        }

        /// <summary>
        /// Projects this vector onto vector <paramref name="v"/> and return the signed magnitude.
        /// </summary>
        /// <param name="v">The vector to project onto.</param>
        /// <param name="mag">The signed magnitude of the vector.</param>
        /// <returns>A projectec vector.</returns>
        public Vector Project(Vector v, out float mag)
        {
            float thisDotV = this * v;
            mag = thisDotV;
            return v * thisDotV;
        }
    }
}
