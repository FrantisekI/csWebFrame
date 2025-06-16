using System.Net;

namespace csWebFrame
{
    public static class AppConstants
    {
        public static readonly string RootDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        public static readonly string AppDir = Path.Combine(RootDirectory, "app");
    }
    class Program
    {
        static void Main(string[] args)
        {
            
            Listener(new string[] { "http://localhost:8060/" });
        }

        /**<summary>
         * Posloucha requesty od uzivatele stranky a vsechny GET requsty posle do objektu FileReader
         * </summary>*/
        public static void Listener(string[] prefixes)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            // URI prefixes are required,
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // Create a listener.
            HttpListener listener = new HttpListener();
            // Add the prefixes.
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }
            listener.Start();
            Console.WriteLine("Listening... on {0}", prefixes);
            FileReader fileReader = new();
            while (listener.IsListening)
            {
                // Note: The GetContext method blocks while waiting for a request.
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                // Obtain a response object.
                HttpListenerResponse response = context.Response;
                if (request.Url == null)
                {
                    Console.WriteLine("Request URL is null.");
                    continue;
                }
                else
                    Console.WriteLine($"Received request: {request.HttpMethod} {request.Url.AbsolutePath}");

                byte[] buffer;
                if (request.HttpMethod == "GET")
                {
                    buffer = fileReader.GetRequest(request.Url.AbsolutePath);
                }
                else
                {
                    buffer = []; //TODO: handle POST request
                }
                    

                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);

                // You must close the output stream.
                output.Close();
            }
            listener.Stop();
        }
    }
}