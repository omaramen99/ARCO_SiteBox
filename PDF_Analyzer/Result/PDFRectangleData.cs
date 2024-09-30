using PDF_Analyzer.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rectangle = PDF_Analyzer.Geometry.Rectangle;

namespace PDF_Analyzer.Result
{
    public class PDFRectangleData
    {
        public double Width { get; set; }
        public double Height { get; set; }

        //[IF WALL OPENING] location of bottomright to the main wall center
        //[IF LUMBER] location of center to the main wall center
        public Vector Location { get; set; }


        public PDFRectangleData(Rectangle rectangle, Vector wallCenter, bool isOpening = true)
        {
            Width = rectangle.Width();
            Height = rectangle.Height();
            Location = isOpening ? wallCenter.To(rectangle.BottomRight) : wallCenter.To(rectangle.Center);
        }


    }
}
