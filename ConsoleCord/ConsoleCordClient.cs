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
        #nullable disable
        public static string ClientName { get; private set; }
        public static int Port { get; private set; }
        internal static Socket ClientSocket { get; private set; }
        public static string SessionName { get; private set; }
        public static string SessionIP { get; private set; }

        internal static byte[] PublicKey { get; set; }
        #nullable enable

        public static void Createclient(string sessionIP, int port, string clientName, string sessionName)
        {
            Port = port;
            ClientName = clientName;
            SessionName = sessionName;
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LoopConnect();
            StartHandshake();
            SendLoop();
        }

        private static void LoopConnect()
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
                        c.WriteLine($"Could not connect to host. {e}");
                        Environment.Exit(-1);
                    }
                }
            }
            c.WriteLine("Connected to server.");
        }

        private static void StartHandshake()
        {
            byte[] buf = new byte[1024];
            byte[] cmdBuf;
            int rec = 0;

            // TODO: ADIS Handshake
            c.WriteLine("Initiating handshake.");

            // TODO: send clhello
            c.WriteLine("Sending CLHELLO...");
            var helloArgs = new string[] { ClientName, "0.0.1", "0.0.1", "0.0.1"};
            // name, arc version, ach version, akg version
            ADISCommand clhello = new(ADISinstruction.clhello, helloArgs);
            CCCCR.SendCommand(clhello);
            c.WriteLine("Awaiting response.");

            // get response
            rec = ClientSocket.Receive(buf);
            cmdBuf = new byte[rec];
            Array.Copy(buf, cmdBuf, rec);
            ADISCommand svHello = ADISCR.DeMarshalCommand(cmdBuf);
            CCCCR.CheckSVHello(svHello);
            c.WriteLine("SVHello is valid.");

            // TODO: send PM key
        }

        private static void SendLoop()
        {
            c.WriteLine("attempting to send packet...");
            // as a test, send an echo request
            ADISCommand echoReq = new(ADISinstruction.echo, new string[] { "hey", "there!" });
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
