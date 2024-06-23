using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace MultiThreadedWebServer
{
    internal class Server
    {
        static readonly string RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..");
        static int index = 0;

        public static void StartWebServer(HttpListener listener)
        {
            listener.Prefixes.Add("http://localhost:5050/");
            listener.Start();
            Console.WriteLine("Listening for requests.");

            while (listener.IsListening)
            {
                HandleRequest(listener.GetContext());
            }
        }

        static void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if (request.Url != null)
            {
                string requestUrl = request.Url.LocalPath;
                Console.WriteLine($"Request received: {requestUrl}");
                string filePath = Path.Combine(RootFolder, requestUrl.TrimStart('/'));

                Stopwatch stopwatch = Stopwatch.StartNew();

                if (!FindFileType(filePath))
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    string errorMessage = $"File type not supported or recognized.";
                    byte[] errorBytes = Encoding.UTF8.GetBytes(errorMessage);
                    response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                    Console.WriteLine($"File type not supported or recognized.");
                    response.Close();
                    stopwatch.Stop();
                    Console.WriteLine($"Request processed in {stopwatch.ElapsedMilliseconds} milliseconds.");
                    return;
                }

                try
                {
                    byte[] cachedResponse = CacheManager.Get(requestUrl);
                    if (cachedResponse != null)
                    {
                        Console.WriteLine("Cached response found.");
                        Console.WriteLine($"Request processed in {stopwatch.ElapsedMilliseconds} milliseconds.");

                        response.OutputStream.Write(cachedResponse, 0, cachedResponse.Length);
                        response.Close();
                        return;
                    }

                    string convertedFilePath = Logic.ConvertToGif(filePath, ++index).ToString();
                    byte[] fileBytes = File.ReadAllBytes(convertedFilePath);

                    CacheManager.Set(requestUrl, fileBytes, 15); 

                    response.ContentType = "image/gif";
                    response.ContentLength64 = fileBytes.Length;
                    response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                    Console.WriteLine("A .gif file has been created.");
                }
                catch (FileNotFoundException)
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    string errorMessage = $"File not found: {requestUrl}";
                    byte[] errorBytes = Encoding.UTF8.GetBytes(errorMessage);
                    response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                    Console.WriteLine($"File not found: {requestUrl}");
                }
                catch (Exception ex)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    string errorMessage = $"Server error: {ex.Message}";
                    byte[] errorBytes = Encoding.UTF8.GetBytes(errorMessage);
                    response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                    Console.WriteLine($"Server error: {ex.Message}");
                }

                response.Close();
                stopwatch.Stop();
                Console.WriteLine($"Request processed in {stopwatch.ElapsedMilliseconds} milliseconds.");
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close();
                Console.WriteLine("Request is null.");
            }
        }

        static bool FindFileType(string filePath)
        {
            string extension = Path.GetExtension(filePath)?.ToLower();

            List<string> Extensions = new List<string> { ".gif", ".qoi", ".png", ".pbm", ".webp", ".tga", ".jpeg", ".jpg", ".tiff", ".bmp" };

            return Extensions.Contains(extension);
        }

        public static void StopWebServer(HttpListener listener)
        {
            if (listener != null && listener.IsListening)
            {
                listener.Stop();
                listener.Close();
                Console.WriteLine("Web server stopped.");
            }
        }
    }
}
