using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileToData
{
    class FileReader
    {
        string _windowsAppDir = @"..\..\..\app";
        string _linuxAppDir = "../../../app";

        string _projectPath;

        public FileReader()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            _projectPath = Path.GetFullPath(Path.Combine(basePath, _linuxAppDir));
        }

        public byte[] ReadFile(string filePath)
        {
            Console.WriteLine(filePath);
            if (filePath.EndsWith('/'))
                filePath = Path.Combine(filePath, "index.html");
            filePath = filePath.Substring(1);

            string pathSource = Path.Combine(_projectPath, filePath);
            try
            {
                using (FileStream fsSource = new FileStream(pathSource,
                    FileMode.Open, FileAccess.Read))
                {
                    // Read the source file into a byte array.
                    byte[] bytes = new byte[fsSource.Length];
                    int numBytesToRead = (int)fsSource.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        // Read may return anything from 0 to numBytesToRead.
                        int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

                        // Break when the end of the file is reached.
                        if (n == 0)
                            break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }
                    numBytesToRead = bytes.Length;

                    return bytes;
                }
            }
            catch (FileNotFoundException ioEx)
            {
                Console.WriteLine(ioEx.Message);
            }
            catch(DirectoryNotFoundException ioEx)
            {
                Console.WriteLine(ioEx.Message);
            }
            return []; //TODO: return 404 not found
        }

    }
}
