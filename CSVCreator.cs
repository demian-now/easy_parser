using System.IO;

namespace TestTask
{
    public class CSVCreator
    {
        readonly string path;
        readonly StreamWriter streamWriter;

        public CSVCreator()
        {
            streamWriter = new StreamWriter("K:\\TestFile.csv", false, System.Text.Encoding.Unicode);
        }
        public CSVCreator(string _path)
        {
            //todo check correctly path
            path = _path;
            streamWriter = new StreamWriter(path);
        }

        public void AddString(string input)
        {
            streamWriter.WriteLine(input);
        }

        public void ClearFile()
        {
            //todo clear file
        }
    }
}
