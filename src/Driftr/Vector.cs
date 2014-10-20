using System;

namespace Driftr
{
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
     
        public float Length
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y);
            }
        }

        public static Vector operator +(Vector L, Vector R)
        {
            return new Vector(L.X + R.X, L.Y + R.Y);
        }

        public static Vector operator -(Vector L, Vector R)
        {
            return new Vector(L.X - R.X, L.Y - R.Y);
        }

        public static Vector operator -(Vector R)
        {
            return new Vector(-R.X, -R.Y);
        }

        public static Vector operator *(Vector L, float R)
        {
            return new Vector(L.X * R, L.Y * R);
        }

        public static Vector operator /(Vector L, float R)
        {
            return new Vector(L.X / R, L.Y / R);
        }

        public static float operator *(Vector L, Vector R)
        {
            return (L.X * R.X + L.Y * R.Y);
        }

        //cross product, in 2d this is a scalar since
        //we know it points in the Z direction
        public static float operator %(Vector L, Vector R)
        {
            return (L.X * R.Y - L.Y * R.X);
        }

        //normalize the vector
        public void normalize()
        {
            float mag = Length;

            X /= mag;
            Y /= mag;
        }

        //project this vector on to v
        public Vector Project(Vector v)
        {
            //projected vector = (this dot v) * v;
            float thisDotV = this * v;
            return v * thisDotV;
        }

        //project this vector on to v, return signed magnitude
        public Vector Project(Vector v, out float mag)
        {
            //projected vector = (this dot v) * v;
            float thisDotV = this * v;
            mag = thisDotV;
            return v * thisDotV;
        }
    }
}
