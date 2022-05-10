using System;
using c = System.Console;
using ADIS;
using ADIS.TLS;
using System.Net;
using System.Net.Sockets;

namespace ConsoleCord
{
    public class ConsoleCordServer
    {
        internal static List<SvClient> Clients = new();
        internal static byte[] gBuffer = new byte[1024];
        #nullable disable
        internal static string ServerName { get; private set; }
        internal static string IpAddress { get; private set; }
        internal static int Port { get; private set; }
        internal static Socket ServerSocket { get; private set; }
        #nullable enable

        internal static void CreateServer(string ip, int port, string serverName)
        {
            Port = port;
            ServerName = serverName;
            IpAddress = ip;
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetupServer();
        }

        private static void SetupServer()
        {
            c.WriteLine("Starting up consolecord server...");
            var ip = IPAddress.Parse(IpAddress);
            IPEndPoint ep = new(ip, Port);
            ServerSocket.Bind(ep);
            ServerSocket.Listen(1);
            ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void AcceptCallback(IAsyncResult asyncResult)
        {
            var socket = ServerSocket.EndAccept(asyncResult);
            SvClient client = new("", socket, Clients.Count);
            Clients.Add(client);
            c.WriteLine($"A client has connected... Assigning id {client.ClientNumber}");
            socket.BeginReceive(gBuffer, 0, gBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), client);
        }

        private static void RecieveCallback(IAsyncResult asyncResult)
        {
            var client = (SvClient)asyncResult.AsyncState!;
            var socket = client.ClientSocket;
            int recieved = socket.EndReceive(asyncResult);
            var tBuf = new byte[recieved];
            Array.Copy(gBuffer, tBuf, recieved);

            // todo: call server message handler to parse instructions

        }

        private static void SendCallback(IAsyncResult asyncResult)
        {
            var client = (SvClient)asyncResult.AsyncState!;
            var socket = client.ClientSocket;
            socket.EndSend(asyncResult);
        }        
    }
}
