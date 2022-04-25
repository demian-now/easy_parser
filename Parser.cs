using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestTask
{
    interface IParser<URL, DATA>
    {
        List<URL> GetItemLinks();
        DATA ParseData();
    }

    public class Parser : IParser<string, DataHolder>
    {
        private readonly string url;
        private IDocument doc;
        public Parser(string _url)
        {
            //todo check correctly url
            url = _url;
        }

        private async Task Init()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            doc = await context.OpenAsync(url);
        }

        public List<string> GetItemLinks()
        {
            Task.WaitAll(Init());

            //find number of last page
            var check = doc.GetElementsByClassName("page-item")
                .Select(m => m.TextContent)
                .ToList();
            var tmp = url.Split('=');

            var check1 = Int32.Parse(tmp.Last());
            var check2 = Int32.Parse(check[check.Count - 2]);

            //if this number bigger than current number return 0 size list
            if (check1 > check2)
                return new List<string>(); //alt: Exception

            //find all blocks with links
            var list = doc.GetElementsByClassName("d-block p-1 product-name gtm-click");

            return list.Cast<IHtmlAnchorElement>()
                     .Select(m => m.Href)
                     .ToList();
        }

        public DataHolder ParseData()
        {
            Task.WaitAll(Init());
            DataHolder result = new DataHolder();

            Thread[] parse = new Thread[7];

            parse[0] = new Thread(() => result.region = ParseFirstByClassName("col-12 select-city-link").Trim(new char[] { ' ', '\n', '\t' }).Remove(0, 10).Trim(new char[] { ' ', '\n', '\t' }));
            parse[1] = new Thread(() => result.actualPrice = ParseFirstByClassName("price"));
            parse[2] = new Thread(() => result.oldPrice = ParseFirstByClassName("old-price"));
            parse[3] = new Thread(() => result.name = ParseFirstByClassName("detail-name"));
            parse[4] = new Thread(() => {
                result.path = "";
                ParseListByClassName(new string[3] { "breadcrumb-item hide-mobile", "breadcrumb-item prev-active", "breadcrumb-item active d-none d-block" }).ForEach(it => result.path += it + '>');
            });
            parse[5] = new Thread(() => result.isInStock = ParseFirstByClassName("ok", "net-v-nalichii"));
            parse[6] = new Thread(() => {
                result.pictures = ParseLinksListByClassName("img-fluid");
                result.pictures.RemoveRange(0, result.pictures.Count / 2+1);
                for (int i = 0; i < result.pictures.Count; i++)
                {
                    result.pictures[i] = result.pictures[i].Remove(0, 28);
                    int count = 0;
                    for (int j = result.pictures[i].Length - 1; j > 0; j--)
                    {
                        if (result.pictures[i][j] == '\"')
                            count++;
                        if (count == 3)
                        {
                            result.pictures[i] = result.pictures[i].Remove(j, result.pictures[i].Length - j);
                            break;
                        }
                    }
                }
            }
            );

            foreach (Thread p in parse)
                p.Start();
            foreach (Thread p in parse)
                p.Join();

            result.productLink = url;
            return result;
        }

        private string ParseFirstByClassName(string className)
        {
            try
            {
                return doc.GetElementsByClassName(className)
                    .Select(m => m.TextContent)
                    .First();
            }
            catch (Exception)
            {
                return "-";
            }
        }

        private string ParseFirstByClassName(string className1, string className2)
        {
            try
            {
                return doc.GetElementsByClassName(className1)
                    .Select(m => m.TextContent)
                    .First();
            }
            catch (Exception)
            {
                return doc.GetElementsByClassName(className2)
                    .Select(m => m.TextContent)
                    .First();
            }
        }

        private List<string> ParseListByClassName(string className)
        {
            try
            {
                return doc.GetElementsByClassName(className)
                    .Select(m => m.TextContent)
                    .ToList();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        private List<string> ParseLinksListByClassName(string className)
        {
            try
            {
                return doc.GetElementsByClassName(className)
                    .Select(m => m.OuterHtml)
                    .ToList();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        private List<string> ParseListByClassName(string[] className)
        {
            var result = new List<string>();
            for (int i = 0; i < className.Length; i++)
                result.AddRange(ParseListByClassName(className[i]));
            return result;
        }
    }
}
