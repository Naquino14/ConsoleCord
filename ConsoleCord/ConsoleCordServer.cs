using System;
using c = System.Console;
using System.Net;
using System.Net.Sockets;
using ADIS;
using ADIS.TLS;
using System.Text;

namespace ConsoleCord
{
    internal class ConsolecordServer
    {
        private List<Socket> Clients = new();
        private byte[] gBuffer = new byte[1024];
        internal string ServerName { get; private set; }
        internal int Port { get; private set; }
        internal Socket ServerSocket { get; private set; }
        internal ConsolecordServer(int port, string serverName)
        {
            this.Port = port;
            this.ServerName = serverName;
            this.ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetupServer();
        }

        private void SetupServer()
        {
            c.WriteLine("Starting consolecord server...");
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            ServerSocket.Listen(1);
            ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private void AcceptCallback(IAsyncResult asyncResult)
        {
            var socket = ServerSocket.EndAccept(asyncResult);
            Clients.Add(socket);
            // TODO: request CLHELLO
            c.WriteLine("A client has connected...");
            socket.BeginReceive(gBuffer, 0, gBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
            ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private void RecieveCallback(IAsyncResult asyncResult) // TODO: manage handshaking
        {
            var socket = (Socket)asyncResult.AsyncState!;
            int recieved = socket.EndReceive(asyncResult);
            var buf = new byte[recieved];
            Array.Copy(gBuffer, buf, recieved);
            //var payload = B2T(buf).Split(' ');
            var command = ADISCR.DeMarshalCommand(buf);
            c.WriteLine($"Payload recieved: {EH.B2S(buf)}. Parsed command: {command}");
            // call the command registry to begin the handshake process.
            c.WriteLine("Responding...");
            // testing
            if (command.instruction == Ins.echo)
            {
                string concatArgs = "";
                if (command.args is not null && command.args.Length > 1)
                    foreach (var s in command.args)
                        concatArgs += $"{s} ";
                byte[] packet = EH.S2B(command.args is not null ? concatArgs : "I cant echo nothing silly!");
                socket.BeginSend(packet, 0, packet.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            }

            socket.BeginReceive(gBuffer, 0, gBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
        }


        private void SendCallback(IAsyncResult asyncResult)
        {
            var socket = (Socket)asyncResult.AsyncState!;
            socket.EndSend(asyncResult);
        }
    }
}
