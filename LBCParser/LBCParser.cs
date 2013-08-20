using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Drawing;
using Debug = System.Diagnostics.Debug;
using System.Text.RegularExpressions;

namespace LBCParser
{
    public class LBCParser
    {

        private HtmlDocument RetrieveDocumentAsync(string uri)
        {
            Post p = new Post(null);

            Debug.WriteLine("Retrieving " + uri + "...");
            var connection = (HttpWebRequest)HttpWebRequest.Create(uri);
            var response = connection.GetResponse();
            
            HtmlDocument doc = new HtmlDocument();

            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                string content = sr.ReadToEnd();
                //Debug.WriteLine("Document size : " + content.Length + " chars");
                doc.OptionAutoCloseOnEnd = true;
                doc.LoadHtml(content);
            }
            Debug.WriteLine("Done.");
            return doc;
        }

        public LBCParser()
        {
        }

        public int GetPageCount(SearchQuery query)
        {
            var doc = RetrieveDocumentAsync(query.Build());
            var paging = doc.GetElementbyId("paging");
            var numPagesLink = paging.ChildNodes[paging.ChildNodes.Count - 2].ChildNodes[1].Attributes["href"].Value;

            var numPagesStr = Regex.Replace(numPagesLink, @"[^(o=)\d+]", "").Replace("o", "").Replace("=", "");
            var numPages = Int32.Parse(numPagesStr);
            
            return numPages;
        }

        /// <summary>
        /// Perform a search on LeBonCoin and returns the <code>page</code>th page of results
        /// </summary>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public IEnumerable<SearchResult> Search(SearchQuery query, int page)
        {
            query.Page = page;

            var document = RetrieveDocumentAsync(query.Build());

            //Debug.WriteLine("Parsing the Document");
            // The main content is inside #ContainerMain
            var body = document.GetElementbyId("ContainerMain");

            // We iterate on each div to find the appropriate one
            // It _should_ be the 38th div according to HAP, but this might change
            // We need to find a div.list

            var list = body.ChildNodes.Where(node => node.Attributes.Contains("class") &&
                                             node.Attributes["class"].Value.Contains("list"))
                                      .FirstOrDefault();
            if (list == null)
            {
                throw new Exception("Couldn't find any div with a .list class");
            }
            // HAP Parses text as a #text node. ChildNodes[0] is always a #text node.
            // The next one is the one we're interested in.
            //Debug.WriteLine("Parsing the list");
            list = list.ChildNodes[1].ChildNodes[1];

            return BuildSearchList(list);
        }

        /// <summary>
        /// Perform a search on LeBonCoin and returns the first page of results
        /// </summary>
        /// <param name="query">The query parameters</param>
        /// <returns>A list of search results</returns>
        public IEnumerable<SearchResult> Search(SearchQuery query)
        {
            return Search(query, 1);
        }

        private string ExtractId(string uri)
        {
            return Regex.Replace(uri, @"[^\d+]", "");
        }

        private IEnumerable<SearchResult> BuildSearchList(HtmlNode baseNode)
        {
            // We should have a div.list-lbc node
            // Content is inside <a> tags.
            // We retrieve data that is inside specific divs
            // .price might be missing if price isn't known
            foreach (var node in baseNode.ChildNodes)
            {
                if (node.Name.Equals("a"))
                {
                    var search = new SearchResult();
                    search.Link = node.Attributes["href"].Value.Replace("?ca=18_s", "");
                    search.Id = ExtractId(search.Link);

                    // The first subnode is always the div.lbc one
                    var subNode = node.ChildNodes[1];

                    // -- Date
                    search.Date = StripLBCSpaces(subNode.ChildNodes[1].InnerText);
                    
                    // -- Preview Image
                    // If there is no image, the div.image will only contain &nbsp;
                    if (!StripLBCSpaces(subNode.ChildNodes[3].InnerText).Equals("&nbsp;"))
                    {
                        search.PreviewImage = GetBitmapFromURI(subNode.ChildNodes[3].ChildNodes[1].ChildNodes[1].Attributes["src"].Value, 
                                                               search.Id + "/preview.jpg");                        
                    }

                    // -- Title
                    search.Title = StripLBCSpaces(subNode.ChildNodes[5].ChildNodes[1].InnerText);

                    // -- Category
                    search.Category = StripLBCSpaces(subNode.ChildNodes[5].ChildNodes[3].InnerText);

                    // -- Location
                    search.Location = StripLBCSpaces(subNode.ChildNodes[5].ChildNodes[5].InnerText);

                    // -- Price
                    // If the price isn't known, this particular div won't be there.
                    if (subNode.ChildNodes[5].ChildNodes.Count > 7)
                    {
                        search.Price = StripLBCSpaces(subNode.ChildNodes[5].ChildNodes[7].InnerText);
                    }

                    yield return search;
                    
                }
            }
           
        }

