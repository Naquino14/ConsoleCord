using System;
using c = System.Console;
using EH = ConsoleCord.EncodingHelper;
using ADIS;
using ADIS.TLS;
using System.Net;
using System.Net.Sockets;

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
        #nullable enable

        public static void CreateClient(string sessionIP, int port, string clientName, string sessionName)
        {
            Port = port;
            ClientName = clientName;
            SessionName = sessionName;
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LoopConnect();
            // handshake
            Debug();
        }

        private static void LoopConnect()
        {
            int attempts = 0;
            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    c.WriteLine($"Connecting to server, attempt {attempts}");
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
        }
        
        private static void Debug()
        {
            var rec = SendPacketAndListen(EH.S2B("echo \"Hello World!\""));
            c.WriteLine(EH.B2S(rec).Skip(EH.B2S(rec).Split(' ').Length + 1));
        }

        private static void SendPacket(byte[] packet) => ClientSocket.Send(packet);
        
        private static byte[] SendPacketAndListen(byte[] packet)
        {
            byte[] buf = new byte[1024], cmdBuf = new byte[0];
            SendPacket(packet);
            int rec = ClientSocket.Receive(buf);
            Array.Copy(buf, cmdBuf, rec);
            return cmdBuf;
        }
    }
}
