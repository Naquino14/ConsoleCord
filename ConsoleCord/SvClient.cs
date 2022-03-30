using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace ConsoleCord
{
    public enum ExpectedRawType
    {
        command,
        key,
        trust
    }
    public class SvClient
    {
        public string ClientName { get; set; }
        public Socket ClientSocket { get; private set; }
        public int ClientNumber { get; internal set; }
        public bool Secured { get; internal set; }
        internal string[] EchoTrustArgs { get; set; }
        internal byte[] CurrentIV { get; set; }
        internal ECDiffieHellmanCng Curve { get; set; }
        internal byte[] sharedPrivateKey = new byte[16];
        internal byte[] svPublicKey = new byte[16];
        internal ExpectedRawType incomingType;

#nullable disable
        public SvClient(string clientName, Socket clientSocket, int clientNumber)
        {
        #nullable enable
            this.ClientName = clientName;
            this.ClientSocket = clientSocket;
            this.ClientNumber = clientNumber;
            this.Secured = false;
        }
    }
}