        public Post GetSinglePost(SearchResult result)
        {
            Post post = new Post(result);
            int currentChild = 1;
            var document = RetrieveDocumentAsync(result.Link);

            var body = document.GetElementbyId("ContainerMain");


            var content = body.ChildNodes.Where(node => node.Attributes.Contains("class") &&
                                             node.Attributes["class"].Value.Contains("content-border"))
                                      .FirstOrDefault();
            if (content == null)
            {
                throw new Exception("Couldn't find any div with a .content-border class");
            }
            // HAP Parses text as a #text node. ChildNodes[0] is always a #text node.
            // The next one is the one we're interested in.
            //Debug.WriteLine("Parsing the content");
            content = content.ChildNodes[1].ChildNodes.Where(node => node.Attributes.Contains("class") &&
                                                                     node.Attributes["class"].Value.Contains("lbcContainer"))
                                                      .FirstOrDefault();

            if (content == null)
            {
                throw new Exception("Couldn't find .lbcContainer");
            }

            // -- User Info
            post.UserName = StripLBCSpaces(content.ChildNodes[1].ChildNodes[3].ChildNodes[1].InnerText);
            post.UserMail = StripLBCSpaces(content.ChildNodes[1].ChildNodes[3].ChildNodes[1].Attributes["href"].Value);

            // -- Images
            // If there is no image, there will be an empty div in place of the image container
            // However, .print-lbcImages won't be there. We need to go up one div, or we'd try to access an unexistant div
            currentChild = 1;
            if (String.IsNullOrWhiteSpace(StripLBCSpaces(content.ChildNodes[5].ChildNodes[currentChild].InnerHtml)))
            {
                currentChild = 3;
                Debug.WriteLine("No images on node :" + result.Title);
            }
            else
            {
                currentChild = 5;
            }


            var infos = content.ChildNodes[5].ChildNodes[currentChild].ChildNodes[1];
            foreach (var node in infos.ChildNodes)
            {
                if (node.Name.Equals("tr"))
                {
                    //Debug.WriteLine("Found a tr");
                    post.Properties.Add(new Tuple<string, string>(StripLBCSpaces(node.ChildNodes[1].InnerText),
                                                                  StripLBCSpaces(node.ChildNodes[3].InnerText)));
                }
            }
            currentChild += 2;
            post.Description = StripLBCSpaces(content.ChildNodes[5].ChildNodes[currentChild].ChildNodes[3].InnerText);

            return post;
        }

        public List<Post> GetPostsFromSearchResults(List<SearchResult> results)
        {
            List<Post> posts = new List<Post>();

            foreach (var res in results)
            {
                posts.Add(GetSinglePost(res));
            }
            
            return posts;
        }

        private Bitmap GetBitmapFromURI(string uri, string output)
        {
            HttpWebRequest webreq = (HttpWebRequest)HttpWebRequest.Create(uri);
            var resStream = webreq.GetResponse().GetResponseStream();

            Bitmap bmp = new Bitmap(resStream);

            return bmp;
        }

        private string StripLBCSpaces(string original)
        {
            return HtmlEntity.DeEntitize(original.Trim().Replace("\n", "").Replace("\t", "").Replace("  ", ""));
        }
    }
}
