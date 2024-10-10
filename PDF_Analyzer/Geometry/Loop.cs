using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_Analyzer.Geometry
{
    public class Loop
    {
        public string Id { get; set; }
        public List<Line> Lines { get; set; }
        public Loop(List<Line> lines)
        {

            List<Line> combinedLines = Rectangle.CombineExtensionLines(lines);
            Lines = combinedLines;






            Guid randomGuid = Guid.NewGuid();
            Id = randomGuid.ToString(); 
        }

        public List<Rectangle> GenerateFramework(double lumberThickness)
        {
            double modifiedThickness = /*insideFrame ? -Math.Abs(lumberThickness) :*/ Math.Abs(lumberThickness);

            List<Rectangle> frames = new List<Rectangle>();

            for (int i = 0; i < Lines.Count; i++)
            {
                var current = Lines[i];

                var A =   Math.Abs(Lines[i].Start.X - Lines[i].End.X);
                var B = Math.Abs(Lines[i].Start.Y - Lines[i].End.Y);

                if (current.IsInternal)
                {
                    //INTERNAL
                    var prev = i == 0 ? Lines[Lines.Count - 1] : Lines[i-1];
                    var next = i == Lines.Count-1? Lines[0] : Lines[i+1];


                    //If clockwise arrangement
                    if (current.IsVertical)
                    {
                        bool up = current.Start.Y >= current.End.Y;

                        if (up)
                        {
                            Vector topLeft = new Vector(current.End.X - lumberThickness, current.End.Y);
                            Vector bottomRight = current.Start.Clone();
                            if (!prev.IsInternal)
                            {
                                bottomRight.Y += modifiedThickness;
                            }else if (!next.IsInternal)
                            {
                                topLeft.Y -= modifiedThickness;
                            }
                            frames.Add(new Rectangle(topLeft, bottomRight));//OK
                        }
                        else
                        {
                            Vector topLeft = current.Start.Clone();
                            Vector bottomRight = new Vector(current.End.X + lumberThickness, current.End.Y);
                            if (!prev.IsInternal)
                            {
                                topLeft.Y -= modifiedThickness;
                            }
                            else if (!next.IsInternal)
                            {
                                bottomRight.Y += modifiedThickness;
                            }
                            frames.Add(new Rectangle(topLeft, bottomRight));//OK
                        }
                    }
                    else if (current.IsHorizontal) 
                    {
                        bool right = current.Start.X <= current.End.X;

                        if (right)
                        {
                            Vector topLeft = new Vector(current.Start.X + modifiedThickness, current.Start.Y - lumberThickness);
                            Vector bottomRight = new Vector(current.End.X - modifiedThickness, current.End.Y );
                            if (!prev.IsInternal)
                            {
                                topLeft.X -= modifiedThickness;
                            }
                            else if (!next.IsInternal)
                            {
                                bottomRight.X += modifiedThickness;
                            }
                            frames.Add(new Rectangle(topLeft, bottomRight));//OK
                        }
                        else
                        {
                            Vector topLeft = new Vector(current.End.X + modifiedThickness, current.End.Y );
                            Vector bottomRight = new Vector(current.Start.X - modifiedThickness, current.Start.Y + lumberThickness);
                            if (!prev.IsInternal)
                            {
                                bottomRight.X += modifiedThickness;
                            }
                            else if (!next.IsInternal)
                            {
                                topLeft.X -= modifiedThickness;
                            }

                            frames.Add(new Rectangle(topLeft, bottomRight));//OK
                        }
                    }


                }
                else
                {
                    //EXTERNAL
                    //If clockwise arrangement
                    if (current.IsVertical)
                    {
                        bool up = current.Start.Y >= current.End.Y;

                        if (up)
                        {
                            //frames.Add(new Rectangle(new Vector(,), new Vector(,)));
                            frames.Add(new Rectangle(new Vector(current.End.X - lumberThickness, current.End.Y - modifiedThickness), new Vector(current.Start.X, current.Start.Y + modifiedThickness)));
                        }
                        else
                        {
                            frames.Add(new Rectangle(new Vector(current.Start.X, current.Start.Y - modifiedThickness), new Vector(current.End.X + lumberThickness, current.End.Y + modifiedThickness)));
                        }

                    }
                    else if (current.IsHorizontal)
                    {
                        bool right = current.Start.X <= current.End.X;

                        if (right)
                        {
                            frames.Add(new Rectangle(new Vector(current.Start.X, current.Start.Y - lumberThickness), current.End));
                        }
                        else
                        {
                            frames.Add(new Rectangle(current.End, new Vector(current.Start.X, current.Start.Y + lumberThickness)));
                        }
                    }

                }

            }


            ////1
            //frames.Add(new Rectangle(new Vector(rectangleLines[0].Start.X + modifiedThickness, rectangleLines[0].Start.Y), new Vector(rectangleLines[0].End.X - modifiedThickness, rectangleLines[0].End.Y + lumberThickness)));

            ////2
            //frames.Add(new Rectangle(new Vector(rectangleLines[1].Start.X - lumberThickness, rectangleLines[1].Start.Y), new Vector(rectangleLines[1].End.X, rectangleLines[1].End.Y)));

            ////3
            //frames.Add(new Rectangle(new Vector(rectangleLines[2].End.X + modifiedThickness, rectangleLines[2].End.Y - lumberThickness), new Vector(rectangleLines[2].Start.X - modifiedThickness, rectangleLines[2].Start.Y)));

            ////4
            //frames.Add(new Rectangle(new Vector(rectangleLines[3].End.X, rectangleLines[3].End.Y), new Vector(rectangleLines[3].Start.X + lumberThickness, rectangleLines[3].Start.Y)));


            ////1
            //frames.Add(new Rectangle(new Vector(Lines[0].Start.X, Lines[0].Start.Y - lumberThickness), Lines[0].End));

            ////2
            //frames.Add(new Rectangle(new Vector(Lines[1].Start.X, Lines[1].Start.Y - modifiedThickness), new Vector(Lines[1].End.X + lumberThickness, Lines[1].End.Y + modifiedThickness)));

            ////3
            //frames.Add(new Rectangle(Lines[2].End, new Vector(Lines[2].Start.X, Lines[2].Start.Y + lumberThickness)));

            ////4
            //frames.Add(new Rectangle(new Vector(Lines[3].End.X - lumberThickness, Lines[3].End.Y - modifiedThickness), new Vector(Lines[3].Start.X, Lines[3].Start.Y + modifiedThickness)));

            return frames;
        }
    }
}
