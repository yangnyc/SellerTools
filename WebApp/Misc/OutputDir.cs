using System;
using System.IO;

namespace WebApp.Misc
{
    public class OutputDir
    {
        readonly string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).FullName;
        public readonly string outputDirectory = Directory.GetParent(Environment.CurrentDirectory).FullName + @"\OutputDir\";
        readonly string defaultFileName = "file";

        public OutputDir()
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
        }

        public string GetNewFileName(string fileExtName)
        {
            long num = 0;
            string fileName = defaultFileName + num;

            while (File.Exists(outputDirectory+fileName+"." +fileExtName))
            {
                fileName = defaultFileName + ++num;
            }

            return outputDirectory+fileName+"." +fileExtName;
        }

        public FileStream? CreateExcelFileStream()
        {
            string fileName = GetNewFileName("xlsx");
            FileStream fileStream = File.Create(Path.GetFullPath(fileName));
            if (File.Exists(fileName))
                return fileStream;
            else
                return null;
        }
    }
}
