using System;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using simplehttpserver;

namespace SimpleHttpServerTestProject
{
    [TestClass]
    public class HttpServerTest
    {
        [TestMethod]
        public void TestMethod1()
        {
        }

        private String GetFirstLine(String request)
        {
            TcpClient client = new TcpClient("localhost", HttpServer.DefaultPort);
            NetworkStream networkStream = client.GetStream();
            //networkStream.Write("request".);
        }
    }
}
