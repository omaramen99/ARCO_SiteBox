using PDF_Analyzer.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rectangle = PDF_Analyzer.Geometry.Rectangle;

namespace PDF_Analyzer.Result
{
    public class PDFSheetAssemblyData
    {
        public string Name { get; set; }
        public int Page {  get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public List<PDFRectangleData> Openings { get; set; }
        public List<PDFRectangleData> Lumbers { get; set; }

        public PDFSheetAssemblyData(string PanelName, int page, List<Rectangle> concreteRectangles, List<Rectangle> lumberRectangles)
        {
            Name = PanelName;
            Page = page;
            Rectangle wallRectangle = concreteRectangles[0];
            Width = wallRectangle.Width();
            Height = wallRectangle.Height();
            Vector wallCenter = wallRectangle.Center.Clone();
            concreteRectangles.RemoveAt(0);
            Openings = concreteRectangles.Select(r => new PDFRectangleData(r, wallCenter)).ToList();
            Lumbers = lumberRectangles.Select(r => new PDFRectangleData(r, wallCenter, false)).ToList();
        }
    }
}
