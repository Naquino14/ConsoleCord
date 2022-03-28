using System;
using c = System.Console;
using System.Net;
using System.Net.Sockets;
using ADIS;
using ADIS.TLS;
using System.Text;

namespace ConsoleCord
{
    public class ConsoleCordClient
    {
        public string ClientName { get; private set; }
        public int Port { get; private set; }
        internal Socket ClientSocket { get; private set; }
        public ConsoleCordClient(int port, string clientName)
        {
            this.Port = port;
            this.ClientName = clientName;
            this.ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LoopConnect();
            StartHandshake();
            SendLoop();
        }

        private void LoopConnect()
        {
            int attempts = 0;
            while(!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    c.WriteLine($"Connecting to server, attempt {attempts}");
                    ClientSocket.Connect(IPAddress.Loopback, Port);
                }
                catch (SocketException e)
                {
                    if (attempts > 10)
                    {
                        c.Clear();
                        c.WriteLine($"Could not connect to host. {e.ToString()}");
                        Environment.Exit(-1);
                    }
                }
            }
            c.WriteLine("Connected to server.");
        }

        private void StartHandshake()
        {
            // TODO: ADIS Handshake

            // TODO: send clhello
        }

        private void SendLoop()
        {
            c.WriteLine("attempting to send packet...");
            // as a test, send an echo request
            Command echoReq = new(Ins.echo, new string[] { "hey", "there!" });
            var packet = ADISCR.MarshalCommand(echoReq);

            ClientSocket.Send(packet);
            c.WriteLine($"Sent packet: {echoReq}");
            var buf = new byte[1024];
            c.WriteLine("Waiting for response...");
            int rec = ClientSocket.Receive(buf);
            var payload = new byte[rec];
            Array.Copy(buf, payload, rec);
            c.WriteLine($"Recieved payload: {EH.B2S(payload)}");

            //for (;;)
            //{
            //    // this is the send loop. this is also where most of the work is prob gonna be done client side
            //}
        }
    }
}
