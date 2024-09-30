using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_Analyzer.Geometry
{
    public class Rectangle
    {
        public Vector TopLeft { get; set; }
        public Vector BottomRight { get; set; }
        public Vector Center { get { return new Vector((TopLeft.X + BottomRight.X) / 2, (TopLeft.Y + BottomRight.Y) / 2, (TopLeft.Z + BottomRight.Z) / 2); } }
        public double Area { get { return Math.Abs((BottomRight.X - TopLeft.X) * (TopLeft.Y - BottomRight.Y)); } }

        //return new Vector((TopLeft.X + BottomRight.X) / 2, (TopLeft.Y + BottomRight.Y) / 2);

        // Constructor to initialize a rectangle with top-left and bottom-right corners
        public Rectangle(Vector topLeft, Vector bottomRight)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        // Constructor to initialize a rectangle with four lines, handling any starting line
        public Rectangle(Line line1, Line line2, Line line3, Line line4)
        {
            // Create a list of lines to make it easier to find intersections
            List<Line> lines = new List<Line> { line1, line2, line3, line4 };

            // Find the intersection points for all possible combinations of lines
            Vector topLeft = null;
            Vector topRight = null;
            Vector bottomRight = null;
            Vector bottomLeft = null;

            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    Vector intersection = lines[i].IntersectionPoint(lines[j]);
                    if (intersection != null)
                    {
                        // Determine the corner based on the line indices
                        if (i == 0 && j == 1 || i == 1 && j == 0)
                        {
                            topLeft = intersection;
                        }
                        else if (i == 0 && j == 2 || i == 2 && j == 0)
                        {
                            topRight = intersection;
                        }
                        else if (i == 0 && j == 3 || i == 3 && j == 0)
                        {
                            bottomRight = intersection;
                        }
                        else if (i == 1 && j == 2 || i == 2 && j == 1)
                        {
                            bottomLeft = intersection;
                        }
                    }
                }
            }

            // Validate that all corners were found
            if (topLeft == null || topRight == null || bottomRight == null || bottomLeft == null)
            {
                throw new ArgumentException("Invalid lines provided. The lines must intersect to form a valid rectangle.");
            }

            // Assign the corners to the rectangle
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        //// Constructor to initialize a rectangle with four lines
        //public Rectangle(Line topLine, Line rightLine, Line bottomLine, Line leftLine)
        //{
        //    // Find the intersection points of the lines to determine the corners
        //    TopLeft = topLine.IntersectionPoint(leftLine);
        //    //TopRight = topLine.IntersectionPoint(rightLine);
        //    BottomRight = bottomLine.IntersectionPoint(rightLine);
        //    //BottomLeft = bottomLine.IntersectionPoint(leftLine);

        //    // Validate that the corners are valid (not null)
        //    if (TopLeft == null || TopRight == null || BottomRight == null || BottomLeft == null)
        //    {
        //        throw new ArgumentException("Invalid lines provided. The lines must intersect to form a valid rectangle.");
        //    }
        //}

        // Method to check if a list of lines forms a rectangle and return the rectangle
        public static Rectangle FromLines(List<Line> lines)
        {
            if (lines.Count < 4)
            {
                //throw new ArgumentException("At least 4 lines are required to form a rectangle.");
                return null;
            }
            if (!IsClosedLoop(lines))
            {
                //}} return null;
            }

            // Combine extended lines
            List<Line> combinedLines = CombineExtensionLines(lines);

            // Find all intersection points
            List<Vector> intersectionPoints = new List<Vector>();
            for (int i = 0; i < combinedLines.Count; i++)
            {
                for (int j = i + 1; j < combinedLines.Count; j++)
                {
                    Vector intersection = combinedLines[i].IntersectionPoint(combinedLines[j]);
                    if (intersection != null)
                    {
                        intersectionPoints.Add(intersection);
                    }
                }
            }

            // Check if there are exactly 4 intersection points
            if (intersectionPoints.Count != 4)
            {
                //}} return null; // Not a rectangle
            }
            //}}//}}
            intersectionPoints = new List<Vector>() { lines[0].Start, lines[0].End, lines[1].Start, lines[1].End, lines[2].Start, lines[2].End, lines[3].Start, lines[3].End };
            //}}//}}

            // Find the top-left and bottom-right corners
            Vector topLeft = intersectionPoints.OrderBy(p => Math.Round(p.X, 4)).ThenBy(p => Math.Round(p.Y, 4)).First();
            Vector bottomRight = intersectionPoints.OrderByDescending(p => Math.Round(p.X, 4)).ThenByDescending(p => Math.Round(p.Y, 4)).First();

            //// Check if the lines form a rectangle (opposite sides are parallel and equal length)
            //if (!IsRectangleShape(combinedLines, topLeft, bottomRight))
            //{
            //    return null; // Not a rectangle
            //}
            // Check if the lines form a rectangle (opposite sides are parallel and equal length)
            if (!IsRectangleShape(combinedLines))
            {
                //}} return null; // Not a rectangle
            }


            // Create and return the rectangle
            return new Rectangle(topLeft, bottomRight);
        }


        /// <summary>
        /// Combines extension lines in a list of lines, handling circular cases.
        /// </summary>
        /// <param name="lines">The list of lines to check and combine.</param>
        /// <returns>A new list of lines with extension lines combined.</returns>
        public static List<Line> CombineExtensionLines(List<Line> lines)
        {
            List<Line> newLines = new List<Line>();
            newLines.Add(lines[0]);
            for (int i = 1; i < lines.Count; i++)
            {
                Line currentLine = lines[i];

                if (currentLine.IsExtensionTo(newLines[newLines.Count - 1]))
                {
                    newLines[newLines.Count - 1].End = currentLine.End;
                }
                else
                {
                    newLines.Add(lines[i]);
                }
            }

            if (newLines[0].IsExtensionTo(newLines[newLines.Count - 1]))
            {
                newLines[0].Start = newLines[newLines.Count - 1].Start;
                newLines.RemoveAt(newLines.Count - 1);
            }

            return newLines;
        }

        // Helper method to check if the lines form a rectangle shape [TODO]
        private static bool IsRectangleShape(List<Line> lines, Vector topLeft, Vector bottomRight)
        {
            bool A = IsClosedLoop(lines, topLeft, bottomRight);
            bool B = IsClosedLoop(lines);
            // Find the two lines that connect the top-left and bottom-right corners
            Line topLine = lines.FirstOrDefault(l => l.ContainsPoint(topLeft) && l.ContainsPoint(lines.FirstOrDefault(l2 => l2.ContainsPoint(topLeft)).Start));
            Line bottomLine = lines.FirstOrDefault(l => l.ContainsPoint(bottomRight) && l.ContainsPoint(lines.FirstOrDefault(l2 => l2.ContainsPoint(bottomRight)).End));

            // Find the two lines that connect the top-right and bottom-left corners
            Line rightLine = lines.FirstOrDefault(l => l.ContainsPoint(bottomRight) && l.ContainsPoint(lines.FirstOrDefault(l2 => l2.ContainsPoint(topLeft)).Start));
            Line leftLine = lines.FirstOrDefault(l => l.ContainsPoint(topLeft) && l.ContainsPoint(lines.FirstOrDefault(l2 => l2.ContainsPoint(bottomRight)).Start));

            // Check if opposite sides are parallel and equal length
            return topLine.IsParallelTo(bottomLine) && topLine.Length == bottomLine.Length &&
                   rightLine.IsParallelTo(leftLine) && rightLine.Length == leftLine.Length;
        }

        /// <summary>
        /// Checks if a list of lines forms a rectangle.
        /// </summary>
        /// <param name="lines">The list of lines to check.</param>
        /// <returns>True if the lines form a rectangle, False otherwise.</returns>
        public static bool IsRectangleShape(List<Line> lines)
        {
            // Check if the lines form a closed loop
            if (!IsClosedLoop(lines))
            {
                return false;
            }

            // Check if there are exactly four lines
            if (lines.Count != 4)
            {
                return false;
            }

            // Check if opposite sides are parallel and have equal lengths
            if (lines[0].IsParallelTo(lines[2]) && lines[1].IsParallelTo(lines[3]) )
               // &&
                //Math.Abs(lines[0].Length() - lines[2].Length()) < 0.1  && Math.Abs(lines[1].Length() - lines[3].Length()) < 0.1)
            {
                // Check if adjacent sides are perpendicular
                if (lines[0].IsPerpendicularTo(lines[1]) && lines[1].IsPerpendicularTo(lines[2]) &&
                    lines[2].IsPerpendicularTo(lines[3]) && lines[3].IsPerpendicularTo(lines[0]))
                {
                    return true;
                }
            }

            return false;
        }


        // Helper method to check if the lines form a closed loop [TODO]
        private static bool IsClosedLoop(List<Line> lines, Vector topLeft, Vector bottomRight)
        {
            // Check if all lines are connected and form a closed loop
            List<Line> connectedLines = new List<Line>();
            connectedLines.Add(lines.FirstOrDefault(l => l.ContainsPoint(topLeft) && l.ContainsPoint(lines.FirstOrDefault(l2 => l2.ContainsPoint(bottomRight)).End)));
            connectedLines.Add(lines.FirstOrDefault(l => l.ContainsPoint(bottomRight) && l.ContainsPoint(lines.FirstOrDefault(l2 => l2.ContainsPoint(topLeft)).End)));

            return connectedLines.Count == 2;
        }
        /// <summary>
        /// Checks if a list of lines forms a closed loop.
        /// </summary>
        /// <param name="lines">The list of lines to check.</param>
        /// <returns>True if the lines form a closed loop, False otherwise.</returns>
        public static bool IsClosedLoop(List<Line> lines)
        {
            // If there are less than 3 lines, it cannot be a closed loop
            if (lines.Count < 3)
            {
                return false;
            }

            // Check if the lines form a continuous chain
            for (int i = 0; i < lines.Count - 1; i++)
            {
                // Check if the end point of the current line matches the start point of the next line
                if (!lines[i].End.Equals(lines[i + 1].Start))
                {
                    return false;
                }
            }

            // Check if the end point of the last line matches the start point of the first line
            if (!lines[lines.Count - 1].End.Equals(lines[0].Start))
            {
                return false;
            }

            // If all conditions are met, the lines form a closed loop
            return true;
        }


        // Method to calculate the width of the rectangle
        public double Width()
        {
            return Math.Abs(BottomRight.X - TopLeft.X);
        }

        // Method to calculate the height of the rectangle
        public double Height()
        {
            return Math.Abs(TopLeft.Y - BottomRight.Y);
        }

        // Method to calculate the area of the rectangle
        public double GetArea()
        {
            return Width() * Height();
        }

        // Method to calculate the perimeter of the rectangle
        public double GetPerimeter()
        {
            return 2 * (Width() + Height());
        }

        // Method to check if a point lies inside the rectangle
        public bool ContainsPoint(Vector point)
        {
            return
                   point.X >= TopLeft.X
                && point.X <= BottomRight.X
                && point.Y >= TopLeft.Y
                && point.Y <= BottomRight.Y;
        }

        // Method to get the center point of the rectangle
        public Vector GetCenter()
        {
            return new Vector((TopLeft.X + BottomRight.X) / 2, (TopLeft.Y + BottomRight.Y) / 2);
        }

        // Method to get the top-right corner of the rectangle
        public Vector TopRight()
        {
            return new Vector(BottomRight.X, TopLeft.Y);
        }

        // Method to get the bottom-left corner of the rectangle
        public Vector BottomLeft()
        {
            return new Vector(TopLeft.X, BottomRight.Y);
        }

        // Method to get the four sides of the rectangle as Line objects
        public List<Line> GetSides()
        {
            List<Line> sides = new List<Line>();

            sides.Add(new Line(TopLeft, TopRight()));
            sides.Add(new Line(TopRight(), BottomRight));
            sides.Add(new Line(BottomRight, BottomLeft()));
            sides.Add(new Line(BottomLeft(), TopLeft));

            return sides;
        }
        public List<Line> GetHorizontalSides()
        {
            List<Line> sides = new List<Line>();

            sides.Add(new Line(TopLeft, TopRight()));
            //sides.Add(new Line(TopRight(), BottomRight));
            sides.Add(new Line(BottomRight, BottomLeft()));
            //sides.Add(new Line(BottomLeft(), TopLeft));

            return sides;
        }
        public List<Line> GetVerticalSides()
        {
            List<Line> sides = new List<Line>();

            //sides.Add(new Line(TopLeft, TopRight()));
            sides.Add(new Line(TopRight(), BottomRight));
            //sides.Add(new Line(BottomRight, BottomLeft()));
            sides.Add(new Line(BottomLeft(), TopLeft));

            return sides;
        }

        // Method to check if a line intersects the rectangle
        public bool IntersectsLine(Line line)
        {
            // Check if any of the rectangle's sides intersect the line
            foreach (Line side in GetSides())
            {
                if (side.IntersectionPoint(line) != null)
                {
                    return true;
                }
            }

            return false;
        }

        // Method to check if the rectangle intersects another rectangle
        public bool IntersectsRectangle(Rectangle otherRectangle)
        {
            // Check if any of the rectangle's sides intersect the other rectangle
            foreach (Line side in GetSides())
            {
                if (otherRectangle.IntersectsLine(side))
                {
                    return true;
                }
            }

            return false;
        }

        // Method to check if the rectangle is completely contained within another rectangle
        public bool IsContainedWithin(Rectangle otherRectangle)
        {
            return ContainsPoint(otherRectangle.TopLeft) && ContainsPoint(otherRectangle.BottomRight);
        }


        public bool HasDiagonalLineFrom(List<Line> lines) // [X] => true // [ ] => false
        {
            foreach (Line line in lines)
            {
                if (line.Start.Equals(this.TopLeft) && line.End.Equals(this.BottomRight) || line.End.Equals(this.TopLeft) && line.Start.Equals(this.BottomRight)) { return true; }
            }
            return false;
        }

        //public List<Rectangle> GenerateFramework(double lumberThickness, bool insideFrame = true)
        //{
        //    double modifiedThickness = /*insideFrame ? -Math.Abs(lumberThickness) :*/ Math.Abs(lumberThickness);

        //    List<Line> rectangleLines = GetSides();
        //    List<Rectangle> frames = new List<Rectangle>();

        //    if (insideFrame)
        //    {
        //        //1
        //        frames.Add(new Rectangle(rectangleLines[0].Start, new Vector(rectangleLines[0].End.X - modifiedThickness, rectangleLines[0].End.Y + lumberThickness)));

        //        //2
        //        frames.Add(new Rectangle(new Vector(rectangleLines[1].Start.X - lumberThickness, rectangleLines[1].Start.Y), new Vector(rectangleLines[1].End.X, rectangleLines[1].End.Y - modifiedThickness)));

        //        //3
        //        frames.Add(new Rectangle(new Vector(rectangleLines[2].End.X + modifiedThickness, rectangleLines[2].End.Y - lumberThickness), rectangleLines[2].Start));

        //        //4
        //        frames.Add(new Rectangle(new Vector(rectangleLines[3].End.X, rectangleLines[3].End.Y + modifiedThickness), new Vector(rectangleLines[3].Start.X + lumberThickness, rectangleLines[3].Start.Y)));
        //    }
        //    else
        //    {
        //        //1
        //        frames.Add(new Rectangle(new Vector(rectangleLines[0].Start.X, rectangleLines[0].Start.Y - lumberThickness), new Vector(rectangleLines[0].End.X + modifiedThickness, rectangleLines[0].End.Y)));

        //        //2
        //        frames.Add(new Rectangle(rectangleLines[1].Start, new Vector(rectangleLines[1].End.X + lumberThickness, rectangleLines[1].End.Y + modifiedThickness)));

        //        //3
        //        frames.Add(new Rectangle(new Vector(rectangleLines[2].End.X - modifiedThickness, rectangleLines[2].End.Y), new Vector(rectangleLines[2].Start.X, rectangleLines[2].Start.Y + lumberThickness)));

        //        //4
        //        frames.Add(new Rectangle(new Vector(rectangleLines[3].End.X - lumberThickness, rectangleLines[3].End.Y - modifiedThickness), rectangleLines[3].Start));
        //    }
        //    return frames;
        //}
        public List<Rectangle> GenerateFramework(double lumberThickness, bool insideFrame = true)
        {
            double modifiedThickness = /*insideFrame ? -Math.Abs(lumberThickness) :*/ Math.Abs(lumberThickness);

            List<Line> rectangleLines = GetSides();

            List<Line> rectangleHLines = GetHorizontalSides();
            List<Line> rectangleVLines = GetVerticalSides();



            List<Rectangle> frames = new List<Rectangle>();

            if (insideFrame)
            {
                //1
                frames.Add(new Rectangle(new Vector(rectangleLines[0].Start.X + modifiedThickness, rectangleLines[0].Start.Y) , new Vector(rectangleLines[0].End.X - modifiedThickness, rectangleLines[0].End.Y + lumberThickness)));

                //2
                frames.Add(new Rectangle(new Vector(rectangleLines[1].Start.X - lumberThickness, rectangleLines[1].Start.Y), new Vector(rectangleLines[1].End.X, rectangleLines[1].End.Y)));

                //3
                frames.Add(new Rectangle(new Vector(rectangleLines[2].End.X + modifiedThickness, rectangleLines[2].End.Y - lumberThickness), new Vector(rectangleLines[2].Start.X - modifiedThickness, rectangleLines[2].Start.Y)));

                //4
                frames.Add(new Rectangle(new Vector(rectangleLines[3].End.X, rectangleLines[3].End.Y), new Vector(rectangleLines[3].Start.X + lumberThickness, rectangleLines[3].Start.Y)));
            }
            else
            {
                //1
                frames.Add(new Rectangle(new Vector(rectangleLines[0].Start.X, rectangleLines[0].Start.Y - lumberThickness), rectangleLines[0].End));

                //2
                frames.Add(new Rectangle(new Vector(rectangleLines[1].Start.X, rectangleLines[1].Start.Y - modifiedThickness), new Vector(rectangleLines[1].End.X + lumberThickness, rectangleLines[1].End.Y + modifiedThickness)));

                //3
                frames.Add(new Rectangle(rectangleLines[2].End, new Vector(rectangleLines[2].Start.X, rectangleLines[2].Start.Y + lumberThickness)));

                //4
                frames.Add(new Rectangle(new Vector(rectangleLines[3].End.X - lumberThickness, rectangleLines[3].End.Y - modifiedThickness), new Vector(rectangleLines[3].Start.X, rectangleLines[3].Start.Y + modifiedThickness)));
            }
            return frames;
        }


        public string ToSVGPath()
        {
            List<Line> rectangleLines = GetSides();
            return $"M {Math.Round(rectangleLines[0].Start.X, 2)} {Math.Round(rectangleLines[0].Start.Y, 2)} L {Math.Round(rectangleLines[0].End.X, 2)} {Math.Round(rectangleLines[0].End.Y, 2)} L {Math.Round(rectangleLines[1].End.X, 2)} {Math.Round(rectangleLines[1].End.Y, 2)} L {Math.Round(rectangleLines[2].End.X, 2)} {Math.Round(rectangleLines[2].End.Y, 2)} L {Math.Round(rectangleLines[3].End.X, 2)} {Math.Round(rectangleLines[3].End.Y, 2)} ";
        }

    }
}

