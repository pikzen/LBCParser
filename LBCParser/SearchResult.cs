using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LBCParser
{
    public class SearchResult
    {
        public string Link { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public Bitmap PreviewImage { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public string Price { get; set; }
        public string Id { get; set; }

        public override string ToString()
        {
            return "   " + Title + "\n" +
                   "-> " + Link + "\n" +
                   " @ " + Location + "\n" +
                   " c " + Category + "\n" +
                   " E " + Price + "\n" +
                   " d " + Date;
        }
    }
}
