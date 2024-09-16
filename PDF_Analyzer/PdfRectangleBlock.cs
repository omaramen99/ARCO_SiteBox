using O2S.Components.PDF4NET.Content;
using PDF_Analyzer.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rectangle = PDF_Analyzer.Geometry.Rectangle;

namespace PDF_Analyzer
{
    public class PdfRectangleBlock
    {
        public Rectangle rectangle { get; set; }
        public PDFPathVisualObject pdfPathObject { get; set; }


        public PdfRectangleBlock(Rectangle rectangle, PDFPathVisualObject pdfPathObject)
        {
            this.rectangle = rectangle;
            this.pdfPathObject = pdfPathObject;
        }

        public List<Line> ToLines()
        {
            return rectangle.GetSides();
        }
    }
}
