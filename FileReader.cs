using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using csWebFrame;

namespace FileToData
{
    class FileReader
    {
        string _windowsAppDir = @"..\..\..";
        string _linuxAppDir = "../../..";

        string _projectPath;
        SitesHolder _sitesHolder = new SitesHolder();

        public FileReader()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            _projectPath = Path.GetFullPath(Path.Combine(basePath, _linuxAppDir)); // TODO replace with Appconstants.RootDir
        }
        
        public byte[] GetRequest(string file) //TODO: might need rename
        {
            if (file.EndsWith(".html") || file.EndsWith("/") || !file.Contains('.'))
            {
                //TODO: make sure, that if user request folder, app crashes 
                if (file.EndsWith("/") || !file.Contains('.'))
                {
                    file = Path.Combine(file, "index.html");
                }
                
                return Encoding.UTF8.GetBytes(_sitesHolder.RenderPage(file));

            }
            else
            {
                return ReadFile("src", file);
            }
        }
        

        public byte[] ConvertToBytes(string filPath)
        {
            try
            {
                using (FileStream fsSource = new FileStream(filPath,
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
        public byte[] ReadFile(string folder, string filePath)
        {
            Console.WriteLine(filePath);
            filePath = filePath.Substring(1);

            string pathSource = Path.Combine(Path.Combine(_projectPath, folder), filePath);
            
            return ConvertToBytes(pathSource);
        }

    }
}
