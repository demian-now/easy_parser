using System.Collections.Generic;

namespace TestTask
{
    public class DataHolder
    {
        public string region;
        public string path;
        public string name;
        public string actualPrice;
        public string oldPrice;
        public bool isInStock;
        public List<string> pictures;
        public string productLink;

        public string GenerateString()
        {
            string result = "";
            result += region + "," + path + "," + name + ',' + actualPrice + ',' + oldPrice + ',' + isInStock + ',' + pictures.ToArray().ToString() + ',' + productLink;
            return result;
        }
    }
}
