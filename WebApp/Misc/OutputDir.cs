using System;
using System.IO;

namespace WebApp.Misc
{
    /// <summary>
    /// Manages the output directory for generated files.
    /// Provides functionality to create unique filenames and file streams.
    /// </summary>
    public class OutputDir
    {
        /// <summary>
        /// The project root directory path.
        /// </summary>
        readonly string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).FullName;

        /// <summary>
        /// The output directory path for generated files.
        /// </summary>
        public readonly string outputDirectory = Directory.GetParent(Environment.CurrentDirectory).FullName + @"\OutputDir\";

        /// <summary>
        /// Default filename prefix for generated files.
        /// </summary>
        readonly string defaultFileName = "file";

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputDir"/> class.
        /// Creates the output directory if it doesn't exist.
        /// </summary>
        public OutputDir()
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
        }

        /// <summary>
        /// Generates a unique filename with the specified extension.
        /// Increments a number suffix until a non-existing filename is found.
        /// </summary>
        /// <param name="fileExtName">The file extension without the dot (e.g., "xlsx", "txt").</param>
        /// <returns>Full path to a unique filename.</returns>
        public string GetNewFileName(string fileExtName)
        {
            long num = 0;
            string fileName = defaultFileName + num;

            while (File.Exists(outputDirectory + fileName + "." + fileExtName))
            {
                fileName = defaultFileName + ++num;
            }

            return outputDirectory + fileName + "." + fileExtName;
        }

        /// <summary>
        /// Creates a new Excel file stream with a unique filename.
        /// </summary>
        /// <returns>A FileStream for the new Excel file, or null if creation failed.</returns>
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
