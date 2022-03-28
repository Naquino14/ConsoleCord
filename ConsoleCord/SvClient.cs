using System.Net;
using System.Net.Sockets;

namespace ConsoleCord
{
    public class SvClient
    {
        public string ClientName { get; set; }
        public Socket ClientSocket { get; private set; }
        public int ClientNumber { get; internal set; }
        public bool Secured { get; internal set; }

        public SvClient(string clientName, Socket clientSocket, int clientNumber)
        {
            this.ClientName = clientName;
            this.ClientSocket = clientSocket;
            this.ClientNumber = clientNumber;
            this.Secured = false;
        }
    }
}
