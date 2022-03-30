using System;
using c = System.Console;
using System.Net;
using System.Net.Sockets;
using ADIS;
using ADIS.TLS;
using System.Text;
using System.Security.Cryptography;

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
        internal static byte[] SvPublicKey { get; set; }
        internal static byte[] PrivateKey { get; set; }
        internal static byte[] CurrentIV { get; set; }
        #nullable enable

        public static void Createclient(string sessionIP, int port, string clientName, string sessionName)
        {
            Port = port;
            ClientName = clientName;
            SessionName = sessionName;
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LoopConnect();
            StartHandshake();
            //SendLoop();
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
            c.WriteLine($"Svhello recieved? {svHello}. Checking...");
            if (svHello.args is not null && svHello.args.Length >= 1 && svHello.args[0] == SessionName)
                c.WriteLine($"SVHello is valid. Hello {svHello.args![0]}!");
            else
                CCCCR.RefuseConnection("Session name and server name mismatch. You cant trick me cheif!");
            ADISCommand reqKey = new(ADISinstruction.reqKey);
            c.WriteLine($"Requesting key from server... {reqKey}");
            CCCCR.SendCommand(reqKey);
            rec = ClientSocket.Receive(buf);
            var key = new byte[rec];
            Array.Copy(buf, key, rec);
            c.Write($"Key from server recieved: ");
            foreach (var b in key)
                c.Write($"{b:X}");
            c.WriteLine($"\nWith a key size of {key.Length} bytes.");
            SvPublicKey = key;

            // send clients key
            c.WriteLine("Sending server public key...");
            ECDiffieHellmanCng clientCng = new();
            clientCng.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            clientCng.HashAlgorithm = CngAlgorithm.Sha256;
            PublicKey = clientCng.PublicKey.ToByteArray();
            CCCCR.SendPacket(PublicKey);
            c.WriteLine("Key sent. Generating shared private key.");

            // keygen shit
            PrivateKey = clientCng.DeriveKeyMaterial(CngKey.Import(SvPublicKey, CngKeyBlobFormat.EccPublicBlob));
            c.Write("Keygen finished. Key: ");
            foreach (var b in PrivateKey)
                c.Write($"{b:X}");
            c.WriteLine($"\nWith a key size of {PrivateKey.Length} bytes.");
            c.WriteLine("Squishing key into 16 bytes...");
            HandshakeHelper.SquishKey(PrivateKey);

            // echotrust
            c.WriteLine("Listening for echoTrust...");
            rec = ClientSocket.Receive(buf);
            var echoTrustPayload = new byte[rec];
            Array.Copy(buf, echoTrustPayload, rec);
            
            // iv request
            c.WriteLine("Requesting IV from the server.");
            ADISCommand reqIV = new(ADISinstruction.reqIV);
            CCCCR.SendCommand(reqIV);
            c.WriteLine("Listening for IV...");
            rec = ClientSocket.Receive(buf);
            CurrentIV = new byte[16];
            Array.Copy(buf, CurrentIV, rec);
            c.WriteLine("IV from server recieved: ");
            foreach (var b in CurrentIV)
                c.Write($"{b:X}");

            // decrypting echoTrust
            c.WriteLine("\nDecrypting echoTrust...");
            c.WriteLine($"Before: {EH.B2S(echoTrustPayload)}");
            Thread.Sleep(1000);
            ARC128 arc = new();
            cmdBuf = arc.Decrypt(echoTrustPayload, PrivateKey, CurrentIV); // for some reason this isnt decrypting properly...
            c.WriteLine($"After: {EH.B2S(cmdBuf)}");
            foreach (var b in cmdBuf)
                c.Write($"{b:X}");
            c.WriteLine();
            c.WriteLine("Attempting to parse echoTrust.");

            ADISCommand echoTrust = ADISCR.DeMarshalCommand(cmdBuf);
            c.WriteLine($"echoTrust parsed: {echoTrust}.\nChecking echoTrust...");
            if (echoTrust.args is null || echoTrust.args.Length != 4) // refuse the connection
                CCCCR.RefuseConnection("Could not establsih a secure connection between the server and the client.");
            else
                c.WriteLine("echoTrust ok. Responding to server...");
            // echo the trust ig?
            // otp args (0^2)^(1^3)

            byte[] trust = HandshakeHelper.OTPArray(
                HandshakeHelper.OTPArray(EH.S2B(echoTrust.args![0]), EH.S2B(echoTrust.args![2])),
                HandshakeHelper.OTPArray(EH.S2B(echoTrust.args![1]), EH.S2B(echoTrust.args![3])));

            var packet = arc.Encrypt(trust, PrivateKey, CurrentIV);
            CCCCR.SendPacket(packet);

            c.WriteLine($"Waiting for trust response...");

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
