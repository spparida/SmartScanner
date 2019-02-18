using System.Collections.Generic;

namespace SmartScanner.OCR.JSONResultsParser
{
    public class Line
    {
        public bool Found { get; set; }
        public string BoundingBox { get; set; }
        public List<Word> Words { get; set; }
    }
}