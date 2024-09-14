using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2S.Components.PDF4NET.Core;
using O2S.Components.PDF4NET;
using O2S.Components.PDF4NET.Content;
using O2S.Components.PDF4NET.Graphics;
using System.Globalization;
using PDF_Analyzer.Geometry;
using Rectangle = PDF_Analyzer.Geometry.Rectangle;
using System.IO;

using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PDF_Analyzer
{

    public class Core
    {
        [STAThread]
        public static void Run()
        {
            PDF4NET_Run();


        }
        public static void RR()
        {

            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                var Text = fileDialog.FileName;
            }
        }
        public static List<int> GetValidPages(PDFFixedDocument document)
        {
            List<int> pages = new List<int>();
            for (int i = 0; i < document.Pages.Count; i++)
            {
                PDFContentExtractor ce = new PDFContentExtractor(document.Pages[i]);
                PDFVisualObjectCollection pageVisualObjects = ce.ExtractVisualObjects(false);
                for (int j = 0; j < pageVisualObjects.Count; j++)
                {
                    var part = pageVisualObjects[j]; 
                    if (part.Type == PDFVisualObjectType.Text)
                    {
                        if ((part as PDFTextVisualObject).TextRun.Text.Contains("PANEL:"))
                        {
                            pages.Add(i);
                            break;
                        }
                    }
                }
            }
            return pages;
        }
        private static void PDF4NET_Run()
        {

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            PDFFixedDocument document = new PDFFixedDocument(appDirectory + "00. Panel Shops - Field Use.pdf");

            List<int> validPages = GetValidPages(document);

            foreach (int page in validPages)
            {
                PDFContentExtractor ce = new PDFContentExtractor(document.Pages[page]);
                PDFVisualObjectCollection pageVisualObjects = ce.ExtractVisualObjects(false);
                //List<Rectangle> rectangleList = new List<Rectangle>();
                List<PdfRectangleBlock> blocksList = new List<PdfRectangleBlock>();
                List<Line> linesList = new List<Line>();
                for (int i = 0; i < pageVisualObjects.Count; i++)
                {
                    switch (pageVisualObjects[i].Type)
                    {
                        case PDFVisualObjectType.Path:
                            PDFPathVisualObject pathVisualObject = pageVisualObjects[i] as PDFPathVisualObject;


                            if ((pathVisualObject.Pen != null) && pathVisualObject.Pen.Color.ColorSpace.Type != PDFColorSpaceType.Gray)
                            {

                                Rectangle rect = GetRectangleFromPath(pathVisualObject);
                                if (rect != null) continue;

                                linesList.AddRange(ToLines(pathVisualObject));

                            }
                            else if ((pathVisualObject.Pen != null) && pathVisualObject.Pen.Color.ColorSpace.Type == PDFColorSpaceType.Gray)
                            {

                                Rectangle rect = GetRectangleFromPath(pathVisualObject);
                                if (rect == null)
                                {
                                    linesList.AddRange(ToLines(pathVisualObject));
                                    continue;
                                }
                                //rectangleList.Add(rect);
                                blocksList.Add(new PdfRectangleBlock(rect, pathVisualObject));

                                ////Log SVG Path in console
                                //foreach (PDFPathItem pathItem in pathVisualObject.PathItems)
                                //{
                                //    //ToString(pathItem);
                                //    Console.WriteLine(PDFPathItemToString(pathItem));
                                //}

                            }

                            break;
                    }
                }
                //Analyze BlocksList
                //reorder the list ascending by rectangle area
                blocksList = blocksList.OrderByDescending/*.OrderBy*/(b => b.rectangle.Area).ToList();

                Rectangle largestRectangle = blocksList[0].rectangle;//sheet bounds// [TODO] get all rects containing BoundingBox instead
                double paddingPercentage = 0.1;
                double min_p = 1 - paddingPercentage;
                double max_p = 1 + paddingPercentage;
                Rectangle sheetLeftHalf = new Rectangle(new Vector(largestRectangle.TopLeft.X * max_p, largestRectangle.TopLeft.Y * max_p), new Vector(largestRectangle.Center.X * min_p, largestRectangle.BottomRight.Y * min_p));

                blocksList = blocksList.Where(b => sheetLeftHalf.IsContainedWithin(b.rectangle)).ToList();

                List<PdfRectangleBlock> blocks = new List<PdfRectangleBlock>();// { blocksList[0] };
                blocks = blocksList.Where(b => b.rectangle.HasDiagonalLineFrom(linesList)).ToList();
                blocks.Insert(0, blocksList[0]);
                //HasDiagonalLineFrom(List < Line > lines)


                StringBuilder wallPath = new StringBuilder();
                StringBuilder framesPath = new StringBuilder();

                foreach (PdfRectangleBlock block in blocks)
                {
                    var pathVisualObject = block.pdfPathObject;
                    foreach (PDFPathItem pathItem in pathVisualObject.PathItems)
                    {
                        //ToString(pathItem);
                        Console.WriteLine(PDFPathItemToString(pathItem));
                        wallPath.AppendLine(PDFPathItemToString(pathItem));
                    }
                }
                List<Rectangle> frames = new List<Rectangle>();
                for (int i = 0; i < blocks.Count; i++)
                {
                    if (i == 0)
                    {
                        frames.AddRange(blocks[0].rectangle.GenerateFramework(1, false));
                    }
                    else
                    {
                        frames.AddRange(blocks[i].rectangle.GenerateFramework(1));
                    }
                }

                foreach (Rectangle rect in frames)
                {
                    Console.WriteLine(rect.ToSVGPath());
                    framesPath.AppendLine(rect.ToSVGPath());
                }

                //generate HTML viewer

                // Specify the path to the file
                string filePath = appDirectory + "SVGViewerTemplate.html";

                // Read the contents of the file into a string
                string fileContents = File.ReadAllText(filePath);



                // Perform the replacement
                string modifiedHtml = fileContents.Replace("<WALLPATH>", wallPath.ToString());
                modifiedHtml = modifiedHtml.Replace("<FRAMESPATH>", framesPath.ToString());


                modifiedHtml = modifiedHtml.Replace("<CURRPAGE>", (page+1).ToString());
                if (validPages[validPages.Count - 1] == page)//last
                {
                    modifiedHtml = modifiedHtml.Replace("<NEXTPAGE>", (validPages[0] + 1).ToString());

                }
                else 
                {
                    modifiedHtml = modifiedHtml.Replace("<NEXTPAGE>", (validPages[validPages.IndexOf(page) + 1] + 1).ToString());
                }
                if (validPages[0] == page)//first
                {
                    modifiedHtml = modifiedHtml.Replace("<PREVPAGE>", (validPages[validPages.Count - 1] + 1).ToString());
                }
                else
                {
                    modifiedHtml = modifiedHtml.Replace("<PREVPAGE>", (validPages[validPages.IndexOf(page) - 1] + 1).ToString());
                }

                


                //<CURRPAGE>
                //<NEXTPAGE>
                //<PREVPAGE>


                Directory.CreateDirectory(appDirectory + "RESULT");
                string savefilePath = appDirectory + "RESULT" + $"\\Page_{page+1}.html";






                using (StreamWriter sw = File.CreateText(savefilePath))
                {
                    string content = modifiedHtml;
                    sw.Write(content);
                }

            }


        }
        public static string PDFPathItemToString(PDFPathItem pdfPathItem)
        {
            string str = (string)null;
            switch (pdfPathItem.Type)
            {
                case PDFPathItemType.MoveTo:
                    str = string.Format((IFormatProvider)CultureInfo.InvariantCulture, "M {0:0.######} {1:0.######}", (object)pdfPathItem.Points[0].X, (object)pdfPathItem.Points[0].Y);
                    break;
                case PDFPathItemType.LineTo:
                    str = string.Format((IFormatProvider)CultureInfo.InvariantCulture, "L {0:0.######} {1:0.######}", (object)pdfPathItem.Points[0].X, (object)pdfPathItem.Points[0].Y);
                    break;
                case PDFPathItemType.Rectangle:
                    str = string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{0:0.######} {1:0.######} {2:0.######} {3:0.######} re", (object)pdfPathItem.Points[0].X, (object)pdfPathItem.Points[0].Y, (object)(pdfPathItem.Points[1].X - pdfPathItem.Points[0].X), (object)(pdfPathItem.Points[1].Y - pdfPathItem.Points[0].Y));
                    break;
                case PDFPathItemType.CCurveTo:
                    str = string.Format((IFormatProvider)CultureInfo.InvariantCulture, "C {0:0.######} {1:0.######} {2:0.######} {3:0.######} {4:0.######} {5:0.######}", (object)pdfPathItem.Points[0].X, (object)pdfPathItem.Points[0].Y, (object)pdfPathItem.Points[1].X, (object)pdfPathItem.Points[1].Y, (object)pdfPathItem.Points[2].X, (object)pdfPathItem.Points[2].Y);
                    break;
                case PDFPathItemType.VCurveTo:
                    str = string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{0:0.######} {1:0.######} {2:0.######} {3:0.######} v", (object)pdfPathItem.Points[0].X, (object)pdfPathItem.Points[0].Y, (object)pdfPathItem.Points[1].X, (object)pdfPathItem.Points[1].Y);
                    break;
                case PDFPathItemType.YCurveTo:
                    str = string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{0:0.######} {1:0.######} {2:0.######} {3:0.######} y", (object)pdfPathItem.Points[0].X, (object)pdfPathItem.Points[0].Y, (object)pdfPathItem.Points[1].X, (object)pdfPathItem.Points[1].Y);
                    break;
                case PDFPathItemType.CloseSubpath:
                    str = "Z";//h
                    break;
            }
            return str;
        }
        public static Rectangle GetRectangleFromPath(PDFPathVisualObject pathVisualObject)
        {
            bool isRectangle = true;
            isRectangle = isRectangle && pathVisualObject.PathItems.Count >= 5;


            isRectangle = isRectangle && pathVisualObject.PathItems[0].Type == PDFPathItemType.MoveTo;
            Rectangle r = null;
            if (isRectangle)
            {
                List<Line> lines = new List<Line>();
                PDFPoint start = pathVisualObject.PathItems[0].Points[0];
                Vector sp = new Vector(start.X, start.Y);

                for (int i = 1; i < pathVisualObject.PathItems.Count; i++)
                {
                    if (pathVisualObject.PathItems[i].Type == PDFPathItemType.LineTo)
                    {
                        PDFPoint end = pathVisualObject.PathItems[i].Points[0];
                        Vector ep = new Vector(end.X, end.Y);
                        lines.Add(new Line(sp, ep));
                        sp = ep;
                    }
                }
                r = Rectangle.FromLines(lines);
            }

            return r;

        }
        public static List<Line> ToLines(PDFPathVisualObject pathVisualObject)
        {
            List<Line> lines = new List<Line>();
            PDFPoint start = pathVisualObject.PathItems[0].Points[0];
            Vector sp = new Vector(start.X, start.Y);

            for (int i = 1; i < pathVisualObject.PathItems.Count; i++)
            {
                if (pathVisualObject.PathItems[i].Type == PDFPathItemType.LineTo)
                {
                    PDFPoint end = pathVisualObject.PathItems[i].Points[0];
                    Vector ep = new Vector(end.X, end.Y);
                    lines.Add(new Line(sp, ep));
                    sp = ep;
                }
            }

            return lines;
        }

    }

}
