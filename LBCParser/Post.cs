using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LBCParser
{
    public class Post
    {
        public string Link { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public List<Bitmap> Images { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public string Price { get; set; }
        public int NumPictures { get; set; }
        public List<Tuple<string, string>> Properties { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public string UserMail { get; set; }
        public string Id { get; set; }

        public Post(SearchResult baseInfo)
        {
            this.Link = baseInfo.Link;
            this.Id = baseInfo.Id;
            this.Title = baseInfo.Title;
            this.Date = baseInfo.Date;
            this.Category = baseInfo.Category;
            this.Location = baseInfo.Location;
            this.Price = baseInfo.Price;
            this.Properties = new List<Tuple<string, string>>();
            this.Images = new List<Bitmap>();
        }

        public override string ToString()
        {
            string properties = "";
            foreach (var tuple in Properties)
            {
                properties += " p " + tuple.Item1 + " : " + tuple.Item2 + "\n";
            }
            return "   " + Title + "\n" +
                   "-> " + Link + "\n" +
                   " @ " + Location + "\n" +
                   " c " + Category + "\n" +
                   " E " + Price + "\n" +
                   " d " + Date + "\n" +
                   " f " + UserName + "\n" +
                   "fm " + UserMail + "\n" +
                   properties + "\n" +
                   "dsc" + Description;
        }
    }
}
