using System.Collections.Specialized;
using System.Net;
using System.Web;

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
            SitesHolder sitesHolder = new SitesHolder();
            FileReader fileReader = new FileReader(sitesHolder);
            while (listener.IsListening) //TODO implement Multithreding
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
                
                System.IO.Stream output = response.OutputStream;
                int statusCode;
                byte[] buffer;
                if (request.HttpMethod == "GET")
                {
                    
                    
                    (statusCode, buffer) = fileReader.GetRequest(request.Url.AbsolutePath);
                    
                    response.ContentLength64 = buffer.Length;
                    output.Write(buffer, 0, buffer.Length);
                }
                else if (request.HttpMethod == "POST")
                {
                    /*
                    // Print detailed information about the POST request
                    Console.WriteLine("\n===== POST REQUEST DETAILS =====\n");
                    Console.WriteLine($"Method: {request.HttpMethod}");
                    Console.WriteLine($"URL: {request.Url}");
                    Console.WriteLine($"Host: {request.UserHostName}");
                    Console.WriteLine($"Content Type: {request.ContentType}");
                    Console.WriteLine($"Content Length: {request.ContentLength64}");

                    // Print all headers
                    Console.WriteLine("\n--- Headers ---");
                    foreach (string key in request.Headers.AllKeys)
                    {
                        Console.WriteLine($"{key}: {request.Headers[key]}");
                    }
                    */
                    StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding);
                    string formData = reader.ReadToEnd();
                    Console.WriteLine(formData);
                    reader.Close();
                    statusCode = 303;

                    var parsedFormData = new Dictionary<string, string>();
                    var pairs = formData.Split('&');
                    foreach (var pair in pairs)
                    {
                        var parts = pair.Split('=');
                        if (parts.Length == 2)
                        {
                            // URL-decode the key and the value
                            var key = HttpUtility.UrlDecode(parts[0]);
                            var value = HttpUtility.UrlDecode(parts[1]);
                            parsedFormData[key] = value;
                        }
                    }
                    sitesHolder.PostRequest(url: request.Url.AbsolutePath, data: parsedFormData);
                    Console.WriteLine(request.Url.AbsolutePath);
                    response.Redirect(request.Url.AbsolutePath);
                }
                else
                {
                    statusCode = 405;
                    buffer = []; //TODO: handle POST request
                }
                response.StatusCode = statusCode;

                // Get a response stream and write the response to it.
                

                // You must close the output stream.
                output.Close();
            }
            listener.Stop();
        }
    }
}