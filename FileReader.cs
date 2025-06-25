using System.Text;

namespace csWebFrame;

/**<summary>
 * Volano z Http Listeneru, kduyz uzivatel dela GET request, tak pokud chce zdroj - jako je obrazek nebo ikona
 * tak se rovnou nacte ze src/ slozky a vrati jeho data v UTF8
 *
 * Pokud chce uzivatel stranku, tak zavola funci v SiteHolderu a opet vrati v UTF8 jeji obsah
 * </summary>*/
class FileReader
{
    SitesHolder _sitesHolder;

    /**<summary>
     * Vytvoří novou instanci FileReader pro práci se soubory
     * </summary>*/
    public FileReader(SitesHolder sitesHolder)
    {
        _sitesHolder = sitesHolder;
    }
    
    /**<summary>
     * Zpracuje GET požadavek na soubor a vrátí odpovídající data s HTTP kódem
     *
     * Pokud uživatel chce nějaký obrázek, nebo jiný podobný obsah, tak se přímo načte ze souborového systému
     * Soubory css se vezmou ze se složek app a components directory
     *
     * Pokud uživatel chce html stránku, tak ta se dynamicky vygeneruje pomocí SiteHolder
     * </summary>*/
    public (int, byte[]) GetRequest(string file, UserSession session)
    {
        if (file.EndsWith(".html") || file.EndsWith("/") || !file.Contains('.'))
        {
            string? page = _sitesHolder.RenderPage(file, session);
            int statusCode = 200;
            if (page == null)
            {
                statusCode = 404;
                page = _sitesHolder.RenderPage("404", session);
            }

            return (statusCode, Encoding.UTF8.GetBytes(page));

        }
        else if (file.EndsWith(".css"))
        {
            string filePath;
            if (file.StartsWith("components"))
            {
                 filePath = Path.Join(" components", file);
            }
            else
            {
                
                filePath = Path.Join(" app", file);
            }
            byte[] fileData = ReadFile( "", filePath);
            return ((fileData == Array.Empty<byte>() ? 404 : 200), fileData);
        }
        else
        {
            byte[] fileData = ReadFile("src", file);
            return ((fileData == Array.Empty<byte>() ? 404 : 200), fileData);
        }
    }

    /**<summary>
     * Na vstupu dostane jaky soubor precist a vrati jeho UTF8 data
     * </summary>*/
    private byte[] ConvertToBytes(string filPath)
    {
        try
        {
            using FileStream fsSource = new FileStream(filPath,
                FileMode.Open, FileAccess.Read);
            
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
            //numBytesToRead = bytes.Length;

            return bytes;
        }
        catch (FileNotFoundException ioEx)
        {
            Console.WriteLine(ioEx.Message);
        }
        catch(DirectoryNotFoundException ioEx)
        {
            Console.WriteLine(ioEx.Message);
        }
        return [];
    }
    /**<summary>
     * Dostane relativni cestu k souboru a vrati jeho UTF8 data
     * </summary>*/
    private byte[] ReadFile(string folder, string filePath)
    {
        filePath = filePath.Substring(1);

        string pathSource = Path.Combine(Path.Combine(AppConstants.RootDirectory, folder), filePath);
        
        return ConvertToBytes(pathSource);
    }

}
