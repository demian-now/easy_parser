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
            if(check1 > check2)
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
            var result = doc.GetElementsByClassName("col-12 select-city-link")
                .Select(m => m.TextContent)
                .First();
            //remove all useless symbols
            result = result.Trim(new char[] { ' ', '\n', '\t' }).Remove(0, 10).Trim(new char[] { ' ', '\n', '\t' });
            return result;
        }

        private string ParseActualPrice()
        {
            var result = doc.GetElementsByClassName("price")
                .Select(m => m.TextContent)
                .First();
            //remove all useless symbols x2
            return result.Remove(result.Length - 5, 5);
        }
        private string ParseOldPrice()
        {
            try
            {
                var result = doc.GetElementsByClassName("old-price") //this block is not always there
                    .Select(m => m.TextContent)
                    .First();
                //remove all useless symbols x3
                return result.Remove(result.Length - 5, 5);
            }
            catch (Exception)
            {
                return "equals"; //alt: get actual price in old
            }
        }

        private string ParseName()
        {
            return doc.GetElementsByClassName("detail-name")
                .Select(m => m.TextContent)
                .First();
        }

        private string ParsePath()
        {
            //in my case, the breadcrumbs are distributed in several blocks
            var list = doc.GetElementsByClassName("breadcrumb-item hide-mobile")
                .Select(m => m.TextContent)
                .ToList();
            list.AddRange(doc.GetElementsByClassName("breadcrumb-item prev-active")
                .Select(m => m.TextContent)
                .ToList());
            list.AddRange(doc.GetElementsByClassName("breadcrumb-item active d-none d-block")
                .Select(m => m.TextContent)
                .ToList());

            string result = "";

            foreach(string i in list)
            {
                result += i + ">";
            }

            return result;
        }

        private string ParseIsInStock()
        {
            try
            {
                return doc.GetElementsByClassName("ok")
                    .Select(m => m.TextContent)
                    .First();
            }catch(Exception)
            {
                return doc.GetElementsByClassName("net-v-nalichii")
                    .Select(m => m.TextContent)
                    .First();
            }
        }

        private List<string> ParsePictureLinks()
        {
            var result = doc.GetElementsByClassName("img-fluid")
                .Select(m => m.OuterHtml)
                .ToList();
            int size = (result.Count - 2) / 2; //delete logo and mini-img
            result.RemoveRange(0, size + 2);

            //delete useless symbols x4
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
