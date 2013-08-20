using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCParser
{
    public class SearchQuery
    {
        public static readonly string BASE_URL = @"http://www.leboncoin.fr/";
        public static readonly string OFFRES = @"offres";
        public static readonly string DEFAULT_CATEGORY = @"annonces";
        public static readonly string SEARCH_PARAMS_BASE = @"?f=a&th=1";

        public static readonly Dictionary<string, string> Keywords = new Dictionary<string, string>()
        {
            { "query", "q=" },
            { "page", "o=" },
            { "minPrice", "ps=" },
            { "maxPrice", "pe=" },
            { "inTitle", "it=" }
        };

        public static readonly Dictionary<string, string> Regions = new Dictionary<string, string>()
        {
            {"Pays de la Loire", "pays_de_la_loire" },
            {"Ile de France", "ile_de_france" }
        };

        public static readonly Dictionary<string, string> Categories = new Dictionary<string, string>()
        {
            {"Toutes Catégories", "annonces" },
            {"-- Véhicules --", "_vehicules_" },
            {"Voitures", "voitures" }
        };



        public string Category { get; set; }
        public string Region { get; set; }
        public string Query { get; set; }
        public string MinPrice { get; set; }
        public string MaxPrice { get; set; }
        public bool InTitle { get; set; }
        public int Page { get; set; }

        public SearchQuery(string q)
        {
            this.Region = "pays_de_la_loire";
            this.Category = DEFAULT_CATEGORY;
            this.Page = 1;
            this.Query = q.Replace(" ", "+");
        }

        /// <summary>
        /// Don't use this constructor, unless you need an empty query for some reason.
        /// It will _NOT_ initialize any field
        /// </summary>
        public SearchQuery()
        {
        }

        public string Build()
        {
            return BASE_URL + 
                   (Category != "" ? Category : DEFAULT_CATEGORY) + "/" +
                   OFFRES + "/" + 
                   Region + "/" +
                   SEARCH_PARAMS_BASE + "&" +
                   (Query != "" ? Keywords["query"] + Query + "&" : "") + 
                   (InTitle ? Keywords["inTitle"] + "1&" : "") +
                   Keywords["page"] + Page;

                   
        }
    }
}
