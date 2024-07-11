using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MultiThreadedWebServer
{
    class Program
    {
        static readonly string RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..");
        protected static HttpListener listener;

        static void Main()
        {
            if (!Directory.Exists(RootFolder))
                Directory.CreateDirectory(RootFolder);
            try
            {
                listener = new HttpListener();
                Server.StartWebServer(listener);
                Console.WriteLine("Web server started.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Server.StopWebServer(listener);
            }
        }
    }
}