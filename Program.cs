using System;
using System.Net;
using System.Drawing.Imaging;
using System.Data.SqlTypes;
using FileToData;

namespace MyApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");


            SimpleListenerExample(new string[] { "http://localhost:8060/" });
        }

        

        // This example requires the System and System.Net namespaces.
        public static void SimpleListenerExample(string[] prefixes)
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

                //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                FileReader fileReader = new();
                byte[] buffer = fileReader.ReadFile(request.Url.AbsolutePath);

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