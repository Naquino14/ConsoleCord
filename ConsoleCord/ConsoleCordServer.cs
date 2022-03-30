using System;
using c = System.Console;
using System.Net;
using System.Net.Sockets;
using ADIS;
using ADIS.TLS;
using System.Text;

namespace ConsoleCord
{
    internal class ConsoleCordServer
    {
        internal static List<SvClient> Clients = new();
        internal static byte[] gBuffer = new byte[1024];
        #nullable disable
        internal static string ServerName { get; private set; }
        internal static int Port { get; private set; }
        internal static Socket ServerSocket { get; private set; }
        internal static byte[] PublicKey { get; set; }
        #nullable enable

        internal static void CreateServer(int port, string serverName)
        {
            Port = port;
            ServerName = serverName;
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetupServer();
        }

        private static void SetupServer()
        {
            c.WriteLine("Starting consolecord server...");
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            ServerSocket.Listen(1);
            ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void AcceptCallback(IAsyncResult asyncResult)
        {
            var socket = ServerSocket.EndAccept(asyncResult);
            SvClient client = new("", socket, Clients.Count);
            Clients.Add(client);
            // TODO: request CLHELLO
            c.WriteLine("A client has connected...");
            socket.BeginReceive(gBuffer, 0, gBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), client);
            ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        internal static void RecieveCallback(IAsyncResult asyncResult) // TODO: manage handshaking
        {
            // this is the main callback for recieving stuff
            var client = (SvClient)asyncResult.AsyncState!;
            var socket = client.ClientSocket;
            int recieved = socket.EndReceive(asyncResult);
            var buf = new byte[recieved];
            Array.Copy(gBuffer, buf, recieved);

            if (!client.Secured) // use adiscr to marshal commands to secure
            {
                switch (client.incomingType)
                {
                    case ExpectedRawType.key:
                        client.incomingType = ExpectedRawType.command;
                        HandshakeHelper.HandshakeSvEchoTrust(buf, client);
                        break;
                    case ExpectedRawType.trust:
                        client.incomingType = ExpectedRawType.command;
                        CCSCR.HandleEchoTrust(client, buf);
                        break;
                    case ExpectedRawType.command:
                        ADISCommand command = ADISCR.DeMarshalCommand(buf);
                        c.WriteLine($"Payload Recieved: {command}");
                        CCSCR.RegisterCommand(command, client);
                        break;
                }
            }
            else
            {

            }

            socket.BeginReceive(gBuffer, 0, gBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), client);
        }


        internal static void SendCallback(IAsyncResult asyncResult)
        {
            var client = (SvClient)asyncResult.AsyncState!;
            client.ClientSocket.EndSend(asyncResult);
        }
    }
}
