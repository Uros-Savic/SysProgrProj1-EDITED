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
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    HttpListenerContext context = (HttpListenerContext)state;
                    HandleRequest(context);
                }, listener.GetContext());
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
                    SendErrorResponse(response, HttpStatusCode.NotFound, "File type not supported or recognized.");
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

                        SendResponse(response, HttpStatusCode.OK, "image/gif", cachedResponse);
                        return;
                    }

                    string convertedFilePath = Logic.ConvertToGif(filePath, ++index).ToString();
                    byte[] fileBytes = File.ReadAllBytes(convertedFilePath);

                    CacheManager.Set(requestUrl, fileBytes, 5);

                    SendResponse(response, HttpStatusCode.OK, "image/gif", fileBytes);
                    Console.WriteLine("A .gif file has been created.");
                }
                catch (FileNotFoundException)
                {
                    SendErrorResponse(response, HttpStatusCode.NotFound, $"File not found: {requestUrl}");
                }
                catch (Exception ex)
                {
                    SendErrorResponse(response, HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                }

                response.Close();
                stopwatch.Stop();
                Console.WriteLine($"Request processed in {stopwatch.ElapsedMilliseconds} milliseconds.");
            }
            else
            {
                SendErrorResponse(response, HttpStatusCode.BadRequest, "Request is null.");
                response.Close();
                Console.WriteLine("Request is null.");
            }
        }

        static void SendResponse(HttpListenerResponse response, HttpStatusCode statusCode, string contentType, byte[] contentBytes)
        {
            response.StatusCode = (int)statusCode;
            response.ContentType = contentType;
            response.ContentLength64 = contentBytes.Length;
            response.OutputStream.Write(contentBytes, 0, contentBytes.Length);
            response.Close();
            Console.WriteLine($"HTTP {(int)statusCode} {statusCode.ToString()}");
        }

        static void SendErrorResponse(HttpListenerResponse response, HttpStatusCode statusCode, string errorMessage)
        {
            response.StatusCode = (int)statusCode;
            byte[] errorBytes = Encoding.UTF8.GetBytes(errorMessage);
            response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
            response.Close();
            Console.WriteLine($"HTTP {(int)statusCode} {statusCode.ToString()}: {errorMessage}");
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
