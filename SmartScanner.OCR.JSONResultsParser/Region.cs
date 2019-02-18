using System.Collections.Generic;

namespace SmartScanner.OCR.JSONResultsParser
{
    public class Region
    {
        public string BoundingBox { get; set; }
        public List<Line> Lines { get; set; }
    }
}
