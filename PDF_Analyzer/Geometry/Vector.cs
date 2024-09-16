using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_Analyzer.Geometry
{
    public class Vector
    {
        /// <summary>
        /// Represents the X-coordinate of the vector.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Represents the Y-coordinate of the vector.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Represents the Z-coordinate of the vector.
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// Calculates and returns the length of the vector.
        /// </summary>
        public double vectorLength
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        /// <summary>
        /// Initializes a new instance of the Vector class with default values (0, 0, 0).
        /// </summary>
        public Vector()
        {
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }

        /// <summary>
        /// Initializes a new instance of the Vector class with specified X and Y coordinates.
        /// </summary>
        /// <param name="x">The X-coordinate of the vector.</param>
        /// <param name="y">The Y-coordinate of the vector.</param>
        public Vector(double x, double y)
        {
            this.X = x;
            this.Y = y;
            this.Z = 0;
        }

        /// <summary>
        /// Initializes a new instance of the Vector class with specified X, Y, and Z coordinates.
        /// </summary>
        /// <param name="x">The X-coordinate of the vector.</param>
        /// <param name="y">The Y-coordinate of the vector.</param>
        /// <param name="z">The Z-coordinate of the vector.</param>
        public Vector(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Returns a normalized version of the current vector.
        /// </summary>
        /// <returns>A new Vector object representing the normalized vector.</returns>
        public Vector Normalize()
        {
            double x = 0;
            double y = 0;
            double z = 0;

            if (vectorLength != 0)
            {
                x = X / vectorLength;
                y = Y / vectorLength;
                z = Z / vectorLength;
            }

            return new Vector(x, y, z);
        }

        /// <summary>
        /// Calculates the distance between the current vector and another point.
        /// </summary>
        /// <param name="point">The other point to calculate the distance to.</param>
        /// <returns>The distance between the two points.</returns>
        public double DistanceTo(Vector point)
        {
            double dx = point.X - this.X;
            double dy = point.Y - this.Y;
            double dz = point.Z - this.Z;

            return Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2) + Math.Pow(dz, 2));
        }

        /// <summary>
        /// Returns a point located at a specified distance along the line between the current vector and another point.
        /// </summary>
        /// <param name="point">The other point to calculate the point between.</param>
        /// <param name="distance">The distance from the current vector to the desired point.</param>
        /// <returns>A new Vector object representing the point at the specified distance.</returns>
        public Vector GetPointAtDistanceBetween(Vector point, double distance)
        {
            double d = this.DistanceTo(point);
            double ratio = distance / d;
            double newX = this.X + ratio * (point.X - this.X);
            double newY = this.Y + ratio * (point.Y - this.Y);
            double newZ = this.Z + ratio * (point.Z - this.Z);

            return new Vector(newX, newY, newZ);
        }

        /// <summary>
        /// Returns the midpoint between the current vector and another point.
        /// </summary>
        /// <param name="point">The other point to calculate the midpoint with.</param>
        /// <returns>A new Vector object representing the midpoint.</returns>
        public Vector GetMidPointBetween(Vector point)
        {
            double distance = this.DistanceTo(point) / 2;
            return GetPointAtDistanceBetween(point, distance);
        }
        public static double DotProduct(Vector v1, Vector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public bool Equals(Vector other, double tolerance = 0.0001)
        {
            if (other == null)
            {
                return false;
            }

            return Math.Abs(this.X - other.X) <= tolerance &&
                   Math.Abs(this.Y - other.Y) <= tolerance &&
                   Math.Abs(this.Z - other.Z) <= tolerance;
        }

        public Vector Clone() 
        {
            return new Vector(X, Y, Z);
        }
        /// <summary>
        /// Returns a string representation of the vector in the format "(X, Y, Z)".
        /// </summary>
        /// <returns>A string representation of the vector.</returns>
        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }
}
