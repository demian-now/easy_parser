using System;
using System.Linq;

namespace TestTask
{
    class Program
    {
        static void Main(string[] args)
        {
            int a = 1;
            CSVCreator creator = new CSVCreator();
            while (a < 100)
            {
                Parser parser = new Parser("https://www.toy.ru/catalog/boy_transport/?filterseccode%5B0%5D=transport&PAGEN_8=" + a.ToString());
                var list = parser.GetItemLinks();
                if (list.Count == 0) break;
                foreach (string i in list)
                {
                    Parser parser1 = new Parser(i);
                    creator.AddString(parser1.ParseData().GenerateString());
                    Console.Write(".");
                }
                a++;
                Console.WriteLine();
            }
        }
    }
}
