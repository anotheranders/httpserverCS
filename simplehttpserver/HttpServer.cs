using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace simplehttpserver
{
    /// <summary>
    /// author: anbo<br />
    /// References
    /// <list type="">
    /// <item>http://msdn.microsoft.com/en-us/library/b6xa24z5(v=vs.110).aspx</item>
    /// <item>http://www.codeproject.com/Articles/17071/Sample-HTTP-Server-Skeleton-in-C</item>
    /// <item>http://msdn.microsoft.com/en-us/library/aa989072(v=vs.80).aspx</item>
    /// <item>http://stackoverflow.com/questions/12009695/net-equivalent-of-javas-bufferedreader</item>
    /// </list>
    class HttpServer
    {
        /// <summary>
        /// The default port of the HTTP server
        /// </summary>
        public static readonly int DefaultPort = 8888;
        private static readonly string RootCatalog = "c:/temp";
        private Boolean _keepOnRunning = true;
        private static readonly IDictionary<String, String> ContentTypes = new Dictionary<string, string>();

        /// <summary>
        /// static constructor http://msdn.microsoft.com/en-us/library/k9x6w0hc.aspx
        /// </summary>
        static HttpServer()
        {
            ContentTypes["html"] = "text/html";
            ContentTypes["htm"] = "text/html";
            ContentTypes["txt"] = "text/plain";

        }

        /// <summary>
        /// Constructs and starts an instance of the HTTP server
        /// </summary>
        /// <param name="port">port number of the HTTP server</param>
        public HttpServer(int port)
        {
            if (port < 0 || port > 65535)
            {
                throw new ArgumentOutOfRangeException("port", port, "port number out of range");
            }
            IPHostEntry ipHostEntry = Dns.GetHostEntry("localhost");
            IPAddress localIpAddress = ipHostEntry.AddressList[0];
            //IPAddress localIpAddress = Dns.Resolve("localhost").AddressList[0];
            TcpListener server = new TcpListener(localIpAddress, port);
            server.Start();
            Console.WriteLine("Server listening on port: {0}", port);
            while (_keepOnRunning)
            {
                TcpClient client = server.AcceptTcpClient();
                //DoIt(client);
                Task.Factory.StartNew(() => DoIt(client));
            }
            server.Stop();
        }

        private static bool DoIt(TcpClient client)
        {
            NetworkStream toFromClientStream = client.GetStream();
            StreamReader fromClient = new StreamReader(toFromClientStream, Encoding.UTF8);
            //while (inReader.Peek() > 0)
            //{
            String line = fromClient.ReadLine();
            if (line == null) return true;
            string[] parts = line.Split(new[] { ' ' }, 3);
            string uri = parts[1];
            Console.WriteLine(uri);
            StreamWriter toClient = new StreamWriter(toFromClientStream, Encoding.UTF8);
            try
            {
                FileStream filesStream = new FileStream(RootCatalog + uri, FileMode.Open, FileAccess.Read);

                toClient.Write("HTTP/1.0 200 OK\r\n");
                //toClient.Write("Content-Type: text/html\r\n");
                toClient.Write("\r\n");

                toClient.Flush();
                CopyStream(filesStream, toFromClientStream);
                toClient.Flush();
            }
            catch (FileNotFoundException ex)
            {
                toClient.Write("HTTP/1.0 404 Not Found\r\n");
                toClient.Write("Content-Type: text/html\r\n");
                toClient.Write("\r\n");
                toClient.Write("<html><head><title>Yes!</title></head><body> {0} </body></html>", ex.Message);
                toClient.Flush();
            }

            fromClient.Close();
            toClient.Close();
            Console.WriteLine(line);
            client.Close();
            //}
            return false;
        }

        /// <summary>
        /// Stops the HTTP server. Graceful shutdown, sometime in the future ...
        /// </summary>
        public void Stop()
        {
            _keepOnRunning = false;
        }

        /// <summary>
        /// http://stackoverflow.com/questions/230128/best-way-to-copy-between-two-stream-instances
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        static void Main(string[] args)
        {
            var port = Port(args);
            new HttpServer(port);
        }

        private static int Port(string[] args)
        {
            int port = DefaultPort;
            if (args.Any())
            {
                String portStr = args[0];
                Console.WriteLine(portStr);
                try
                {
                    port = int.Parse(portStr);
                }
                catch (FormatException ex)
                {
                    Console.WriteLine("Illegal port number: {0}", portStr);
                    Console.WriteLine("Will use port {0}", DefaultPort);
                }
            }
            return port;
        }
    }
}
