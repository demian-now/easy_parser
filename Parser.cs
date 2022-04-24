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
            //todo check
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

            parse[0] = new Thread(() => result.region = ParseRegion());
            parse[1] = new Thread(() => result.actualPrice = ParseActualPrice());
            parse[2] = new Thread(() => result.oldPrice = ParseOldPrice());
            parse[3] = new Thread(() => result.name = ParseName());
            parse[4] = new Thread(() => result.path = ParsePath());
            parse[5] = new Thread(() => result.isInStock = ParseIsInStock());
            parse[6] = new Thread(() => result.pictures = ParsePictureLinks());

            foreach (Thread p in parse)
                p.Start();
            foreach (Thread p in parse)
                p.Join();

            result.productLink = url;

            return result;
        }

        private string ParseRegion()
        {
            var result = doc.GetElementsByClassName("col-12 select-city-link").Select(m => m.TextContent).First();
            result = result.Trim(new char[] { ' ', '\n', '\t' }).Remove(0, 10).Trim(new char[] { ' ', '\n', '\t' });
            return result;
        }

        private string ParseActualPrice()
        {
            var result = doc.GetElementsByClassName("price").Select(m => m.TextContent).First();
            return result.Remove(result.Length - 6, 5);
        }
        private string ParseOldPrice()
        {
            try
            {
                var result = doc.GetElementsByClassName("old-price").Select(m => m.TextContent).First();
                return result.Remove(result.Length - 6, 5);
            }
            catch (Exception)
            {
                return "equals";
            }
        }

        private string ParseName()
        {
            return doc.GetElementsByClassName("description").Select(m => m.TextContent).First();
        }

        private string ParsePath()
        {
            return doc.GetElementsByClassName("breadcrumb").Select(m => m.TextContent).First();
        }

        private bool ParseIsInStock()
        {
            if (doc.GetElementsByClassName("breadcrumb").Select(m => m.TextContent).ToList().Count == 0)
                return false;
            return true;
        }

        private List<string> ParsePictureLinks()
        {
            var result = doc.GetElementsByClassName("img-fluid").Select(m => m.OuterHtml).ToList();
            int size = (result.Count - 2) / 2;
            result.RemoveRange(0, size + 2);
            for (int i = 0; i < result.Count; i++)
            {
                result[i] = result[i].Remove(0, 28);
                int count = 0;
                for (int j = result[i].Length - 1; j > 0; j--)
                {
                    if (result[i][j] == '\"')
                        count++;
                    if (count == 3)
                    {
                        result[i] = result[i].Remove(j, result[i].Length - j);
                        break;
                    }
                }
            }
            return result;
        }
    }
}
