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
        public string isInStock;
        public List<string> pictures;
        public string productLink;

        public string GenerateString()
        {
            string result = "";
            string picturesLink = "";
            pictures.ForEach(p => picturesLink += p + " | ");
            result += region + ";" + path + ";" + name + ';' + actualPrice + ';' + oldPrice + ';' + isInStock + ';' + picturesLink + ';' + productLink;
            return result;
        }
    }
}
