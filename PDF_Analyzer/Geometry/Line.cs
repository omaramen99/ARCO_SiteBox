using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_Analyzer.Geometry
{
    public class Line
    {
        public Vector Start { get; set; }
        public Vector End { get; set; }

        // Constructor to initialize a line with start and end points
        public Line(Vector start, Vector end)
        {
            Start = start;
            End = end;
        }

        // Method to calculate the length of the line
        public double Length()
        {
            return Start.DistanceTo(End);
        }

        // Method to get the midpoint of the line
        public Vector Midpoint()
        {
            return Start.GetMidPointBetween(End);
        }

        // Method to get a point on the line at a given distance from the start point
        public Vector GetPointAtDistance(double distance)
        {
            return Start.GetPointAtDistanceBetween(End, distance);
        }

        // Method to check if a point lies on the line
        public bool ContainsPoint(Vector point)
        {
            // Calculate the distance between the start point and the given point
            double distanceToStart = Start.DistanceTo(point);

            // Calculate the distance between the end point and the given point
            double distanceToEnd = End.DistanceTo(point);

            // Calculate the length of the line
            double lineLength = Length();

            // Check if the sum of the distances to the start and end points equals the length of the line
            return Math.Abs(distanceToStart + distanceToEnd - lineLength) < 0.0001; // Using a small tolerance for floating-point comparison
        }

        // Method to get the direction vector of the line
        public Vector DirectionVector()
        {
            return new Vector(End.X - Start.X, End.Y - Start.Y, End.Z - Start.Z);
        }

        // Method to get the normalized direction vector of the line
        public Vector NormalizedDirectionVector()
        {
            return DirectionVector().Normalize();
        }

        // Method to get the equation of the line in the form Ax + By + C = 0
        public Tuple<double, double, double> GetEquation()
        {
            // Calculate the direction vector
            Vector direction = DirectionVector();

            // Calculate the coefficients A, B, and C
            double A = direction.Y;
            double B = -direction.X;
            double C = -A * Start.X - B * Start.Y;

            return Tuple.Create(A, B, C);
        }

        // Method to calculate the intersection point of two lines
        public Vector IntersectionPoint(Line otherLine)
        {
            // Get the equations of the two lines
            Tuple<double, double, double> thisEquation = GetEquation();
            Tuple<double, double, double> otherEquation = otherLine.GetEquation();

            // Calculate the determinant of the system of equations
            double determinant = thisEquation.Item1 * otherEquation.Item2 - thisEquation.Item2 * otherEquation.Item1;
            ////determinant = Math.Abs(determinant);
            // If the determinant is zero, the lines are parallel or coincident
            if (Math.Abs(determinant) < 0.0001)
            {
                return null; // Return null to indicate no intersection
            }

            // Calculate the intersection point
            double x =- (thisEquation.Item3 * otherEquation.Item2 - thisEquation.Item2 * otherEquation.Item3) / determinant;
            double y =- (thisEquation.Item1 * otherEquation.Item3 - thisEquation.Item3 * otherEquation.Item1) / determinant;

            return new Vector(x, y);
        }

        // Method to check if two lines are parallel
        public bool IsParallelTo(Line otherLine)
        {
            // Get the direction vectors of the two lines
            Vector thisDirection = DirectionVector();
            Vector otherDirection = otherLine.DirectionVector();

            // Check if the cross product of the direction vectors is zero
            return Math.Abs(thisDirection.X * otherDirection.Y - thisDirection.Y * otherDirection.X) < 0.0001;
        }

        // Method to check if two lines are perpendicular
        public bool IsPerpendicularTo(Line otherLine)
        {
            // Get the direction vectors of the two lines
            Vector thisDirection = DirectionVector();
            Vector otherDirection = otherLine.DirectionVector();

            // Check if the dot product of the direction vectors is zero
            return Math.Abs(thisDirection.X * otherDirection.X + thisDirection.Y * otherDirection.Y) < 0.0001;
        }

        // Method to check if one line is an extension of another
        public bool IsExtensionTo(Line otherLine)
        {
            // Check if the lines are parallel
            if (!IsParallelTo(otherLine))
            {
                return false;
            }

            // Check if the end point of this line coincides with the start point of the other line
            if (End.X == otherLine.Start.X && End.Y == otherLine.Start.Y && End.Z == otherLine.Start.Z)
            {
                return true;
            }

            // Check if the start point of this line coincides with the end point of the other line
            if (Start.X == otherLine.End.X && Start.Y == otherLine.End.Y && Start.Z == otherLine.End.Z)
            {
                return true;
            }

            return false;
        }

        // Method to get the distance between a point and the line
        public double DistanceToPoint(Vector point)
        {
            // Get the direction vector of the line
            Vector direction = DirectionVector();

            // Calculate the vector from the start point to the given point
            Vector vectorToPoint = new Vector(point.X - Start.X, point.Y - Start.Y, point.Z - Start.Z);

            // Calculate the projection of the vectorToPoint onto the direction vector
            double projection = Vector.DotProduct(vectorToPoint, direction) / direction.vectorLength;

            // Calculate the vector from the start point to the projection point
            Vector projectionVector = new Vector(projection * direction.X, projection * direction.Y, projection * direction.Z);

            // Calculate the distance between the given point and the projection point
            return vectorToPoint.DistanceTo(projectionVector);
        }

        public static Rectangle GetBoundingBox(List<Line> lines) 
        {
            Vector top_left = new Vector();
            Vector bottom_right = new Vector();

            if (lines.Any())
            {
                top_left = lines[0].Start.Clone();
                bottom_right = lines[0].Start.Clone();

                foreach (Line line in lines) 
                {
                    Vector s = line.Start;
                    Vector e = line.End;

                    double min_x = s.X < e.X ? s.X : e.X;
                    double min_y = s.Y < e.Y ? s.Y : e.Y;

                    double max_x = s.X > e.X ? s.X : e.X;
                    double max_y = s.Y > e.Y ? s.Y : e.Y;



                    if (min_x < top_left.X) top_left.X = min_x;
                    if (min_y < top_left.Y) top_left.Y = min_y;

                    if (max_x > bottom_right.X) bottom_right.X = max_x;
                    if (max_y > bottom_right.Y) bottom_right.Y = max_y;

                }
            }



            return new Rectangle(top_left, bottom_right);
        }

        //public string ToSVGPath() 
        //{
        //    return "M ";
        //}
    }
}
